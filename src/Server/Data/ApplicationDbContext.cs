using Microsoft.AspNetCore.Identity.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore;
using VendorManagementSystem.Shared.Models;

namespace VendorManagementSystem.Server.Data;

public class ApplicationDbContext : IdentityDbContext<ApplicationUser>
{
    public ApplicationDbContext(DbContextOptions<ApplicationDbContext> options)
        : base(options) { }

    public DbSet<Supplier> Suppliers => Set<Supplier>();
    public DbSet<Category> Categories => Set<Category>();
    public DbSet<Product> Products => Set<Product>();
    public DbSet<SupplierProduct> SupplierProducts => Set<SupplierProduct>();
    public DbSet<PriceList> PriceLists => Set<PriceList>();
    public DbSet<PurchaseOrder> PurchaseOrders => Set<PurchaseOrder>();
    public DbSet<PurchaseOrderItem> PurchaseOrderItems => Set<PurchaseOrderItem>();
    public DbSet<ApprovalRule> ApprovalRules => Set<ApprovalRule>();
    public DbSet<ApprovalLog> ApprovalLogs => Set<ApprovalLog>();
    public DbSet<GoodsReceipt> GoodsReceipts => Set<GoodsReceipt>();
    public DbSet<GoodsReceiptItem> GoodsReceiptItems => Set<GoodsReceiptItem>();
    public DbSet<VendorRating> VendorRatings => Set<VendorRating>();
    public DbSet<RatingCriteria> RatingCriteria => Set<RatingCriteria>();
    public DbSet<Contract> Contracts => Set<Contract>();
    public DbSet<AccountsPayable> AccountsPayables => Set<AccountsPayable>();
    public DbSet<Document> Documents => Set<Document>();
    public DbSet<Notification> Notifications => Set<Notification>();
    public DbSet<AuditLog> AuditLogs => Set<AuditLog>();

