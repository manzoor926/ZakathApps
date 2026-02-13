using Microsoft.EntityFrameworkCore;
using ZakathApp.API.Models;

namespace ZakathApp.API.Data
{
    public class ZakathDbContext : DbContext
    {
        public ZakathDbContext(DbContextOptions<ZakathDbContext> options) : base(options)
        {
        }

        // ========================================
        // CORE ENTITIES
        // ========================================
        public DbSet<User> Users { get; set; }
        public DbSet<CategoryMaster> CategoryMasters { get; set; }
        public DbSet<AssetCategoryMaster> AssetCategoryMasters { get; set; }
        public DbSet<ItemMaster> ItemMasters { get; set; }
        public DbSet<ZakathMaster> ZakathMasters { get; set; }

        // ========================================
        // TRANSACTION ENTITIES
        // ========================================
        public DbSet<IncomeDetail> IncomeDetails { get; set; }
        public DbSet<ExpenseDetail> ExpenseDetails { get; set; }
        public DbSet<CurrentAsset> CurrentAssets { get; set; }
        
        // ========================================
        // ZAKATH ENTITIES
        // ========================================
        public DbSet<ZakathCalculation> ZakathCalculations { get; set; }
        public DbSet<ZakathPayment> ZakathPayments { get; set; }
        public DbSet<ZakathRecipient> ZakathRecipients { get; set; }

        // ========================================
        // SYSTEM ENTITIES
        // ========================================
        public DbSet<Notification> Notifications { get; set; }
        public DbSet<AppSetting> AppSettings { get; set; }
        public DbSet<AuditLog> AuditLogs { get; set; }
        public DbSet<MarketValueUpdate> MarketValueUpdates { get; set; }

        // ========================================
        // ENHANCED ENTITIES (Multi-Currency, Multi-Language, Madhab)
        // ========================================
        public DbSet<Currency> Currencies { get; set; }
        public DbSet<UserCurrency> UserCurrencies { get; set; }
        public DbSet<CurrencyExchangeRate> CurrencyExchangeRates { get; set; }
        public DbSet<SupportedLanguage> SupportedLanguages { get; set; }
        public DbSet<Translation> Translations { get; set; }
        public DbSet<Madhab> Madhabs { get; set; }
        public DbSet<MadhabZakathRule> MadhabZakathRules { get; set; }
        public DbSet<AssetZakathEligibility> AssetZakathEligibilities { get; set; }

        // ========================================
        // MODEL CONFIGURATION
        // ========================================
        protected override void OnModelCreating(ModelBuilder modelBuilder)
        {
            base.OnModelCreating(modelBuilder);

            // Configure indexes for better performance
            ConfigureIndexes(modelBuilder);

            // Configure relationships
            ConfigureRelationships(modelBuilder);

            // Configure decimal precision
            ConfigureDecimalPrecision(modelBuilder);
        }

        private void ConfigureIndexes(ModelBuilder modelBuilder)
        {
            // User indexes
            modelBuilder.Entity<User>()
                .HasIndex(u => u.MobileNumber)
                .IsUnique();

            modelBuilder.Entity<User>()
                .HasIndex(u => u.Email);

            // Income indexes
            modelBuilder.Entity<IncomeDetail>()
                .HasIndex(i => new { i.UserID, i.DateReceived });

            // Expense indexes
            modelBuilder.Entity<ExpenseDetail>()
                .HasIndex(e => new { e.UserID, e.DateOfTransaction });

            // Asset indexes
            modelBuilder.Entity<CurrentAsset>()
                .HasIndex(a => new { a.UserID, a.IsZakathApplicable });

            // Zakath Calculation indexes
            modelBuilder.Entity<ZakathCalculation>()
                .HasIndex(z => new { z.UserID, z.CalculationDate });

            // Notification indexes
            modelBuilder.Entity<Notification>()
                .HasIndex(n => new { n.UserID, n.IsRead, n.ScheduledDate });

            // Translation indexes
            modelBuilder.Entity<Translation>()
                .HasIndex(t => new { t.TranslationKey, t.LanguageCode })
                .IsUnique();

            // Currency indexes
            modelBuilder.Entity<Currency>()
                .HasIndex(c => c.CurrencyCode)
                .IsUnique();

            // Exchange Rate indexes
            modelBuilder.Entity<CurrencyExchangeRate>()
                .HasIndex(e => new { e.FromCurrencyID, e.ToCurrencyID, e.EffectiveDate });
        }

