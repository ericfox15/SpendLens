-- Budget Helper Initial Schema
-- DbUp runs this once and tracks it in SchemaVersions.
-- Never edit a deployed script — add a new numbered script instead.

-- Users & permissions
CREATE TABLE dbo.[User] (
    UserId       INT IDENTITY PRIMARY KEY,
    Username     NVARCHAR(100) NOT NULL UNIQUE,
    PasswordHash NVARCHAR(200) NOT NULL
);

CREATE TABLE dbo.Permission (
    UserId     INT  NOT NULL REFERENCES dbo.[User](UserId),
    Permission BIGINT NOT NULL,
    PRIMARY KEY (UserId, Permission)
);

-- Financial structure
CREATE TABLE dbo.Account (
    AccountId   INT IDENTITY PRIMARY KEY,
    Name        NVARCHAR(100) NOT NULL,
    AccountType TINYINT       NOT NULL,  -- EAccountType: 1=Checking 2=CreditCard 3=Cash 4=Savings
    IsActive    BIT           NOT NULL DEFAULT 1
);

CREATE TABLE dbo.Category (
    CategoryId   INT IDENTITY PRIMARY KEY,
    Name         NVARCHAR(100)  NOT NULL,
    MonthlyLimit DECIMAL(12, 2) NULL        -- NULL = no limit; set to trigger over-budget warnings
);

-- Bill = recurring obligation (like a medication order in eMAR)
CREATE TABLE dbo.Bill (
    BillId         INT IDENTITY PRIMARY KEY,
    AccountId      INT            NOT NULL REFERENCES dbo.Account(AccountId),
    CategoryId     INT            NOT NULL REFERENCES dbo.Category(CategoryId),
    Name           NVARCHAR(100)  NOT NULL,
    ExpectedAmount DECIMAL(12, 2) NOT NULL,
    ScheduleType   TINYINT        NOT NULL,  -- 1=Monthly 2=Biweekly 3=Weekly
    DayOfMonth     TINYINT        NULL,      -- used for Monthly schedule
    AnchorDateUTC  DATETIMEOFFSET NULL,      -- used for Biweekly/Weekly anchor
    IsAutoPay      BIT            NOT NULL DEFAULT 0,
    IsActive       BIT            NOT NULL DEFAULT 1
);

-- Payment = one due-instance of a bill (or an ad-hoc expense when BillId IS NULL)
CREATE TABLE dbo.Payment (
    PaymentId    INT IDENTITY PRIMARY KEY,
    BillId       INT            NULL     REFERENCES dbo.Bill(BillId),   -- NULL = ad-hoc (PRN)
    AccountId    INT            NOT NULL REFERENCES dbo.Account(AccountId),
    CategoryId   INT            NOT NULL REFERENCES dbo.Category(CategoryId),
    DueAtUTC     DATETIMEOFFSET NULL,
    PaidAtUTC    DATETIMEOFFSET NULL,
    Amount       DECIMAL(12, 2) NULL,
    Status       TINYINT        NOT NULL,  -- 0=Pending 1=Paid 2=Skipped
    Note         NVARCHAR(500)  NULL,
    ModifiedBy   INT            NOT NULL,
    ModifiedUTC  DATETIMEOFFSET NOT NULL DEFAULT SYSDATETIMEOFFSET(),
    -- Prevents double-generation of the same bill+due-date combination
    CONSTRAINT UQ_Payment_Bill_Due UNIQUE (BillId, DueAtUTC)
);

-- PaymentHistory = conflict log for offline-sync scenarios (mirrors eMAR conflict resolution)
CREATE TABLE dbo.PaymentHistory (
    PaymentHistoryId INT IDENTITY PRIMARY KEY,
    PaymentId        INT            NOT NULL REFERENCES dbo.Payment(PaymentId),
    Status           TINYINT        NOT NULL,
    Amount           DECIMAL(12, 2) NULL,
    Note             NVARCHAR(500)  NULL,
    ModifiedBy       INT            NOT NULL,
    ModifiedUTC      DATETIMEOFFSET NOT NULL,
    IsOfflineConflict BIT           NOT NULL DEFAULT 0
);

-- DB-driven settings (mirrors eMAR's Setting.General table)
CREATE TABLE dbo.Setting (
    Path  NVARCHAR(200) PRIMARY KEY,
    Value NVARCHAR(MAX) NOT NULL
);

-- Audit log — every request is logged here by AuditRequestGlobalAttribute
CREATE TABLE dbo.AuditLog (
    AuditLogId BIGINT IDENTITY PRIMARY KEY,
    UserId     INT            NULL,       -- NULL for anonymous requests (login)
    Route      NVARCHAR(400)  NOT NULL,
    Method     NVARCHAR(10)   NOT NULL,
    CreatedUTC DATETIMEOFFSET NOT NULL DEFAULT SYSDATETIMEOFFSET()
);

-- ── Seed data ──────────────────────────────────────────────────────────────
-- Default categories
INSERT INTO dbo.Category (Name, MonthlyLimit) VALUES
    ('Rent',           NULL),
    ('Utilities',      300.00),
    ('Groceries',      600.00),
    ('Dining',         200.00),
    ('Gas',            150.00),
    ('Entertainment',  100.00),
    ('Health',         NULL),
    ('Transportation', NULL),
    ('Savings',        NULL),
    ('Uncategorized',  NULL);

-- Default settings (action routes for the pass pipeline)
INSERT INTO dbo.Setting (Path, Value) VALUES
    ('Pass:ShowAutoPay',           'true'),
    ('Budget:OverBudgetWarnPercent', '100'),
    ('Pass:ActionRoutes', '{
        "PAY":   ["CHECK_OVER_BUDGET", "GET_NOTE", "SUBMIT_BOTH", "NOTIFICATION_SUCCESS"],
        "SKIP":  ["GET_SKIP_REASON",   "SUBMIT_BOTH", "NOTIFICATION_SUCCESS"],
        "RESET": ["SUBMIT_BOTH",       "NOTIFICATION_SUCCESS"]
    }');

-- Default account
INSERT INTO dbo.Account (Name, AccountType, IsActive) VALUES ('Checking', 1, 1);
