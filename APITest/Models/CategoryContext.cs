using Microsoft.EntityFrameworkCore;

namespace APITest.Models
{
    public class CategoryContext : DbContext
    {
        public CategoryContext(DbContextOptions<CategoryContext> options) : base(options) { }

        public DbSet<Category> Categories { get; set; } = null!;
    }
}
