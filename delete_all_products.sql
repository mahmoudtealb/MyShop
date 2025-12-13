-- SQL Script to delete all products from MyShop database
-- Run this script in SQL Server Management Studio

USE [MyShop]
GO

-- Delete all products
DELETE FROM [dbo].[Products]
GO

-- Verify deletion (should return 0 rows)
SELECT COUNT(*) AS RemainingProducts FROM [dbo].[Products]
GO

