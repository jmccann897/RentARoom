using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Rendering;
using Microsoft.EntityFrameworkCore;
using RentARoom.DataAccess.Data;
using RentARoom.DataAccess.Repository.IRepository;
using RentARoom.Models;
using RentARoom.Models.ViewModels;

namespace RentARoom.Controllers
{
    public class PropertiesController : Controller
    {
        private readonly IUnitOfWork _unitOfWork;

        public PropertiesController(IUnitOfWork unitOfWork)
        {
            _unitOfWork = unitOfWork;
        }

        public IActionResult Index()
        {
            List<Property> objPropertyList = _unitOfWork.Property.GetAll().ToList();
            return View(objPropertyList);
        }     

        public IActionResult Upsert(int? id)
        {
            PropertyVM propertyVM = new()
            {
                PropertyTypeList = _unitOfWork.PropertyType
                .GetAll().Select(u => new SelectListItem
                {
                    Text = u.Name,
                    Value = u.Id.ToString()
                }),
                Property = new Property()
            };

            //create
            if(id == null || id == 0)
            {
                return View(propertyVM);
            }
            else
            {
                //update
                propertyVM.Property = _unitOfWork.Property.Get(u => u.Id == id);
                return View(propertyVM);
            }            
        }

        [HttpPost]
        public IActionResult Upsert(PropertyVM propertyVM, IFormFile? file)
        {
            if (ModelState.IsValid)
            {
                _unitOfWork.Property.Add(propertyVM.Property);
                _unitOfWork.Save();
                TempData["success"] = "Property created successfully";
                return RedirectToAction("Index", "Properties");
            }
            else
            {
                //if issue, need to re-populate dropdown
                propertyVM.PropertyTypeList = _unitOfWork.PropertyType
                .GetAll().Select(u => new SelectListItem
                {
                    Text = u.Name,
                    Value = u.Id.ToString()
                });
                return View(propertyVM);
            }
            
        }
     
        public IActionResult Delete(int? id)
        {
            if (id == null || id == 0)
            {
                return NotFound();
            }
            Property? propertyFromDb = _unitOfWork.Property.Get(u => u.Id == id);
            return View(propertyFromDb);
        }

        [HttpPost, ActionName("Delete")]
        public IActionResult DeletePOST(int? id)
        {
            Property obj = _unitOfWork.Property.Get(u => u.Id == id);
            if (obj == null)
            {
                return NotFound();
            }
            _unitOfWork.Property.Remove(obj);
            _unitOfWork.Save();
            TempData["success"] = "Property deleted successfully";
            return RedirectToAction("Index", "Properties");

        }

        //private bool PropertyExists(int id)
        //{
        //    return _propertyRepo.Any(e => e.Id == id);
        //}

        //// GET: Properties/ShowSearchForm
        //public async Task<IActionResult> ShowSearchForm()
        //{
        //    return View();
        //}

        //// POST: Properties/ShowSearchResults
        //public async Task<IActionResult> ShowSearchResults(String SearchPhrase)
        //{
        //    return View("Index", await _db.Property.Where(x => x.Address.Contains(SearchPhrase)).ToListAsync());
        //}

        //// GET: Properties/Details/5
        //public async Task<IActionResult> Details(int? id)
        //{
        //    if (id == null)
        //    {
        //        return NotFound();
        //    }

        //    var @property = await _db.Property
        //        .FirstOrDefaultAsync(m => m.Id == id);
        //    if (@property == null)
        //    {
        //        return NotFound();
        //    }

        //    return View(@property);
        //}
    }
}
