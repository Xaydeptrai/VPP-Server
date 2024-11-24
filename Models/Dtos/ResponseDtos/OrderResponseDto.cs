using vpp_server.Models.Emuns;

namespace vpp_server.Models.Dtos.ResponseDtos
{
    public class OrderResponseDto
    {
        public int Id { get; set; }
        public string Name { get; set; }
        public DateTime OrderDate { get; set; }
        public DateTime ShippingDate { get; set; }
        public string Address { get; set; }
        public OrderStatus OrderStatus { get; set; }
        public PaymentMethod PaymentMethod { get; set; }
        public PaymentStatus PaymentStatus { get; set; }
        public int OrderTotal { get; set; }
        public List<ProductItemDto> OrderDetails { get; set; }
    }
}
