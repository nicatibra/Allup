using System.ComponentModel.DataAnnotations;

namespace AllupPraktika.ViewModels
{
    public class OrderVM
    {
        [Required]
        public string Address { get; set; }

        public List<BasketInOrderItemVM>? BasketInOrderItemsVMs { get; set; }
    }
}
