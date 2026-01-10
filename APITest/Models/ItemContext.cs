using Microsoft.EntityFrameworkCore;

namespace APITest.Models
{
    public class ItemContext : DbContext
    {
        public ItemContext(DbContextOptions<ItemContext> options)
            : base(options)
        {

        }

        public DbSet<ItemDTO> Items { get; set; } = null!;
    }
}
