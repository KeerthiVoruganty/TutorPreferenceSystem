using System;
using System.Collections.Generic;
using System.Linq;
using System.Security.Claims;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Rendering;
using Microsoft.EntityFrameworkCore;
using SessionalAllocation.Models;
using SessionalAllocation.Data;
using Microsoft.AspNetCore.Identity;
using SessionalAllocation.ViewModels;

namespace SessionalAllocation.Controllers
{
    [Authorize(Roles = "Admin")]
    public class AdminController : Controller
    {
        private readonly ApplicationDbContext _context;
        private readonly RoleManager<ApplicationRole> _roleManager;
        private readonly UserManager<ApplicationUser> _userManager;

        public AdminController(ApplicationDbContext context, RoleManager<ApplicationRole> roleManager, UserManager<ApplicationUser> userManager)
        {
            _context = context;
            _roleManager = roleManager;
            _userManager = userManager;
        }

        public IActionResult Index()
        {
            return View();
        }

        // GET: All departments
        public async Task<IActionResult> AllDepartments(int? id)
        {
            var departments = _context.Department.Include(u => u.SchoolNavigation).Include(u => u.DepartmentOwnerNavigation);
            return View(await departments.ToListAsync());
        }

        // GET: All Units of a department
        public async Task<IActionResult> Units(int? id)
        {
            var units = _context.Unit.Include(u => u.DepartmentNavigation).Include(u => u.UnitOwnerNavigation).Where(u => u.Department == id) ;
            return View(await units.ToListAsync());
        }

        // GET: Units/Details/5
        public async Task<IActionResult> DetailsUnit(int? id)
        {
            if (id == null)
            {
                return NotFound();
            }

            var unit = await _context.Unit
                .Include(u => u.DepartmentNavigation)
                .Include(u => u.UnitOwnerNavigation)
                .FirstOrDefaultAsync(m => m.Id == id);
            if (unit == null)
            {
                return NotFound();
            }

            return View(unit);
        }

        // GET: Units/Edit/5
        public async Task<IActionResult> EditUnit(int? id)
        {
            if (id == null)
            {
                return NotFound();
            }

            var unit = await _context.Unit.FindAsync(id);
            if (unit == null)
            {
                return NotFound();
            }
            ViewData["Department"] = new SelectList(_context.Department, "Id", "Name", unit.Department);
            ViewData["UnitOwner"] = new SelectList(_context.Users, "Id", "FullName", unit.UnitOwner).Prepend(new SelectListItem("None", null));
            return View(unit);
        }

        // POST: Units/Edit/5
        // To protect from overposting attacks, please enable the specific properties you want to bind to, for 
        // more details see http://go.microsoft.com/fwlink/?LinkId=317598.
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> EditUnit(int id, [Bind("Id,UnitCode,UnitName,Department,UnitOwner")] Unit unit)
        {
            if (id != unit.Id)
            {
                return NotFound();
            }

            if (ModelState.IsValid)
            {
                try
                {
                    if (unit.UnitOwner == "None")
                    {
                        unit.UnitOwner = null;
                    }
                    else
                    {   // if UnitOwner is not in convenor role, give convenor role
                        var user = await _context.Users.FindAsync(unit.UnitOwner);
                        if (!(await _userManager.IsInRoleAsync(user, "Convenor")))
                        {
                            await _userManager.AddToRoleAsync(user, "Convenor");
                        }
                    }
                    _context.Update(unit);
                    await _context.SaveChangesAsync();
                }
                catch (DbUpdateConcurrencyException)
                {
                    if (!UnitExists(unit.Id))
                    {
                        return NotFound();
                    }
                    else
                    {
                        throw;
                    }
                }
                return RedirectToAction("Units", new { Id = unit.Department });
            }
            ViewData["Department"] = new SelectList(_context.Department, "Id", "Id", unit.Department);
            ViewData["UnitOwner"] = new SelectList(_context.Users, "Id", "Id", unit.UnitOwner).Prepend(new SelectListItem("None", null));
            return RedirectToAction("Units", new { Id = unit.Department });
        }

        private bool UnitExists(int id)
        {
            return _context.Unit.Any(e => e.Id == id);
        }

