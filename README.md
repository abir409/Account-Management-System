



?? Author

      ????? Name: Md Mahabubul Alam Abir
      ?? Email: mahbubabir09@gmail.com
      ?? Role: Junior .NET Developer (Test Submission)
















# ?? Mini Account Management System

A basic accounting application built using ASP.NET Core Razor Pages, SQL Server, and stored procedures — developed for a Junior .NET Developer test task.
---
## ?? Technologies Used

- **ASP.NET Core Razor Pages (.NET 8)**
- **MS SQL Server**
- **Stored Procedures only (No LINQ)**
- **ASP.NET Identity (custom roles)**
- **Bootstrap 5**
- **ClosedXML (Excel export)**

---
## ?? Screenshots

### ?? Register and Login Page








### ?? Voucher Entry Form



### ?? Chart of Accounts with Hierarchy











### ?? Voucher List with Export Option



---
## ?? Core Features

### ? User Roles & Permissions
- Roles: `Admin`, `Accountant`, `Viewer`
- Role-based access to modules using ASP.NET Identity

### ? Chart of Accounts
- Create/Edit/Delete accounts
- Parent/Child hierarchy supported
- Stored procedure: `sp_ManageChartOfAccounts`

### ? Voucher Entry
- Types: Journal, Payment, Receipt
- Add multiple debit/credit lines
- Uses stored procedure: `sp_SaveVoucher`
- TVP used for bulk insert of entries

### ? Voucher Listing
- View saved vouchers
- Filtered by date/type (can be added)
- Optional: detailed entries per voucher

### ? Excel Export (Bonus)
- Export vouchers to `.xlsx` using `ClosedXML`
- Available on voucher list page
---

For Database Setup go to the next page



## ??? Database Setup

All database operations are handled using **stored procedures** and **TVPs**.
**sp_ManageChartOfAccounts

USE [MiniAccountDB]
GO
/****** Object:  StoredProcedure [dbo].[sp_ManageChartOfAccounts]    Script Date: 6/30/2025 11:33:34 PM ******/
SET ANSI_NULLS ON
GO
SET QUOTED_IDENTIFIER ON
GO
ALTER PROCEDURE [dbo].[sp_ManageChartOfAccounts]
    @Action NVARCHAR(10),        -- 'INSERT', 'UPDATE', 'DELETE'
    @AccountId INT = NULL,
    @AccountName NVARCHAR(100) = NULL,
    @ParentId INT = NULL,
    @AccountType NVARCHAR(50) = NULL
AS
BEGIN
    SET NOCOUNT ON;

    IF @Action = 'INSERT'
    BEGIN
        INSERT INTO ChartOfAccounts (AccountName, ParentId, AccountType)
        VALUES (@AccountName, @ParentId, @AccountType)
    END

    ELSE IF @Action = 'UPDATE'
    BEGIN
        UPDATE ChartOfAccounts
        SET AccountName = @AccountName,
            ParentId = @ParentId,
            AccountType = @AccountType
        WHERE AccountId = @AccountId
    END

    ELSE IF @Action = 'DELETE'
    BEGIN
        DELETE FROM ChartOfAccounts WHERE AccountId = @AccountId
    END
END;


**TVP

CREATE TYPE TVP_VoucherEntry AS TABLE
(
    AccountId INT,
    DebitAmount DECIMAL(18,2),
    CreditAmount DECIMAL(18,2)
);


**sp_SaveVoucher

USE [MiniAccountDB]
GO
/****** Object:  StoredProcedure [dbo].[sp_SaveVoucher]    Script Date: 6/30/2025 11:40:31 PM ******/
SET ANSI_NULLS ON
GO
SET QUOTED_IDENTIFIER ON
GO
ALTER PROCEDURE [dbo].[sp_SaveVoucher]
    @VoucherType NVARCHAR(50),
    @VoucherDate DATE,
    @ReferenceNo NVARCHAR(100),
    @Entries TVP_VoucherEntry READONLY  -- Table-valued parameter
AS
BEGIN
    SET NOCOUNT ON;

    INSERT INTO Vouchers (VoucherType, VoucherDate, ReferenceNo)
    VALUES (@VoucherType, @VoucherDate, @ReferenceNo);

    DECLARE @VoucherId INT = SCOPE_IDENTITY();

    INSERT INTO VoucherEntries (VoucherId, AccountId, DebitAmount, CreditAmount)
    SELECT @VoucherId, AccountId, DebitAmount, CreditAmount
    FROM @Entries;
END;	



#### ? Files:
- sp_ManageChartOfAccounts.sql: For insert/update/delete chart of accounts
- sp_SaveVoucher.sql: For saving voucher master and entries using TVP
- TVP_VoucherEntry.sql: Table-valued parameter type for entries


## ?? How to Run the Project

1. **Restore NuGet packages**:
dotnet restore

2. **Configure DB connection string** in `appsettings.json`:
```json
"ConnectionStrings": {
  "DefaultConnection": "Server=.;Database=MiniAccountDB;Trusted_Connection=True;TrustServerCertificate=True"
}

Apply Identity migration (only if needed):
dotnet ef database update

Run the app:
dotnet run

Open browser and go to:
https://localhost:7139/

?? Folder Structure
/Pages
  ??? Accounts         ? Chart of Accounts (Create/Manage)
  ??? Vouchers         ? Voucher entry, listing, export
  ??? Shared           ? Layout and _LoginPartial
