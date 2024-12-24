namespace AllupPraktika.Models
{
    public class Category : BaseEntity
    {
        public string Name { get; set; }
        public string Image { get; set; }

        //relational
        public List<Product> Products { get; set; }
    }
}
