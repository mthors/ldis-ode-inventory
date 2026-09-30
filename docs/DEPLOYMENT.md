# LDIS — Deployment & Operations Guide

## 1. System Overview

The **Lightweight Desktop Inventory System (LDIS)** is a standalone, offline-first Windows Forms desktop application developed in C# on .NET Framework 4.8 and SQLite. It provides product management, stock operations, audit trails, dashboard metrics, CSV exports, and online database backups for factory and warehouse workstations.

---

## 2. System Requirements & Prerequisites

### Supported Operating Systems
* **Windows 7 SP1** (32-bit `x86` and 64-bit `x64`)
* **Windows 8 / 8.1** (32-bit and 64-bit)
* **Windows 10** (all editions)
* **Windows 11** (all editions)

### Runtime Prerequisites
* **Microsoft .NET Framework 4.8 Runtime**
  * Built-in on Windows 11 and recent builds of Windows 10.
  * For Windows 7 SP1, install via the Microsoft .NET Framework 4.8 Offline Installer (`ndp48-x86-x64-allos-enu.exe`) or Windows Update (KB4486166).
  * Note: Windows 7 requires Service Pack 1 (Build 7601) to install .NET Framework 4.8.
* **Native C++ Runtimes:**
  * **None required.** The SQLite native engine (`SQLite.Interop.dll`) is statically compiled and links directly to standard Windows operating system libraries (`KERNEL32.dll`, `USER32.dll`, `ADVAPI32.dll`). No Visual C++ Redistributable packages are required.

---

## 3. Package Structure & Files

The release package is distributed as a clean standalone directory (`LDIS-v1.2.0/`) or compressed archive (`LDIS-v1.2.0.zip`):

```text
LDIS-v1.2.0/
├── LDIS.App.exe                        - Main Windows Forms application executable (with embedded icon)
├── LDIS.App.exe.config                 - .NET Framework 4.8 CLR startup configuration
├── LDIS.Core.dll                       - Domain logic, data access, export, and backup services
├── ClosedXML.dll                       - OpenXML spreadsheet generation engine
├── DocumentFormat.OpenXml.dll          - OpenXML standard document format library
├── ExcelNumberFormat.dll               - Excel number formatting utility
├── System.IO.FileSystem.Primitives.dll - File system primitives dependency
├── System.IO.Packaging.dll             - Open packaging conventions library
├── System.Data.SQLite.dll              - Managed ADO.NET SQLite data provider
├── README.txt                          - Plaintext quick-start guide for end users
├── x86/
│   └── SQLite.Interop.dll              - 32-bit native SQLite engine (auto-loaded on 32-bit systems)
└── x64/
    └── SQLite.Interop.dll              - 64-bit native SQLite engine (auto-loaded on 64-bit systems)
```

> **Note on Application Icon:** The application icon is embedded directly into the PE header of `LDIS.App.exe` as a Win32 `RT_GROUP_ICON` resource (`app.ico` is a source/build asset only and is intentionally not distributed alongside the binary).

---

## 4. Installation & Deployment Procedure

LDIS is an **xcopy-deployable** application that requires no installer, no Windows Registry configuration, and no COM registrations.

### Standard Desktop Deployment
1. Extract `LDIS-v1.2.0.zip` to the target computer (e.g. `C:\LDIS` or `C:\Program Files\LDIS`).
2. Right-click `LDIS.App.exe` and select **Send to → Desktop (create shortcut)**.
3. Launch the application from the shortcut.

### Data Storage Modes
LDIS automatically configures its storage location upon launch:

1. **Standard Mode (Default):**
   * The database is stored in the current Windows user's local application data folder:
     `%LocalAppData%\LDIS\inventory.db`
     (`C:\Users\<Username>\AppData\Local\LDIS\inventory.db`)
   * Standard user accounts have full write access to this location.
   * Multiple Windows user accounts on the same computer maintain isolated databases.

2. **Portable Mode (Optional):**
   * If a folder named `database` exists directly alongside `LDIS.App.exe`, LDIS automatically operates in portable mode, storing data in `.\database\inventory.db`.
   * This mode is ideal for running directly from a USB flash drive.

---

## 5. Application Updates & Data Safety

* **Database Preservation:**
  Application updates do **not** affect existing user databases.
  To update the application, replace `LDIS.App.exe`, `LDIS.Core.dll`, and `System.Data.SQLite.dll` in the application folder. The user's database at `%LocalAppData%\LDIS\inventory.db` remains untouched.
