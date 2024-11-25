using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using vpp_server.Data;
using vpp_server.Models.Dtos.ResponseDtos;
using vpp_server.Models.Emuns;
using vpp_server.Models;
using Microsoft.EntityFrameworkCore;
using vpp_server.Models.Dtos.RequestDtos;

namespace vpp_server.Controllers
{
    [ApiController]
    [Route("api/v1/order")]
    public class OrderController : ControllerBase
    {
        private readonly AppDbContext _context;
        private readonly UserManager<ApplicationUser> _userManager;

        public OrderController(AppDbContext context, UserManager<ApplicationUser> userManager)
        {
            _context = context;
            _userManager = userManager;
        }

        [Authorize(Roles = "Customer")]
        [HttpPost("create")]
        public async Task<IActionResult> CreateOrder([FromBody] OrderRequestDto orderRequestDto)
        {
            var userId = _userManager.Users.FirstOrDefault(u => u.Email == _userManager.GetUserId(User))?.Id;
            if (string.IsNullOrEmpty(userId))
            {
                return BadRequest(new ResponseDto { IsSuccess = false, Message = "Invalid user." });
            }

            var cart = await _context.Carts.Include(c => c.CartItems)
                                           .ThenInclude(ci => ci.Product)
                                           .FirstOrDefaultAsync(c => c.UserId == userId);
            if (cart == null || !cart.CartItems.Any())
            {
                return BadRequest(new ResponseDto { IsSuccess = false, Message = "Cart is empty." });
            }

            var orderHeader = new OrderHeader
            {
                UserId = userId,
                OrderDate = DateTime.UtcNow,
                TrackingNumber = GenerateTrackingNumber(),
                ShippingDate = orderRequestDto.ShippingDate,
                Address = orderRequestDto.Address,
                OrderStatus = OrderStatus.Pending,
                PaymentMethod = orderRequestDto.PaymentMethod,
                PaymentStatus = PaymentStatus.Pending,
                OrderTotal = cart.CartItems.Sum(ci => ci.Quantity * ci.Product.Price),
                OrderDetails = cart.CartItems.Select(ci => new OrderDetail
                {
                    ProductId = ci.ProductId,
                    Quantity = ci.Quantity,
                    Price = ci.Product.Price
                }).ToList()
            };

            _context.OrderHeaders.Add(orderHeader);
            _context.CartItems.RemoveRange(cart.CartItems);
            await _context.SaveChangesAsync();

            return Ok(new ResponseDto { IsSuccess = true, Message = "Order created successfully", Result = orderHeader.Id });
        }

        [Authorize(Roles = "Customer")]
        [HttpGet("user-orders")]
        public async Task<IActionResult> GetUserOrders([FromQuery] string? trackingNumber, [FromQuery] string? sortBy = "OrderDate", int pageNumber = 1, int pageSize = 10)
        {
            var user = _userManager.Users.FirstOrDefault(u => u.Email == _userManager.GetUserId(User));
            if (string.IsNullOrEmpty(user?.Id))
            {
                return BadRequest(new ResponseDto { IsSuccess = false, Message = "Invalid user." });
            }

            var query = _context.OrderHeaders.AsQueryable();

            if (!string.IsNullOrEmpty(trackingNumber))
            {
                query = query.Where(oh => oh.TrackingNumber == trackingNumber);
            }

            query = query.Where(oh => oh.UserId == user.Id);

            query = sortBy switch
            {
                "OrderStatus" => query.OrderBy(oh => oh.OrderStatus),
                _ => query.OrderByDescending(oh => oh.OrderDate)
            };

            var totalItems = await query.CountAsync();
            var orders = await query.Skip((pageNumber - 1) * pageSize).Take(pageSize).ToListAsync();

            var orderResponses = orders.Select(order => new OrderHeaderDto
            {
                Id = order.Id,
                Name = user.Name,
                OrderDate = order.OrderDate,
                ShippingDate = order.ShippingDate,
                Address = order.Address,
                OrderStatus = order.OrderStatus,
                PaymentMethod = order.PaymentMethod,
                PaymentStatus = order.PaymentStatus,
                OrderTotal = order.OrderTotal,
                TrackingNumber = order.TrackingNumber
            }).ToList();

            var response = new PagedResponseDto<OrderHeaderDto>
            {
                Items = orderResponses,
                PageNumber = pageNumber,
                PageSize = pageSize,
                TotalItems = totalItems,
                TotalPages = (int)Math.Ceiling(totalItems / (double)pageSize)
            };

            return Ok(new ResponseDto { Result = response, IsSuccess = true, Message = "User orders retrieved successfully" });
        }

