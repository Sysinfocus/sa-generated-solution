namespace Demo.Data.Sqlite.Configurations;

public class MCPConfiguration : IEntityTypeConfiguration<MCP>
{
    void IEntityTypeConfiguration<MCP>.Configure(EntityTypeBuilder<MCP> builder)
    {
        builder.HasKey(p => p.Id);
    }
}

