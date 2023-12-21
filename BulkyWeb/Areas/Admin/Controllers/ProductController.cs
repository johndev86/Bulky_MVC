using BulkyBook.DataAccess.Repository.IRepository;
using BulkyBook.Models;
using BulkyBook.Models.ViewModels;
using BulkyBook.Utility;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Rendering;

namespace BulkyBookWeb.Areas.Admin.Controllers
{
    [Area("Admin")]
    [Authorize(Roles = SD.Role_Admin)]
    public class ProductController : Controller
    {
        private readonly IWebHostEnvironment _webHostEnvironment;
        private readonly IUnitOfWork _unitOfWork;
        public ProductController(IUnitOfWork unitOfWork, IWebHostEnvironment webHostEnvironment)
        {
            _unitOfWork = unitOfWork;
            _webHostEnvironment = webHostEnvironment;
        }
        public IActionResult Index()
        {
            List<Product> objProductsList = _unitOfWork.Product.GetAll(includeProperties: "Category").ToList();

            return View(objProductsList);
        }

        public IActionResult Upsert(int? id)
        {

            ProductVM productVM = new()
            {
                CategoryList = _unitOfWork.Category.GetAll().Select(i => new SelectListItem(i.Name, i.Id.ToString())),
                Product = new Product()
            };

            if (id == null || id  == 0)
            {
                return View(productVM);
            }
            else
            {
                productVM.Product = _unitOfWork.Product.Get(u => u.Id == id, "ProductImages");
            }

            return View(productVM);
        }

        [HttpPost]
        public IActionResult Upsert(ProductVM productVM, List<IFormFile> files)
        {

            if (productVM.Product.Id == 0)
            {
                _unitOfWork.Product.Add(productVM.Product);
                TempData["success"] = "Product created successfully";
            }
            else
            {
                _unitOfWork.Product.Update(productVM.Product);
                TempData["success"] = "Product updated successfully";
            }

            _unitOfWork.Save();

            if (ModelState.IsValid)
            {
                string wwwRootPath = _webHostEnvironment.WebRootPath;
                if (files != null)
                {
                    foreach(IFormFile file in files)
                    {
                        string fileName = Guid.NewGuid().ToString() + Path.GetExtension(file.FileName);
                        string productPath = @"images\products\product-" + productVM.Product.Id;
                        string finalPath = Path.Combine(wwwRootPath, productPath);
                        
                        if (!Directory.Exists(finalPath))
                            Directory.CreateDirectory(finalPath);

                        using (var fileStream = new FileStream(Path.Combine(finalPath, fileName), FileMode.Create))
                        {
                            file.CopyTo(fileStream);
                        }

                        ProductImage productImage = new()
                        {
                            ImageUrl = @"\" + productPath + @"\" + fileName,
                            ProductId = productVM.Product.Id
                        };

                        if(productVM.Product.ProductImages == null)
                            productVM.Product.ProductImages = new List<ProductImage>();

                        productVM.Product.ProductImages.Add(productImage);

                    }

                    _unitOfWork.Product.Update(productVM.Product);
                    _unitOfWork.Save();

                }

                return RedirectToAction("Index");
            } else
            {
                productVM.CategoryList = _unitOfWork.Category.GetAll().Select(i => new SelectListItem(i.Name, i.Id.ToString()));

                return View(productVM);
            }
        }

        public IActionResult DeleteImage(int imageId)
        {
            var productImage = _unitOfWork.ProductImage.Get(u=>u.Id == imageId);
            if (productImage != null)
            {
                if (!string.IsNullOrEmpty(productImage.ImageUrl))
                {
                    var oldImagePath = Path.Combine(_webHostEnvironment.WebRootPath, productImage.ImageUrl.TrimStart('\\'));

                    if (System.IO.File.Exists(oldImagePath))
                    {
                        System.IO.File.Delete(oldImagePath);
                    }
                }

                _unitOfWork.ProductImage.Remove(productImage);
                _unitOfWork.Save();

                TempData["success"] = "Deleted Successfully";
            }

            return RedirectToAction("Upsert", new { id = productImage.ProductId });
        }

        #region
        [HttpGet]
        public IActionResult GetAll()
        {
            List<Product> objProductList = _unitOfWork.Product.GetAll(includeProperties:"Category").ToList();
            return Json(new { data = objProductList });
        }

        [HttpDelete]
        public IActionResult Delete(int? id)
        {
            Product delete = _unitOfWork.Product.Get(u => u.Id == id);
            if (delete == null)
            {
                return Json(new { success = false, message = "Error while deleting" });
            }


            string productPath = @"images\products\product-" + id;
            string finalPath = Path.Combine(_webHostEnvironment.WebRootPath, productPath);

            if (Directory.Exists(finalPath))
            {
                string[] filePaths = Directory.GetFiles(finalPath);
                foreach(string filePath in filePaths)
                {
                    System.IO.File.Delete($"{filePath}");
                }
                Directory.Delete(finalPath);
            }

            _unitOfWork.Product.Remove(delete);
            _unitOfWork.Save();

            return Json(new { success = true, message = "Delete successful" });
        }
        #endregion
    }
}
