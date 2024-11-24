using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using vpp_server.Data;
using vpp_server.Models.Dtos.RequestDtos;
using vpp_server.Models.Dtos.ResponseDtos;
using vpp_server.Models;
using Microsoft.EntityFrameworkCore;


namespace vpp_server.Controllers
{
    [ApiController]
    [Route("api/v1/cart")]
    public class CartController : ControllerBase
    {
        private readonly AppDbContext _context;
        private readonly UserManager<ApplicationUser> _userManager;

        public CartController(AppDbContext context, UserManager<ApplicationUser> userManager)
        {
            _context = context;
            _userManager = userManager;
        }

        [Authorize(Roles = "Customer")]
        [HttpPost("create-cart")]
        public async Task<IActionResult> CreateCart()
        {
            var userId = _userManager.Users.FirstOrDefault(u => u.Email == _userManager.GetUserId(User))?.Id;
            if (string.IsNullOrEmpty(userId))
            {
                return BadRequest(new ResponseDto { IsSuccess = false, Message = "Invalid user." });
            }

            var cart = await _context.Carts.Include(c => c.CartItems).FirstOrDefaultAsync(c => c.UserId == userId);
            if (cart == null)
            {
                cart = new Cart { UserId = userId };
                _context.Carts.Add(cart);
                await _context.SaveChangesAsync();
            }

            await _context.SaveChangesAsync();

            return Ok(new ResponseDto { IsSuccess = true, Message = "Create cart successfully", Result = null });
        }

        [Authorize(Roles = "Customer")]
        [HttpPost("add-item")]
        public async Task<IActionResult> AddItemToCart([FromBody] CartItemRequestDto cartItemRequestDto)
        {
            var userId = _userManager.Users.FirstOrDefault(u => u.Email == _userManager.GetUserId(User))?.Id;
            if (string.IsNullOrEmpty(userId))
            {
                return BadRequest(new ResponseDto { IsSuccess = false, Message = "Invalid user." });
            }

            var cart = await _context.Carts.Include(c => c.CartItems).FirstOrDefaultAsync(c => c.UserId == userId);
            if (cart == null)
            {
                cart = new Cart { UserId = userId };
                _context.Carts.Add(cart);
                await _context.SaveChangesAsync();
            }

            var existingItem = cart.CartItems.FirstOrDefault(item => item.ProductId == cartItemRequestDto.ProductId);
            if (existingItem != null)
            {
                existingItem.Quantity += cartItemRequestDto.Quantity;
            }
            else
            {
                var product = await _context.Products.FindAsync(cartItemRequestDto.ProductId);
                if (product == null)
                {
                    return BadRequest(new ResponseDto { IsSuccess = false, Message = "Product not found." });
                }

                var newItem = new CartItem
                {
                    CartId = cart.Id,
                    ProductId = cartItemRequestDto.ProductId,
                    Quantity = cartItemRequestDto.Quantity
                };
                cart.CartItems.Add(newItem);
            }

            await _context.SaveChangesAsync();

            return Ok(new ResponseDto { IsSuccess = true, Message = "Item added to cart successfully", Result = null });
        }

        [Authorize(Roles = "Customer")]
        [HttpGet("items")]
        public async Task<IActionResult> GetCartItems()
        {
            var userId = _userManager.Users.FirstOrDefault(u => u.Email == _userManager.GetUserId(User))?.Id;
            if (string.IsNullOrEmpty(userId))
            {
                return BadRequest(new ResponseDto { IsSuccess = false, Message = "Invalid user." });
            }

            var cart = await _context.Carts.Include(c => c.CartItems)
                                           .ThenInclude(ci => ci.Product)
                                           .FirstOrDefaultAsync(c => c.UserId == userId);
            if (cart == null)
            {
                return NotFound(new ResponseDto { IsSuccess = false, Message = "Cart not found." });
            }

            var cartItems = cart.CartItems.Select(ci => new ProductItemDto
            {
                Id = ci.ProductId,
                Name = ci.Product.Name,
                ImageUrl = ci.Product.ImageUrl,
                Quantity = ci.Quantity,
                Price = ci.Product.Price
            }).ToList();

            var cartResponse = new CartResponseDto
            {
                Items = cartItems
            };

            return Ok(new ResponseDto { IsSuccess = true, Message = "Cart items retrieved successfully", Result = cartResponse });
        }

