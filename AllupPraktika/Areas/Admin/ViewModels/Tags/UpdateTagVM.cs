using System.ComponentModel.DataAnnotations;

namespace AllupPraktika.Areas.Admin.ViewModels
{
    public class UpdateTagVM
    {
        [Required(ErrorMessage = "The field is required!")]
        [MaxLength(30, ErrorMessage = "There can be max 30 symbols!")]
        public string Name { get; set; }
    }
}
