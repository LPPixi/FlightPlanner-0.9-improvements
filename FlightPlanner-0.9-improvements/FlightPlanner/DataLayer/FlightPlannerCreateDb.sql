declare @databaseName sysname;
set @databaseName = 'FlightPlanner';

set nocount on;

-- better:
if db_id(@databaseName) is not null
	BEGIN
	   PRINT 'Database already exists';
	END
ELSE
	BEGIN
		-- CREATE DATABASE @databaseName does not work, use dynamic SQL:
		-- exec ('use master; CREATE DATABASE ' + @databaseName  + ';')
		create database FlightPlanner;		
	END;

-- You need a go after creating a database so that "use FlightPlanner" works, the GO has to be removed when
-- this file should be executed by ADO.NET
-- After a GO the variable @databaseName is no longer available.
-- GO 

use FlightPlanner;	


PRINT 'Creating tables';

-- Remove all foreign key constraints first before dropping any tables.
-- This ensures that the tables can be dropped in any order.
-- Constraint names must be unique.


IF OBJECT_ID('FK_Plane_PlaneType', 'F') IS NOT NULL
	alter TABLE dbo.Plane drop constraint FK_Plane_PlaneType;

 
IF OBJECT_ID('FK_Plane_Airline', 'F') IS NOT NULL
	alter TABLE dbo.Plane drop constraint FK_Plane_Airline;


IF OBJECT_ID('FK_Pilot_Airline', 'F') IS NOT NULL
	alter TABLE dbo.Pilot drop constraint FK_Pilot_Airline;


IF OBJECT_ID('FK_PilotTraining_Pilot', 'F') IS NOT NULL
	alter TABLE dbo.PilotTraining drop constraint FK_PilotTraining_Pilot;


IF OBJECT_ID('FK_PilotTraining_Training', 'F') IS NOT NULL
	alter TABLE dbo.PilotTraining drop constraint FK_PilotTraining_Training;


IF OBJECT_ID('FK_Flight_Plane', 'F') IS NOT NULL
	alter TABLE dbo.Flight drop constraint FK_Flight_Plane;


IF OBJECT_ID('FK_PilotRoster_Pilot', 'F') IS NOT NULL
	alter TABLE dbo.PilotRoster drop constraint FK_PilotRoster_Pilot;


IF OBJECT_ID('FK_PilotRoster_Flight', 'F') IS NOT NULL
	alter TABLE dbo.PilotRoster drop constraint FK_PilotRoster_Flight;


IF OBJECT_ID('FK_Booking_Customer', 'F') IS NOT NULL
	alter TABLE dbo.Booking drop constraint FK_Booking_Customer;


IF OBJECT_ID('FK_Booking_Flight', 'F') IS NOT NULL
	alter TABLE dbo.Booking drop constraint FK_Booking_Flight;


select * from INFORMATION_SCHEMA.REFERENTIAL_CONSTRAINTS;

-- https://stackoverflow.com/questions/9565818/drop-if-exists-vs-drop
-- Instead of OBJECT_ID: 
-- drop table if exists dbo.Airline

-- since SQL server 2016
drop table if exists dbo.Airline


IF OBJECT_ID('dbo.Airline', 'U') IS NOT NULL
	DROP TABLE dbo.Airline


-- second parameter is optional
IF OBJECT_ID('dbo.PlaneType') IS NOT NULL
	DROP TABLE dbo.PlaneType


IF OBJECT_ID('dbo.Plane', 'U') IS NOT NULL
	DROP TABLE dbo.Plane;


IF OBJECT_ID('dbo.Pilot', 'U') IS NOT NULL
	DROP TABLE dbo.Pilot


IF OBJECT_ID('dbo.PilotTraining', 'U') IS NOT NULL
	DROP TABLE dbo.PilotTraining


IF OBJECT_ID('dbo.Training', 'U') IS NOT NULL
	DROP TABLE dbo.Training


