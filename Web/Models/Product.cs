using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace Web.Models
{
    public class Product
    {
        [Key]
        public int ProductId { get; set; }

        [Required(ErrorMessage = "Tên sản phẩm là bắt buộc")]
        [StringLength(150, ErrorMessage = "Tên sản phẩm không được vượt quá 150 ký tự")]
        public string? ProductName { get; set; }

        [ForeignKey("Category")] // Chỉ định tên thuộc tính khóa ngoại ở đây
        public int CategoryId { get; set; }

        public Category? Category { get; set; }

        public string? ProductDescription { get; set; }

        [Column(TypeName = "decimal(8,2)")]
        public decimal? ProductPrice { get; set; }

        [Column(TypeName = "decimal(2,2)")]
        public decimal? ProductDiscount { get; set; } // Tên thuộc tính đã sửa

        [StringLength(300)]
        public string? ProductPhoto { get; set; }

        [ForeignKey("Size")]
        public int SizeId { get; set; }

        public Size? Size { get; set; }

        [ForeignKey("Color")]
        public int ColorId { get; set; }

        public Color? Color { get; set; }

        public bool IsTrendy { get; set; } // Tên thuộc tính đã sửa

        public bool IsArrived { get; set; }
    }
}
