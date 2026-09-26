# LDIS — Lightweight Desktop Inventory System

LDIS (Lightweight Desktop Inventory System) is an offline-first Windows desktop application designed for inventory and warehouse management on factory workstations running Windows 7 SP1 through Windows 11.

---

## Key Features

* **Product Management:** Product catalog with SKU uniqueness, category organization, active/inactive lifecycle, and attribute tracking (Brand, Color, Size, Gender).
* **Atomic Stock Operations:**
  * **Stock IN:** Record inbound inventory receipts.
  * **Stock OUT:** Record outbound stock dispatches with strict non-negative stock protection.
  * **Stock ADJUSTMENT:** Record positive or negative corrections with mandatory audit reasons.
* **Immutable Audit Trail:** Append-only inventory transaction history tracking date, type, delta, unit price, reference numbers, and operator details.
* **Live Dashboard & Filtering:** Real-time KPI summary cards (Total Active Products, Total Units, Low Stock, Out of Stock) with interactive multi-criteria filtering.
* **RFC 4180 CSV Export:** UTF-8 with BOM exports for product lists and transaction history, formatted for immediate compatibility with Microsoft Excel.
* **Online SQLite Backup:** Consistent online database snapshots using SQLite's native backup API, safe against active reads and writes.

---

## Technology Stack

* **Operating System:** Windows 7 SP1 32/64-bit and newer (Windows 8, 8.1, 10, 11)
* **Framework:** .NET Framework 4.8
* **UI:** Windows Forms (WinForms) with Common-Controls 6.0 visual styles
* **Database:** SQLite via official `System.Data.SQLite` ADO.NET provider (v1.0.118.0)
* **Architecture:** `WinForms UI → Service Layer → Repository Layer → SQLite (ADO.NET)`
* **Design Philosophy:** Offline-first, zero cloud dependencies, zero external database servers, no ORM, no DI container.

---

## System Requirements

1. **Operating System:** Windows 7 SP1 (32-bit or 64-bit) or higher.
2. **Runtime:** Microsoft .NET Framework 4.8.
3. **External Dependencies:** None. SQLite native interop libraries (`x86` and `x64`) are statically compiled and bundled with the application.

---

## Build from Source

To build the complete solution using MSBuild:

```powershell
# Build Debug configuration
& "C:\Windows\Microsoft.NET\Framework64\v4.0.30319\MSBuild.exe" "LDIS.sln" /p:Configuration=Debug /t:Rebuild /verbosity:minimal

# Build Release configuration
& "C:\Windows\Microsoft.NET\Framework64\v4.0.30319\MSBuild.exe" "LDIS.sln" /p:Configuration=Release /t:Rebuild /verbosity:minimal
```

---

## Automated Test Suite

The solution contains a self-contained automated test suite covering all milestones:

```powershell
& "tests\LDIS.Core.Tests\bin\Release\LDIS.Core.Tests.exe"
```

---

## Packaging & Deployment

To create a clean, deterministic standalone distribution package:

```powershell
& powershell -NoProfile -ExecutionPolicy Bypass -File "scripts\package-release.ps1"
```

This generates:
* `dist\LDIS-v1.0.0\` (extracted application folder)
* `dist\LDIS-v1.0.0.zip` (compressed standalone release archive)

For full deployment and operations instructions, see [DEPLOYMENT.md](docs/DEPLOYMENT.md).

---

## Data Storage

* **Standard Mode (Default):** `%LocalAppData%\LDIS\inventory.db`
* **Portable Mode:** `.\database\inventory.db` (activated when a `database\` folder exists alongside `LDIS.App.exe`).