IF OBJECT_ID('dbo.Flight', 'U') IS NOT NULL
	DROP TABLE dbo.Flight


IF OBJECT_ID('dbo.Customer', 'U') IS NOT NULL
	DROP TABLE dbo.Customer


IF OBJECT_ID('dbo.PilotRoster', 'U') IS NOT NULL
	DROP TABLE dbo.PilotRoster


IF OBJECT_ID('dbo.Booking', 'U') IS NOT NULL
	DROP TABLE dbo.Booking


-- "On update cascade" or "on delete cascade" must not create multiple or circular paths.
-- Multiple path: When there are two paths from the table where a record is deleted/updated to the table where
-- the "on update cascade"/"on delete cascade" are specified. 
CREATE TABLE dbo.Airline (
	Id int primary key,
	RegisteredCompanyName nvarchar(40),
	Country nvarchar(40) NOT NULL,
	HeadQuarters nvarchar(40) NOT NULL,	
);


CREATE TABLE dbo.PlaneType (
	Id nvarchar(40) primary key,
	Seats int NOT NULL,
	Velocity int NOT NULL
);


CREATE TABLE dbo.Plane (
	Id int primary key,
	OwnershipDate date NOT NULL,
	LastMaintenance date NOT NULL,
	PlaneTypeId nvarchar(40),
	AirlineId int,
	CONSTRAINT FK_Plane_PlaneType FOREIGN KEY(PlaneTypeId)
	REFERENCES PlaneType(Id) on delete set null on update cascade,
	CONSTRAINT FK_Plane_Airline
	FOREIGN KEY (AirlineId) REFERENCES Airline(Id) 
	on delete set null on update no action 
);


CREATE TABLE dbo.Pilot (
	Id int primary key,
	LastName nvarchar(40) NOT NULL,
	Birthday date NOT NULL,
	-- use check constraints to implement an enum:
	Qualification nvarchar(40) CHECK (Qualification IN('Captain', 'Copilot')), 
	FlightHours int NOT NULL CHECK (FlightHours >= 0),
	FirstDate date NOT NULL,
	AirlineId int NOT NULL default -1,
	CONSTRAINT FK_Pilot_Airline
	FOREIGN KEY (AirlineId) REFERENCES Airline(Id) on delete set default
);


CREATE TABLE dbo.Training (
	Id int primary key,
	Description nvarchar(40) NOT NULL,
	Level int NOT NULL,
	CHECK (Level > 0 and Level <= 5)
);


CREATE TABLE dbo.PilotTraining(
	PilotId int NOT NULL,
	TrainingId int NOT NULL,
	Date date NOT NULL,
	CONSTRAINT PK_TrainingsRoster PRIMARY KEY (PilotId, TrainingId),
	CONSTRAINT FK_PilotTraining_Pilot
	FOREIGN KEY (PilotId) REFERENCES Pilot(Id) on delete cascade on update cascade,
	CONSTRAINT FK_PilotTraining_Training
	FOREIGN KEY (TrainingId) REFERENCES Training(Id) on delete cascade on update cascade,
);


CREATE TABLE dbo.Flight (
	Id int primary key,
	Departure nvarchar(40) NOT NULL,
	Destination nvarchar(40) NOT NULL,
	Duration int NOT NULL, -- duration in minutes
	DepartureDate date NOT NULL,
	PlaneId int,
	CONSTRAINT FK_Flight_Plane
	FOREIGN KEY (PlaneId) REFERENCES Plane(Id)
);


CREATE TABLE dbo.Customer (
	Id int primary key,
	LastName nvarchar(40) NOT NULL,
	Birthday date NOT NULL,
	City nvarchar(40) NOT NULL,
);