    protected override void OnModelCreating(ModelBuilder builder)
    {
        base.OnModelCreating(builder);

        // ── Supplier ──────────────────────────────────────────────────
        builder.Entity<Supplier>(e =>
        {
            e.HasIndex(s => s.Email).IsUnique();
            e.HasIndex(s => s.TaxCode).IsUnique().HasFilter("[TaxCode] IS NOT NULL");
            e.HasIndex(s => s.Name);
            e.Property(s => s.Status).HasConversion<string>();
            e.Property(s => s.Rank).HasConversion<string>();
        });

        // ── Category ─────────────────────────────────────────────────
        builder.Entity<Category>(e =>
        {
            e.HasIndex(c => c.Name).IsUnique();
        });

        // ── Product ──────────────────────────────────────────────────
        builder.Entity<Product>(e =>
        {
            e.HasIndex(p => p.SKU).IsUnique();
            e.HasOne(p => p.Category)
                .WithMany(c => c.Products)
                .HasForeignKey(p => p.CategoryId)
                .OnDelete(DeleteBehavior.Restrict);
        });

        // ── SupplierProduct (composite key) ──────────────────────────
        builder.Entity<SupplierProduct>(e =>
        {
            e.HasKey(sp => new { sp.SupplierId, sp.ProductId });

            e.HasOne(sp => sp.Supplier)
                .WithMany(s => s.SupplierProducts)
                .HasForeignKey(sp => sp.SupplierId)
                .OnDelete(DeleteBehavior.Cascade);

            e.HasOne(sp => sp.Product)
                .WithMany(sp => sp.SupplierProducts)
                .HasForeignKey(sp => sp.ProductId)
                .OnDelete(DeleteBehavior.Cascade);

            e.Property(sp => sp.Price).HasPrecision(18, 2);
        });

        // ── PriceList ────────────────────────────────────────────────
        builder.Entity<PriceList>(e =>
        {
            e.HasOne(pl => pl.Supplier)
                .WithMany(s => s.PriceLists)
                .HasForeignKey(pl => pl.SupplierId)
                .OnDelete(DeleteBehavior.Cascade);

            e.HasOne(pl => pl.Product)
                .WithMany()
                .HasForeignKey(pl => pl.ProductId)
                .OnDelete(DeleteBehavior.Cascade);

            e.Property(pl => pl.Price).HasPrecision(18, 2);
            e.HasIndex(pl => new { pl.SupplierId, pl.ProductId, pl.EffectiveDate });
        });

        // ── PurchaseOrder ────────────────────────────────────────────
        builder.Entity<PurchaseOrder>(e =>
        {
            e.HasIndex(po => po.PONumber).IsUnique();
            e.Property(po => po.Status).HasConversion<string>();
            e.Property(po => po.TotalAmount).HasPrecision(18, 2);

            e.HasOne(po => po.Supplier)
                .WithMany(s => s.PurchaseOrders)
                .HasForeignKey(po => po.SupplierId)
                .OnDelete(DeleteBehavior.Restrict);
        });

        // ── PurchaseOrderItem ────────────────────────────────────────
        builder.Entity<PurchaseOrderItem>(e =>
        {
            e.HasOne(i => i.PurchaseOrder)
                .WithMany(po => po.Items)
                .HasForeignKey(i => i.PurchaseOrderId)
                .OnDelete(DeleteBehavior.Cascade);

            e.HasOne(i => i.Product)
                .WithMany(p => p.PurchaseOrderItems)
                .HasForeignKey(i => i.ProductId)
                .OnDelete(DeleteBehavior.Restrict);

            e.Property(i => i.UnitPrice).HasPrecision(18, 2);
            e.Property(i => i.TotalPrice).HasPrecision(18, 2);
        });

        // ── ApprovalRule ─────────────────────────────────────────────
        builder.Entity<ApprovalRule>(e =>
        {
            e.Property(a => a.MaxAmount).HasPrecision(18, 2);
        });

        // ── ApprovalLog ──────────────────────────────────────────────
        builder.Entity<ApprovalLog>(e =>
        {
            e.HasOne(a => a.PurchaseOrder)
                .WithMany(po => po.ApprovalLogs)
                .HasForeignKey(a => a.PurchaseOrderId)
                .OnDelete(DeleteBehavior.Cascade);

            e.Property(a => a.Action).HasConversion<string>();
        });

        // ── GoodsReceipt ─────────────────────────────────────────────
        builder.Entity<GoodsReceipt>(e =>
        {
            e.HasOne(g => g.PurchaseOrder)
                .WithMany(po => po.GoodsReceipts)
                .HasForeignKey(g => g.PurchaseOrderId)
                .OnDelete(DeleteBehavior.Cascade);
        });

        // ── GoodsReceiptItem ─────────────────────────────────────────
        builder.Entity<GoodsReceiptItem>(e =>
        {
            e.HasOne(gi => gi.GoodsReceipt)
                .WithMany(g => g.Items)
                .HasForeignKey(gi => gi.GoodsReceiptId)
                .OnDelete(DeleteBehavior.Cascade);

            e.HasOne(gi => gi.Product)
                .WithMany()
                .HasForeignKey(gi => gi.ProductId)
                .OnDelete(DeleteBehavior.Restrict);
        });

        // ── VendorRating ─────────────────────────────────────────────
        builder.Entity<VendorRating>(e =>
        {
            e.HasOne(vr => vr.Supplier)
                .WithMany(s => s.VendorRatings)
                .HasForeignKey(vr => vr.SupplierId)
                .OnDelete(DeleteBehavior.Cascade);

            e.HasOne(vr => vr.PurchaseOrder)
                .WithOne(po => po.VendorRating)
                .HasForeignKey<VendorRating>(vr => vr.PurchaseOrderId)
                .OnDelete(DeleteBehavior.Restrict);
        });

        // ── RatingCriteria ───────────────────────────────────────────
        builder.Entity<RatingCriteria>(e =>
        {
            e.HasIndex(r => r.Name).IsUnique();
        });

        // ── Contract ─────────────────────────────────────────────────
        builder.Entity<Contract>(e =>
        {
            e.HasIndex(c => c.ContractNumber).IsUnique();
            e.Property(c => c.Value).HasPrecision(18, 2);
            e.Property(c => c.Status).HasConversion<string>();
            e.HasIndex(c => c.ExpiryDate);

            e.HasOne(c => c.Supplier)
                .WithMany(s => s.Contracts)
                .HasForeignKey(c => c.SupplierId)
                .OnDelete(DeleteBehavior.Cascade);
        });

        // ── AccountsPayable ─────────────────────────────────────────
        builder.Entity<AccountsPayable>(e =>
        {
            e.HasOne(ap => ap.PurchaseOrder)
                .WithOne(po => po.AccountsPayable)
                .HasForeignKey<AccountsPayable>(ap => ap.PurchaseOrderId)
                .OnDelete(DeleteBehavior.Cascade);

            e.Property(ap => ap.Amount).HasPrecision(18, 2);
            e.Property(ap => ap.PaidAmount).HasPrecision(18, 2);
            e.Property(ap => ap.Status).HasConversion<string>();
            e.HasIndex(ap => ap.DueDate);
        });

        // ── Document ─────────────────────────────────────────────────
        builder.Entity<Document>(e =>
        {
            e.HasOne(d => d.Supplier)
                .WithMany(s => s.Documents)
                .HasForeignKey(d => d.SupplierId)
                .OnDelete(DeleteBehavior.Cascade);
        });

        // ── Notification ─────────────────────────────────────────────
        builder.Entity<Notification>(e =>
        {
            e.HasIndex(n => new { n.UserId, n.IsRead });
            e.Property(n => n.Type).HasConversion<string>();
        });

        // ── AuditLog ─────────────────────────────────────────────────
        builder.Entity<AuditLog>(e =>
        {
            e.HasIndex(a => a.Timestamp);
            e.HasIndex(a => new { a.EntityType, a.EntityId });
        });
    }
}
