namespace Demo.Data.Sqlite;

public sealed class AppDbContext(DbContextOptions<AppDbContext> options) : IdentityDbContext<IdentityUser>(options)
{
    // Add DbSet
    public new DbSet<User> Users { get; set; }

    protected override void OnModelCreating(ModelBuilder modelBuilder)
    {
        base.OnModelCreating(modelBuilder);

        // Add Configuration
        modelBuilder.ApplyConfiguration(new UserConfiguration());
    }
}