-- for the m:n relationship between Pilot and Flight
-- PilotRoster = Dienstplan der Piloten
CREATE TABLE PilotRoster (
	PilotId int,
	FlightId int,
	CONSTRAINT PK_PilotRoster PRIMARY KEY(PilotId, FlightId),
	CONSTRAINT FK_PilotRoster_Pilot
	FOREIGN KEY (PilotId) REFERENCES Pilot(Id) on delete cascade on update cascade,
	CONSTRAINT FK_PilotRoster_Flight
	FOREIGN KEY (FlightId) REFERENCES Flight(Id) on delete cascade on update cascade,
);

-- for the m:n relationship between Flight and Customer (Passenger)
CREATE TABLE Booking (
	FlightId int default -1,
	CustomerId int,
	Seats int NOT NULL,
	TravelClass int NOT NULL,
	Price MONEY NOT NULL,
	CONSTRAINT PK_Booking PRIMARY KEY (FlightId, CustomerId),
	CONSTRAINT FK_Booking_Customer
	FOREIGN KEY (CustomerId) REFERENCES Customer(Id) on delete cascade on update cascade,
	CONSTRAINT FK_Booking_Flight
	FOREIGN KEY (FlightId) REFERENCES Flight(Id) on delete no action on update cascade
);


PRINT 'Creating tables finished'

-- Turn on SQLCMD mode in Query menu
-- :r .\InsertData.sql

use FlightPlanner;


INSERT INTO PlaneType VALUES ('Boeing 747', 660, 920);
INSERT INTO PlaneType VALUES ('Airbus A380', 853, 1185);
INSERT INTO PlaneType VALUES ('Concorde', 128, 2179);


INSERT INTO Airline VALUES (-1, 'Default', 'Default', 'Default');
INSERT INTO Airline VALUES (1, 'Austrian Airlines', 'Austria', 'Vienna');
INSERT INTO Airline VALUES (2, 'Air France', 'France', 'Paris');
INSERT INTO Airline VALUES (3, 'Iberia', 'Spain', 'Madrid');


INSERT INTO Plane VALUES (10, '19811203', '20001230', 'Concorde', 2);
INSERT INTO Plane VALUES (11, '19900602', '20051023', 'Boeing 747', 2);
INSERT INTO Plane VALUES (21, '19931227', '20150928', 'Airbus A380', 1);
INSERT INTO Plane VALUES (22, '19990722', '20170316', 'Boeing 747', 1);
INSERT INTO Plane VALUES (33, '20000117', '20190418', 'Concorde', 3);


INSERT INTO Flight VALUES (203, 'Berlin', 'Paris', 120, '20180202', 21);
INSERT INTO Flight VALUES (204, 'Frankfurt', 'Atlanta', 900, '20180410', 11);
INSERT INTO Flight VALUES (205, 'Berlin', 'Paris', 360, '20160522', 10);
INSERT INTO Flight VALUES (206, 'Hawaii', 'Moskau', 480, '20170612', 22);
INSERT INTO Flight VALUES (207, 'Paris', 'London', 120, '20190202', 33);
INSERT INTO Flight VALUES (208, 'Hawaii', 'Atlanta', 150, '20140503', 11);
INSERT INTO Flight VALUES (209, 'Moskau', 'Paris', 210, '20130213', 10);
INSERT INTO Flight VALUES (210, 'Berlin', 'Moskau', 120, '20120112', 33);


INSERT INTO Customer VALUES (1000, 'Scott', '20020110', 'Vienna');
INSERT INTO Customer VALUES (1001, 'Meier', '20011231', 'Prague');
INSERT INTO Customer VALUES (1002, 'Huber', '20020515', 'Rome');
INSERT INTO Customer VALUES (1003, 'King', '20000923', 'Rome');
INSERT INTO Customer VALUES (1004, 'Puyol', '20000917', 'Paris');
INSERT INTO Customer VALUES (1005, 'Becaud', '20000916', 'London');


