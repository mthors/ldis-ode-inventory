# Product Requirements Document (PRD)

## 1. Product Overview

**Product Name:** Lightweight Desktop Inventory System (LDIS)

**Target Runtime OS:** Windows 7 SP1 (32-bit/64-bit) and newer Windows versions

**Primary Users:** Small warehouse / factory inventory operators

**Application Type:** Offline-first native desktop application

**Primary Goal:**
Provide a simple, reliable, lightweight inventory application for recording stock-in, stock-out, stock adjustments, current stock, and transaction history on low-spec Windows machines.

---

# 2. Product Principles

LDIS follows these principles:

1. **Offline-first**

   * Application must work without an internet connection.
   * No web browser is required.
   * No external database server is required.

2. **Lightweight**

   * Designed for old Windows 7 hardware.
   * Avoid Electron, Chromium-based UI, web servers, and unnecessary background services.

3. **Simple**

   * Core inventory operations must be fast and easy to perform.
   * Avoid enterprise-level features unless they are required.

4. **Data safety**

   * Stock changes must be atomic and consistent.
   * Transaction history must be preserved.
   * Database backup and restore must be supported.

5. **Maintainable**

   * UI, business logic, and database access should be separated.
   * Database schema should remain simple enough for future maintenance.

---

# 3. Technology Stack

| Component               | Technology                                               |
| ----------------------- | -------------------------------------------------------- |
| Language                | C#                                                       |
| UI                      | Windows Forms                                            |
| Runtime                 | .NET Framework 4.8                                       |
| Database                | SQLite                                                   |
| SQLite Provider         | System.Data.SQLite                                       |
| Excel Export            | ClosedXML or compatible .NET Framework-supported library |
| CSV Export              | Native C# implementation or CsvHelper                    |
| PDF Export              | Deferred to V2                                           |
| Source Control          | Git                                                      |
| Development Environment | Visual Studio / compatible C# IDE + Google Antigravity   |

### Runtime Requirement

LDIS targets **.NET Framework 4.8** with **Windows 7 SP1** as the minimum supported operating system.

The development environment may run on a modern Windows machine. The application itself must be tested on Windows 7 SP1 before release.

---

# 4. V1 Scope

## 4.1 Product Management

Each sellable inventory variant is represented as an individual SKU.

### Product fields

* Item ID
* SKU
* Item Name
* Category
* Brand
* Color
* Size
* Gender
* Purchase Price
* Selling Price
* Minimum Stock Level
* Current Stock
* Active/Inactive status

### Example

```text
Model: Nike Air Max 270

SKU             Size    Color
--------------------------------
AM270-BLK-42    42      Black
AM270-BLK-43    43      Black
AM270-WHT-42    42      White
```

Each SKU maintains its own stock quantity.

### V1 does NOT include

* Complex parent/child product variant generation
* Dynamic custom attributes
* Generic metadata engine
* Product inheritance system

These may be considered in a future version if actual requirements arise.

---

# 5. Stock Operations

## 5.1 Stock In

Operator can record incoming inventory.

Required information:

* SKU
* Quantity
* Unit Cost
* Date
* Reference Number
* Supplier
* Notes

V1 should support entering multiple items in one receiving operation if practical.

Example:

```text
PO-001

SKU             Quantity
------------------------
SHOE-BLK-42       20
SHOE-BLK-43       15
SHOE-WHT-42       10
```

The operation must be executed as a database transaction.

---

## 5.2 Stock Out

Operator can record outgoing inventory.

Required information:

* SKU
* Quantity
* Unit Selling Price
* Date
* Reference Number
* Customer/Destination
* Notes

Before committing:

```text
Requested quantity <= Current stock
```

If insufficient stock exists, the application must prevent the operation unless negative stock is explicitly enabled in configuration.

---

## 5.3 Stock Adjustment

Used for:

* Damaged goods
* Lost goods
* Physical stock count differences
* Data correction

Adjustment requires:

* SKU
* Adjustment quantity
* Reason
* Notes
* Date

Adjustment quantity may be positive or negative.

Example:

```text
System stock: 25
Physical stock: 23

Adjustment: -2
Reason: Physical stock count
```

---

# 6. Inventory Transactions

Every stock-changing operation creates an immutable transaction record.

Transaction fields:

```text
TransactionID
ItemID
TransactionType
Quantity
UnitPrice
ReferenceNumber
SupplierOrCustomer
Reason
Notes
TransactionDate
CreatedBy
```

Transaction types:

```text
IN
OUT
ADJUSTMENT
```

The transaction history must not be silently modified when current stock changes.

Corrections should be performed through a new adjustment transaction.

---

# 7. Current Stock

The `Items` table maintains the current stock quantity.

Example:

