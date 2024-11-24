namespace vpp_server.Models.Dtos.RequestDtos
{
    public class ProductRequestDto
    {
        public string Name { get; set; }
        public int Price { get; set; }
        public string Description { get; set; }
        public string ImageUrl { get; set; }
        public int Stock { get; set; }
        public int CatalogId { get; set; }
        public bool IsDeleted { get; set; }
    }
}
