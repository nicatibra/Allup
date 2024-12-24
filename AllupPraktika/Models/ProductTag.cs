namespace AllupPraktika.Models
{
    public class ProductTag
    {
        public int Id { get; set; }

        public int ProductId { get; set; }

        public int TagId { get; set; }


        //additional
        public Product Product { get; set; }
        public Tag Tag { get; set; }

    }
}