        [Authorize(Roles = "Customer")]
        [HttpGet("details/{trackingNumber}")]
        public async Task<IActionResult> GetOrderDetailsByTrackingNumber(string trackingNumber)
        {
            var user = _userManager.Users.FirstOrDefault(u => u.Email == _userManager.GetUserId(User));
            if (string.IsNullOrEmpty(user.Id))
            {
                return BadRequest(new ResponseDto { IsSuccess = false, Message = "Invalid user." });
            }

            var order = await _context.OrderHeaders.Include(oh => oh.OrderDetails)
                                                   .ThenInclude(od => od.Product)
                                                   .FirstOrDefaultAsync(oh => oh.TrackingNumber.Contains(trackingNumber) && oh.UserId == user.Id);
            if (order == null)
            {
                return NotFound(new ResponseDto { IsSuccess = false, Message = "Order not found." });
            }

            var orderResponse = new OrderResponseDto
            {
                Id = order.Id,
                Name = user.Name,
                OrderDate = order.OrderDate,
                ShippingDate = order.ShippingDate,
                Address = order.Address,
                OrderStatus = order.OrderStatus,
                PaymentMethod = order.PaymentMethod,
                PaymentStatus = order.PaymentStatus,
                OrderTotal = order.OrderTotal,
                OrderDetails = order.OrderDetails.Select(od => new ProductItemDto
                {
                    Id = od.ProductId,
                    Name = od.Product.Name,
                    ImageUrl = od.Product.ImageUrl,
                    Quantity = od.Quantity,
                    Price = od.Price
                }).ToList()
            };

            return Ok(new ResponseDto { IsSuccess = true, Message = "Order details retrieved successfully", Result = orderResponse });
        }

        [Authorize(Roles = "Customer")]
        [HttpPost("cancel/{trackingNumber}")]
        public async Task<IActionResult> CancelOrder(string trackingNumber)
        {
            var user = _userManager.Users.FirstOrDefault(u => u.Email == _userManager.GetUserId(User));
            if (string.IsNullOrEmpty(user?.Id))
            {
                return BadRequest(new ResponseDto { IsSuccess = false, Message = "Invalid user." });
            }

            var order = await _context.OrderHeaders.FirstOrDefaultAsync(oh => oh.TrackingNumber == trackingNumber && oh.UserId == user.Id);
            if (order == null)
            {
                return NotFound(new ResponseDto { IsSuccess = false, Message = "Order not found." });
            }

            if (order.OrderDate.AddHours(6) < DateTime.UtcNow)
            {
                return BadRequest(new ResponseDto { IsSuccess = false, Message = "Order can only be canceled within 6 hours of creation." });
            }

            order.OrderStatus = OrderStatus.Cancelled;
            await _context.SaveChangesAsync();

            return Ok(new ResponseDto { IsSuccess = true, Message = "Order canceled successfully" });
        }

        [Authorize(Roles = "Admin")]
        [HttpGet("admin-orders")]
        public async Task<IActionResult> GetAllOrdersForAdmin([FromQuery] string? trackingNumber, [FromQuery] DateTime? orderDate, [FromQuery] string? sortBy = "OrderDate", int pageNumber = 1, int pageSize = 10)
        {
            var query = _context.OrderHeaders.Include(oh => oh.User).AsQueryable();

            if (!string.IsNullOrEmpty(trackingNumber))
            {
                query = query.Where(oh => oh.TrackingNumber.Contains(trackingNumber));
            }

            if (orderDate.HasValue)
            {
                query = query.Where(oh => oh.OrderDate.Date == orderDate.Value.Date);
            }

            query = sortBy switch
            {
                "OrderStatus" => query.OrderBy(oh => oh.OrderStatus),
                _ => query.OrderByDescending(oh => oh.OrderDate)
            };

            var totalItems = await query.CountAsync();
            var orders = await query.Skip((pageNumber - 1) * pageSize).Take(pageSize).ToListAsync();

            var orderResponses = orders.Select(order => new OrderHeaderDto
            {
                Id = order.Id,
                Name = order.User.Name,
                OrderDate = order.OrderDate,
                ShippingDate = order.ShippingDate,
                Address = order.Address,
                OrderStatus = order.OrderStatus,
                PaymentMethod = order.PaymentMethod,
                PaymentStatus = order.PaymentStatus,
                OrderTotal = order.OrderTotal,
                TrackingNumber = order.TrackingNumber,
            }).ToList();

            var response = new PagedResponseDto<OrderHeaderDto>
            {
                Items = orderResponses,
                PageNumber = pageNumber,
                PageSize = pageSize,
                TotalItems = totalItems,
                TotalPages = (int)Math.Ceiling(totalItems / (double)pageSize)
            };

            return Ok(new ResponseDto { Result = response, IsSuccess = true, Message = "All orders retrieved successfully" });
        }
        private string GenerateTrackingNumber()
        {
            return $"TRK-{Guid.NewGuid().ToString().ToUpper().Substring(0, 8)}";
        }

        [Authorize(Roles = "Admin")]
        [HttpPut("edit/{trackingNumber}")]
        public async Task<IActionResult> EditOrder(string trackingNumber, [FromBody] EditOrderRequestDto editOrderRequestDto)
        {
            var order = await _context.OrderHeaders.FirstOrDefaultAsync(oh => oh.TrackingNumber == trackingNumber);
            if (order == null)
            {
                return NotFound(new ResponseDto { IsSuccess = false, Message = "Order not found." });
            }

            order.ShippingDate = editOrderRequestDto.ShippingDate;
            order.Address = editOrderRequestDto.Address;
            order.PaymentMethod = editOrderRequestDto.PaymentMethod;
            order.PaymentStatus = editOrderRequestDto.PaymentStatus;
            order.OrderStatus = editOrderRequestDto.OrderStatus;

            await _context.SaveChangesAsync();

            return Ok(new ResponseDto { IsSuccess = true, Message = "Order updated successfully" });
        }

    }
}