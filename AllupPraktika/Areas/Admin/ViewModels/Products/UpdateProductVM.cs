using AllupPraktika.Models;
using System.ComponentModel.DataAnnotations;

namespace AllupPraktika.Areas.Admin.ViewModels
{
    public class UpdateProductVM
    {
        public IFormFile? MainPhoto { get; set; }
        public IFormFile? HoverPhoto { get; set; }
        public List<IFormFile>? AdditionalPhotos { get; set; }

        public string Name { get; set; }
        public string Description { get; set; }
        public string ProductCode { get; set; }


        [Required]
        public decimal? Price { get; set; }
        public int DiscountPercentage { get; set; }

        public List<ProductImage>? ProductImages { get; set; }
        public List<int>? ImageIds { get; set; }


        [Required]
        public int? CategoryId { get; set; }
        public List<Category>? Categories { get; set; }

        public int? BrandId { get; set; }
        public List<Brand>? Brands { get; set; }

        public List<int>? TagIds { get; set; }
        public List<Tag>? Tags { get; set; }



    }
}
