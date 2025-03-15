using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using ResourceServer.Models;

namespace ResourceServer.Controllers
{
    [Route("api/[controller]")]
    [ApiController]
    [Authorize]
    public class ProductsController : ControllerBase
    {
        private static readonly List<Product> Products = new List<Product>
        {
            new Product { Id = 1, Name = "Product A", Price = 10.0M, Description = "Test Product A" },
            new Product { Id = 2, Name = "Product B", Price = 20.0M, Description = "Test Product B"  },
            new Product { Id = 3, Name = "Product C", Price = 30.0M, Description = "Test Product C"  }
        };

        private static int _nextId = 4;

        [HttpGet("GetAll")]
        public ActionResult<List<Product>> GetAll()
        {
            return Ok(Products);
        }

        [HttpGet("GetById/{id}")]
        public ActionResult GetProductById(int id)
        {
            var product = Products.FirstOrDefault(p => p.Id == id);
            if (product is null)
                return NotFound(new { message = $"Product with ID {id} not found" });
            return Ok(product);
        }

        [HttpPost("Add")]
        public IActionResult Add([FromBody] Product product)
        {
            if (!ModelState.IsValid)
                return BadRequest(ModelState);

            product.Id = _nextId++;
            Products.Add(product);
            return CreatedAtAction(
                nameof(GetProductById),
                new { id = product.Id },
                product
            );
        }

        [HttpPut("Update/{id}")]
        public IActionResult Update(int id, [FromBody] Product updateProd)
        {
            if (!ModelState.IsValid)
                return BadRequest(ModelState);

            var existingProduct = Products.FirstOrDefault(p => p.Id == id);
            if (existingProduct is null)
                return NotFound(new { message = $"Product with ID {id} not found" });

            existingProduct.Name = updateProd.Name;
            existingProduct.Description = updateProd.Description;
            existingProduct.Price = updateProd.Price;

            return NoContent();
        }

        [HttpDelete("Delete/{id}")]
        public IActionResult DeleteProduct(int id)
        {
            var product = Products.FirstOrDefault(p => p.Id == id);
            if (product is null)
                return NotFound(new { message = $"Product with ID {id} not found" });

            Products.Remove(product);
            return NoContent();
        }
    }
}