        // GET: Units/Details/5
        public async Task<IActionResult> DetailsDepartment(int? id)
        {
            if (id == null)
            {
                return NotFound();
            }

            var department = await _context.Department
                .Include(u => u.DepartmentOwnerNavigation)
                .Include(u => u.SchoolNavigation)
                .FirstOrDefaultAsync(m => m.Id == id);
            if (department == null)
            {
                return NotFound();
            }

            return View(department);
        }

        // GET: Departments/Edit/5
        public async Task<IActionResult> EditDepartment(int? id)
        {
            if (id == null)
            {
                return NotFound();
            }

            var department = await _context.Department.FindAsync(id);
            if (department == null)
            {
                return NotFound();
            }
            ViewData["School"] = new SelectList(_context.School, "Id", "Name", department.School);
            ViewData["DepartmentOwner"] = new SelectList(_context.Users, "Id", "FullName", department.DepartmentOwner).Prepend(new SelectListItem("None", null));
            return View(department);
        }

        // POST: Departments/Edit/5
        // To protect from overposting attacks, please enable the specific properties you want to bind to, for 
        // more details see http://go.microsoft.com/fwlink/?LinkId=317598.
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> EditDepartment(int id, [Bind("Id,Name,School,DepartmentOwner")] Department department)
        {
            if (id != department.Id)
            {
                return NotFound();
            }

            if (ModelState.IsValid)
            {
                try
                {
                    if (department.DepartmentOwner == "None")
                    {
                        department.DepartmentOwner = null;
                    }
                    else
                    {
                        var user = await _context.Users.FindAsync(department.DepartmentOwner);
                        if (!(await _userManager.IsInRoleAsync(user, "DepartmentChair")))
                        {
                            await _userManager.AddToRoleAsync(user, "DepartmentChair");
                        }
                    }

                    _context.Update(department);
                    await _context.SaveChangesAsync();
                }
                catch (DbUpdateConcurrencyException)
                {
                    if (!DepartmentExists(department.Id))
                    {
                        return NotFound();
                    }
                    else
                    {
                        throw;
                    }
                }
                return RedirectToAction(nameof(AllDepartments));
            }
            ViewData["School"] = new SelectList(_context.School, "Id", "Id", department.School);
            ViewData["UnitOwner"] = new SelectList(_context.Users, "Id", "Id", department.DepartmentOwner).Prepend(new SelectListItem("None", null));
            return RedirectToAction("AllDepartments");
        }

        private bool DepartmentExists(int id)
        {
            return _context.Department.Any(e => e.Id == id);
        }

        // getting users and roles
        //https://stackoverflow.com/questions/51004516/net-core-2-1-identity-get-all-users-with-their-associated-roles/51005445#51005445
        public async Task<IActionResult> ListUsers()
        {
            var users = _userManager.Users.Include(u => u.UserRoles).ThenInclude(ur => ur.Role);
            return View(await users.ToListAsync());
        }
        // https://csharp-video-tutorials.blogspot.com/2019/07/add-or-remove-users-from-role-in-aspnet.html
        [HttpGet]
        public async Task<IActionResult> ManageUser(string id)
        {

            if (id == null)
            {
                return NotFound();
            }
            // get the selected users data
            var user = await _context.Users.FindAsync(id);

            if (user == null)
            {
                return NotFound();
            }

            // set the UserRoles view model with data
            var model = new UserRolesViewModel
            {
                UserId = user.Id,
                UserFullName = user.FullName,
                UserRoles = new Dictionary<string, bool>()
            };

            // get roles and check if user has role
            foreach (var role in _context.Roles.ToList())
            {
                model.UserRoles.Add(role.Name, await _userManager.IsInRoleAsync(user, role.Name));
            }
            return View(model);
        }


        [HttpPost]
        public async Task<IActionResult> ManageUser(UserRolesViewModel model)
        {

            var user = await _context.Users.FindAsync(model.UserId);
            // iterate through each available role and either set or remove role from user
            foreach (var role in model.UserRoles)
            {
                // role is selected but user not currently in role, add to role
                if (role.Value && !(await _userManager.IsInRoleAsync(user, role.Key)))
                {
                    await _userManager.AddToRoleAsync(user, role.Key);
                }   // role is not selected but user is in role, remove from role
                else if (!role.Value && await _userManager.IsInRoleAsync(user, role.Key))
                {
                    await _userManager.RemoveFromRoleAsync(user, role.Key);
                }
            }
            // just sent them back to the user manager, maybe send them to the list of users would be better?
            return RedirectToAction("ListUsers");
        }

    }
}