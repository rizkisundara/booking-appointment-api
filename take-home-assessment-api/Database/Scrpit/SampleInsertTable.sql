-- Insert sample data if tables are empty
IF NOT EXISTS (SELECT 1 FROM dbo.agency)
BEGIN
    INSERT INTO dbo.agency (name, is_active) VALUES ('Main Agency', 1);
    INSERT INTO dbo.agency (name, is_active) VALUES ('Branch Office', 1);
    PRINT 'Sample data inserted into agency table.';
END
ELSE
BEGIN
    PRINT 'Agency table already contains data.';
END
GO

IF NOT EXISTS (SELECT 1 FROM dbo.agency_setting)
BEGIN
    INSERT INTO dbo.agency_setting (agency_id, max_appointments) VALUES (1, 15);
    INSERT INTO dbo.agency_setting (agency_id, max_appointments) VALUES (2, 10);
    PRINT 'Sample data inserted into agency_setting table.';
END
ELSE
BEGIN
    PRINT 'Agency_setting table already contains data.';
END
GO

IF NOT EXISTS (SELECT 1 FROM dbo.customer)
BEGIN
    INSERT INTO dbo.customer (full_name, phone, email) VALUES ('John Doe', '123-456-7890', 'john@example.com');
    INSERT INTO dbo.customer (full_name, phone, email) VALUES ('Jane Smith', '098-765-4321', 'jane@example.com');
    PRINT 'Sample data inserted into customer table.';
END
ELSE
BEGIN
    PRINT 'Customer table already contains data.';
END
GO
