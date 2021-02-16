using System;
using System.IO;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;
using Microsoft.Extensions.Logging;
using SessionalAllocation.Models;

namespace SessionalAllocation.Areas.Identity.Pages.Account.Manage
{
    public class ResumeUploadModel : PageModel
    {
        private readonly UserManager<ApplicationUser> _userManager;
        private readonly SignInManager<ApplicationUser> _signInManager;
        private readonly ILogger<ResumeUploadModel> _logger;

        // limit file size to 10MB
        public const int fileSizeLimit = 10000000;

        // byte variable for each mime type
        private static readonly byte[] DOC = { 208, 207, 17, 224, 161, 177, 26, 225 };
        private static readonly byte[] ZIP_DOCX = { 80, 75, 3, 4 };
        private static readonly byte[] PDF = { 37, 80, 68, 70, 45, 49, 46 };

        public ResumeUploadModel(
            UserManager<ApplicationUser> userManager,
            SignInManager<ApplicationUser> signInManager,
            ILogger<ResumeUploadModel> logger)
        {
            _userManager = userManager;
            _signInManager = signInManager;
            _logger = logger;
        }

        [BindProperty]
        public InputModel Input { get; set; }

        [TempData]
        public string StatusMessage { get; set; }

        public class InputModel
        {
            public IFormFile File { get; set; }
        }

        public async Task<IActionResult> OnGet()
        {
            var user = await _userManager.GetUserAsync(User);

            if (user == null)
            {
                return NotFound($"Unable to load user with ID '{_userManager.GetUserId(User)}'.");
            }

            return Page();
        }

        public async Task<IActionResult> OnPostAsync(string returnUrl = null)
        {
            // File upload by converting files to binary format, storing in the database

            if (ModelState.IsValid)
            {
                var user = await _userManager.GetUserAsync(User);
                if (user == null)
                {
                    return NotFound($"Unable to load user with ID '{_userManager.GetUserId(User)}'.");
                }

                var filePath = Path.GetTempFileName();

                // check if file is selected 
                if (Input.File != null)
                {
                    // Check Input File Actual Mime Type
                    using (var memoryStream = new MemoryStream())
                    {
                        await Input.File.CopyToAsync(memoryStream);
                        byte[] fileByte = memoryStream.ToArray();
                        string fileMime = GetMimeType(fileByte, Input.File.FileName);

                        // Check Mime Type of Input File
                        if (Input.File.ContentType != fileMime &&
                            !string.Equals(fileMime, "application/msword", StringComparison.OrdinalIgnoreCase) &&
                            !string.Equals(fileMime, "application/vnd.openxmlformats-officedocument.wordprocessingml.document", StringComparison.OrdinalIgnoreCase) &&
                            !string.Equals(fileMime, "application/pdf", StringComparison.OrdinalIgnoreCase))
                        {
                            StatusMessage = "File Format is Invalid! Do not change extension of file!";
                            return Page();
                        }
                    }

                    // Check file size
                    if (Input.File.Length > fileSizeLimit)
                    {
                        StatusMessage = "File size exceeds 10MB!";
                        return Page();
                    }

                    using (var stream = new FileStream(filePath, FileMode.Create))
                    {
                        await Input.File.CopyToAsync(stream);
                    }
                }
                else
                {
                    StatusMessage = "Please Select Resume To Upload!";
                    return Page();
                }

                // convert file to byte array format, update database
                using (var memoryStream = new MemoryStream())
                {
                    await Input.File.CopyToAsync(memoryStream);
                    user.ResumeFileName = Input.File.FileName;
                    user.ResumeContent = memoryStream.ToArray();

                    var resumeFileUploadResult = await _userManager.UpdateAsync(user);
                    if (!resumeFileUploadResult.Succeeded)
                    {
                        foreach (var error in resumeFileUploadResult.Errors)
                        {
                            ModelState.AddModelError(string.Empty, error.Description);
                        }
                        StatusMessage = "Resume Upload Failed!";
                        return Page();
                    }
                }
                await _signInManager.RefreshSignInAsync(user);
                StatusMessage = "Resume Uploaded!";
                return RedirectToPage();
            }

            return RedirectToPage();
        }

        public static string GetMimeType(byte[] fileByte, string fileName)
        {
            // Default Unknown Mime Type
            string mimeType = "application/octet-stream"; 

            // Ensure fileName is not null or empty
            if (string.IsNullOrWhiteSpace(fileName))
            {
                return mimeType;
            }

            //Get file extension
            string extension = Path.GetExtension(fileName) == null
                                   ? string.Empty
                                   : Path.GetExtension(fileName).ToUpper();

            //Get the Mime Type
            if (fileByte.Take(8).SequenceEqual(DOC))
            {
                mimeType = "application/msword";
            }
            else if (fileByte.Take(4).SequenceEqual(ZIP_DOCX))
            {
                mimeType = extension == ".DOCX" ? "application/vnd.openxmlformats-officedocument.wordprocessingml.document" : "application/x-zip-compressed";
            }
            else if (fileByte.Take(7).SequenceEqual(PDF))
            {
                mimeType = "application/pdf";
            }

            return mimeType;
        }
    }
}