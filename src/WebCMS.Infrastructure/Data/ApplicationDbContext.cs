using System.Linq.Expressions;
using Microsoft.EntityFrameworkCore;
using WebCMS.Core.Entities;

namespace WebCMS.Infrastructure.Data;

/// <summary>
/// 應用程式資料庫上下文
/// </summary>
public class ApplicationDbContext : DbContext
{
    public ApplicationDbContext(DbContextOptions<ApplicationDbContext> options)
        : base(options)
    {
    }

    public DbSet<AdminUser> AdminUsers => Set<AdminUser>();
    public DbSet<LoginAttempt> LoginAttempts => Set<LoginAttempt>();
    public DbSet<Language> Languages => Set<Language>();
    public DbSet<Role> Roles => Set<Role>();
    public DbSet<UserRole> UserRoles => Set<UserRole>();
    public DbSet<Function> Functions => Set<Function>();
    public DbSet<RolePermission> RolePermissions => Set<RolePermission>();
    public DbSet<Category> Categories => Set<Category>();
    public DbSet<Article> Articles => Set<Article>();
    public DbSet<Tag> Tags => Set<Tag>();
    public DbSet<ArticleTag> ArticleTags => Set<ArticleTag>();
    public DbSet<SiteSettings> SiteSettings => Set<SiteSettings>();
    public DbSet<HeaderSettings> HeaderSettings => Set<HeaderSettings>();
    public DbSet<FooterSettings> FooterSettings => Set<FooterSettings>();
    public DbSet<LanguageResource> LanguageResources => Set<LanguageResource>();