INSERT INTO Pilot VALUES (111, 'Nemec', '20020603', 'Captain', 80, '20150101', 2);
INSERT INTO Pilot VALUES (222, 'Nordmann', '19990410', 'Captain', 150, '20130101', 2);
INSERT INTO Pilot VALUES (333, 'Nagel', '20000227', 'Copilot', 100, '20160101', 1);
INSERT INTO Pilot VALUES (444, 'Nero', '20000813', 'Copilot', 60, '20150101', 1);
INSERT INTO Pilot VALUES (555, 'Nano', '19920410', 'Captain', 120, '20140101', 2);
INSERT INTO Pilot VALUES (666, 'Nano1', '19920410', 'Captain', 120, '20140101', 3);
INSERT INTO Pilot VALUES (777, 'Nano2', '19920410', 'Captain', 100, '20140101', 2);
INSERT INTO Pilot VALUES (888, 'Nano3', '19920410', 'Captain', 800, '20140101', 3);
INSERT INTO Pilot VALUES (999, 'Nano4', '19920410', 'Copilot', 700, '20140101', 1);
INSERT INTO Pilot VALUES (112, 'Nano5', '19920410', 'Captain', 600, '20140101', 3);
INSERT INTO Pilot VALUES (113, 'Nano6', '19920410', 'Captain', 500, '20140101', 1);
INSERT INTO Pilot VALUES (114, 'Nano7', '19920410', 'Copilot', 400, '20140101', 3);



INSERT INTO Training VALUES (1, 'Flight Training', 1);
INSERT INTO Training VALUES (2, 'Emergency Training', 1);
INSERT INTO Training VALUES (3, 'Flight Training', 2);
INSERT INTO Training VALUES (4, 'Airbus A380 Training', 5);
INSERT INTO Training VALUES (5, 'Airbus A380 Training', 4);
INSERT INTO Training VALUES (6, 'Airbus A380 Training', 3);
INSERT INTO Training VALUES (7, 'Airbus A380 Training', 2);
INSERT INTO Training VALUES (8, 'Airbus A380 Training', 1);



INSERT INTO PilotTraining VALUES (111, 1, '2019-02-23');
INSERT INTO PilotTraining VALUES (222, 1, '2016-02-04');
INSERT INTO PilotTraining VALUES (111, 2, '2014-03-12');
INSERT INTO PilotTraining VALUES (444, 4, '2019-02-02');
INSERT INTO PilotTraining VALUES (555, 2, '2013-09-12');


INSERT INTO PilotRoster VALUES (111, 204);
INSERT INTO PilotRoster VALUES (222, 208);
INSERT INTO PilotRoster VALUES (111, 205);
INSERT INTO PilotRoster VALUES (444, 206);
INSERT INTO PilotRoster VALUES (555, 207);
INSERT INTO PilotRoster VALUES (333, 203);
INSERT INTO PilotRoster VALUES (444, 203);
INSERT INTO PilotRoster VALUES (111, 209);
INSERT INTO PilotRoster VALUES (555, 210);


INSERT INTO Booking VALUES (209, 1000, 1, 2, 240);
INSERT INTO Booking VALUES (206, 1002, 2, 1, 300);
INSERT INTO Booking VALUES (205, 1003, 3, 2, 180);
INSERT INTO Booking VALUES (204, 1004, 1, 2, 150);
INSERT INTO Booking VALUES (207, 1005, 2, 1, 250);
INSERT INTO Booking VALUES (209, 1002, 3, 2, 340);
INSERT INTO Booking VALUES (206, 1003, 1, 1, 250);
INSERT INTO Booking VALUES (205, 1004, 1, 2, 150);
INSERT INTO Booking VALUES (204, 1003, 2, 2, 220);
INSERT INTO Booking VALUES (207, 1001, 3, 1, 450);



use FlightPlanner;

Select * from Airline;
Select * from Plane;
Select * from PlaneType;

Select * from Pilot;
select * from PilotTraining;
select * from Training;
Select * from PilotRoster;

Select * from Customer;
Select * from Booking;
Select * from Flight;


EndScript:

Select * from PilotRoster;
select * from Airline;
select * from Pilot;
select * from Flight;
select * from Booking;