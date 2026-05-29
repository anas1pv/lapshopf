using lapshop.Bl;
using lapshop.Domains;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using System;
using System.Linq;
using System.Threading.Tasks;

namespace lapshop.Areas.admin.Controllers
{
    [Area("admin")]
    [Authorize(Roles = "Admin")]
    public class CouponsController : Controller
    {
        private readonly LapShopContext _context;

        public CouponsController(LapShopContext context)
        {
            _context = context;
        }

        public async Task<IActionResult> List()
        {
            var coupons = await _context.TbCoupons
                .OrderByDescending(c => c.CouponId)
                .ToListAsync();
            return View(coupons);
        }

        public async Task<IActionResult> Edit(int? id)
        {
            var coupon = new TbCoupon();
            if (id != null)
            {
                var dbCoupon = await _context.TbCoupons.FindAsync(id);
                if (dbCoupon == null)
                {
                    return NotFound();
                }
                coupon = dbCoupon;
            }
            return View(coupon);
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Save(TbCoupon coupon)
        {
            if (!ModelState.IsValid)
            {
                return View("Edit", coupon);
            }

            // Check if coupon code is unique
            var codeExists = await _context.TbCoupons
                .AnyAsync(c => c.CouponCode.ToLower() == coupon.CouponCode.Trim().ToLower() && c.CouponId != coupon.CouponId);
            
            if (codeExists)
            {
                ModelState.AddModelError("CouponCode", "This coupon code already exists.");
                return View("Edit", coupon);
            }

            coupon.CouponCode = coupon.CouponCode.Trim().ToUpper();

            if (coupon.CouponId == 0)
            {
                _context.TbCoupons.Add(coupon);
                TempData["SuccessMessage"] = "Coupon created successfully!";
            }
            else
            {
                _context.Entry(coupon).State = EntityState.Modified;
                TempData["SuccessMessage"] = "Coupon updated successfully!";
            }

            await _context.SaveChangesAsync();
            return RedirectToAction(nameof(List));
        }

        public async Task<IActionResult> Delete(int id)
        {
            var coupon = await _context.TbCoupons.FindAsync(id);
            if (coupon != null)
            {
                _context.TbCoupons.Remove(coupon);
                await _context.SaveChangesAsync();
                TempData["SuccessMessage"] = "Coupon deleted successfully!";
            }
            return RedirectToAction(nameof(List));
        }
    }
}
