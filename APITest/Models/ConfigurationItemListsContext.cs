using Microsoft.EntityFrameworkCore;

namespace APITest.Models
{
    public class ConfigurationItemListsContext : DbContext
    {
        public ConfigurationItemListsContext(DbContextOptions<ConfigurationItemListsContext> options) : base(options) { }

        protected override void OnConfiguring(DbContextOptionsBuilder optionsBuilder)
        {
            base.OnConfiguring(optionsBuilder);
        }

        public DbSet<ConfigurationItemLists> ConfigurationItemLists { get; set; } = null!;
    }
}