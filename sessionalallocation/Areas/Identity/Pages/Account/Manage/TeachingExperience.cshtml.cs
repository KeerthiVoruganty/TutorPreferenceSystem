using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Linq;
using System.Text.Encodings.Web;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Identity.UI.Services;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;
using SessionalAllocation.Models;

namespace SessionalAllocation.Areas.Identity.Pages.Account.Manage
{
    public class TeachingExperienceModel : PageModel
    {
        private readonly UserManager<ApplicationUser> _userManager;
        private readonly SignInManager<ApplicationUser> _signInManager;

        public TeachingExperienceModel(
        UserManager<ApplicationUser> userManager,
        SignInManager<ApplicationUser> signInManager
            )
        {
            _userManager = userManager;
            _signInManager = signInManager;
        }

        [TempData]
        public string StatusMessage { get; set; }

        [BindProperty]
        public InputModel Input { get; set; }
        public class InputModel
        {
            public string NumberYearsWorkExperience { get; set; }
            public string PreviousTeachingExperience { get; set; }
            public string Publications { get; set; }
            public string TutorTraining { get; set; }
            public string OtherTraining { get; set; }
            public string CanvasTraining { get; set; }
            public string FSETSessionalInduction { get; set; }
            public string SwinburneSessionalInduction { get; set; }
            public string AustralianWorkRights { get; set; }
            public string WorkRights { get; set; }
            public string VisaType { get; set; }
            public string VisaNumber { get; set; }
            public bool UserFullyRegistered { get; set; }
            public string YoutubeURL { get; set; }
        }

        
        public async Task<IActionResult> OnGetAsync()
        {
            var user = await _userManager.GetUserAsync(User);
            if (user == null)
            {
                return NotFound($"Unable to load user with ID '{_userManager.GetUserId(User)}'.");
            }
            var numberYearsWorkExperience = user.NumberYearsWorkExperience;
            var previousTeachingExperience = user.PreviousTeachingExperience;
            var publications = user.Publications;
            var tutorTraining = user.TutorTraining;
            var otherTraining = user.OtherTraining;
            var canvasTraining = user.CanvasTraining;
            var fsetSessionalInduction = user.FSETSessionalInduction;
            var swinburneSessionalInduction = user.SwinburneSessionalInduction;
            var australianWorkRights = user.AustralianWorkRights;
            var workRights = user.WorkRights;
            var visaType = user.VisaType;
            var visaNumber = user.VisaNumber;
            var userFullyRegistered = user.UserFullyRegistered;

            Input = new InputModel
            {
                NumberYearsWorkExperience = numberYearsWorkExperience,
                PreviousTeachingExperience = previousTeachingExperience,
                Publications = publications,
                TutorTraining = tutorTraining,
                OtherTraining = otherTraining,
                CanvasTraining = canvasTraining,
                FSETSessionalInduction = fsetSessionalInduction,
                SwinburneSessionalInduction = swinburneSessionalInduction,
                AustralianWorkRights = australianWorkRights,
                WorkRights = workRights,
                VisaType = visaType,
                VisaNumber = visaNumber,
                UserFullyRegistered = userFullyRegistered,
                YoutubeURL = user.YoutubeUrl
            };
            return Page();

        }

        public async Task<IActionResult> OnPostAsync()
        {
            if (!ModelState.IsValid)
            {
                return Page();
            }

            var user = await _userManager.GetUserAsync(User);
            if (user == null)
            {
                return NotFound($"Unable to load user with ID '{_userManager.GetUserId(User)}'.");
            }

            var numberYearsWorkExperience = user.NumberYearsWorkExperience;
            if (Input.NumberYearsWorkExperience != user.NumberYearsWorkExperience)
            {
                user.NumberYearsWorkExperience = Input.NumberYearsWorkExperience;
            }
            var previousTeachingExperience = user.PreviousTeachingExperience;
            if (Input.PreviousTeachingExperience != user.PreviousTeachingExperience)
            {
                user.PreviousTeachingExperience = Input.PreviousTeachingExperience;
            }
            var publications = user.Publications;
            if (Input.Publications != user.Publications)
            {
                user.Publications = Input.Publications;
            }
            var tutorTraining = user.TutorTraining;
            if (Input.TutorTraining != user.TutorTraining)
            {
                user.TutorTraining = Input.TutorTraining;
            }
            var otherTraining = user.OtherTraining;
            if (Input.OtherTraining != user.OtherTraining)
            {
                user.OtherTraining = Input.OtherTraining;
            }
            var canvasTraining = user.CanvasTraining;
            if (Input.CanvasTraining != user.CanvasTraining)
            {
                user.CanvasTraining = Input.CanvasTraining;
            }
            var fsetSessionalInduction = user.FSETSessionalInduction;
            if (Input.FSETSessionalInduction != user.FSETSessionalInduction)
            {
                user.FSETSessionalInduction = Input.FSETSessionalInduction;
            }
            var swinburneSessionalInduction = user.SwinburneSessionalInduction;
            if (Input.SwinburneSessionalInduction != user.SwinburneSessionalInduction)
            {
                user.SwinburneSessionalInduction = Input.SwinburneSessionalInduction;
            }
            var australianWorkRights = user.AustralianWorkRights;
            if (Input.AustralianWorkRights != user.AustralianWorkRights)
            {
                user.AustralianWorkRights = Input.AustralianWorkRights;
            }
            var workRights = user.WorkRights;
            if (Input.WorkRights != user.WorkRights)
            {
                user.WorkRights = Input.WorkRights;
            }
            var visaType = user.VisaType;
            if (Input.VisaType != user.VisaType)
            {
                user.VisaType = Input.VisaType;
            }
            var visaNumber = user.VisaNumber;
            if (Input.VisaNumber != user.VisaNumber)
            {
                user.VisaNumber= Input.VisaNumber;
            }

            if (Input.YoutubeURL != user.YoutubeUrl) { user.YoutubeUrl = Input.YoutubeURL; }
            await _userManager.UpdateAsync(user);
            await _signInManager.RefreshSignInAsync(user);
            var result = await FullRegistrationAsync();
            user.UserFullyRegistered = result;
            await _userManager.UpdateAsync(user);
            StatusMessage = "Your Qualification Details have been updated";
            return RedirectToPage();

        }
        public async Task<bool> FullRegistrationAsync()
        {
            var user = await _userManager.GetUserAsync(User);
            if (user.FirstName != null && user.LastName != null && user.Street != null &&
                user.City != null && user.State != null && user.PostalCode != null &&
                user.Country != null && user.YoutubeUrl != null && user.Qualification != null &&
                user.QualificationName != null && user.QulificationCompletionYear != null &&
                user.CitizenshipStudyStatus != null &&
                user.NumberYearsWorkExperience != null && user.PreviousTeachingExperience != null &&
                user.TutorTraining != null && user.OtherTraining != null && user.CanvasTraining != null &&
                user.FSETSessionalInduction != null && user.SwinburneSessionalInduction != null &&
                user.WorkRights != null & user.ResumeFileName != null)
            // user.AreYouStudying != null &&
            {
                return user.UserFullyRegistered = true;
            }
            else
            {
                return user.UserFullyRegistered = false;
            }

        }
    }
}