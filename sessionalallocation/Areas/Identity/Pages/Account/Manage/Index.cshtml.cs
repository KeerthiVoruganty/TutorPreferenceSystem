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
    public partial class IndexModel : PageModel
    {
        private readonly UserManager<ApplicationUser> _userManager;
        private readonly SignInManager<ApplicationUser> _signInManager;
        private readonly IEmailSender _emailSender;

        public IndexModel(
            UserManager<ApplicationUser> userManager,
            SignInManager<ApplicationUser> signInManager,
            IEmailSender emailSender)
        {
            _userManager = userManager;
            _signInManager = signInManager;
            _emailSender = emailSender;
        }

        public string Username { get; set; }

        public bool IsEmailConfirmed { get; set; }

        [TempData]
        public string StatusMessage { get; set; }

        [BindProperty]
        public InputModel Input { get; set; }

        public class InputModel
        {
            [Required]
            [EmailAddress]
            public string Email { get; set; }

            [Phone]
            [Display(Name = "Phone number")]
            public string PhoneNumber { get; set; }

            [StringLength(15)]
            [RegularExpression(@"^[a-zA-Z''-'\s]{1,30}$",
            ErrorMessage = "Invalid Format. First Name cannot include numbers or special characters")]
            public string FirstName { get; set; }

            public string LastName { get; set; }

            public string Street { get; set;}

            [RegularExpression(@"^[a-zA-Z''-'\s]{1,30}$",
             ErrorMessage = "Invalid Format. City cannot be numbers or special characters")]
            public string City { get; set; }

            [RegularExpression(@"^[a-zA-Z''-'\s]{1,30}$",
             ErrorMessage = "Invalid Format. State cannot be numbers or special characters")]
            public string State { get; set; }

            [RegularExpression(@"^[a-zA-Z''-'\s]{1,30}$",
             ErrorMessage = "Invalid Format. Country cannot be numbers or special characters")]
            public string Country { get; set; }

            [RegularExpression("([1-9][0-9]*)",
            ErrorMessage = "Please enter a valid Postcode")]
            public string PostalCode { get; set; }

            public string YoutubeUrl { get; set; }

            [Display(Name = "LinkedIn")]
            public string LinkedInProfileUrl { get; set; }

            [Display(Name = "Swinburne ID:")]
            public string SwinburneID { get; set; }

            [Display(Name = "Preferred Name")]
            public string PreferedName { get; set; }

            public bool UserFullyRegistered { get; set; }
        }

        public async Task<IActionResult> OnGetAsync()
        {
            var user = await _userManager.GetUserAsync(User);
            if (user == null)
            {
                return NotFound($"Unable to load user with ID '{_userManager.GetUserId(User)}'.");
            }

            var userName = await _userManager.GetUserNameAsync(user);
            var email = await _userManager.GetEmailAsync(user);
            var phoneNumber = await _userManager.GetPhoneNumberAsync(user);
            var firstName = user.FirstName;
            var lastName = user.LastName;
            var street = user.Street;
            var city = user.City;
            var state = user.State;
            var country = user.Country;
            var postalCode = user.PostalCode;
            var youtubeUrl = user.YoutubeUrl;
            var linkedInProfileUrl = user.LinkedInProfileUrl;
            var preferredName = user.PreferredName;
            var swinburneId = user.SwinburneID;
            var userFullyRegistered = user.UserFullyRegistered;

            Username = userName;

            Input = new InputModel
            {
                Email = email,
                PhoneNumber = phoneNumber,
                FirstName = firstName,
                LastName = lastName,
                Street = street,
                City = city,
                State = state,
                Country = country,
                PostalCode = postalCode,
                YoutubeUrl = youtubeUrl,
                LinkedInProfileUrl = linkedInProfileUrl,
                PreferedName = preferredName,
                SwinburneID = swinburneId,
                UserFullyRegistered = userFullyRegistered
            };

            IsEmailConfirmed = await _userManager.IsEmailConfirmedAsync(user);

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

            var email = await _userManager.GetEmailAsync(user);
            if (Input.Email != email)
            {
                var setEmailResult = await _userManager.SetEmailAsync(user, Input.Email);
                if (!setEmailResult.Succeeded)
                {
                    var userId = await _userManager.GetUserIdAsync(user);
                    throw new InvalidOperationException($"Unexpected error occurred setting email for user with ID '{userId}'.");
                }
            }

            var phoneNumber = await _userManager.GetPhoneNumberAsync(user);
            if (Input.PhoneNumber != phoneNumber)
            {
                var setPhoneResult = await _userManager.SetPhoneNumberAsync(user, Input.PhoneNumber);
                if (!setPhoneResult.Succeeded)
                {
                    var userId = await _userManager.GetUserIdAsync(user);
                    throw new InvalidOperationException($"Unexpected error occurred setting phone number for user with ID '{userId}'.");
                }
            }

            var firstName = user.FirstName;
            if (Input.FirstName != firstName)
            {
                user.FirstName = Input.FirstName;     
            // Error checking will need to be added here to ensure in the event something fails it updates.
            }

            var lastName = user.LastName;
            if (Input.LastName != lastName)
            {
                user.LastName = Input.LastName;      
            // Error checking will need to be added here to ensure in the event something fails it updates.
            }
            var street = user.Street;
            if (Input.Street != street)
            {
                user.Street = Input.Street;
                // Error checking will need to be added here to ensure in the event something fails it updates.
            }
            var city = user.City;
            if (Input.Street != city)
            {
                user.City = Input.City;
                // Error checking will need to be added here to ensure in the event something fails it updates.
            }
            var country = user.Country;
            if (Input.Country != country)
            {
                user.Country = Input.Country;
                // Error checking will need to be added here to ensure in the event something fails it updates.
            }
            var postalCode = user.PostalCode; 
            if (Input.PostalCode != postalCode)
            {
                user.PostalCode = Input.PostalCode;
                // Error checking will need to be added here to ensure in the event something fails it updates.
            }

            var youtubeUrl = user.YoutubeUrl;
            if (Input.YoutubeUrl != youtubeUrl)
            {
                user.YoutubeUrl = Input.YoutubeUrl;
                // Error checking will need to be added here to ensure in the event something fails it updates.
            }
            var swinburneId = user.SwinburneID;
            if (Input.SwinburneID != swinburneId)
            {
                user.SwinburneID = Input.SwinburneID;
            }
            var preferredName = user.PreferredName;
            if (Input.PreferedName != preferredName)
            {
                user.PreferredName = Input.PreferedName;
            }
            var linkedInProfileUrl = user.LinkedInProfileUrl;
            if (Input.LinkedInProfileUrl != user.LinkedInProfileUrl)
            {
                user.LinkedInProfileUrl = Input.LinkedInProfileUrl;
            }
            await _userManager.UpdateAsync(user);
            await _signInManager.RefreshSignInAsync(user);
            // Add the fullRegistation after the refresh
            var result = await FullRegistrationAsync();
            user.UserFullyRegistered = result;
            await _userManager.UpdateAsync(user);
            StatusMessage = "Your details have been successfully updated";
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




        public async Task<IActionResult> OnPostSendVerificationEmailAsync()
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


            var userId = await _userManager.GetUserIdAsync(user);
            var email = await _userManager.GetEmailAsync(user);
            var code = await _userManager.GenerateEmailConfirmationTokenAsync(user);
            var callbackUrl = Url.Page(
                "/Account/ConfirmEmail",
                pageHandler: null,
                values: new { userId = userId, code = code },
                protocol: Request.Scheme);
            await _emailSender.SendEmailAsync(
                email,
                "Confirm your email",
                $"Please confirm your account by <a href='{HtmlEncoder.Default.Encode(callbackUrl)}'>clicking here</a>.");

            StatusMessage = "Verification email sent. Please check your email.";
            return RedirectToPage();
        }
    }
}