```text
Items
--------------------------------
SKU             CurrentStock
--------------------------------
AM270-BLK-42        25
AM270-BLK-43        12
AM270-WHT-42         0
```

Transaction history remains the source of audit history.

When stock changes, the application must atomically:

1. Validate the operation.
2. Insert the transaction.
3. Update current stock.
4. Commit.

If any step fails:

```text
ROLLBACK
```

No partial stock operation may remain.

---

# 8. Database Schema

## Categories

```sql
CREATE TABLE Categories (
    CategoryID INTEGER PRIMARY KEY AUTOINCREMENT,
    CategoryName TEXT NOT NULL UNIQUE
);
```

## Items

```sql
CREATE TABLE Items (
    ItemID INTEGER PRIMARY KEY AUTOINCREMENT,
    SKU TEXT NOT NULL UNIQUE,
    Name TEXT NOT NULL,
    CategoryID INTEGER,
    Brand TEXT,
    Color TEXT,
    Size TEXT,
    Gender TEXT,
    PurchasePrice INTEGER NOT NULL DEFAULT 0,
    SellingPrice INTEGER NOT NULL DEFAULT 0,
    MinStockLevel INTEGER NOT NULL DEFAULT 0,
    CurrentStock INTEGER NOT NULL DEFAULT 0,
    IsActive INTEGER NOT NULL DEFAULT 1,
    FOREIGN KEY(CategoryID) REFERENCES Categories(CategoryID)
);
```

Prices are stored as integers representing the smallest currency unit / whole Rupiah value.

Example:

```text
75000 = Rp75.000
```

This avoids floating-point currency precision problems.

## Inventory Transactions

```sql
CREATE TABLE InventoryTransactions (
    TransactionID INTEGER PRIMARY KEY AUTOINCREMENT,
    ItemID INTEGER NOT NULL,
    TransactionType TEXT NOT NULL
        CHECK(TransactionType IN ('IN', 'OUT', 'ADJUSTMENT')),
    Quantity INTEGER NOT NULL,
    UnitPrice INTEGER NOT NULL DEFAULT 0,
    ReferenceNumber TEXT,
    SupplierOrCustomer TEXT,
    Reason TEXT,
    Notes TEXT,
    TransactionDate DATETIME NOT NULL DEFAULT CURRENT_TIMESTAMP,
    CreatedBy TEXT,
    FOREIGN KEY(ItemID) REFERENCES Items(ItemID)
);
```

## Indexes

```sql
CREATE INDEX IX_Transactions_ItemID
ON InventoryTransactions(ItemID);

CREATE INDEX IX_Transactions_Date
ON InventoryTransactions(TransactionDate);

CREATE INDEX IX_Transactions_Type
ON InventoryTransactions(TransactionType);

CREATE INDEX IX_Items_Category
ON Items(CategoryID);

CREATE INDEX IX_Items_Name
ON Items(Name);
```

SQLite foreign-key enforcement must be enabled for every database connection:

```sql
PRAGMA foreign_keys = ON;
```

---

# 9. Search & Filtering

V1 supports:

### Product search

* SKU
* Item name
* Brand
* Color
* Size
* Category

### Stock status

* All
* In Stock
* Low Stock
* Out of Stock

### Transaction filters

* All
* Stock In
* Stock Out
* Adjustment

### Date filters

* Today
* Yesterday
* Last 7 Days
* This Month
* Last Month
* This Year
* Custom Range
* All Time

Filtering should be performed through parameterized SQL queries.

---

# 10. Dashboard

Main screen should display:

```text
Total SKUs
Total Units in Stock
Low Stock Items
Out of Stock Items
Today's Stock In
Today's Stock Out
```

Optional:

```text
Total Inventory Purchase Value
```

The dashboard should remain lightweight and must not load unnecessary transaction data into memory.

---

# 11. Transaction History

The transaction history screen displays:

```text
Date
SKU
Item
Type
Quantity
Unit Price
Reference
Supplier/Customer
Reason
Notes
Created By
```

Features:

* Search
* Date filtering
* Transaction type filtering
* Sorting
* Export

---

# 12. Export

## V1

### CSV

Must export the currently filtered data.

### Excel

Must export the currently filtered data to `.xlsx`.

Excel export should include:

* Column headers
* Formatted numbers
* Currency formatting
* Basic column sizing

## V2

PDF reporting may be added later.

PDF generation is NOT part of the initial implementation.

---

# 13. Backup & Restore

Application must provide:

### Backup

Create a timestamped database backup:

```text
LDIS_Backup_2026-09-25_1030.db
```

### Restore

User can select a backup database and restore it.

Before restore:

1. Close active database operations.
2. Create an automatic safety backup of the current database.
3. Validate the selected backup.
4. Replace/restore the database.
5. Reopen the database connection.

