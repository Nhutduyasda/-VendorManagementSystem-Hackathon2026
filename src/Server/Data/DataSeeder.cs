using Microsoft.AspNetCore.Identity;
using Microsoft.EntityFrameworkCore;
using VendorManagementSystem.Shared.Models;

namespace VendorManagementSystem.Server.Data;

public static class DataSeeder
{
    public static async Task SeedAsync(IServiceProvider services)
    {
        var roleManager = services.GetRequiredService<RoleManager<IdentityRole>>();
        var userManager = services.GetRequiredService<UserManager<ApplicationUser>>();
        var db = services.GetRequiredService<ApplicationDbContext>();

        // ── Roles ────────────────────────────────────────────────
        string[] roles = ["Admin", "Manager", "Staff", "Vendor"];
        foreach (var role in roles)
        {
            if (!await roleManager.RoleExistsAsync(role))
                await roleManager.CreateAsync(new IdentityRole(role));
        }

        // ── Admin user ───────────────────────────────────────────
        const string adminEmail = "admin@vms.local";
        if (await userManager.FindByEmailAsync(adminEmail) is null)
        {
            var admin = new ApplicationUser
            {
                UserName = adminEmail,
                Email = adminEmail,
                FirstName = "System",
                LastName = "Admin",
                Department = "IT",
                EmailConfirmed = true,
                IsActive = true
            };
            var result = await userManager.CreateAsync(admin, "Admin@123!");
            if (result.Succeeded)
                await userManager.AddToRoleAsync(admin, "Admin");
        }

        // ── Categories ───────────────────────────────────────────
        if (!await db.Categories.AnyAsync())
        {
            db.Categories.AddRange(
                new Category { Name = "Raw Materials", Description = "Nguyên vật liệu thô" },
                new Category { Name = "Office Supplies", Description = "Văn phòng phẩm" },
                new Category { Name = "IT Equipment", Description = "Thiết bị công nghệ thông tin" },
                new Category { Name = "Packaging", Description = "Vật liệu đóng gói" },
                new Category { Name = "Spare Parts", Description = "Phụ tùng thay thế" },
                new Category { Name = "Chemicals", Description = "Hóa chất" }
            );
            await db.SaveChangesAsync();
        }

        // ── Rating Criteria ──────────────────────────────────────
        if (!await db.RatingCriteria.AnyAsync())
        {
            db.RatingCriteria.AddRange(
                new RatingCriteria { Name = "Quality", Weight = 0.35 },
                new RatingCriteria { Name = "Price", Weight = 0.30 },
                new RatingCriteria { Name = "Delivery", Weight = 0.25 },
                new RatingCriteria { Name = "Service", Weight = 0.10 }
            );
            await db.SaveChangesAsync();
        }

        // ── Approval Rules ──────────────────────────────────────
        if (!await db.ApprovalRules.AnyAsync())
        {
            db.ApprovalRules.AddRange(
                new ApprovalRule { RoleName = "Staff", MaxAmount = 10_000_000m },
                new ApprovalRule { RoleName = "Manager", MaxAmount = 100_000_000m },
                new ApprovalRule { RoleName = "Admin", MaxAmount = decimal.MaxValue }
            );
            await db.SaveChangesAsync();
        }
    }
}
