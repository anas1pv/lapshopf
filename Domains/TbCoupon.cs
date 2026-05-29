using System;
using System.ComponentModel.DataAnnotations;

namespace lapshop.Domains
{
    public class TbCoupon
    {
        [Key]
        public int CouponId { get; set; }

        [Required(ErrorMessage = "Please enter a coupon code.")]
        [MaxLength(50)]
        [Display(Name = "Coupon Code")]
        public string CouponCode { get; set; } = string.Empty;

        [Required(ErrorMessage = "Please enter discount percentage.")]
        [Range(1, 100, ErrorMessage = "Discount must be between 1% and 100%.")]
        [Display(Name = "Discount Percent (%)")]
        public decimal DiscountPercent { get; set; }

        [Required(ErrorMessage = "Please enter an expiry date.")]
        [Display(Name = "Expiry Date")]
        public DateTime ExpiryDate { get; set; } = DateTime.Today.AddDays(30);

        [Required]
        [Display(Name = "Active status")]
        public bool IsActive { get; set; } = true;
    }
}
