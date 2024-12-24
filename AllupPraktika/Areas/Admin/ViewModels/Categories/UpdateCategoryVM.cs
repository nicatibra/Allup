using System.ComponentModel.DataAnnotations;

namespace AllupPraktika.Areas.Admin.ViewModels
{
    public class UpdateCategoryVM
    {
        [Required(ErrorMessage = "The field is required!")]
        [MaxLength(30, ErrorMessage = "There can be max 30 symbols!")]
        public string Name { get; set; }

        public string Image { get; set; }

        public IFormFile? Photo { get; set; }

    }
}
