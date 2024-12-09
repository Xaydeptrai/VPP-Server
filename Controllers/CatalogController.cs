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
    [Route("api/v1/catalogs")]
    public class CatalogController : ControllerBase
    {
        private readonly AppDbContext _context;

        public CatalogController(AppDbContext context)
        {
            _context = context;
        }

        [HttpGet]
        public async Task<IActionResult> GetCatalogs()
        {
            try
            {
                var catalogs = await _context.Catalogs.Include(c => c.Products).ToListAsync();
                var catalogDtos = catalogs.Select(c => new CatalogResponseDto
                {
                    Id = c.Id,
                    Name = c.Name,
                    ImageUrl = c.ImageUrl
                }).ToList();

                return Ok(new ResponseDto { Result = catalogDtos, IsSuccess = true });
            }
            catch (Exception ex)
            {
                return StatusCode(500, new ResponseDto { IsSuccess = false, Message = ex.Message });
            }
        }

        [Authorize(Roles = "Admin")]
        [HttpPost]
        public async Task<IActionResult> AddCatalog([FromBody] CatalogRequestDto catalogDto)
        {
            try
            {
                var catalog = new Catalog
                {
                    Name = catalogDto.Name,
                    ImageUrl = catalogDto.ImageUrl
                };

                _context.Catalogs.Add(catalog);
                await _context.SaveChangesAsync();

                var responseDto = new CatalogResponseDto
                {
                    Id = catalog.Id,
                    Name = catalog.Name,
                    ImageUrl = catalog.ImageUrl
                };

                return CreatedAtAction(nameof(GetCatalogs), new { id = catalog.Id }, new ResponseDto { Result = responseDto, IsSuccess = true });
            }
            catch (Exception ex)
            {
                return StatusCode(500, new ResponseDto { IsSuccess = false, Message = ex.Message });
            }
        }

        [Authorize(Roles = "Admin")]
        [HttpPut("{id}")]
        public async Task<IActionResult> UpdateCatalog(int id, [FromBody] CatalogRequestDto catalogDto)
        {
            try
            {
                var catalog = await _context.Catalogs.FindAsync(id);
                if (catalog == null)
                {
                    return NotFound(new ResponseDto { IsSuccess = false, Message = "Catalog not found" });
                }

                catalog.Name = catalogDto.Name;
                catalog.ImageUrl = catalogDto.ImageUrl;

                _context.Entry(catalog).State = EntityState.Modified;
                await _context.SaveChangesAsync();

                return Ok(new ResponseDto { IsSuccess = true, Message = "Catalog updated successfully" });
            }
            catch (Exception ex)
            {
                return StatusCode(500, new ResponseDto { IsSuccess = false, Message = ex.Message });
            }
        }

        [Authorize(Roles = "Admin")]
        [HttpDelete("{id}")]
        public async Task<IActionResult> DeleteCatalog(int id)
        {
            try
            {
                var catalog = await _context.Catalogs.FindAsync(id);
                if (catalog == null)
                {
                    return NotFound(new ResponseDto { IsSuccess = false, Message = "Catalog not found" });
                }

                _context.Catalogs.Remove(catalog);
                await _context.SaveChangesAsync();
                return Ok(new ResponseDto { IsSuccess = true, Message = "Catalog deleted successfully" });
            }
            catch (Exception ex)
            {
                return StatusCode(500, new ResponseDto { IsSuccess = false, Message = ex.Message });
            }
        }
    }
}
