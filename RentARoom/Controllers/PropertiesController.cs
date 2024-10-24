using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Rendering;
using Microsoft.EntityFrameworkCore;
using RentARoom.DataAccess.Data;
using RentARoom.Models;

namespace RentARoom.Controllers
{
    public class PropertiesController : Controller
    {
        private readonly ApplicationDbContext _db;

        public PropertiesController(ApplicationDbContext db)
        {
            _db = db;
        }

        public async Task<IActionResult> Index()
        {
            List<Property> objPropertyList = await _db.Property.ToListAsync();
            return View(objPropertyList);
        }     

        public IActionResult Create()
        {
            return View();
        }

        [HttpPost]
        public IActionResult Create(Property obj)
        {
            if (ModelState.IsValid)
            {
                _db.Property.Add(obj);
                _db.SaveChanges();
                TempData["success"] = "Property created successfully";
                return RedirectToAction("Index", "Properties");
            }
            return View();
        }

        public IActionResult Edit(int? id)
        {
            if(id == null || id == 0)
            {
                return NotFound();
            }
            Property? propertyFromDb = _db.Property.FirstOrDefault(u=>u.Id == id);
            return View(propertyFromDb);
        }

        [HttpPost]
        public IActionResult Edit(Property obj)
        {
            if (ModelState.IsValid)
            {
                _db.Property.Update(obj);
                _db.SaveChanges();
                TempData["success"] = "Property updated successfully";
                return RedirectToAction("Index", "Properties");
            }
            return View();
        }

        public IActionResult Delete(int? id)
        {
            if (id == null || id == 0)
            {
                return NotFound();
            }
            Property? propertyFromDb = _db.Property.FirstOrDefault(u => u.Id == id);
            return View(propertyFromDb);
        }

        [HttpPost, ActionName("Delete")]
        public IActionResult DeletePOST(int? id)
        {
            Property obj = _db.Property.Find(id);
            if(obj == null)
            {
                return NotFound();
            }
            _db.Property.Remove(obj);
            _db.SaveChanges();
            TempData["success"] = "Property deleted successfully";
            return RedirectToAction("Index", "Properties");

        }

        private bool PropertyExists(int id)
        {
            return _db.Property.Any(e => e.Id == id);
        }

        // GET: Properties/ShowSearchForm
        public async Task<IActionResult> ShowSearchForm()
        {
            return View();
        }

        // POST: Properties/ShowSearchResults
        public async Task<IActionResult> ShowSearchResults(String SearchPhrase)
        {
            return View("Index", await _db.Property.Where(x => x.Address.Contains(SearchPhrase)).ToListAsync());
        }

        // GET: Properties/Details/5
        public async Task<IActionResult> Details(int? id)
        {
            if (id == null)
            {
                return NotFound();
            }

            var @property = await _db.Property
                .FirstOrDefaultAsync(m => m.Id == id);
            if (@property == null)
            {
                return NotFound();
            }

            return View(@property);
        }
    }
}
