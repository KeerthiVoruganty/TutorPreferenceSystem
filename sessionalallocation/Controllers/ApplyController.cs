using System;
using System.Collections.Generic;
using System.Linq;
using System.Security.Claims;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc.Rendering;
using Microsoft.EntityFrameworkCore;
using SessionalAllocation.Models;
using SessionalAllocation.Data;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Http;

namespace SessionalAllocation.Controllers
{
    [Authorize]
    public class ApplyController : Controller
    {
        private readonly ApplicationDbContext _context;
        //private readonly UserManager<ApplicationUser> _userManager;
        

        private void FillPreferences()
        { 
            List<SelectListItem> preferences = new List<SelectListItem>(); 
            for (int i = 0; i < 4; i++)
            {
                preferences.Add(new SelectListItem()
                {
                    Text = i.ToString(),
                    Value = i.ToString()
                });
            }

            ViewBag.LoadPreferences = preferences;
        }

        public ApplyController(ApplicationDbContext context)
        {
           // UserManager<ApplicationUser> userManager;
            _context = context;
            //_userManager = userManager;
        }

        // retrieves all units
        public async Task<IActionResult> Index()
        {
            // add the check here before doing anything
            //var user = await _userManager.GetUserAsync(User);
            //if (user.UserFullyRegistered == false)
            //{
            //    // this will need to be polished in terms of how the user is told.
            //    return RedirectToAction("Index", "Areas");
            //}
            //else
            //{
                var units = _context.Unit.Include(u => u.DepartmentNavigation).Include(u => u.UnitOwnerNavigation);
                return View(await units.ToListAsync());
            //}
        }

        // Retrieve Classes for specified unit that the applicant has not applied for
        public async Task<IActionResult> Classes(int? id)
        {
            //FillPreferences();
            //get the logged in users id
            var ident = User.Identity as ClaimsIdentity;
            var userId = ident.Claims.FirstOrDefault(c => c.Type == ClaimTypes.NameIdentifier)?.Value;

            // I think this works correctly, needs further testing. Gets all available classes of selected unit that applicant has not already applied for
            var unitDbContext = _context.Class.Include(AspNetUsers => AspNetUsers.TutorAllocatedNavigation).Include(u => u.Unit)
                .Where(c => c.UnitId == id && (!c.Applications.Any(a => a.AppliedClass == c.Id && a.Applicant == userId)));
            return View(await unitDbContext.OrderBy(x=>x.StartDate).ThenBy(x=>x.StartTimeScheduled).ToListAsync());
        }

        // Used to apply for classes
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Apply(int id)//, IFormCollection form)
        {
            //get the logged in users id
            var ident = User.Identity as ClaimsIdentity;
            var userID = ident.Claims.FirstOrDefault(c => c.Type == ClaimTypes.NameIdentifier)?.Value;

            // get preference value from dropdown
            //string prefValue = form["preference" + id].ToString();

            Applications application = new Applications();
            application.Applicant = userID; // id of logged in user
            application.AppliedClass = id;
            application.ProvisionallyAllocated = false;
            application.Approved = false;
            //application.Preference = int.Parse(prefValue);

            // set convenor rating for tutor if doesn't exist
            var convenorId = _context.Class.Include(c => c.Unit).Where(c => c.Id == id).FirstOrDefault().Unit.UnitOwner;
            if (convenorId != null)
            {
                if (!_context.TutorPreference.Where(p => p.ConvenorId == convenorId && p.TutorId == userID).Any())
                {
                    var tutorPreference = new TutorPreference // constructor sets rating to 5 by default
                    {
                        ConvenorId = convenorId,
                        TutorId = userID
                    };    
                    // save to database
                    _context.Add(tutorPreference);
                    await _context.SaveChangesAsync();
                }
            }
            if (ModelState.IsValid)
            {
                _context.Add(application);
                await _context.SaveChangesAsync();
                return RedirectToAction(nameof(Index));
            }
            return RedirectToAction(nameof(Index));
        }
        // For Applying All classes belongs to one unit
        public async Task<IActionResult> ApplyAll (Dictionary<string,string> idParms)
        {
            //get the logged in users id
            var ident = User.Identity as ClaimsIdentity;
            var userID = ident.Claims.FirstOrDefault(c => c.Type == ClaimTypes.NameIdentifier)?.Value;

            List<int> idList = new List<int>() ;

            foreach(KeyValuePair<string, string> item in idParms)
            {
                int tempIndex = Convert.ToInt32(item.Key);
                int tempResult = Convert.ToInt32(item.Value);
                idList.Add(tempResult);
            }

            foreach (int idNum in idList)
            {
                Applications application = new Applications();
                application.Applicant = userID; // id of logged in user
                application.AppliedClass = idNum;
                //TODO: Review setting allocation here. Should we let Database do this?
                application.ProvisionallyAllocated = false;
                application.Approved = false;

                // set convenor rating for tutor if doesn't exist
                var convenorId = _context.Class.Include(c => c.Unit).Where(c => c.Id == idNum).FirstOrDefault().Unit.UnitOwner;
                if (convenorId != null)
                {
                    if (!_context.TutorPreference.Where(p => p.ConvenorId == convenorId && p.TutorId == userID).Any())
                    {
                        var tutorPreference = new TutorPreference // constructor sets rating to 5 by default
                        {
                            ConvenorId = convenorId,
                            TutorId = userID
                        };
                        // save to database
                        _context.Add(tutorPreference);
                        await _context.SaveChangesAsync();
                    }
                }

                if (ModelState.IsValid)
                {
                    _context.Add(application);
                    await _context.SaveChangesAsync();
                }
            }
            return RedirectToAction(nameof(Index));
            //return View("../Apply/Classes/");
        }

        // GET: /Apply/MyApplications
        // GET: Applications for logged in user
        public async Task<IActionResult> MyApplications()
        {
            // get the logged in users id
            var ident = User.Identity as ClaimsIdentity;
            var userId = ident.Claims.FirstOrDefault(c => c.Type == ClaimTypes.NameIdentifier)?.Value;
            var unitDbContext = _context.Applications.Include(a => a.ApplicantNavigation).Include(a => a.AppliedClassNavigation).Include(a => a.AppliedClassNavigation.Unit).Where(a => a.Applicant == userId);

            // function called here will generate dropdown list
            // parameter has to be dynamic in order to generate selected preference
            FillPreferences();

            return View(await unitDbContext.ToListAsync());
        }

        // GET: Apply/Remove/5
        public async Task<IActionResult> Remove(int? id)
        {
            if (id == null)
            {
                return NotFound();
            }

            var applications = await _context.Applications
                .Include(a => a.ApplicantNavigation)
                .Include(a => a.AppliedClassNavigation).Include(a => a.AppliedClassNavigation.Unit)
                .FirstOrDefaultAsync(m => m.ApplicationId == id);
            if (applications == null)
            {
                return NotFound();
            }

            return View(applications);
        }

        // POST: Apply/Remove
        [HttpPost, ActionName("Remove")]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> RemoveConfirmed(int id)
        {
            var applications = await _context.Applications.FindAsync(id);
            _context.Applications.Remove(applications);
            await _context.SaveChangesAsync();
            return RedirectToAction(nameof(MyApplications));
        }

        // function to update applicant preferences
        public async Task<IActionResult> UpdatePref(int id)
        {
            var applications = await _context.Applications.FindAsync(id);

            // get value from text field
            var prefValue = Request.Form["selectPref"];

            // convert value obtained to integer
            applications.Preference = Int32.Parse(prefValue);

            await _context.SaveChangesAsync();
            
            return RedirectToAction(nameof(MyApplications));
        }
    }
}