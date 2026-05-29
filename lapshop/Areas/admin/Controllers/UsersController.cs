using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using lapshop.Bl;
using System.Linq;
using System.Threading.Tasks;
using System.Collections.Generic;

namespace lapshop.Areas.admin.Controllers
{
    [Area("admin")]
    [Authorize(Roles = "Admin")]
    public class UsersController : Controller
    {
        private readonly UserManager<ApplicationUser> _userManager;
        private readonly RoleManager<IdentityRole> _roleManager;

        public UsersController(UserManager<ApplicationUser> userManager, RoleManager<IdentityRole> roleManager)
        {
            _userManager = userManager;
            _roleManager = roleManager;
        }

        public async Task<IActionResult> List(string search)
        {
            var users = _userManager.Users.AsQueryable();

            if (!string.IsNullOrWhiteSpace(search))
            {
                search = search.Trim().ToLower();
                users = users.Where(u => u.Email.ToLower().Contains(search) 
                                      || u.FirstName.ToLower().Contains(search)
                                      || u.LastName.ToLower().Contains(search));
            }

            var userList = users.ToList();
            var userViewModels = new List<UserViewModel>();
            var allRoles = _roleManager.Roles.Select(r => r.Name).ToList();

            foreach (var user in userList)
            {
                var roles = await _userManager.GetRolesAsync(user);
                userViewModels.Add(new UserViewModel
                {
                    Id = user.Id,
                    Email = user.Email,
                    FirstName = user.FirstName,
                    LastName = user.LastName,
                    Roles = roles
                });
            }

            ViewBag.Search = search;
            ViewBag.AllRoles = allRoles;
            return View(userViewModels);
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> UpdateRoles(string userId, List<string> roles)
        {
            var user = await _userManager.FindByIdAsync(userId);
            if (user == null) return NotFound();

            // Don't allow editing the root admin
            if (user.Email == "admin@lapshop.com")
            {
                TempData["Error"] = "Cannot modify the root admin account.";
                return RedirectToAction(nameof(List));
            }

            var currentRoles = await _userManager.GetRolesAsync(user);

            // Remove all current roles
            await _userManager.RemoveFromRolesAsync(user, currentRoles);

            // Add selected roles (ensure at least Customer)
            if (roles == null || roles.Count == 0)
            {
                roles = new List<string> { "Customer" };
            }

            foreach (var role in roles)
            {
                // Ensure role exists before adding
                if (await _roleManager.RoleExistsAsync(role))
                {
                    await _userManager.AddToRoleAsync(user, role);
                }
            }

            // Refresh security stamp to force re-login
            await _userManager.UpdateSecurityStampAsync(user);

            TempData["Success"] = $"Roles updated for {user.Email} successfully.";
            return RedirectToAction(nameof(List));
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> DeleteUser(string userId)
        {
            var user = await _userManager.FindByIdAsync(userId);
            if (user == null) return NotFound();

            if (user.Email == "admin@lapshop.com")
            {
                TempData["Error"] = "Cannot delete the root admin account.";
                return RedirectToAction(nameof(List));
            }

            var result = await _userManager.DeleteAsync(user);
            if (result.Succeeded)
            {
                TempData["Success"] = $"User {user.Email} has been deleted.";
            }
            else
            {
                TempData["Error"] = "Failed to delete user.";
            }

            return RedirectToAction(nameof(List));
        }
    }

    public class UserViewModel
    {
        public string Id { get; set; }
        public string Email { get; set; }
        public string FirstName { get; set; }
        public string LastName { get; set; }
        public IList<string> Roles { get; set; }
    }
}
