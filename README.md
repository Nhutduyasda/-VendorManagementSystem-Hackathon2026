# Vendor Management System - Hackathon 2026

## 🎯 Project Progress

- [x] Phase 0: Setup & Database Schema (Commit: `5364db5`)
- [ ] Phase 1: Authentication & User Management
- [ ] Phase 2: Purchase Order
- [ ] Phase 3: Goods Receipt
- [ ] Phase 4: Vendor Rating
- [ ] Phase 5: Advanced Features
- [ ] Phase 6: Vendor Portal
- [ ] Phase 7: AI & Real-time

## 🛠️ Tech Stack

- **.NET 10.0.102** — ASP.NET Core Hosted Blazor WebAssembly
- **EF Core 10.0.2** — SQL Server LocalDB
- **ASP.NET Core Identity** — JWT + Cookie dual authentication
- **SignalR** — Real-time notifications
- **Cloudinary** — Document/file uploads
- **Tailwind CSS 4.1.8** — UI styling

## 📦 Solution Structure

```
VendorManagementSystem/
├── src/
│   ├── Server/          # ASP.NET Core Web API + SignalR Hub
│   ├── Client/          # Blazor WebAssembly SPA
│   └── Shared/          # DTOs, Models, Enums
└── VendorManagementSystem.slnx
```

## 🗄️ Database Schema (18 Entities)

Supplier, Category, Product, SupplierProduct, PriceList, PurchaseOrder, PurchaseOrderItem, ApprovalRule, ApprovalLog, GoodsReceipt, GoodsReceiptItem, VendorRating, RatingCriteria, Contract, AccountsPayable, Document, Notification, AuditLog

## 🏃‍♂️ How to Run

```bash
# Database
cd src/Server
dotnet ef database update

# Run (Server hosts Client automatically)
dotnet run
```

Open browser at `https://localhost:7250`

### Default Admin Account
- **Email:** admin@vms.local
- **Password:** Admin@123!
