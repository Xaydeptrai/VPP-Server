namespace vpp_server.Models.Dtos.RequestDtos
{
    public class UpdateProductRequestDto
    {
        public string? Name { get; set; }
        public decimal? Price { get; set; }
        public string? Description { get; set; }
        public string? ImageUrl1 { get; set; }
        public string? ImageUrl2 { get; set; }
        public string? ImageUrl3 { get; set; }
        public string? ImageUrl4 { get; set; }
        public int? Stock { get; set; }
        public bool? IsDeleted { get; set; }
        public int? CatalogId { get; set; }
    }
}
