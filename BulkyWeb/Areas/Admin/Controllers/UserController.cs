using BulkyBook.DataAccess.Repository.IRepository;
using BulkyBook.Models.ViewModels;
using BulkyBook.Models;
using BulkyBook.Utility;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Rendering;
using BulkyBook.DataAccess.Data;
using Microsoft.EntityFrameworkCore;
using BulkyBook.DataAccess.Repository;
using Microsoft.AspNetCore.Identity;

namespace BulkyBookWeb.Areas.Admin.Controllers
{
    [Area("Admin")]
    [Authorize(Roles = SD.Role_Admin)]
    public class UserController : Controller
    {
        private readonly UserManager<IdentityUser> _userManager;
        private readonly RoleManager<IdentityRole> _roleManager;
        private readonly IUnitOfWork _unitOfWork;

        public UserController(
            IUnitOfWork unitOfWork, 
            UserManager<IdentityUser> userManager,
            RoleManager<IdentityRole> roleManager)
        {
            _unitOfWork = unitOfWork;
            _userManager = userManager;
            _roleManager = roleManager;
        }
        public IActionResult Index()
        {
            return View();
        }

        public IActionResult Edit(string id)
        {

            ApplicationUser ApplicationUser = _unitOfWork.ApplicationUser.Get(u => u.Id == id);
            ApplicationUser.Role = _userManager.GetRolesAsync(ApplicationUser).GetAwaiter().GetResult().ToArray()[0];

            ApplicationUserVM ApplicationUserVM = new()
            {
                ApplicationUser = ApplicationUser,
                Roles = _roleManager.Roles.Select(i => new SelectListItem
                {
                    Text = i.Name,
                    Value = i.Name
                }),
                Companies = _unitOfWork.Company.GetAll().Select(i => new SelectListItem
                {
                    Text = i.Name,
                    Value = i.Id.ToString()
                })
            };

            return View(ApplicationUserVM);
        }

        [HttpPost]
        [ActionName("Edit")]
        public IActionResult Edit_POST(ApplicationUserVM applicationUserVM)
        {
            ApplicationUser ApplicationUser = _unitOfWork.ApplicationUser.Get(u=>u.Id == applicationUserVM.ApplicationUser.Id);

            if (applicationUserVM.ApplicationUser.Role != SD.Role_Company)
            {
                ApplicationUser.CompanyId = null;
            } else
            {
                ApplicationUser.CompanyId = applicationUserVM.ApplicationUser.CompanyId;
            }

            _unitOfWork.ApplicationUser.Update(ApplicationUser);
            _unitOfWork.Save();

            var oldRoles = _userManager.GetRolesAsync(ApplicationUser).GetAwaiter().GetResult();
            _userManager.RemoveFromRolesAsync(ApplicationUser, oldRoles).GetAwaiter().GetResult();
            _userManager.AddToRoleAsync(ApplicationUser, applicationUserVM.ApplicationUser.Role).GetAwaiter().GetResult();
            _userManager.UpdateAsync(ApplicationUser).GetAwaiter().GetResult();

            return RedirectToAction("Index");
        }

        [HttpPost]
        public IActionResult LockUnlock([FromBody] string id)
        {
            var applicationUser = _unitOfWork.ApplicationUser.Get(u => u.Id == id);
            if (applicationUser == null)
            {
                return Json(new {success = false, message = "Error while Locking/Unlocking"});
            }

            if (applicationUser.LockoutEnd != null && applicationUser.LockoutEnd > DateTime.Now)
            {
                applicationUser.LockoutEnd = DateTime.Now;
            } else
            {
                applicationUser.LockoutEnd = DateTime.Now.AddYears(1000);
            }
            _unitOfWork.ApplicationUser.Update(applicationUser);
            _unitOfWork.Save();

            return Json(new { success=true, message="Operation Successful" });
        }

        #region
        [HttpGet]
        public IActionResult GetAll()
        {
            List<ApplicationUser> objUsersList = _unitOfWork.ApplicationUser.GetAll(includeProperties:"Company").ToList();

            foreach(var user in objUsersList)
            {
                var role = _userManager.GetRolesAsync(user).GetAwaiter().GetResult().ToList()[0];
                user.Role = role;

                if (user.Company ==  null)
                {
                    user.Company = new()
                    {
                        Name = ""
                    };
                }
            }

            return Json(new { data = objUsersList });
        }

        [HttpDelete]
        public IActionResult Delete(int? id)
        {

            return Json(new { success = true, message = "Delete successful" });
        }
        #endregion
    }
}
