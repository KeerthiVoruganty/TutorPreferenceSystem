using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.IO;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;
using Microsoft.AspNetCore.Mvc.Rendering;
using Microsoft.EntityFrameworkCore;
using SessionalAllocation.Data;
using SessionalAllocation.Models;

namespace SessionalAllocation.Areas.Identity.Pages.Account.Manage
{
    public class TimeAvailabilityModel : PageModel
    {
        //DBContext declaration and Initialisation to get/set data from DB.
        private readonly SessionalAllocation.Data.ApplicationDbContext _context;
        private readonly UserManager<ApplicationUser> _userManager;
        private readonly SignInManager<ApplicationUser> _signInManager;

        public TimeAvailabilityModel(SessionalAllocation.Data.ApplicationDbContext context,
            UserManager<ApplicationUser> userManager,
            SignInManager<ApplicationUser> signInManager)
        {
            _context = context;
            _userManager = userManager;
            _signInManager = signInManager;
        }
        

        //Utilise the StatusMessage on error/success of operations.
        [TempData]
        public string StatusMessage { get; set; }

        //TimeAvailability Model
        [BindProperty]
        public TimeAvailability TimeAvailability { get; set; }

        //Fetch Availability List.
        public IList<TimeAvailability> TimeAvailabilityList { get; set; }

       

        //GET Page.
        public async Task OnGetAsync()
        {
            //Fetch User Identity from DB.
            var user = await _userManager.GetUserAsync(User);
            //Fetch all TimeAvailabilities related to the User. Sort by Day of Week, then Start Time before sending to webpage.
            TimeAvailabilityList = await _context.TimeAvailability.Where(ta => ta.ApplicantNavigation == user).OrderBy(ta => ta.WeekDay).ThenBy(ta => ta.FromTime).ToListAsync();
            
        }
       /* public Boolean exist()
        {
            foreach (var item in this.TimeAvailabilityList)
            {
                if (item.Equals(TimeAvailability.WeekDay))
                {
                    StatusMessage = "Error: Exists";
                    return false;
                }
            }
            return true;
        }*/
        //POST on Page. Usually when submitting another time.
        public async Task<IActionResult> OnPostAsync()
        {
           
                // If User Selected All Day, Force the times to go to the Min and Max.
                if (TimeAvailability.IsAllDay)
                {
                    TimeAvailability.FromTime = new DateTime(2019, 01, 01, 0, 0, 0);
                    TimeAvailability.EndTime = new DateTime(2019, 01, 01, 23, 59, 59);
                }

                else
                {
                    //Validate the Time provided against the Modeltype. Throw an error if they somehow supplied incorrect time information.
                    if (!ModelState.IsValid)
                    {
                        StatusMessage = "Error: You have supplied something wrong in your availability. Please check the fields and try again.";
                        return RedirectToPage();
                    }
                }
            
                //Assign logged in user to TimeAvailability Instance
                TimeAvailability.ApplicantNavigation = await _userManager.GetUserAsync(User);

                //Commit to DB.
                _context.TimeAvailability.Add(TimeAvailability);
                await _context.SaveChangesAsync();

                //Send Success Message and reload page.
                StatusMessage = "Success! Availablity Time has been added.";
                return RedirectToPage();
            
        }

        //Delete specific Time Instance
        public async Task<IActionResult> OnPostDeleteAsync (int id)
        {
            var taInstance = await _context.TimeAvailability.FindAsync(id);
            var user = await _userManager.GetUserAsync(User);
            var taOwner = _context.TimeAvailability.Find(id).ApplicantNavigation;

            //Double check if Time Instance belongs to this user first before deleting
            if (taOwner != user)
            {
                StatusMessage = "Error: Delete error occured. Please try deleting again.";
                return RedirectToPage();
            }
            else
            {
                _context.TimeAvailability.Remove(taInstance);
                await _context.SaveChangesAsync();
                StatusMessage = "Success: Availability was removed.";
                return RedirectToPage();
            }
        }


    }

}