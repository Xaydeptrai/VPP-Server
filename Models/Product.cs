namespace vpp_server.Models
{
    public class Product
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
        public bool IsDeleted { get; set; } = false;
        public DateTime CreateDate { get; set; } = DateTime.UtcNow;
        public DateTime UpdateDate { get; set; } = DateTime.UtcNow;
        public Catalog Catalog { get; set; }
    }
}
