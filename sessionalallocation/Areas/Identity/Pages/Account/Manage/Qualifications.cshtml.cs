using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;
using SessionalAllocation.Models;
using System.ComponentModel.DataAnnotations;
using System.Threading.Tasks;

namespace SessionalAllocation.Areas.Identity.Pages.Account.Manage
{
    public class QualificationsModel : PageModel
    {
        private readonly UserManager<ApplicationUser> _userManager;
        private readonly SignInManager<ApplicationUser> _signInManager;

        // Enum List. Used for Applicant to select their Highest Level of Qualification


        public QualificationsModel(
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
            [EnumDataType(typeof(HighestQualificationType))]
            public HighestQualificationType QualificationType { get; set; }
            public string QualificationName { get; set; }
            [Display(Name = "Completed Year")]
            [StringLength(4)]
            [RegularExpression("([1-9][0-9]*)",
            ErrorMessage = "Please enter a valid year")]
            public string QualificationCompletionYear { get; set; }

            public string CurrentStudyingQualification { get; set; }
            public CurrentStudyingQualType CurrentQualificationType { get; set; }
            public string isStudyasInternationalStudent { get; set; }
            public bool StudyingAtSwinburne { get; set; }
            public string AreYouStudying { get; set; }
            public bool UserFullyRegistered { get; set; }
            public string YouTubeURL { get; set; }
        }

        public async Task<IActionResult> OnGetAsync()
        {
            var user = await _userManager.GetUserAsync(User);
            if (user == null)
            {
                return NotFound($"Unable to load user with ID '{_userManager.GetUserId(User)}'.");
            }

            Input = new InputModel
            {
                QualificationType = user.Qualification,
                QualificationName = user.QualificationName,
                QualificationCompletionYear = user.QulificationCompletionYear,
                CurrentStudyingQualification = user.CurrentStudyingQualification,
                CurrentQualificationType = user.CurrentQualificationType,
                isStudyasInternationalStudent = user.CitizenshipStudyStatus,
                StudyingAtSwinburne = user.StudyingAtSwinburne,
                AreYouStudying = user.AreYouStudying,
                UserFullyRegistered = user.UserFullyRegistered,
                YouTubeURL = user.YoutubeUrl
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

            user.Qualification = Input.QualificationType;

            var qualificationName = user.QualificationName;
            if (Input.QualificationName != qualificationName)
            {
                user.QualificationName = Input.QualificationName;
                // Error checking will need to be added here to ensure in the event something fails it updates.
            }
            var qulificationCompletionYear = user.QulificationCompletionYear;
            if (Input.QualificationCompletionYear != qulificationCompletionYear)
            {
                user.QulificationCompletionYear = Input.QualificationCompletionYear;
                // Error checking will need to be added here to ensure in the event something fails it updates.
            }
            var currentStudyingQualification = user.CurrentStudyingQualification;
            if (Input.CurrentStudyingQualification != user.CurrentStudyingQualification)
            {
                user.CurrentStudyingQualification = Input.CurrentStudyingQualification;
                // Error checking will need to be added here to ensure in the event something fails it updates.
            }
            var currentQualificationType = user.CurrentQualificationType;
            if (Input.CurrentQualificationType != user.CurrentQualificationType)
            {
                user.CurrentQualificationType = Input.CurrentQualificationType;
                // Error checking will need to be added here to ensure in the event something fails it updates.
            }
            var citizenshipStudyStatus = user.CitizenshipStudyStatus;
            if (Input.isStudyasInternationalStudent != user.CitizenshipStudyStatus)
            {
                user.CitizenshipStudyStatus = Input.isStudyasInternationalStudent;
                // Error checking will need to be added here to ensure in the event something fails it updates.
            }
            var studyingAtSwinburne = user.StudyingAtSwinburne;
            if (Input.StudyingAtSwinburne != user.StudyingAtSwinburne)
            {
                user.StudyingAtSwinburne = Input.StudyingAtSwinburne;
                // Error checking will need to be added here to ensure in the event something fails it updates.
            }
            var areYouStudying = user.AreYouStudying;
            if (Input.AreYouStudying != user.AreYouStudying)
            {
                user.AreYouStudying = Input.AreYouStudying;
                // Error checking will need to be added here to ensure in the event something fails it updates.
            }

            if (Input.YouTubeURL != user.YoutubeUrl) { user.YoutubeUrl = Input.YouTubeURL; }


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