-- ============================================================================
-- Lightweight Desktop Inventory System (LDIS)
-- Baseline SQLite Database Schema
-- Matches PRD Section 8 Specification
-- ============================================================================

PRAGMA foreign_keys = ON;

-- 1. Categories Table
CREATE TABLE IF NOT EXISTS Categories (
    CategoryID INTEGER PRIMARY KEY AUTOINCREMENT,
    CategoryName TEXT NOT NULL UNIQUE
);

-- 2. Items Table
CREATE TABLE IF NOT EXISTS Items (
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

-- 3. Inventory Transactions Table
CREATE TABLE IF NOT EXISTS InventoryTransactions (
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

-- 4. Indexes
CREATE INDEX IF NOT EXISTS IX_Transactions_ItemID
ON InventoryTransactions(ItemID);

CREATE INDEX IF NOT EXISTS IX_Transactions_Date
ON InventoryTransactions(TransactionDate);

CREATE INDEX IF NOT EXISTS IX_Transactions_Type
ON InventoryTransactions(TransactionType);

CREATE INDEX IF NOT EXISTS IX_Items_Category
ON Items(CategoryID);

CREATE INDEX IF NOT EXISTS IX_Items_Name
ON Items(Name);
