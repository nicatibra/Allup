namespace AllupPraktika.Models
{
    public class Tag : BaseEntity
    {
        public string Name { get; set; }

        //additional
        public List<ProductTag> ProductTags { get; set; }
    }
}
