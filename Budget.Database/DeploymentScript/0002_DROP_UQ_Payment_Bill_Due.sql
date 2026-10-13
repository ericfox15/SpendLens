-- Drop unique constraint that blocks ad-hoc payment imports
-- Multiple imported transactions can share NULL BillId + same date
IF EXISTS (SELECT 1 FROM sys.objects WHERE name = 'UQ_Payment_Bill_Due' AND type = 'UQ')
    ALTER TABLE dbo.Payment DROP CONSTRAINT UQ_Payment_Bill_Due;
