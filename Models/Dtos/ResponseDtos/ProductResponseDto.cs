namespace vpp_server.Models.Dtos.ResponseDtos
{
    public class ProductResponseDto
    {
        public int Id { get; set; }
        public string Name { get; set; }
        public int Price { get; set; }
        public string Description { get; set; }
        public string ImageUrl1 { get; set; }
        public string ?ImageUrl2 { get; set; }
        public string ?ImageUrl3 { get; set; }
        public string ?ImageUrl4 { get; set; }
        public int Stock { get; set; }
        public int CatalogId { get; set; }
        public bool IsDeleted { get; set; }
        public string CatalogName { get; set; }
        public DateTime CreateDate { get; set; }
        public DateTime UpdateDate { get; set; }
    }
}
