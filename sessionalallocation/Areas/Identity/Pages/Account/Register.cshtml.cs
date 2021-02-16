using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Text.Encodings.Web;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Identity.UI.Services;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;
using Microsoft.AspNetCore.Mvc.Rendering;
using Microsoft.Extensions.Logging;
using SessionalAllocation.Models;

namespace SessionalAllocation.Areas.Identity.Pages.Account
{
    [AllowAnonymous]
    public class RegisterModel : PageModel
    {
        private readonly SignInManager<ApplicationUser> _signInManager;
        private readonly UserManager<ApplicationUser> _userManager;
        private readonly ILogger<RegisterModel> _logger;
        private readonly RoleManager<ApplicationRole> _roleManger;
        private readonly IEmailSender _emailSender;

        public RegisterModel(
            UserManager<ApplicationUser> userManager,
            SignInManager<ApplicationUser> signInManager,
            ILogger<RegisterModel> logger,
            RoleManager<ApplicationRole> roleManager,
            IEmailSender emailSender)
        {
            _userManager = userManager;
            _signInManager = signInManager;
            _logger = logger;
            _roleManger = roleManager;
            _emailSender = emailSender;
        }

        [BindProperty]
        public InputModel Input { get; set; }

        public string ReturnUrl { get; set; }

        public class InputModel
        {
            // new model is here with extra fields added
            [Required]
            [StringLength(15)]
            [RegularExpression(@"^[a-zA-Z''-'\s]{1,30}$",
             ErrorMessage = "Invalid Format. First Name cannot include numbers or special characters")]
            [Display(Name = "First Name")]
            public string FirstName { get; set; }

            [Required]
            [StringLength(25)]
            [Display(Name = "Surname")]
            public string LastName { get; set; }

            [Required]
            [StringLength(25)]
            [Display(Name = "Street")]
            public string Street { get; set; }

            [Required]
            [StringLength(20)]
            [RegularExpression(@"^[a-zA-Z''-'\s]{1,30}$",
             ErrorMessage = "Invalid Format. City cannot be numbers or special characters")]
            [Display(Name = "City")]
            public string City { get; set; }

            [Required]
            [StringLength(10)]
            [RegularExpression(@"^[a-zA-Z''-'\s]{1,30}$",
             ErrorMessage = "Invalid Format. State cannot be numbers or special characters")]
            [Display(Name = "State")]
            public string State { get; set; }

            // change this later on to int
            [Required]
            [StringLength(4)]
            [RegularExpression("([1-9][0-9]*)",
            ErrorMessage = "Please enter a valid Postcode")]
            [Display(Name = "Postcode")]
            public string PostalCode { get; set; }

            [Required]
            [StringLength(9)]
            [Display(Name = "Country")]
            [RegularExpression(@"^[a-zA-Z''-'\s]{1,30}$",
             ErrorMessage = "Invalid Format. Country cannot be numbers or special characters")]
            public string Country { get; set; }


            [Required]
            [EmailAddress]
            [Display(Name = "Email")]
            public string Email { get; set; }

            [Required]
            [StringLength(100, ErrorMessage = "The {0} must be at least {2} and at max {1} characters long.", MinimumLength = 6)]
            [DataType(DataType.Password)]
            [Display(Name = "Password")]
            public string Password { get; set; }

            [DataType(DataType.Password)]
            [Display(Name = "Confirm password")]
            [Compare("Password", ErrorMessage = "The password and confirmation password do not match.")]
            public string ConfirmPassword { get; set; }
        }

        public void OnGet(string returnUrl = null)
        {
            ReturnUrl = returnUrl;
        }

      /*  [HttpPost]
        [AllowAnonymous]
        [ValidateAntiForgeryToken]
        public async Task<ActionResult> Register(RegisterModel model)
        {
            if (ModelState.IsValid)
            {
                var user = new ApplicationUser { UserName = Input.Email, Email = Input.Email };
                var result = await _userManager.CreateAsync(user, Input.Password);
                if (result.Succeeded)
                {
                    var code = await _userManager.GenerateEmailConfirmationTokenAsync(user);
                    var callbackUrl = Url.Action(
                       "ConfirmEmail", "Account",
                       new { userId = user.Id, code = code },
                       protocol: Request.Scheme);

                    await _emailSender.SendEmailAsync(user.Id,
                       "Confirm your account",
                       "Please confirm your account by clicking this link: <a href=\""
                                                       + callbackUrl + "\">link</a>");
                    // ViewBag.Link = callbackUrl;   // Used only for initial demo.
                  //  return View("DisplayEmail");
                }
               // AddErrors(result);
            }

            // If we got this far, something failed, redisplay form
          //  return View(model);
        }*/

        public async Task<IActionResult> OnPostAsync(string returnUrl = null)
        {
            returnUrl = returnUrl ?? Url.Content("~/");
            // add the user role as well as their information on search.
            // we might move this into a function later and just call it.
            
            string applicantRole = "Applicant";
            string applicantDescription = "User has applied for job.";

            

            if (ModelState.IsValid)
            {
                var user = new ApplicationUser { UserName = Input.Email, Email = Input.Email, FirstName= Input.FirstName,
                                                 LastName = Input.LastName, Street = Input.Street, City = Input.City, State = Input.State,
                                                 PostalCode = Input.PostalCode, Country = Input.Country};
                var result = await _userManager.CreateAsync(user, Input.Password);
                
                if (result.Succeeded)
                {
                    if (await _roleManger.FindByNameAsync(applicantRole) == null)
                    {
                        await _roleManger.CreateAsync(new ApplicationRole(applicantRole, applicantDescription, DateTime.Now));
                    }

                    await _userManager.AddToRoleAsync(user, applicantRole);
                    


                    _logger.LogInformation("User created a new account with password.");

                   var code = await _userManager.GenerateEmailConfirmationTokenAsync(user);
                    var callbackUrl = Url.Page(
                        "/Account/ConfirmEmail",
                        pageHandler: null,
                        values: new { userId = user.Id, code = code },
                        protocol: Request.Scheme);
                    
              

                    await _emailSender.SendEmailAsync(Input.Email, "Confirm your email",
                        $"Please confirm your account by <a href='{HtmlEncoder.Default.Encode(callbackUrl)}'>clicking here</a>.");

                    await _signInManager.SignInAsync(user, isPersistent: false);
                    return LocalRedirect(returnUrl);
                }
                
                foreach (var error in result.Errors)
                {
                    ModelState.AddModelError(string.Empty, error.Description);
                }
            }

            // If we got this far, something failed, redisplay form
            return Page();
        }
    }
}
