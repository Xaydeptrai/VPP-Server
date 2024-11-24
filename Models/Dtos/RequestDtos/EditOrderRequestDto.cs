using vpp_server.Models.Emuns;

namespace vpp_server.Models.Dtos.RequestDtos
{
    public class EditOrderRequestDto
    {
        public DateTime ShippingDate { get; set; }
        public string Address { get; set; }
        public PaymentMethod PaymentMethod { get; set; }
        public PaymentStatus PaymentStatus { get; set; }
        public OrderStatus OrderStatus { get; set; }
    }
}
