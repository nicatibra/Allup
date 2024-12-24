namespace AllupPraktika.Models
{
    public class Product : BaseEntity
    {
        public string Name { get; set; }
        public string Description { get; set; }
        public string ProductCode { get; set; }

        public decimal Price { get; set; }
        public int DiscountPercentage { get; set; }
        public decimal DiscountPrice { get; set; }

        public bool Availablity { get; set; }
        public int Quantity { get; set; }

        //relational
        public List<ProductImage> ProductImages { get; set; }

        //Brand ucun
        public int? BrandId { get; set; }
        public Brand Brand { get; set; }

        //Category
        public Category Category { get; set; }
        public int CategoryId { get; set; }

        //Tag
        public List<ProductTag> ProductTags { get; set; }


    }
}
