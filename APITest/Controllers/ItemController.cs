using APITest.Models;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;

namespace APITest.Controllers
{
    [Route("/items")]
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

        [HttpGet]
        [Route("/{id}")]
        public async Task<ActionResult<ItemDTO>> GetItemsWithId(int id)
        {
            ItemDTO? item = await _context.Items.FindAsync(id);

            if (item == null)
            {
                return NotFound();
            }

            return new ActionResult<ItemDTO>(item);
        }

        [HttpGet]
        [Route("/find/{position}")]
        public async Task<ActionResult<IEnumerable<ItemDTO>>> GetItemsWithPosition(int position)
        {
            return await _context.Items.Where(item => item.Position == position).ToListAsync(); 
        }

        [HttpPost]
        public async Task<ActionResult<ItemDTO>> PostItem(ItemDTO item)
        {
            _context.Items.Add(item);
            await _context.SaveChangesAsync();

            return CreatedAtAction(nameof(item), new { id = item.Id }, item);
        }

        [HttpPut]
        [Route("/items/{id}")]
        public async Task<ActionResult> PutItem(int id, ItemDTO inputItem)
        {
            ItemDTO? item = await _context.Items.FindAsync(id);

            if (item == null)
            {
                return NotFound();
            }

            item.Name = inputItem.Name;
            item.Position = inputItem.Position;

            await _context.SaveChangesAsync();
            return NoContent();
        }

        [HttpDelete]
        [Route("/items/{id}")]
        public async Task<ActionResult> DeleteItem(int id)
        {
            if (await _context.Items.FindAsync(id) is ItemDTO item){
                _context.Items.Remove(item);
                await _context.SaveChangesAsync();
                return NoContent();
            }

            return NotFound();
        }
    }
}
