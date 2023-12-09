using Dal.Repository.IRepository;
using Microsoft.AspNetCore.Mvc;
using Models;
using Models.ViewModels;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace Web.Controllers
{
    public class ProductController : Controller
    {
        private readonly IProductRepository _prodRepo;
        public ProductController(IProductRepository prodRepo)
        {
            _prodRepo = prodRepo;
        }
        public IActionResult Index()
        {
            return View();
        }
        public async Task<IActionResult> Upsert(int? id)
        {

            ProductVM productVM = new()
            {
                Product = new Product(),
                CategorySelectList = _prodRepo.GetAllDropDownList("Category")
            };
            if (id == null)
            {
                // this is For create
                return View(productVM);
            }
            else
            {
                productVM.Product = await _prodRepo.Find(id.GetValueOrDefault());
                if (productVM.Product == null)
                {
                    return NotFound();
                }
                return View(productVM);
            }
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Upsert(ProductVM productVM)
        {
            if (ModelState.IsValid)
            {
                if (productVM.Product.Id == 0)
                {
                    // create
                    await _prodRepo.Add(productVM.Product);

                }
                else
                {
                    _prodRepo.Update(productVM.Product);
                }
                await _prodRepo.Save();
                TempData["success"] = "Product created successfully";
                return RedirectToAction("Index");
            }
            productVM.CategorySelectList = _prodRepo.GetAllDropDownList("Category");

            return View(productVM);
        }

        #region API CALL
        [HttpGet]
        public async Task<IActionResult> GetAll()
        {
            IEnumerable<Product> productList = await _prodRepo.GetAll(includeProperties: "Category");

            return Json(new { data = productList });
        }

        [HttpDelete]
        public async Task<IActionResult> Delete(int? id)
        {
            var obj = await _prodRepo.Find(id.GetValueOrDefault());
            if (obj == null)
            {
                return Json(new { success = false, Message = "Error while deleting" });
            }
            await _prodRepo.Remove(obj);
            await _prodRepo.Save();
            return Json(new { success = true, Message = "Delete Successful" });
        }
        #endregion

    }
}
