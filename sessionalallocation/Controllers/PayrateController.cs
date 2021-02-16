using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using SessionalAllocation.Data;
using SessionalAllocation.Models;
using SessionalAllocation.ViewModels;

namespace SessionalAllocation.Controllers
{
    // authorising departmentchair for now
    [Authorize(Roles = "DepartmentChair")]
    public class PayrateController : Controller
    {
        private readonly ApplicationDbContext _context;
        public PayrateController(ApplicationDbContext context)
        {
            _context = context;
        }
        public async Task<IActionResult> Index()
        {
            // get roles and check if user has role
            var payrates = _context.Payrate.ToList();
            return View(payrates);
        }

        [HttpGet]
        public async Task<IActionResult> Edit(int id)
        {
            // get the selected users data
            var payrate = await _context.Payrate.FindAsync(id);

            if (payrate == null)
            {
                return NotFound();
            }

            return View(payrate);
        }
        [HttpPost]
        public async Task<IActionResult> Edit(Payrate payrate)
        {
            _context.Update(payrate);
            await _context.SaveChangesAsync();
            return RedirectToAction("Index");
        }

        [HttpGet]
        public async Task<IActionResult> EditAll()
        {
            // get the selected users data
            var model = new PayratesViewModel
            {
                Payrates = new Dictionary<int, Payrate>()
            };

            // get roles and check if user has role
            foreach (var payrate in _context.Payrate.ToList())
            {
                model.Payrates.Add(payrate.Id, payrate);
            }
            return View(model);
        }
        [HttpPost]
        public async Task<IActionResult> EditAll(PayratesViewModel model)
        {
            foreach (var payrate in model.Payrates)
            {
                _context.Update(payrate.Value);
                await _context.SaveChangesAsync();
            }
            return RedirectToAction("Index");
        }
    }
}