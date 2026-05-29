using lapshop.Bl;
using lapshop.Domains;
using lapshop.Models;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using Newtonsoft.Json;

namespace lapshop.Controllers
{
    public class OrderController : Controller
    {
        private IItems itemService;
        private UserManager<ApplicationUser> _userManager;
        private ISalesInvoice salesInvoiceService;
        private readonly LapShopContext _context;
        private readonly IEmailSender _emailSender;

        public OrderController(IItems itemservice, UserManager<ApplicationUser> userManager, ISalesInvoice ssalesInvoiceService, LapShopContext context, IEmailSender emailSender)
        {
            itemService = itemservice;
            _userManager = userManager;
            salesInvoiceService = ssalesInvoiceService;
            _context = context;
            _emailSender = emailSender;
        }

        public IActionResult Wishlist()
        {
            return View();
        }

        public IActionResult Cart()
        {
            //-----------------------------------------------------------------------------------
            //string sesstionCart = string.Empty;
            //if (HttpContext.Request.Cookies["Cart"] != null)
            //    sesstionCart = HttpContext.Request.Cookies["Cart"];
            //var cart = JsonConvert.DeserializeObject<ShoppingCart>(sesstionCart);
            //return View(cart);
            //-----------------------------------------------------------------------------------
            //string sessionCart = string.Empty;
            string cookies = HttpContext.Request.Cookies["Cart"];
            var cart = new ShoppingCart(); // بنبدأ بسلة فاضية دايماً

            if (!string.IsNullOrEmpty(cookies))
            {
                cart = JsonConvert.DeserializeObject<ShoppingCart>(cookies);
            }
            //if (HttpContext.Session.GetString("Cart") != null && HttpContext.Session.GetString("Cart") != "")
            //    sessionCart = HttpContext.Session.GetString("Cart");
            //var cart = JsonConvert.DeserializeObject<ShoppingCart>(CookiesCart);
            return View(cart);
        }

        [Authorize]
        public async Task<IActionResult> MyOrders()
        {
            var user = await _userManager.GetUserAsync(User);
            if (user == null) return RedirectToAction("Login", "Users");
            
            var invoices = salesInvoiceService.GetAll()
                            .Where(a => a.CustomerId == Guid.Parse(user.Id))
                            .OrderByDescending(a => a.InvoiceDate)
                            .ToList();
            return View(invoices);
        }

        [Authorize]
        public async Task<IActionResult> OrderDetails(int id)
        {
            var user = await _userManager.GetUserAsync(User);
            if (user == null) return RedirectToAction("Login", "Users");

            var invoice = salesInvoiceService.GetAll()
                            .FirstOrDefault(a => a.InvoiceId == id && a.CustomerId == Guid.Parse(user.Id));
            
            if (invoice == null)
            {
                return RedirectToAction("MyOrders");
            }

            var itemsService = HttpContext.RequestServices.GetRequiredService<ISalesInvoiceItems>();
            var items = itemsService.GetSalesInvoiceId(id);

            ViewBag.Invoice = invoice;
            return View(items);
        }

        [Authorize]
        public async Task<IActionResult> Checkout()
        {
            string cookies = HttpContext.Request.Cookies["Cart"];
            if (string.IsNullOrEmpty(cookies))
            {
                return RedirectToAction("Cart");
            }

            var cart = JsonConvert.DeserializeObject<ShoppingCart>(cookies);
            if (cart == null || cart.lstItems.Count == 0)
            {
                return RedirectToAction("Cart");
            }

            var user = await _userManager.GetUserAsync(User);
            ViewBag.UserFirstName = user?.FirstName ?? "";
            ViewBag.UserLastName = user?.LastName ?? "";
            ViewBag.UserEmail = user?.Email ?? "";
            ViewBag.UserPhone = user?.PhoneNumber ?? "";

            return View(cart);
        }

