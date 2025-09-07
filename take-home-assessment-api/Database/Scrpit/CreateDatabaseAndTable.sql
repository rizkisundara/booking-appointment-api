-- DB
IF DB_ID('DB_Booking') IS NULL
    CREATE DATABASE DB_Booking;
GO
USE DB_Booking;
GO

-- 1) agency
CREATE TABLE dbo.agency(
  id           INT IDENTITY(1,1) PRIMARY KEY,
  name         VARCHAR(150) NOT NULL,
  is_active    BIT NOT NULL DEFAULT 1,
  created_by   INT NULL,
  created_on   DATETIME NOT NULL DEFAULT GETDATE(),
  modified_by  INT NULL,
  modified_on  DATETIME NULL,
  deleted_by   INT NULL,
  deleted_on   DATETIME NULL,
  is_delete    BIT NOT NULL DEFAULT 0
);

-- 2) customer
CREATE TABLE dbo.customer(
  id           INT IDENTITY(1,1) PRIMARY KEY,
  full_name    VARCHAR(150) NOT NULL,
  phone        VARCHAR(30) NULL,
  email        VARCHAR(150) NULL,
  created_by   INT NULL,
  created_on   DATETIME NOT NULL DEFAULT GETDATE(),
  modified_by  INT NULL,
  modified_on  DATETIME NULL,
  deleted_by   INT NULL,
  deleted_on   DATETIME NULL,
  is_delete    BIT NOT NULL DEFAULT 0
);

-- 3) agency_setting (kuota per hari)
CREATE TABLE dbo.agency_setting(
  agency_id        INT PRIMARY KEY,
  max_appointments INT NOT NULL DEFAULT 10,
  created_by   INT NULL,
  created_on   DATETIME NOT NULL DEFAULT GETDATE(),
  modified_by  INT NULL,
  modified_on  DATETIME NULL,
  deleted_by   INT NULL,
  deleted_on   DATETIME NULL,
  is_delete    BIT NOT NULL DEFAULT 0,
  CONSTRAINT FK_setting_agency FOREIGN KEY (agency_id) REFERENCES dbo.agency(id)
);

-- 4) holiday
CREATE TABLE dbo.holiday(
  id          INT IDENTITY(1,1) PRIMARY KEY,
  agency_id   INT NOT NULL,
  off_date    DATE NOT NULL,
  reason      VARCHAR(200) NULL,
  created_by   INT NULL,
  created_on   DATETIME NOT NULL DEFAULT GETDATE(),
  modified_by  INT NULL,
  modified_on  DATETIME NULL,
  deleted_by   INT NULL,
  deleted_on   DATETIME NULL,
  is_delete    BIT NOT NULL DEFAULT 0,
  CONSTRAINT FK_holiday_agency FOREIGN KEY (agency_id) REFERENCES dbo.agency(id),
  CONSTRAINT UQ_holiday UNIQUE (agency_id, off_date)
);

-- 5) appointment
CREATE TABLE dbo.appointment(
  id            INT IDENTITY(1,1) PRIMARY KEY,
  agency_id     INT NOT NULL,
  customer_id   INT NOT NULL,
  appt_date     DATE NOT NULL,
  token_number  INT NOT NULL,             -- 2025090401
  status        VARCHAR(30) NOT NULL DEFAULT 'Booked',
  notes         VARCHAR(500) NULL,
  created_by   INT NULL,
  created_on   DATETIME NOT NULL DEFAULT GETDATE(),
  modified_by  INT NULL,
  modified_on  DATETIME NULL,
  deleted_by   INT NULL,
  deleted_on   DATETIME NULL,
  is_delete    BIT NOT NULL DEFAULT 0,
  CONSTRAINT FK_appt_agency   FOREIGN KEY (agency_id)   REFERENCES dbo.agency(id),
  CONSTRAINT FK_appt_customer FOREIGN KEY (customer_id) REFERENCES dbo.customer(id),
  CONSTRAINT UQ_token UNIQUE (agency_id, appt_date, token_number)
);

CREATE TABLE dbo.agency_quota_override(
  agency_id        INT NOT NULL,
  appt_date        DATE NOT NULL,
  max_appointments INT NOT NULL,
  created_on       DATETIME NOT NULL DEFAULT GETDATE(),
  modified_on      DATETIME NULL,
  deleted_on       DATETIME NULL,
  is_delete        BIT NOT NULL DEFAULT 0,
  CONSTRAINT PK_quota_override PRIMARY KEY (agency_id, appt_date),
  CONSTRAINT FK_quota_override_agency FOREIGN KEY (agency_id) REFERENCES dbo.agency(id)
);