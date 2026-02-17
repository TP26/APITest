using Microsoft.AspNetCore.Mvc;
using APITest.Models;
using Microsoft.EntityFrameworkCore;

namespace APITest.Controllers
{
    [Route("/categories")]
    [ApiController]
    public class CategoryController : ControllerBase
    {
        private readonly CategoryContext _context;

        public CategoryController(CategoryContext context)
        {
            _context = context;
        }

        [HttpGet]
        public async Task<ActionResult<IEnumerable<Category>>> GetCategories()
        {
            return await _context.Categories.ToListAsync();
        }

        [HttpGet("{id}")]
        public async Task<ActionResult<Category>> GetCategory(int id)
        {
            Category? category = await _context.Categories.FindAsync(id);

            if (category == null)
            {
                return NotFound();
            }

            return category;
        }

        [HttpDelete]
        [Route("/categories/{id}")]
        public async Task<ActionResult> DeleteCategory(int id)
        {
            if (await _context.Categories.FindAsync(id) is Category category)
            {
                _context.Categories.Remove(category);
                await _context.SaveChangesAsync();
                return NoContent();
            }

            return NotFound();
        }
    }
}
