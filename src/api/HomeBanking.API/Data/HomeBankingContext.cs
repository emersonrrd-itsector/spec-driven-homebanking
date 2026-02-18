using System.Security.Cryptography;
using System.Text;
using HomeBanking.Domain;
using Microsoft.EntityFrameworkCore;

namespace HomeBanking.API.Data;

/// <summary>
/// Entity Framework Core DbContext for the HomeBanking application.
/// Configured to use InMemory provider.
/// </summary>
public class HomeBankingContext : DbContext
{
    /// <summary>
    /// Initializes a new instance of the <see cref="HomeBankingContext"/> class.
    /// </summary>
    /// <param name="options">The context options.</param>
    public HomeBankingContext(DbContextOptions<HomeBankingContext> options)
        : base(options)
    {
    }

    /// <summary>
    /// Gets the Users DbSet.
    /// </summary>
    public DbSet<User> Users => this.Set<User>();

    /// <summary>
    /// Gets the Accounts DbSet.
    /// </summary>
    public DbSet<Account> Accounts => this.Set<Account>();

    /// <summary>
    /// Gets the Transactions DbSet.
    /// </summary>
    public DbSet<Transaction> Transactions => this.Set<Transaction>();

    /// <summary>
    /// Seeds demo data into the database if it's empty.
    /// </summary>
    public void SeedIfEmpty()
    {
        // Only seed if database is empty
        if (this.Users.Any())
        {
            return;
        }

        const string DemoEmail = "admin@homebank.local";
        const string DemoPassword = "demo123";

        var userId = Guid.Parse("550e8400-e29b-41d4-a716-446655440000");
        var account1Id = Guid.Parse("550e8400-e29b-41d4-a716-446655440001");
        var account2Id = Guid.Parse("550e8400-e29b-41d4-a716-446655440002");

        // Hash the demo password using SHA256
        var passwordHash = this.HashPassword(DemoPassword);

        // Create user
        var user = new User
        {
            Id = userId,
            Email = DemoEmail,
            PasswordHash = passwordHash,
        };

        this.Users.Add(user);
        this.SaveChanges();

        // Create accounts
        var now = DateTime.UtcNow;
        var checking = new Account
        {
            Id = account1Id,
            UserId = userId,
            Name = "Checking",
            Balance = 5000.00m,
            Currency = "USD",
            AccountNumber = "1001234567890",
            LastUpdated = now,
        };

        var savings = new Account
        {
            Id = account2Id,
            UserId = userId,
            Name = "Savings",
            Balance = 15000.00m,
            Currency = "USD",
            AccountNumber = "2001234567890",
            LastUpdated = now,
        };

        this.Accounts.AddRange(checking, savings);
        this.SaveChanges();

        // Create transactions
        var account1Transactions = this.GenerateSampleTransactions(account1Id, now);
        this.Transactions.AddRange(account1Transactions);

        var account2Transactions = this.GenerateSampleTransactions(account2Id, now.AddDays(-5));
        this.Transactions.AddRange(account2Transactions);

        this.SaveChanges();
    }

    /// <summary>
    /// Configures the model relationships and constraints using Fluent API.
    /// </summary>
    /// <param name="modelBuilder">The model builder.</param>
    protected override void OnModelCreating(ModelBuilder modelBuilder)
    {
        base.OnModelCreating(modelBuilder);

        // Configure User entity
        modelBuilder.Entity<User>(entity =>
        {
            entity.HasKey(u => u.Id);
            entity.Property(u => u.Email).IsRequired().HasMaxLength(256);
            entity.Property(u => u.PasswordHash).IsRequired();
            entity.HasMany(u => u.Accounts)
                .WithOne(a => a.User)
                .HasForeignKey(a => a.UserId)
                .OnDelete(DeleteBehavior.Cascade);
        });

        // Configure Account entity
        modelBuilder.Entity<Account>(entity =>
        {
            entity.HasKey(a => a.Id);
            entity.Property(a => a.UserId).IsRequired();
            entity.Property(a => a.Name).IsRequired().HasMaxLength(256);
            entity.Property(a => a.Balance).HasColumnType("decimal(18,2)");
            entity.Property(a => a.Currency).IsRequired().HasMaxLength(3);
            entity.Property(a => a.AccountNumber).IsRequired().HasMaxLength(50);
            entity.Property(a => a.LastUpdated).IsRequired();
            entity.HasMany(a => a.Transactions)
                .WithOne(t => t.Account)
                .HasForeignKey(t => t.AccountId)
                .OnDelete(DeleteBehavior.Cascade);
        });

        // Configure Transaction entity
        modelBuilder.Entity<Transaction>(entity =>
        {
            entity.HasKey(t => t.Id);
            entity.Property(t => t.AccountId).IsRequired();
            entity.Property(t => t.Amount).HasColumnType("decimal(18,2)");
            entity.Property(t => t.Date).IsRequired();
            entity.Property(t => t.Description).IsRequired().HasMaxLength(512);
            entity.Property(t => t.Category).IsRequired().HasMaxLength(50);
            entity.Property(t => t.Type).IsRequired().HasMaxLength(10);
        });
    }

    /// <summary>
    /// Generates ~15 sample transactions for an account.
    /// </summary>
    /// <param name="accountId">The account ID.</param>
    /// <param name="baseDate">The base date for transactions.</param>
    /// <returns>A list of sample transactions.</returns>
    private List<Transaction> GenerateSampleTransactions(Guid accountId, DateTime baseDate)
    {
        var transactions = new List<Transaction>();

        var sampleData = new[]
        {
            ("Salary deposit", "Salary", "Credit", 3500.00m, 0),
            ("Walmart groceries", "Groceries", "Debit", 87.50m, 1),
            ("Electric bill", "Utilities", "Debit", 125.00m, 2),
            ("Netflix subscription", "Entertainment", "Debit", 15.99m, 3),
            ("Restaurant - Luigi's", "Food & Dining", "Debit", 45.30m, 4),
            ("Uber ride", "Transport", "Debit", 22.50m, 5),
            ("Costco shopping", "Groceries", "Debit", 156.72m, 6),
            ("Gas station", "Transport", "Debit", 65.00m, 7),
            ("Movie tickets", "Entertainment", "Debit", 32.00m, 8),
            ("Whole Foods groceries", "Groceries", "Debit", 94.25m, 9),
            ("Water bill", "Utilities", "Debit", 58.00m, 10),
            ("Pizza Hut delivery", "Food & Dining", "Debit", 28.99m, 11),
            ("Amazon purchase", "Other", "Debit", 89.99m, 12),
            ("ATM withdrawal", "Other", "Debit", 200.00m, 13),
            ("Freelance income", "Salary", "Credit", 800.00m, 14),
        };

        foreach (var (description, category, type, amount, dayOffset) in sampleData)
        {
            transactions.Add(new Transaction
            {
                Id = Guid.NewGuid(),
                AccountId = accountId,
                Amount = amount,
                Date = baseDate.AddDays(-dayOffset),
                Description = description,
                Category = category,
                Type = type,
            });
        }

        return transactions;
    }

    /// <summary>
    /// Hashes a password using SHA256 (for demo purposes only; production should use bcrypt).
    /// </summary>
    /// <param name="password">The password to hash.</param>
    /// <returns>The hexadecimal string representation of the hash.</returns>
    private string HashPassword(string password)
    {
        using (var sha256 = SHA256.Create())
        {
            var hashedBytes = sha256.ComputeHash(Encoding.UTF8.GetBytes(password));
            return BitConverter.ToString(hashedBytes).Replace("-", string.Empty).ToLowerInvariant();
        }
    }
}
