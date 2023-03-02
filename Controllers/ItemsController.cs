using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using WebApplication3.Data;
using WebApplication3.Models;

namespace WebApplication3.Controllers
{
    [Route("api/[controller]")]
    [ApiController]

    public class ItemsController : Controller
    {
        private readonly UserContext _context;
        public ItemsController(UserContext context)
        {
            _context = context;
        }

        [HttpGet("ItemsGet")]
        public async Task<ActionResult<IEnumerable<Items>>> GetProducts()
        {
            var products = await _context.Items.ToListAsync();
            return products;
        }

        [HttpPost("ItemAdd")]
        [Authorize(Roles = "Admin")]
        public async Task<IActionResult> Items([FromBody] Items items)
        {
            _context.Items.Add(items);
            await _context.SaveChangesAsync();

            return Ok(items);
        }

        [BindProperty]
        public Items Item { get; set; }
        [HttpGet("ItemById")]
        public async Task<IActionResult> OnGetAsync(int? id)
        {
            if (id == null || _context.Items == null)
            {
                return NotFound();
            }

            var contact = await _context.Items.FirstOrDefaultAsync(m => m.Id == id);

            if (contact == null)
            {
                return NotFound();
            }
            else
            {
                Item = contact;
            }
            return Ok(Item);
        }

        [HttpPost("ItemDelete")]
        [Authorize(Roles = "Admin")]
        public async Task<IActionResult> ItemDelete([FromBody] int Id)
        {
            if (Id == 0 || _context.Items == null)
            {
                return NotFound();
            }
            var itemFind = await _context.Items.FindAsync(Id);

            if (itemFind != null)
            {
                Item = itemFind;
                _context.Items.Remove(Item);
                await _context.SaveChangesAsync();
            }
            var products = await _context.Items.ToListAsync();
            return Ok(products);
        }

        [HttpGet("Search")]
        public async Task<ActionResult<IEnumerable<Items>>> Search([FromQuery] string keyword)
        {
            if (string.IsNullOrEmpty(keyword))
            {
                // Return all items if no search keyword is provided
                return await _context.Items.ToListAsync();
            }
            else
            {
                // Search for items that contain the keyword in the name or description
                var products = await _context.Items.Where(p =>
                    p.ItemName.Contains(keyword))
                    .ToListAsync();
                return products;
            }
        }
    }
}