        [HttpPost]
        public IActionResult ValidateCoupon(string code)
        {
            if (string.IsNullOrWhiteSpace(code))
                return Json(new { valid = false, message = "Please enter a coupon code." });

            var cleanedCode = code.Trim().ToUpper();
            var coupon = _context.TbCoupons
                .FirstOrDefault(c => c.CouponCode == cleanedCode && c.IsActive && c.ExpiryDate >= DateTime.Today);

            if (coupon != null)
            {
                return Json(new { valid = true, discount = coupon.DiscountPercent, message = $"{coupon.DiscountPercent.ToString("0")}% discount applied!" });
            }

            return Json(new { valid = false, message = "Invalid or expired coupon code." });
        }

        [HttpGet]
        [Authorize]
        public async Task<IActionResult> CheckOrderUpdates(long lastCheck)
        {
            var user = await _userManager.GetUserAsync(User);
            if (user == null)
                return Json(new { updates = new object[] { } });

            var lastCheckDate = DateTimeOffset.FromUnixTimeMilliseconds(lastCheck).UtcDateTime;
            var userId = Guid.Parse(user.Id);

            var updatedOrders = _context.TbSalesInvoices
                .Where(i => i.CustomerId == userId && i.UpdatedDate != null && i.UpdatedDate > lastCheckDate)
                .Select(i => new
                {
                    orderId = i.InvoiceId,
                    status = i.CurrentState == 2 ? "Shipped" :
                             i.CurrentState == 3 ? "Delivered" :
                             i.CurrentState == 4 ? "Canceled" : "Pending",
                    updatedAt = i.UpdatedDate.Value.ToString("yyyy-MM-ddTHH:mm:ss")
                })
                .ToList();

            return Json(new { updates = updatedOrders });
        }

