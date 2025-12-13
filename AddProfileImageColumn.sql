-- Migration: Add ProfileImage column to AspNetUsers table
-- Run this script in your SQL Server database if migration doesn't work

USE [MyShop];
GO

IF NOT EXISTS (SELECT * FROM sys.columns WHERE object_id = OBJECT_ID(N'[dbo].[AspNetUsers]') AND name = 'ProfileImage')
BEGIN
    ALTER TABLE [dbo].[AspNetUsers]
    ADD [ProfileImage] nvarchar(max) NULL;
    
    PRINT 'ProfileImage column added successfully';
END
ELSE
BEGIN
    PRINT 'ProfileImage column already exists';
END
GO

