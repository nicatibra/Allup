namespace AllupPraktika.ViewModels
{
    public class GetProductVM
    {
        public int Id { get; set; }
        public string Name { get; set; }
        public string Image { get; set; }
        public string SecondaryImage { get; set; }
        public decimal Price { get; set; }
        public int DiscountPercentage { get; set; }
        public decimal DiscountPrice { get; set; }

        public string Description { get; set; }
    }
}