        [HttpPost]
        [Authorize]
        public async Task<IActionResult> PlaceOrder(string address, string phone, string notes, string promoCode)
        {
            string cookies = HttpContext.Request.Cookies["Cart"];
            if (string.IsNullOrEmpty(cookies))
            {
                return RedirectToAction("Cart");
            }

            var cart = JsonConvert.DeserializeObject<ShoppingCart>(cookies);
            if (cart == null || cart.lstItems.Count == 0)
            {
                return RedirectToAction("Cart");
            }

            var user = await _userManager.GetUserAsync(User);

            // Validate coupon and calculate discount
            decimal discountPercent = 0;
            if (!string.IsNullOrWhiteSpace(promoCode))
            {
                var cleanedCode = promoCode.Trim().ToUpper();
                var coupon = _context.TbCoupons
                    .FirstOrDefault(c => c.CouponCode == cleanedCode && c.IsActive && c.ExpiryDate >= DateTime.Today);
                if (coupon != null)
                {
                    discountPercent = coupon.DiscountPercent;
                }
            }
            
            // Format notes to include address, phone, coupon, and notes
            string shippingNotes = $"Address: {address} | Phone: {phone}";
            if (discountPercent > 0)
            {
                shippingNotes += $" | Coupon: {promoCode.Trim().ToUpper()} ({discountPercent}% off)";
            }
            if (!string.IsNullOrEmpty(notes))
            {
                shippingNotes += $" | Notes: {notes}";
            }

            List<TbSalesInvoiceItem> lstInvoiceItems = new List<TbSalesInvoiceItem>();
            foreach (var item in cart.lstItems)
            {
                // Apply discount to each item price
                decimal finalPrice = discountPercent > 0
                    ? item.Price * (1 - discountPercent / 100m)
                    : item.Price;

                lstInvoiceItems.Add(new TbSalesInvoiceItem()
                {
                    ItemId = item.ItemId,
                    Qty = item.Qty,
                    InvoicePrice = Math.Round(finalPrice, 2)
                });
            }

            TbSalesInvoice oSalesInvoice = new TbSalesInvoice()
            {
                InvoiceDate = DateTime.Now,
                CustomerId = Guid.Parse(user.Id),
                DelivryDate = DateTime.Now.AddDays(5),
                Notes = shippingNotes,
                CreatedBy = user.Id,
                CreatedDate = DateTime.Now
            };

            salesInvoiceService.Save(oSalesInvoice, lstInvoiceItems, true);

            try
            {
                var itemsListHtml = string.Join("", cart.lstItems.Select(item => {
                    decimal itemFinalPrice = discountPercent > 0 ? item.Price * (1 - discountPercent / 100m) : item.Price;
                    decimal itemTotal = itemFinalPrice * item.Qty;
                    return $@"
                        <tr style='border-bottom: 1px solid rgba(255,255,255,0.06);'>
                            <td style='padding: 12px; color: #ffffff;'>{item.ItemName}</td>
                            <td style='padding: 12px; color: #86868b; text-align: center;'>{item.Qty}</td>
                            <td style='padding: 12px; color: #00f3ff; text-align: right;'>${itemFinalPrice:N2}</td>
                            <td style='padding: 12px; color: #00f3ff; text-align: right;'>${itemTotal:N2}</td>
                        </tr>";
                }));

                decimal cartTotal = cart.lstItems.Sum(item => (discountPercent > 0 ? item.Price * (1 - discountPercent / 100m) : item.Price) * item.Qty);

                string emailBody = $@"
                    <div style='font-family: ""Outfit"", ""Inter"", Arial, sans-serif; background: #0a0a0c; color: #ffffff; padding: 40px 20px; border-radius: 12px; max-width: 600px; margin: 0 auto; border: 1px solid rgba(255,255,255,0.06);'>
                        <div style='text-align: center; margin-bottom: 30px;'>
                            <h1 style='color: #00f3ff; font-size: 28px; font-weight: 800; letter-spacing: 2px; margin: 0;'>LAPSHOP</h1>
                            <p style='color: #86868b; font-size: 12px; text-transform: uppercase; letter-spacing: 1px; margin: 5px 0 0 0;'>Order Confirmed</p>
                        </div>
                        
                        <p style='font-size: 16px; color: #f5f5f7;'>Dear {user.FirstName} {user.LastName},</p>
                        <p style='color: #86868b; font-size: 14px; line-height: 1.6;'>Thank you for your order! We are preparing it for shipment. Here are the details of your order:</p>
                        
                        <div style='background: rgba(255,255,255,0.02); border: 1px solid rgba(255,255,255,0.04); border-radius: 8px; padding: 20px; margin: 25px 0;'>
                            <p style='margin: 0 0 10px 0; font-size: 14px; color: #86868b;'>Order ID: <strong style='color: #ffffff;'>#{oSalesInvoice.InvoiceId}</strong></p>
                            <p style='margin: 0 0 10px 0; font-size: 14px; color: #86868b;'>Order Date: <strong style='color: #ffffff;'>{oSalesInvoice.InvoiceDate:yyyy-MM-dd HH:mm}</strong></p>
                            <p style='margin: 0 0 10px 0; font-size: 14px; color: #86868b;'>Delivery Estimate: <strong style='color: #ffffff;'>{oSalesInvoice.DelivryDate:yyyy-MM-dd}</strong></p>
                            <p style='margin: 0; font-size: 14px; color: #86868b;'>Shipping Info: <strong style='color: #ffffff;'>{shippingNotes}</strong></p>
                        </div>

                        <h3 style='color: #ffffff; border-bottom: 1px solid rgba(255,255,255,0.1); padding-bottom: 8px; font-size: 16px;'>Items Ordered</h3>
                        <table style='width: 100%; border-collapse: collapse; font-size: 13px;'>
                            <thead>
                                <tr style='border-bottom: 1px solid rgba(255,255,255,0.1);'>
                                    <th style='padding: 8px 12px; text-align: left; color: #86868b;'>Product</th>
                                    <th style='padding: 8px 12px; text-align: center; color: #86868b; width: 50px;'>Qty</th>
                                    <th style='padding: 8px 12px; text-align: right; color: #86868b; width: 80px;'>Price</th>
                                    <th style='padding: 8px 12px; text-align: right; color: #86868b; width: 80px;'>Total</th>
                                </tr>
                            </thead>
                            <tbody>
                                {itemsListHtml}
                            </tbody>
                            <tfoot>
                                <tr>
                                    <td colspan='3' style='padding: 15px 12px 5px 12px; font-weight: bold; color: #ffffff; text-align: right;'>Total:</td>
                                    <td style='padding: 15px 12px 5px 12px; font-weight: bold; color: #00f3ff; text-align: right; font-size: 15px;'>${cartTotal:N2}</td>
                                </tr>
                            </tfoot>
                        </table>

                        <div style='margin-top: 40px; text-align: center; border-top: 1px solid rgba(255,255,255,0.06); padding-top: 25px;'>
                            <p style='color: #86868b; font-size: 12px; margin: 0;'>If you have any questions, please contact us from the storefront or reply to this email.</p>
                            <p style='color: #00f3ff; font-size: 12px; font-weight: bold; margin: 5px 0 0 0;'>Thank you for choosing LapShop!</p>
                        </div>
                    </div>";

                await _emailSender.SendEmailAsync(user.Email, $"Order Confirmation #{oSalesInvoice.InvoiceId} - LapShop", emailBody);
            }
            catch (Exception ex)
            {
                System.Diagnostics.Debug.WriteLine($"Failed to send order confirmation email: {ex.Message}");
            }

            // Clear the cart cookie after saving order
            HttpContext.Response.Cookies.Delete("Cart");

            return RedirectToAction("OrderSuccess");
        }

