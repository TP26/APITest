using Microsoft.EntityFrameworkCore;

namespace APITest.Models
{
    public class ConfigurationItemListsContext : DbContext
    {
        public ConfigurationItemListsContext(DbContextOptions<ConfigurationItemListsContext> options) : base(options) { }

        public DbSet<ConfigurationItemLists> ConfigurationItemLists { get; set; } = null!;
    }
}