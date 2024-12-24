namespace AllupPraktika.Areas.Admin.ViewModels
{
    public class GetProductAdminVM
    {
        public int Id { get; set; }
        public string Image { get; set; }
        public string Name { get; set; }
        public decimal Price { get; set; }
        public decimal DiscountPercentage { get; set; }
        public decimal DiscountPrice { get; set; }


        public string CategoryName { get; set; }
    }
}
