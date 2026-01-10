using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using APITest.Models;

namespace APITest.Controllers
{
    [Route("api/TodoItems")]
    [ApiController]
    public class ItemController : ControllerBase
    {
        private readonly ItemContext _context;

        public ItemController(ItemContext context)
        {
            _context = context;
        }

        [HttpGet]
        public async Task<ActionResult<IEnumerable<ItemDTO>>> GetItems()
        {
            return await _context.Items.ToListAsync();
        }
    }
}