        [Authorize(Roles = "Customer")]
        [HttpDelete("remove-item/{productId}")]
        public async Task<IActionResult> RemoveItemFromCart(int productId)
        {
            var userId = _userManager.Users.FirstOrDefault(u => u.Email == _userManager.GetUserId(User))?.Id;
            if (string.IsNullOrEmpty(userId))
            {
                return BadRequest(new ResponseDto { IsSuccess = false, Message = "Invalid user." });
            }

            var cart = await _context.Carts.Include(c => c.CartItems).FirstOrDefaultAsync(c => c.UserId == userId);
            if (cart == null)
            {
                return NotFound(new ResponseDto { IsSuccess = false, Message = "Cart not found." });
            }

            var cartItem = cart.CartItems.FirstOrDefault(ci => ci.ProductId == productId);
            if (cartItem == null)
            {
                return NotFound(new ResponseDto { IsSuccess = false, Message = "Product not found in cart." });
            }

            cart.CartItems.Remove(cartItem);
            await _context.SaveChangesAsync();

            return Ok(new ResponseDto { IsSuccess = true, Message = "Item removed from cart successfully" });
        }

        [Authorize(Roles = "Customer")]
        [HttpDelete("delete-cart")]
        public async Task<IActionResult> DeleteCart()
        {
            var userId = _userManager.Users.FirstOrDefault(u => u.Email == _userManager.GetUserId(User))?.Id;
            if (string.IsNullOrEmpty(userId))
            {
                return BadRequest(new ResponseDto { IsSuccess = false, Message = "Invalid user." });
            }

            var cart = await _context.Carts.Include(c => c.CartItems).FirstOrDefaultAsync(c => c.UserId == userId);
            if (cart == null)
            {
                return NotFound(new ResponseDto { IsSuccess = false, Message = "Cart not found." });
            }

            _context.CartItems.RemoveRange(cart.CartItems);
            await _context.SaveChangesAsync();

            return Ok(new ResponseDto { IsSuccess = true, Message = "Cart and all items removed successfully" });
        }

        [Authorize(Roles = "Customer")]
        [HttpPut("update-item")]
        public async Task<IActionResult> UpdateItemQuantity([FromBody] CartItemRequestDto cartItemRequestDto)
        {
            var userId = _userManager.Users.FirstOrDefault(u => u.Email == _userManager.GetUserId(User))?.Id;
            if (string.IsNullOrEmpty(userId))
            {
                return BadRequest(new ResponseDto { IsSuccess = false, Message = "Invalid user." });
            }

            var cart = await _context.Carts.Include(c => c.CartItems).FirstOrDefaultAsync(c => c.UserId == userId);
            if (cart == null)
            {
                return NotFound(new ResponseDto { IsSuccess = false, Message = "Cart not found." });
            }

            var cartItem = cart.CartItems.FirstOrDefault(ci => ci.ProductId == cartItemRequestDto.ProductId);
            if (cartItem == null)
            {
                return NotFound(new ResponseDto { IsSuccess = false, Message = "Product not found in cart." });
            }

            if (cartItemRequestDto.Quantity == 0)
            {
                cart.CartItems.Remove(cartItem);
            }
            else
            {
                cartItem.Quantity = cartItemRequestDto.Quantity;
            }

            await _context.SaveChangesAsync();

            return Ok(new ResponseDto { IsSuccess = true, Message = "Item quantity updated successfully" });
        }
    }
}