    protected override void OnModelCreating(ModelBuilder modelBuilder)
    {
        base.OnModelCreating(modelBuilder);

        // AdminUser 設定
        modelBuilder.Entity<AdminUser>(entity =>
        {
            entity.HasIndex(e => e.Account).IsUnique();
            entity.Property(e => e.Account).HasMaxLength(50).IsRequired();
            entity.Property(e => e.PasswordHash).HasMaxLength(256).IsRequired();
            entity.Property(e => e.DisplayName).HasMaxLength(100).IsRequired();
        });

        // Language 設定
        modelBuilder.Entity<Language>(entity =>
        {
            entity.HasIndex(e => e.LanguageCode).IsUnique();
            entity.Property(e => e.LanguageCode).HasMaxLength(10).IsRequired();
            entity.Property(e => e.LanguageName).HasMaxLength(50).IsRequired();
            entity.HasData(
                new Language { Id = 1, LanguageCode = "zh-TW", LanguageName = "繁體中文", IsActive = true, SortOrder = 1, CreatedAt = new DateTime(2026, 2, 11, 0, 0, 0, DateTimeKind.Utc), UpdatedAt = new DateTime(2026, 2, 11, 0, 0, 0, DateTimeKind.Utc) },
                new Language { Id = 2, LanguageCode = "en-US", LanguageName = "English", IsActive = true, SortOrder = 2, CreatedAt = new DateTime(2026, 2, 11, 0, 0, 0, DateTimeKind.Utc), UpdatedAt = new DateTime(2026, 2, 11, 0, 0, 0, DateTimeKind.Utc) },
                new Language { Id = 3, LanguageCode = "ja-JP", LanguageName = "日本語", IsActive = true, SortOrder = 3, CreatedAt = new DateTime(2026, 2, 11, 0, 0, 0, DateTimeKind.Utc), UpdatedAt = new DateTime(2026, 2, 11, 0, 0, 0, DateTimeKind.Utc) }
            );
        });

        // LoginAttempt 設定
        modelBuilder.Entity<LoginAttempt>(entity =>
        {
            entity.Property(e => e.Account).HasMaxLength(50).IsRequired();
            entity.Property(e => e.IpAddress).HasMaxLength(45);
            entity.HasIndex(e => e.Account);
            entity.HasIndex(e => e.AttemptedAt);
        });

        // Role 設定
        modelBuilder.Entity<Role>(entity =>
        {
            // 複合唯一索引：Name + LanguageCode
            entity.HasIndex(e => new { e.Name, e.LanguageCode }).IsUnique();
            entity.Property(e => e.Name).HasMaxLength(50).IsRequired();
            entity.Property(e => e.LanguageCode).HasMaxLength(10).IsRequired().HasDefaultValue("zh-TW");
            entity.Property(e => e.Description).HasMaxLength(200);
        });

        // UserRole 設定（多對多關聯）
        modelBuilder.Entity<UserRole>(entity =>
        {
            entity.HasKey(e => new { e.UserId, e.RoleId });

            entity.HasOne(e => e.User)
                .WithMany(u => u.UserRoles)
                .HasForeignKey(e => e.UserId)
                .OnDelete(DeleteBehavior.Cascade);

            entity.HasOne(e => e.Role)
                .WithMany(r => r.UserRoles)
                .HasForeignKey(e => e.RoleId)
                .OnDelete(DeleteBehavior.Cascade);
        });

        // Function 設定
        modelBuilder.Entity<Function>(entity =>
        {
            // Code 全局唯一，不受語言影響（菜單代碼統一）
            entity.HasIndex(e => e.Code).IsUnique();
            entity.Property(e => e.Name).HasMaxLength(100).IsRequired();
            entity.Property(e => e.LanguageCode).HasMaxLength(10).IsRequired().HasDefaultValue("zh-TW");
            entity.Property(e => e.Code).HasMaxLength(50).IsRequired();
            entity.Property(e => e.Url).HasMaxLength(500);
            entity.Property(e => e.Icon).HasMaxLength(100);

            // 自我參照關聯（父子關係）
            entity.HasOne(e => e.Parent)
                .WithMany(e => e.Children)
                .HasForeignKey(e => e.ParentId)
                .OnDelete(DeleteBehavior.Restrict);
        });

        // RolePermission 設定（多對多關聯）
        modelBuilder.Entity<RolePermission>(entity =>
        {
            // 複合主鍵：RoleId + FunctionId + LanguageCode
            // 允許同一角色的同一功能在不同語言有不同的權限
            entity.HasKey(e => new { e.RoleId, e.FunctionId, e.LanguageCode });

            // 複合唯一索引（已通過主鍵定義）
            entity.HasIndex(e => new { e.RoleId, e.FunctionId, e.LanguageCode }).IsUnique();

            entity.Property(e => e.LanguageCode).HasMaxLength(10).IsRequired().HasDefaultValue("zh-TW");

            entity.HasOne(e => e.Role)
                .WithMany(r => r.RolePermissions)
                .HasForeignKey(e => e.RoleId)
                .OnDelete(DeleteBehavior.Cascade);

            entity.HasOne(e => e.Function)
                .WithMany(f => f.RolePermissions)
                .HasForeignKey(e => e.FunctionId)
                .OnDelete(DeleteBehavior.Cascade);
        });

        // Category 設定
        modelBuilder.Entity<Category>(entity =>
        {
            // 複合唯一索引：Slug + LanguageCode
            entity.HasIndex(e => new { e.Slug, e.LanguageCode }).IsUnique();
            entity.Property(e => e.Name).HasMaxLength(20).IsRequired();
            entity.Property(e => e.Slug).HasMaxLength(100).IsRequired();
            entity.Property(e => e.LanguageCode).HasMaxLength(10).IsRequired().HasDefaultValue("zh-TW");
            entity.Property(e => e.MetaTitle).HasMaxLength(100);
            entity.Property(e => e.MetaDescription).HasMaxLength(200);
            entity.Property(e => e.MetaKeywords).HasMaxLength(200);

            // 自我參照關聯（父子關係）
            entity.HasOne(e => e.Parent)
                .WithMany(e => e.Children)
                .HasForeignKey(e => e.ParentId)
                .OnDelete(DeleteBehavior.Restrict);
        });

        // Article 設定
        modelBuilder.Entity<Article>(entity =>
        {
            // 複合唯一索引：Slug + LanguageCode
            entity.HasIndex(e => new { e.Slug, e.LanguageCode }).IsUnique();
            entity.Property(e => e.Title).HasMaxLength(200).IsRequired();
            entity.Property(e => e.Content).IsRequired();
            entity.Property(e => e.Slug).HasMaxLength(200).IsRequired();
            entity.Property(e => e.LanguageCode).HasMaxLength(10).IsRequired().HasDefaultValue("zh-TW");
            entity.Property(e => e.MetaTitle).HasMaxLength(100);
            entity.Property(e => e.MetaDescription).HasMaxLength(200);
            entity.Property(e => e.MetaKeywords).HasMaxLength(200);

            // 分類關聯
            entity.HasOne(e => e.Category)
                .WithMany()
                .HasForeignKey(e => e.CategoryId)
                .OnDelete(DeleteBehavior.Restrict);

            // 建立者關聯
            entity.HasOne(e => e.Creator)
                .WithMany()
                .HasForeignKey(e => e.CreatedBy)
                .OnDelete(DeleteBehavior.NoAction);

            // 更新者關聯
            entity.HasOne(e => e.Updater)
                .WithMany()
                .HasForeignKey(e => e.UpdatedBy)
                .OnDelete(DeleteBehavior.NoAction);
        });

        // Tag 設定
        modelBuilder.Entity<Tag>(entity =>
        {
            entity.HasIndex(e => e.Name).IsUnique();
            entity.Property(e => e.Name).HasMaxLength(50).IsRequired();
        });

        // ArticleTag 設定（多對多關聯）
        modelBuilder.Entity<ArticleTag>(entity =>
        {
            entity.HasKey(e => new { e.ArticleId, e.TagId });

            entity.HasOne(e => e.Article)
                .WithMany(a => a.ArticleTags)
                .HasForeignKey(e => e.ArticleId)
                .OnDelete(DeleteBehavior.Cascade);

            entity.HasOne(e => e.Tag)
                .WithMany(t => t.ArticleTags)
                .HasForeignKey(e => e.TagId)
                .OnDelete(DeleteBehavior.Cascade);
        });

        // SiteSettings 設定（每種語言一筆記錄）
        modelBuilder.Entity<SiteSettings>(entity =>
        {
            // 複合唯一索引：避免同語言重複記錄
            entity.HasIndex(e => e.LanguageCode).IsUnique();
            entity.Property(e => e.LanguageCode).HasMaxLength(10).IsRequired().HasDefaultValue("zh-TW");
            entity.Property(e => e.MetaTitle).HasMaxLength(100);
            entity.Property(e => e.MetaDescription).HasMaxLength(200);
            entity.Property(e => e.MetaKeywords).HasMaxLength(200);
            entity.Property(e => e.FaviconPath).HasMaxLength(500);
        });

        // HeaderSettings 設定（每種語言一筆記錄）
        modelBuilder.Entity<HeaderSettings>(entity =>
        {
            entity.HasIndex(e => e.LanguageCode).IsUnique();
            entity.Property(e => e.LanguageCode).HasMaxLength(10).IsRequired().HasDefaultValue("zh-TW");
            entity.Property(e => e.HtmlContent).HasColumnType("nvarchar(max)");
        });

        // FooterSettings 設定（每種語言一筆記錄）
        modelBuilder.Entity<FooterSettings>(entity =>
        {
            entity.HasIndex(e => e.LanguageCode).IsUnique();
            entity.Property(e => e.LanguageCode).HasMaxLength(10).IsRequired().HasDefaultValue("zh-TW");
            entity.Property(e => e.HtmlContent).HasColumnType("nvarchar(max)");
        });

        // LanguageResource 設定
        modelBuilder.Entity<LanguageResource>(entity =>
        {
            // 複合唯一索引：LanguageCode + ResourceKey
            entity.HasIndex(e => new { e.LanguageCode, e.ResourceKey, e.IsDeleted }).IsUnique();

            entity.Property(e => e.LanguageCode).HasMaxLength(10).IsRequired().HasDefaultValue("zh-TW");
            entity.Property(e => e.ResourceKey).HasMaxLength(500).IsRequired();
            entity.Property(e => e.ResourceValue).IsRequired();
            entity.Property(e => e.ResourceType).HasMaxLength(50).IsRequired().HasDefaultValue("Label");
            entity.Property(e => e.Description).HasMaxLength(500);
            entity.Property(e => e.CreatedBy).HasMaxLength(100);
            entity.Property(e => e.UpdatedBy).HasMaxLength(100);

            // 全局查詢篩選器（軟刪除）
            // 在下方統一設定所有 ISoftDeletable 實體
        });

        // 為所有實作 ISoftDeletable 的實體設定全域查詢篩選器
        foreach (var entityType in modelBuilder.Model.GetEntityTypes())
        {
            if (typeof(ISoftDeletable).IsAssignableFrom(entityType.ClrType))
            {
                modelBuilder.Entity(entityType.ClrType)
                    .HasQueryFilter(CreateSoftDeleteFilter(entityType.ClrType));
            }
        }
    }

    private static LambdaExpression CreateSoftDeleteFilter(Type entityType)
    {
        var parameter = Expression.Parameter(entityType, "e");
        var property = Expression.Property(parameter, nameof(ISoftDeletable.IsDeleted));
        var condition = Expression.Equal(property, Expression.Constant(false));
        return Expression.Lambda(condition, parameter);
    }

    public override int SaveChanges()
    {
        UpdateTimestamps();
        return base.SaveChanges();
    }

    public override Task<int> SaveChangesAsync(CancellationToken cancellationToken = default)
    {
        UpdateTimestamps();
        return base.SaveChangesAsync(cancellationToken);
    }

    private void UpdateTimestamps()
    {
        var entries = ChangeTracker.Entries<BaseEntity>();
        foreach (var entry in entries)
        {
            if (entry.State == EntityState.Added)
            {
                entry.Entity.CreatedAt = DateTime.UtcNow;
                entry.Entity.UpdatedAt = DateTime.UtcNow;
            }
            else if (entry.State == EntityState.Modified)
            {
                entry.Entity.UpdatedAt = DateTime.UtcNow;
            }
        }
    }
}
