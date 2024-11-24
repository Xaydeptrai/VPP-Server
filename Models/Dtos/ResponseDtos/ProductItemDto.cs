namespace vpp_server.Models.Dtos.ResponseDtos
{
    public class ProductItemDto
    {
        public int Id { get; set; }
        public string Name { get; set; }
        public string ImageUrl { get; set; }
        public int Quantity { get; set; }
        public int Price { get; set; }
    }
}
