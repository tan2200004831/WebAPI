CREATE DATABASE NIIEPayDB;
GO

USE NIIEPayDB;
GO

CREATE TABLE Accounts (
    AccountNumber NVARCHAR(50) PRIMARY KEY,
    AccountHolder NVARCHAR(100) NOT NULL,
    Phone NVARCHAR(20) NOT NULL,
    CitizenId NVARCHAR(20) NOT NULL,
    ExpiryDate DATETIME NOT NULL,
    AvailableBalance DECIMAL(18, 2) NOT NULL
);

CREATE TABLE Transactions (
    TransactionId NVARCHAR(50) PRIMARY KEY,
    AccountNumber NVARCHAR(50) NOT NULL,
    Amount DECIMAL(18, 2) NOT NULL,
    TransactionTime DATETIME NOT NULL,
    BalanceAfter DECIMAL(18, 2) NOT NULL,
    Note NVARCHAR(255) NOT NULL,
    TransactionType NVARCHAR(20),
    FOREIGN KEY (AccountNumber) REFERENCES Accounts(AccountNumber)
);

CREATE TABLE Savings (
    SavingId NVARCHAR(50) PRIMARY KEY,
    AccountNumber NVARCHAR(50) NOT NULL,
    Amount DECIMAL(18, 2) NOT NULL,
    TermMonths INT NOT NULL,
    InterestRate FLOAT NOT NULL,
    StartDate DATETIME NOT NULL,
    MaturityDate DATETIME NOT NULL,
    AutoRenew BIT NOT NULL,
    FOREIGN KEY (AccountNumber) REFERENCES Accounts(AccountNumber)
);

CREATE TABLE InterestRates (
    TermMonths INT PRIMARY KEY,
    InterestRatePercent FLOAT NOT NULL
);

INSERT INTO InterestRates (TermMonths, InterestRatePercent) VALUES
(1, 3.5),
(2, 3.7),
(3, 3.8),
(6, 4.8),
(9, 4.9),
(12, 5.2),
(18, 5.5),
(24, 5.8),
(36, 5.8);