        private void ConfigureRelationships(ModelBuilder modelBuilder)
        {
            // User -> PreferredMadhab (optional)
            modelBuilder.Entity<User>()
                .HasOne(u => u.PreferredMadhab)
                .WithMany(m => m.Users)
                .HasForeignKey(u => u.PreferredMadhabID)
                .OnDelete(DeleteBehavior.SetNull);

            // User -> UserCurrencies
            modelBuilder.Entity<UserCurrency>()
                .HasOne(uc => uc.User)
                .WithMany(u => u.UserCurrencies)
                .HasForeignKey(uc => uc.UserID)
                .OnDelete(DeleteBehavior.Cascade);

            // User -> Incomes
            modelBuilder.Entity<IncomeDetail>()
                .HasOne(i => i.User)
                .WithMany(u => u.Incomes)
                .HasForeignKey(i => i.UserID)
                .OnDelete(DeleteBehavior.Cascade);

            // User -> Expenses
            modelBuilder.Entity<ExpenseDetail>()
                .HasOne(e => e.User)
                .WithMany(u => u.Expenses)
                .HasForeignKey(e => e.UserID)
                .OnDelete(DeleteBehavior.Cascade);

            // User -> Assets
            modelBuilder.Entity<CurrentAsset>()
                .HasOne(a => a.User)
                .WithMany(u => u.Assets)
                .HasForeignKey(a => a.UserID)
                .OnDelete(DeleteBehavior.Cascade);

            // User -> ZakathCalculations
            modelBuilder.Entity<ZakathCalculation>()
                .HasOne(z => z.User)
                .WithMany(u => u.ZakathCalculations)
                .HasForeignKey(z => z.UserID)
                .OnDelete(DeleteBehavior.Cascade);

            // User -> ZakathPayments
            modelBuilder.Entity<ZakathPayment>()
                .HasOne(p => p.User)
                .WithMany(u => u.ZakathPayments)
                .HasForeignKey(p => p.UserID)
                .OnDelete(DeleteBehavior.Cascade);

            // User -> Notifications
            modelBuilder.Entity<Notification>()
                .HasOne(n => n.User)
                .WithMany(u => u.Notifications)
                .HasForeignKey(n => n.UserID)
                .OnDelete(DeleteBehavior.Cascade);

            // Currency relationships
            modelBuilder.Entity<CurrencyExchangeRate>()
                .HasOne(e => e.FromCurrency)
                .WithMany()
                .HasForeignKey(e => e.FromCurrencyID)
                .OnDelete(DeleteBehavior.Restrict);

            modelBuilder.Entity<CurrencyExchangeRate>()
                .HasOne(e => e.ToCurrency)
                .WithMany()
                .HasForeignKey(e => e.ToCurrencyID)
                .OnDelete(DeleteBehavior.Restrict);

            // Madhab relationships
            modelBuilder.Entity<MadhabZakathRule>()
                .HasOne(r => r.Madhab)
                .WithMany(m => m.MadhabZakathRules)
                .HasForeignKey(r => r.MadhabID)
                .OnDelete(DeleteBehavior.Cascade);

            // Asset Zakath Eligibility
            modelBuilder.Entity<AssetZakathEligibility>()
                .HasOne(e => e.CurrentAsset)
                .WithMany(a => a.AssetZakathEligibilities)
                .HasForeignKey(e => e.AssetID)
                .OnDelete(DeleteBehavior.Cascade);
        }

        private void ConfigureDecimalPrecision(ModelBuilder modelBuilder)
        {
            // Configure all decimal properties to use (18,2) precision
            foreach (var property in modelBuilder.Model.GetEntityTypes()
                .SelectMany(t => t.GetProperties())
                .Where(p => p.ClrType == typeof(decimal) || p.ClrType == typeof(decimal?)))
            {
                if (!property.GetColumnType()?.Contains("decimal") ?? true)
                {
                    property.SetColumnType("decimal(18,2)");
                }
            }

            // Exchange rates need higher precision (18,6)
            modelBuilder.Entity<CurrencyExchangeRate>()
                .Property(e => e.Rate)
                .HasColumnType("decimal(18,6)");
        }

        // ========================================
        // HELPER METHODS
        // ========================================
        
        public override int SaveChanges()
        {
            UpdateTimestamps();
            return base.SaveChanges();
        }

        public override async Task<int> SaveChangesAsync(CancellationToken cancellationToken = default)
        {
            UpdateTimestamps();
            return await base.SaveChangesAsync(cancellationToken);
        }

        private void UpdateTimestamps()
        {
            var entries = ChangeTracker.Entries()
                .Where(e => e.State == EntityState.Added || e.State == EntityState.Modified);

            foreach (var entry in entries)
            {
                if (entry.State == EntityState.Added)
                {
                    if (entry.Property("CreatedDate") != null)
                        entry.Property("CreatedDate").CurrentValue = DateTime.Now;
                }

                if (entry.State == EntityState.Modified)
                {
                    if (entry.Property("LastModifiedDate") != null)
                        entry.Property("LastModifiedDate").CurrentValue = DateTime.Now;
                }
            }
        }
    }
}
