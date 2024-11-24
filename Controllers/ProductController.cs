using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using vpp_server.Data;
using vpp_server.Models;
using vpp_server.Models.Dtos.RequestDtos;
using vpp_server.Models.Dtos.ResponseDtos;

namespace vpp_server.Controllers
{
    [ApiController]
    [Route("api/v1/products")]
    public class ProductController : ControllerBase
    {
        private readonly AppDbContext _context;

        public ProductController(AppDbContext context)
        {
            _context = context;
        }

        [HttpGet]
        public async Task<IActionResult> GetProducts(string name = null, int? minPrice = null, int? maxPrice = null, int? catalogId = null, string sortBy = null, string sortOrder = "asc")
        {
            try
            {
                var query = _context.Products.Include(p => p.Catalog).AsQueryable();

                if (!string.IsNullOrEmpty(name))
                {
                    query = query.Where(p => p.Name.Contains(name));
                }

                if (minPrice.HasValue)
                {
                    query = query.Where(p => p.Price >= minPrice.Value);
                }

                if (maxPrice.HasValue)
                {
                    query = query.Where(p => p.Price <= maxPrice.Value);
                }

                if (catalogId.HasValue)
                {
                    query = query.Where(p => p.CatalogId == catalogId.Value);
                }

                if (!string.IsNullOrEmpty(sortBy))
                {
                    switch (sortBy.ToLower())
                    {
                        case "name":
                            query = sortOrder.ToLower() == "desc" ? query.OrderByDescending(p => p.Name) : query.OrderBy(p => p.Name);
                            break;
                        case "price":
                            query = sortOrder.ToLower() == "desc" ? query.OrderByDescending(p => p.Price) : query.OrderBy(p => p.Price);
                            break;
                        default:
                            query = query.OrderBy(p => p.Name);
                            break;
                    }
                }
                else
                {
                    query = query.OrderBy(p => p.CreateDate);
                }

                var products = await query.ToListAsync();
                var productDtos = products.Select(p => new ProductResponseDto
                {
                    Id = p.Id,
                    Name = p.Name,
                    Price = p.Price,
                    Description = p.Description,
                    ImageUrl = p.ImageUrl,
                    Stock = p.Stock,
                    CatalogId = p.CatalogId,
                    CatalogName = p.Catalog.Name,
                    CreateDate = p.CreateDate,
                    UpdateDate = p.UpdateDate
                }).ToList();

                return Ok(new ResponseDto { Result = productDtos, IsSuccess = true });
            }
            catch (Exception ex)
            {
                return StatusCode(500, new ResponseDto { IsSuccess = false, Message = ex.Message });
            }
        }

        [Authorize(Roles = "Admin")]
        [HttpPost]
        public async Task<IActionResult> AddProduct([FromBody] ProductRequestDto productDto)
        {
            try
            {
                var catalog = await _context.Catalogs.FindAsync(productDto.CatalogId);
                if (catalog == null)
                {
                    return NotFound(new ResponseDto { IsSuccess = false, Message = "Catalog not found" });
                }

                var product = new Product
                {
                    Name = productDto.Name,
                    Price = productDto.Price,
                    Description = productDto.Description,
                    ImageUrl = productDto.ImageUrl,
                    Stock = productDto.Stock,
                    CatalogId = productDto.CatalogId,
                    CreateDate = DateTime.UtcNow,
                    UpdateDate = DateTime.UtcNow
                };

                _context.Products.Add(product);
                await _context.SaveChangesAsync();

                var responseDto = new ProductResponseDto
                {
                    Id = product.Id,
                    Name = product.Name,
                    Price = product.Price,
                    Description = product.Description,
                    ImageUrl = product.ImageUrl,
                    Stock = product.Stock,
                    CatalogId = product.CatalogId,
                    CatalogName = catalog.Name,
                    CreateDate = product.CreateDate,
                    UpdateDate = product.UpdateDate
                };

                return CreatedAtAction(nameof(GetProducts), new { id = product.Id }, new ResponseDto { Result = responseDto, IsSuccess = true });
            }
            catch (Exception ex)
            {
                return StatusCode(500, new ResponseDto { IsSuccess = false, Message = ex.Message });
            }
        }

        [Authorize(Roles = "Admin")]
        [HttpPut("{id}")]
        public async Task<IActionResult> UpdateProduct(int id, [FromBody] ProductRequestDto productDto)
        {
            try
            {
                var product = await _context.Products.FindAsync(id);
                if (product == null)
                {
                    return NotFound(new ResponseDto { IsSuccess = false, Message = "Product not found" });
                }

                product.Name = productDto.Name;
                product.Price = productDto.Price;
                product.Description = productDto.Description;
                product.ImageUrl = productDto.ImageUrl;
                product.Stock = productDto.Stock;
                product.CatalogId = productDto.CatalogId;
                product.UpdateDate = DateTime.UtcNow;

                _context.Entry(product).State = EntityState.Modified;
                await _context.SaveChangesAsync();

                return Ok(new ResponseDto { IsSuccess = true, Message = "Product updated successfully" });
            }
            catch (Exception ex)
            {
                return StatusCode(500, new ResponseDto { IsSuccess = false, Message = ex.Message });
            }
        }

        [Authorize(Roles = "Admin")]
        [HttpDelete("{id}")]
        public async Task<IActionResult> DeleteProduct(int id)
        {
            try
            {
                var product = await _context.Products.FindAsync(id);
                if (product == null)
                {
                    return NotFound(new ResponseDto { IsSuccess = false, Message = "Product not found" });
                }

                _context.Products.Remove(product);
                await _context.SaveChangesAsync();
                return Ok(new ResponseDto { IsSuccess = true, Message = "Product deleted successfully" });
            }
            catch (Exception ex)
            {
                return StatusCode(500, new ResponseDto { IsSuccess = false, Message = ex.Message });
            }
        }
    }
}