        [Authorize]
        public IActionResult OrderSuccess()
        {
            return View();
        }

        public IActionResult AddToCart(int itemId)
        {
            //    ShoppingCart cart;

            //    if (HttpContext.Request.Cookies["Cart"] != null)
            //        cart = JsonConvert.DeserializeObject<ShoppingCart>(HttpContext.Request.Cookies["Cart"]);
            //    else
            //        cart = new ShoppingCart();

            //    var item = itemService.GetById(itemId);

            //    var itemInList = cart.lstItems.Where(a => a.ItemId == itemId).FirstOrDefault();

            //    if (itemInList != null)
            //    {
            //        itemInList.Qty++;
            //        itemInList.Total = itemInList.Qty * itemInList.Price;
            //    }
            //    else
            //    {
            //        cart.lstItems.Add(new ShoppingCartItem
            //        {
            //            ItemId = item.ItemId,
            //            ItemName = item.ItemName,
            //            Price = item.SalesPrice,
            //            Qty = 1,
            //            Total = item.SalesPrice
            //        });
            //    }
            //    cart.Total = cart.lstItems.Sum(a => a.Total);

            //    HttpContext.Response.Cookies.Append("Cart", JsonConvert.SerializeObject(cart));

            //    return RedirectToAction("Cart");
            //-----------------------------------------------------------------------------------
            ShoppingCart cart;
            String Cookies = string.Empty;
            Cookies = HttpContext.Request.Cookies["Cart"];
            if (Cookies != null)
                cart = JsonConvert.DeserializeObject<ShoppingCart>(Cookies);
            else
                cart = new ShoppingCart();
            
            var item = itemService.GetById(itemId);
            var itemInList = cart.lstItems.Where(a => a.ItemId == itemId).FirstOrDefault();

            if (itemInList != null)
            {
                itemInList.Qty++;
                itemInList.Total = itemInList.Qty * itemInList.Price;
            }
            else
            {
                var price = item.DiscountPrice.HasValue && item.DiscountPrice.Value > 0 ? item.DiscountPrice.Value : item.SalesPrice;
                cart.lstItems.Add(new ShoppingCartItem
                {
                    ItemId = item.ItemId,
                    ItemName = item.ItemName,
                    Price = price,
                    ImageName = item.ImageName, // Populated ImageName
                    Qty = 1,
                    Total = price
                });
            }
            cart.Total = cart.lstItems.Sum(a => a.Total);

            var josonconvertCookies = JsonConvert.SerializeObject(cart);
            CookieOptions options = new CookieOptions
            {
                Expires = DateTime.Now.AddDays(7),
                HttpOnly = true,
                Secure = true
            };
            HttpContext.Response.Cookies.Append("Cart", josonconvertCookies, options);
            return RedirectToAction("Cart");
        }

