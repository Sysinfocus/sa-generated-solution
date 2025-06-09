namespace Demo.Data.Sqlite;

public sealed class AppDbContext(DbContextOptions<AppDbContext> options) : IdentityDbContext<IdentityUser>(options)
{
    // Add DbSet
	public DbSet<MCP> MCPServers { get; set; }
    public new DbSet<User> Users { get; set; }

    protected override void OnModelCreating(ModelBuilder modelBuilder)
    {
        base.OnModelCreating(modelBuilder);

        // Add Configuration
		modelBuilder.ApplyConfiguration(new MCPConfiguration());
        modelBuilder.ApplyConfiguration(new UserConfiguration());
    }
}