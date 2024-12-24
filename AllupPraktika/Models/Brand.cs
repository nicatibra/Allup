namespace AllupPraktika.Models
{
    public class Brand : BaseEntity
    {
        public string Name { get; set; }

        //relational

        public List<Product>? Products { get; set; }
    }
}