The application must never overwrite the current database without first creating a safety backup.

---

# 14. UI Design

Target minimum screen resolution:

```text
1024 × 768
```

Use standard WinForms controls.

Main layout:

```text
┌───────────────────────────────────────────────┐
│ LDIS   Search: [_____________]   [+ IN] [OUT]│
├──────────────┬────────────────────────────────┤
│ Filters      │                                │
│              │        Inventory Grid          │
│ Category     │                                │
│ Brand        │                                │
│ Color        │                                │
│ Size         │                                │
│ Stock Status │                                │
│              │                                │
├──────────────┴────────────────────────────────┤
│ Total: 1,250 | Low: 12 | Out: 4 | Export ...│
└───────────────────────────────────────────────┘
```

Keyboard support:

```text
Ctrl + N    New Item
Ctrl + F    Search
Ctrl + I    Stock In
Ctrl + O    Stock Out
Ctrl + E    Export
Ctrl + S    Save
Esc         Close dialog
```

---

# 15. Application Architecture

The application should use a simple layered architecture:

```text
┌─────────────────────────┐
│       WinForms UI       │
├─────────────────────────┤
│      Service Layer      │
├─────────────────────────┤
│     Repository Layer    │
├─────────────────────────┤
│     SQLite Database     │
└─────────────────────────┘
```

### UI Layer

Responsible for:

* Forms
* Controls
* User input
* Displaying results

### Service Layer

Responsible for:

* Stock-in logic
* Stock-out validation
* Adjustment logic
* Backup/restore
* Business rules

### Repository Layer

Responsible for:

* SQL queries
* CRUD operations
* Database connections
* Transactions

The UI should NOT contain raw SQL queries.

---

# 16. Data Integrity

All stock-changing operations must use SQLite database transactions.

Example:

```text
BEGIN TRANSACTION

Validate stock
       ↓
Insert transaction
       ↓
Update Items.CurrentStock
       ↓
COMMIT
```

If an error occurs:

```text
ROLLBACK
```

No partial inventory operation is allowed.

---

# 17. Performance Requirements

Target dataset:

```text
Products:       1,000+
Transactions:   100,000+
```

The application should remain responsive on low-spec dual-core hardware.

Normal product searches and stock lookups should feel immediate.

Performance optimization should be based on actual testing rather than premature optimization.

---

# 18. Deployment

Application should support portable deployment where practical.

Example:

```text
LDIS\
│
├── LDIS.exe
├── System.Data.SQLite.dll
├── SQLite.Interop.dll
├── config\
├── database\
│   └── inventory.db
└── backups\
```

The database should preferably be stored in an application data directory rather than relying on write permissions inside `Program Files`.

---

# 19. V1 Explicitly Excluded

The following are NOT required for the first release:

* User authentication
* Role/permission management
* Multi-PC synchronization
* Cloud database
* Web interface
* Mobile application
* Dynamic custom attributes
* Complex product variant engine
* Supplier management module
* Customer management module
* Accounting integration
* Barcode scanner integration
* PDF reporting
* Advanced analytics
* Network database
* Multi-warehouse support

These can be evaluated after the core application is proven useful.

---

# 20. Development Milestones

## Milestone 1 — Project Foundation

* Create Git repository
* Create .NET Framework 4.8 WinForms solution
* Configure SQLite
* Create database initialization
* Create schema
* Verify application launches on Windows 7 test environment

## Milestone 2 — Product Management

* Product list
* Add product
* Edit product
* Deactivate product
* Category management
* Search products

## Milestone 3 — Stock Operations

* Stock In
* Stock Out
* Adjustment
* Current stock
* Transaction history
* Atomic database transactions

## Milestone 4 — Dashboard & Filtering

* Dashboard
* Stock status
* Date filtering
* Transaction filtering
* Search

## Milestone 5 — Export & Backup

* CSV
* Excel
* Database backup
* Database restore

## Milestone 6 — Testing & Deployment

* Test on modern Windows
* Test on Windows 7 SP1
* Test backup/restore
* Test invalid stock operations
* Test database corruption/recovery scenarios
* Package release build
* Create installation/deployment instructions

---

# 21. Definition of Done for V1

V1 is considered complete when an operator can:

1. Create a product.
2. Search for a product.
3. Receive stock.
4. Issue stock.
5. Adjust stock.
6. See current stock immediately.
7. View complete transaction history.
8. Filter transaction history.
9. Export data to CSV.
10. Export data to Excel.
11. Backup the database.
12. Restore a database backup.
13. Run the application offline.
14. Run the application on the target Windows 7 machine.
15. Recover safely from an interrupted stock operation without corrupting inventory data.
