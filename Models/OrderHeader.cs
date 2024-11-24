using vpp_server.Models.Emuns;

namespace vpp_server.Models
{
    public class OrderHeader
    {
        public int Id { get; set; }
        public string TrackingNumber { get; set; }
        public string UserId { get; set; }
        public ApplicationUser User { get; set; }
        public string Address { get; set; }
        public DateTime OrderDate { get; set; }
        public DateTime ShippingDate { get; set; }
        public OrderStatus OrderStatus { get; set; }
        public PaymentMethod PaymentMethod { get; set; }
        public PaymentStatus PaymentStatus { get; set; }
        public int OrderTotal { get; set; }
        public ICollection<OrderDetail> OrderDetails { get; set; }
    }
}