        [HttpGet]
        public IActionResult AddToCartAjax(int itemId)
        {
            ShoppingCart cart;
            string cookies = HttpContext.Request.Cookies["Cart"];
            if (cookies != null)
                cart = JsonConvert.DeserializeObject<ShoppingCart>(cookies);
            else
                cart = new ShoppingCart();
            
            var item = itemService.GetById(itemId);
            var itemInList = cart.lstItems.Where(a => a.ItemId == itemId).FirstOrDefault();

            if (itemInList != null)
            {
                itemInList.Qty++;
                itemInList.Total = itemInList.Qty * itemInList.Price;
            }
            else
            {
                var price = item.DiscountPrice.HasValue && item.DiscountPrice.Value > 0 ? item.DiscountPrice.Value : item.SalesPrice;
                cart.lstItems.Add(new ShoppingCartItem
                {
                    ItemId = item.ItemId,
                    ItemName = item.ItemName,
                    Price = price,
                    ImageName = item.ImageName,
                    Qty = 1,
                    Total = price
                });
            }
            cart.Total = cart.lstItems.Sum(a => a.Total);

            var jsonCookies = JsonConvert.SerializeObject(cart);
            CookieOptions options = new CookieOptions
            {
                Expires = DateTime.Now.AddDays(7),
                HttpOnly = true,
                Secure = true
            };
            HttpContext.Response.Cookies.Append("Cart", jsonCookies, options);

            int totalQty = cart.lstItems.Sum(x => (int)x.Qty);

            return Json(new { success = true, cartCount = totalQty });
        }

        public IActionResult RemoveFromCart(int itemId)
        {
            ShoppingCart cart;
            string cookies = HttpContext.Request.Cookies["Cart"];
            if (cookies != null)
            {
                cart = JsonConvert.DeserializeObject<ShoppingCart>(cookies);
                var itemToRemove = cart.lstItems.FirstOrDefault(a => a.ItemId == itemId);
                if (itemToRemove != null)
                {
                    cart.lstItems.Remove(itemToRemove);
                    cart.Total = cart.lstItems.Sum(a => a.Total);
                    CookieOptions options = new CookieOptions
                    {
                        Expires = DateTime.Now.AddDays(7),
                        HttpOnly = true,
                        Secure = true
                    };
                    HttpContext.Response.Cookies.Append("Cart", JsonConvert.SerializeObject(cart), options);
                }
            }
            return RedirectToAction("Cart");
        }

        [HttpPost]
        public IActionResult UpdateQuantity(int itemId, int qty)
        {
            ShoppingCart cart;
            string cookies = HttpContext.Request.Cookies["Cart"];
            if (cookies != null)
            {
                cart = JsonConvert.DeserializeObject<ShoppingCart>(cookies);
                var item = cart.lstItems.FirstOrDefault(a => a.ItemId == itemId);
                if (item != null)
                {
                    item.Qty = qty;
                    item.Total = qty * item.Price;
                    cart.Total = cart.lstItems.Sum(a => a.Total);

                    var jsonCookies = JsonConvert.SerializeObject(cart);
                    CookieOptions options = new CookieOptions
                    {
                        Expires = DateTime.Now.AddDays(7),
                        HttpOnly = true,
                        Secure = true
                    };
                    HttpContext.Response.Cookies.Append("Cart", jsonCookies, options);
                }
            }
            return Ok();
        }

        private async Task SaveOrder(ShoppingCart oShopingCart)
        {
            try
            {
                List<TbSalesInvoiceItem> lstInvoiceItems = new List<TbSalesInvoiceItem>();
                foreach (var item in oShopingCart.lstItems)
                {
                    lstInvoiceItems.Add(new TbSalesInvoiceItem()
                    {
                        ItemId = item.ItemId,
                        Qty = item.Qty,
                        InvoicePrice = item.Price
                    });
                }
                var user = await _userManager.GetUserAsync(User);
                TbSalesInvoice oSalesInvoice = new TbSalesInvoice()
                {
                    InvoiceDate = DateTime.Now,
                    CustomerId = Guid.Parse(user.Id),
                    DelivryDate = DateTime.Now.AddDays(5),
                    CreatedBy = user.Id,
                    CreatedDate = DateTime.Now
                };

                salesInvoiceService.Save(oSalesInvoice, lstInvoiceItems, true);
            }
            catch (Exception)
            {

            }
        }
    }
}