* **Schema Initialization:**
  [`DatabaseInitializer.cs`](file:///c:/Users/Hype%20AMD/IT/Project/ode%20inventory/src/LDIS.Core/Data/DatabaseInitializer.cs) queries `PRAGMA user_version;`. On existing databases (`user_version >= 1`), schema creation is skipped entirely, preserving all existing items, stock balances, and transaction history.

---

## 6. Backup & Recovery Operations

### In-App Backup
1. In `MainForm`, click the **Backup DB...** button in the top-right header.
2. Select a target directory (such as a secondary drive, network share, or USB flash drive) and click **Save**.
3. LDIS performs a consistent online SQLite backup snapshot using the SQLite online backup API (`sqlite3_backup_*`), safe against active reads and writes.

### Manual Backup / File Copy
* Alternatively, when the application is closed, users can copy `%LocalAppData%\LDIS\inventory.db` to an archive location.

---

## 7. Product Onboarding Workflow (Excel Import)

LDIS supports onboarding product masters in bulk via `.xlsx` spreadsheets.

### Onboarding Steps
1. **Download Template:**
   * Open the **Import...** dialog from the main window toolbar and click **Download Template (.xlsx)**.
   * Save `LDIS_Product_Import_Template.xlsx`.
2. **Populate the Workbook:**
   * Enter new products into the `Products` worksheet using the exactly 10 supported columns:
     `SKU`, `Name`, `Category`, `Brand`, `Color`, `Size`, `Gender`, `Purchase Price`, `Selling Price`, `Min Stock Level` (or `Min Stock`).
   * Review the companion `Instructions` worksheet for constraints, data types, and controlled values (e.g., `Unisex`, `Men`, `Women`, `Kids`, `None / Unspecified`).
   * Existing categories must already be created in LDIS before import (categories are not auto-created).
3. **Select & Inspect:**
   * In the **Import...** dialog, click **Browse...** to select the populated workbook.
   * Inspection runs immediately in-memory without making any database changes.
   * If any errors exist, the **Validation Errors** tab lists each issue by row, column, entered value, and description. The **Import Products** button remains disabled until all errors are resolved.
4. **Preview & Confirm:**
   * Switch to the **Preview Valid Products** tab to inspect all valid products to be imported.
   * Click **Import Products** to trigger the final confirmation prompt.
5. **Atomic Commit:**
   * Upon user confirmation, all valid products are inserted in a single atomic database transaction.
   * Every imported product starts with `CurrentStock = 0` and `IsActive = 1`.
   * If any database error occurs during insertion, the entire batch is rolled back.

> **Operational Note:** ClosedXML loads workbooks into memory; unusually large workbooks may increase memory usage.

---

## 8. Windows 7 SP1 Release Verification Checklist

The codebase preserves the Windows 7 SP1 and .NET Framework 4.8 target configuration. Because development occurs on modern Windows environments, actual Windows 7 runtime compatibility remains to be empirically tested on target hardware. Final production sign-off on Windows 7 factory computers requires completing this checklist on an actual physical or virtual Windows 7 SP1 machine:

- [ ] **Windows 7 Service Pack 1:** Confirm OS is Windows 7 SP1 (Build 7601) via `winver`.
- [ ] **.NET 4.8 Installation:** Verify .NET 4.8 is present (Release key `>= 528040` in `HKLM\SOFTWARE\Microsoft\NET Framework Setup\NDP\v4\Full`).
- [ ] **Clean Extraction:** Extract `LDIS-v1.2.0.zip` to a standard user directory (e.g. `C:\LDIS`).
- [ ] **First Launch:** Launch `LDIS.App.exe` as a standard (non-admin) user. Confirm startup without missing DLL errors.
- [ ] **Database Creation:** Verify `%LocalAppData%\LDIS\inventory.db` is created and status bar shows connected database.
- [ ] **Core Workflow:**
  - [ ] Create Category
  - [ ] Create Product
  - [ ] Stock IN (+10)
  - [ ] Stock OUT (-3)
  - [ ] Stock ADJUSTMENT (-1)
  - [ ] Verify Dashboard KPIs
  - [ ] View Transaction History
- [ ] **CSV Export:** Export Products and Transactions to `.csv`; confirm files open cleanly in Microsoft Excel or Notepad.
- [ ] **Excel Import:** Download template, populate products, validate, preview, and import; confirm products appear with 0 stock.
- [ ] **Database Backup:** Run "Backup DB..." and confirm `.db` snapshot is written.
- [ ] **Relaunch:** Close and reopen `LDIS.App.exe`; confirm data persists.
