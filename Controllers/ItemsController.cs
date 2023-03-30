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

        // Constructor injection to get the database context
        public ItemsController(UserContext context)
        {
            _context = context;
        }

        // POST method to add a new item to the database
        // It requires the user to be authorized with the "Admin" role

        [HttpPost("ItemAdd")]
        [Authorize(Roles = "Admin")]
        public async Task<IActionResult> Items([FromBody] Items items)
        {
            _context.Items.Add(items); // Adds the new item to the database context
            await _context.SaveChangesAsync(); // Saves changes to the database

            return Ok(items); // Returns a success status code along with the added item
        }

        // GET method to retrieve all items from the database
        [HttpGet("ItemsGet")]
        public async Task<ActionResult<IEnumerable<Items>>> GetProducts()
        {
            var products = await _context.Items.ToListAsync(); // Retrieves all items from the database
            return products; // Returns the retrieved items

        }
        // GET method to search for items that contain a keyword in their name or description
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

        // GET method to retrieve a range of items from the database based on the provided start index and count

        [HttpGet("load")]
        public IActionResult GetItems(int startIndex, int count)
        {
            var items = LoadItemsFromDataSource(startIndex, count); // Loads items from the database based on the provided start index and count
            return Json(items); // Returns the loaded items in JSON format
        }

        // Helper method to load a range of items from the database based on the provided start index and count

        private List<Items> LoadItemsFromDataSource(int startIndex, int count)
        {
            var items = _context.Items
               .OrderBy(x => x.Id) // Orders the items by ID
               .Skip(startIndex) // Skips the specified number of items from the beginning
               .Take(count) // Takes the specified number of items from the skipped index
               .ToList(); // Converts the resulting items to a list
            return items; // Returns the resulting list of items
        }

        // Property to bind an item for editing
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
                Item = contact; // Binds the retrieved item to the Item property
            }
            return Ok(Item); // Returns the retrieved item
        }

        // This endpoint is used to delete an item from the database.
        // It is also decorated with the [Authorize(Roles = "Admin")] attribute to ensure that only users with the "Admin" role can access it.
        [HttpPost("ItemDelete")]
        [Authorize(Roles = "Admin")]
        public async Task<IActionResult> ItemDelete([FromBody] int Id)
        {
            // Check if the Id is valid and if the Items table exists in the database.
            if (Id == 0 || _context.Items == null)
            {
                return NotFound(); // Returns HTTP 404 Not Found status code if the Id or the Items table is invalid.
            }
            // Find the item with the given Id in the Items table.
            var itemFind = await _context.Items.FindAsync(Id);

            // If the item is found, delete it from the database.
            if (itemFind != null)
            {
                Item = itemFind;
                _context.Items.Remove(Item);
                await _context.SaveChangesAsync();
            }

            // Retrieve all the remaining items from the Items table and return them to the client.
            var products = await _context.Items.ToListAsync();
            return Ok(products); // Returns the remaining items in the Items table.
        }

    }
}
