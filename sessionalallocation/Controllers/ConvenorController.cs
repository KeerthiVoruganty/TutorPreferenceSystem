using System;
using System.Collections.Generic;
using System.Globalization;
using System.IO;
using System.Linq;
using System.Security.Claims;
using System.Threading.Tasks;
using DocumentFormat.OpenXml;
using DocumentFormat.OpenXml.Packaging;
using DocumentFormat.OpenXml.Spreadsheet;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Hosting;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Rendering;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Internal;
using SessionalAllocation.Data;
using SessionalAllocation.Models;
using SessionalAllocation.ViewModels;

namespace SessionalAllocation.Controllers
{
    [Authorize(Roles = "Convenor")]
    public class ConvenorController : Controller
    {
        private readonly ApplicationDbContext _context;
        private readonly IHostingEnvironment _hostingEnvironment;
        public ConvenorController(ApplicationDbContext context, IHostingEnvironment hostingEnvironment)
        {
            _context = context;
            _hostingEnvironment = hostingEnvironment;
        }
        private void FillRatings()
        {
            List<SelectListItem> ratings = new List<SelectListItem>();
            for (int i = 0; i <= 10; i++)
            {
                ratings.Add(new SelectListItem()
                {
                    Text = i.ToString(),
                    Value = i.ToString()
                });
            }

            ViewBag.LoadRatings = ratings;
        }

        public async Task<IActionResult> Index()
        {
            var ident = User.Identity as ClaimsIdentity;
            var userId = ident.Claims.FirstOrDefault(c => c.Type == ClaimTypes.NameIdentifier)?.Value;
            var units = _context.Unit.Include(u => u.DepartmentNavigation).Include(u => u.UnitOwnerNavigation).Where(u => u.UnitOwner == userId);
            return View(await units.ToListAsync());
        }

        // Retrieve Classes for specified unit
        public async Task<IActionResult> Classes(int? id, string filter)
        {
            var unitDbContext = _context.Class.Include(AspNetUsers => AspNetUsers.TutorAllocatedNavigation).Include(Unit => Unit.Unit).Where(Class => Class.UnitId == id);
            if (filter == "Allocated")
            {
                unitDbContext = _context.Class.Include(AspNetUsers => AspNetUsers.TutorAllocatedNavigation).Include(Unit => Unit.Unit).Where(Class => Class.UnitId == id && Class.Allocated);
               var unitDbContext1 = _context.Class.Include(AspNetUsers => AspNetUsers.TutorAllocatedNavigation).Include(Unit => Unit.Unit).Where(Class => Class.UnitId == id && Class.Allocated);
            }   
             if (filter == "NotAllocated")
            {
                unitDbContext = _context.Class.Include(AspNetUsers => AspNetUsers.TutorAllocatedNavigation).Include(Unit => Unit.Unit).Where(Class => Class.UnitId == id && !Class.Allocated);
            }
            return View(await unitDbContext.OrderBy(x => x.StartDate).ThenBy(x => x.StartTimeScheduled).ToListAsync());
        }
        // Convener nomination of applicant
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Nominate(int id)
        {
            // get the application details, class details
            Applications app = await _context.Applications.FindAsync(id);
            Class aClass = await _context.Class.Include(u => u.TutorAllocatedNavigation).FirstOrDefaultAsync(c => c.Id == app.AppliedClass);

            // setting previous applicant details if exists
            Applications prevApp = null;
            if (aClass.Allocated)
            {
                prevApp = _context.Applications.Where(a => a.AppliedClass == aClass.Id && a.Applicant == aClass.TutorAllocatedNavigation.Id).First();
                prevApp.ProvisionallyAllocated = false;
                prevApp.Approved = false;
            }

            aClass.TutorAllocated = app.Applicant;
            app.ProvisionallyAllocated = true;
            aClass.Allocated = true;

            if (ModelState.IsValid)
            {
                _context.Update(aClass);
                _context.Update(app);
                if (prevApp != null)
                {
                    _context.Update(prevApp);
                }
                await _context.SaveChangesAsync();
            }
            return RedirectToAction("Classes", new { Id = aClass.UnitId });
        }

        // Retrieve Applicants for specified class
        public async Task<IActionResult> Applicants(int? id)
        {
            var applicants = _context.Applications.Include(a => a.ApplicantNavigation).Include(a => a.AppliedClassNavigation).Where(Applications => Applications.AppliedClass == id);
            var ident = User.Identity as ClaimsIdentity;
            var convenorId = ident.Claims.FirstOrDefault(c => c.Type == ClaimTypes.NameIdentifier)?.Value;
            List<ApplicantsViewModel> model = new List<ApplicantsViewModel>();
            foreach (var a in applicants)
            {
                var rating = _context.TutorPreference.Where(r => r.TutorId == a.Applicant && r.ConvenorId == convenorId).FirstOrDefault();
                ApplicantsViewModel temp = new ApplicantsViewModel();
                temp.Application = a;
                if (rating != null)
                {
                    temp.rating = rating.rating;
                }

                model.Add(temp);
            }
            FillRatings();
            // sort by rating and preference
            model.Sort((x, y) => y.Application.Preference.CompareTo(x.Application.Preference));
            model.Sort((x, y) => y.rating.CompareTo(x.rating));
            return View(model);
        }

        // function to update tutor ratings
        public async Task<IActionResult> UpdateRating(string id)
        {
            var applicant = await _context.Users.FindAsync(id);
            var ident = User.Identity as ClaimsIdentity;
            var convenorId = ident.Claims.FirstOrDefault(c => c.Type == ClaimTypes.NameIdentifier)?.Value;

            // get the relevant TutorPreference
            var tutorPreference = _context.TutorPreference.Where(t => t.ConvenorId == convenorId && t.TutorId == applicant.Id).FirstOrDefault();
            // set convenor rating for tutor if doesn't exist

            // tutorPreference will be null if no rating exists for that convenor (can happen if user apply to class for a unit with no convenor set)
            if (tutorPreference == null)
            {
                tutorPreference = new TutorPreference // constructor sets rating to 5 by default
                {
                    ConvenorId = convenorId,
                    TutorId = applicant.Id
                };
            }

            // get value from text field
            var ratingValue = Request.Form["selectPref"];
            // get classId for redirection
            var classId = Request.Form["classId"];
            // update rating
            tutorPreference.rating = Int32.Parse(ratingValue);

            _context.Update(tutorPreference);
            await _context.SaveChangesAsync();

            return RedirectToAction("Applicants", new { id = Int32.Parse(classId) });
        }

        [HttpGet]
        public async Task<IActionResult> TutorPayrates(int? id)
        {
            if (id == null)
            {
                return NotFound();
            }

            // get selected unit details
            var unit = await _context.Unit.FindAsync(id);
            // get all classes with an approved tutor
            var classes = await _context.Class.Include(c => c.TutorAllocatedNavigation).Where(Class => Class.UnitId == id && Class.Allocated && Class.Approved).ToListAsync();
            var payrates = await _context.Payrate.ToListAsync();
            // payrates dictionary
            //Dictionary<string, Payrate> payrateValues = new Dictionary<string, Payrate>();

            //foreach (Payrate p in payrates)
            //{
            //    payrateValues.Add(p.Code, p);
            //}

            if (unit == null)
            {
                return NotFound();
            }

            var model = new UnitTutorsViewModel
            {
                UnitCode = unit.UnitCode,
                Tutors = new Dictionary<int, TutorPayrateViewModel>()
            };

            foreach (Class c in classes)
            {
                // get calendar. Used to get week # of year
                //https://docs.microsoft.com/en-us/dotnet/api/system.globalization.calendar.getweekofyear?redirectedfrom=MSDN&view=netframework-4.8#System_Globalization_Calendar_GetWeekOfYear_System_DateTime_System_Globalization_CalendarWeekRule_System_DayOfWeek_
                CultureInfo myCI = new CultureInfo("en-AU");
                Calendar myCal = myCI.Calendar;
                CalendarWeekRule myCWR = myCI.DateTimeFormat.CalendarWeekRule;
                DayOfWeek myFirstDOW = myCI.DateTimeFormat.FirstDayOfWeek;
                int startWeek = myCal.GetWeekOfYear(c.StartDate, myCWR, myFirstDOW);
                int endWeek = startWeek + 12;   // data does not have end week, just assuming 12 weeks of classes + mid semester break

                TutorPayrateViewModel temp = new TutorPayrateViewModel
                {
                    Weeks = new Dictionary<int, bool>(),
                    ClassStartDate = c.DateOnlyString,
                    ClassStartTime = c.StartTimeScheduled,
                    ClassDuration = c.EndTimeScheduled - c.StartTimeScheduled,
                    ClassType = c.ClassType,
                    ClassDayOfWeek = c.DayOfWeek,
                    TutorFullName = c.TutorAllocatedNavigation.LastName + ", " + c.TutorAllocatedNavigation.FirstName,
                    NewStaff = false,
                    TutorId = c.TutorAllocated,
                    TutorFirstName = c.TutorAllocatedNavigation.FirstName,
                    TutorLastName = c.TutorAllocatedNavigation.LastName,
                    TutorEmail = c.TutorAllocatedNavigation.Email,
                    TutorAddress = c.TutorAllocatedNavigation.Street,
                    TutorSuburb = c.TutorAllocatedNavigation.City,
                    TutorPostCode = c.TutorAllocatedNavigation.PostalCode,
                    TutorMobileNumber = c.TutorAllocatedNavigation.PhoneNumber,
                };

                for (int i = startWeek; i <= endWeek; i++)
                {
                    temp.Weeks.Add(i, true); // assume all weeks are teaching weeks for now, allow user to modify with checkboxes
                }
                if (c.TutorAllocatedNavigation.Qualification.ToString() == "PhD")
                {
                    temp.StaffStatus = "Sessional with PhD";
                }
                else
                {
                    temp.StaffStatus = "Sessional without PhD";
                }

                if (c.ClassType.Contains("Lecture"))
                {
                    // not quite sure what requirements are for other lecture payrates, setting to lecturing repeat for now
                    temp.PayrateCode = "LD";
                }
                else if (c.ClassType.Contains("Tutorial") || c.ClassType.Contains("Workshop") || c.ClassType.Contains("Practical") || c.ClassType.Contains("Demonstration") || c.ClassType.Contains("Lab"))
                { // may have missed some class types
                    if (temp.StaffStatus.Equals("Sessional with PhD"))
                    {
                        temp.PayrateCode = "TH";
                    }
                    else
                    {
                        temp.PayrateCode = "TF";
                    }
                }
                model.Tutors.Add(c.Id, temp);
            }
            ViewData["Payrates"] = new SelectList(_context.Payrate, "Code", "Code");
            return View(model);
        }

        [HttpPost]
        public IActionResult TutorPayrates(UnitTutorsViewModel model)
        {
            ViewData["Payrates"] = new SelectList(_context.Payrate, "Code", "Code");
            return View(model);
        }

        [HttpPost]
        public IActionResult TutorPayratesConfirm(UnitTutorsViewModel model)
        {
            return View(model);
        }

        [HttpPost]
        public async Task<IActionResult> GeneratePayratesFile(UnitTutorsViewModel model)
        {
            // set file name
            var fileName = "StaffPayrates_" + model.UnitCode + ".xlsx";
            var filePath = Path.Combine(_hostingEnvironment.WebRootPath, fileName);
            using (SpreadsheetDocument spreadSheetDocument = SpreadsheetDocument.Create(filePath, SpreadsheetDocumentType.Workbook))
            {
                // create woorkbookpart
                WorkbookPart workbookPart = spreadSheetDocument.AddWorkbookPart();

                spreadSheetDocument.WorkbookPart.Workbook = new Workbook();
                spreadSheetDocument.WorkbookPart.Workbook.Sheets = new Sheets();

                // styles part
                WorkbookStylesPart stylesPart = spreadSheetDocument.WorkbookPart.AddNewPart<WorkbookStylesPart>();
                stylesPart.Stylesheet = GenerateStyleSheet();
                stylesPart.Stylesheet.Save();


                //create worksheetPart Teaching events
                WorksheetPart worksheetPartTeachingEvents = workbookPart.AddNewPart<WorksheetPart>();
                SheetData sheetDataTeachingEvents = new SheetData();
                worksheetPartTeachingEvents.Worksheet = new Worksheet();//sheetDataTeachingEvents);

                // set column widths
                Columns colsTeachingEvents = TeachingEventsColumns();
                worksheetPartTeachingEvents.Worksheet.Append(colsTeachingEvents);
                worksheetPartTeachingEvents.Worksheet.Append(sheetDataTeachingEvents);

                // create merge cells for O1-T1
                MergeCells mergeCells = new MergeCells();

                mergeCells.Append(new MergeCell() { Reference = new StringValue("O1:T1") });

                worksheetPartTeachingEvents.Worksheet.Append(mergeCells);
                // create sheet
                Sheets sheets = spreadSheetDocument.WorkbookPart.Workbook.GetFirstChild<Sheets>();
                Sheet sheetTeachingEvents = new Sheet()
                {
                    Id = spreadSheetDocument.WorkbookPart.GetIdOfPart(worksheetPartTeachingEvents),
                    SheetId = 1,
                    Name = "1. Teaching Events"
                };
                sheets.Append(sheetTeachingEvents);

                // create worksheetPart New or Edited Staff Details
                WorksheetPart worksheetPartNewStaff = workbookPart.AddNewPart<WorksheetPart>();
                SheetData sheetDataNewStaff = new SheetData();
                worksheetPartNewStaff.Worksheet = new Worksheet();

                Columns colsNewStaff = NewStaffColumns();
                worksheetPartNewStaff.Worksheet.Append(colsNewStaff);
                worksheetPartNewStaff.Worksheet.Append(sheetDataNewStaff);

                // create merge cells for O1-T1
                MergeCells mergeCellsNewStaff = new MergeCells();

                mergeCellsNewStaff.Append(new MergeCell() { Reference = new StringValue("A1:G1") });
                worksheetPartNewStaff.Worksheet.Append(mergeCellsNewStaff);

                // create sheet
                Sheet sheetNewStaff = new Sheet()
                {
                    Id = spreadSheetDocument.WorkbookPart.GetIdOfPart(worksheetPartNewStaff),
                    SheetId = 2,
                    Name = "2. New or Edited Staff Details"
                };
                sheets.Append(sheetNewStaff);

                // header row Teaching Events
                Row headers = new Row();
                // column A
                headers.Append(StringCell("Subject Code", 3));
                // column B
                headers.Append(StringCell("Class Type", 3));
                // column C (blank column), need to change format
                headers.Append(EmptyCell(6));
                // column D
                headers.Append(StringCell("Day of week", 3));
                // column E
                headers.Append(StringCell("Start Time", 3));
                // column F
                headers.Append(StringCell("Duration", 3));
                // column G (blank column)
                headers.Append(EmptyCell(6));
                // column H
                headers.Append(StringCell("Staff Name", 3));
                // column I (blank column)
                headers.Append(EmptyCell(6));
                // column J
                headers.Append(StringCell("Weeks", 3));
                // column K
                headers.Append(StringCell("Pay rate", 3));// payrate code
                // column L (blank column)
                headers.Append(EmptyCell(6));
                // column M
                headers.Append(StringCell("Staff Status", 3));
                // column N (blank column)
                headers.Append(EmptyCell(6));
                // column O
                headers.Append(StringCell("FACULTY SUPPORT STAFF USE ONLY - PLEASE REFRAIN FROM AMENDING COLUMNS O-T", 7));

                // add the headers to the sheet
                sheetDataTeachingEvents.AppendChild(headers);

                List<int> blackCol = new List<int> { 3, 7, 9, 12, 14 };
                // Row 2 displays headers for faculty staff use
                Row row2 = new Row();
                for (int i = 0; i < 14; i++) // column A-N
                {
                    if (blackCol.Contains(i + 1))
                    {
                        row2.Append(EmptyCell(6));
                    }
                    else
                    {
                        row2.Append(EmptyCell(3));
                    }
                }
                // column O
                row2.Append(StringCell("Pay Rate", 1));
                // column P
                row2.Append(StringCell("No. of sessions", 1));
                // column Q
                row2.Append(StringCell("Hours", 1));
                // column R
                row2.Append(StringCell("Cost", 1));
                // column S
                row2.Append(StringCell("Cost inc. on-costs", 1));
                // column T
                row2.Append(StringCell("Notes/Comments", 1));

                // add the row to the sheet
                sheetDataTeachingEvents.AppendChild(row2);

                // warning row for NewStaff sheet
                Row warningRow = new Row();
                warningRow.Append(StringCell("**ONLY COMPLETE THIS SECTION FOR NEW SESSIONAL STAFF WHO ARE NOT ON SWINBURNE'S PAYROLL", 13));
                sheetDataNewStaff.AppendChild(warningRow);
                // Header data for NewStaff sheet
                Row headersNewStaff = new Row();
                // Column A
                headersNewStaff.Append(StringCell("Surname", 4));
                // Column B
                headersNewStaff.Append(StringCell("FirstName", 4));
                // Column C
                headersNewStaff.Append(StringCell("Email", 4));
                // Column D
                headersNewStaff.Append(StringCell("Address", 4));
                // Column E
                headersNewStaff.Append(StringCell("Suburb", 4));
                // Column F
                headersNewStaff.Append(StringCell("Post Code", 4));
                // Column G
                headersNewStaff.Append(StringCell("Home Phone", 4));
                // Column H
                headersNewStaff.Append(StringCell("Work Phone", 4));
                // Column I
                headersNewStaff.Append(StringCell("Mobile Phone", 4));
                sheetDataNewStaff.AppendChild(headersNewStaff);

                foreach (TutorPayrateViewModel tutor in model.Tutors.Values)
                {

                    // get weeks that tutor is teaching
                    List<int> teachingWeeks = new List<int>();
                    foreach (var kv in tutor.Weeks)
                    {
                        if (kv.Value)
                        {
                            teachingWeeks.Add(kv.Key);
                        }
                    }

                    Row row = new Row();
                    // column A
                    row.Append(StringCell(model.UnitCode, 2));
                    // column B
                    row.Append(StringCell(tutor.ClassType, 2));
                    // column C (blank column)
                    row.Append(EmptyCell(6));
                    // column D 
                    row.Append(StringCell(tutor.ClassDayOfWeek, 2));
                    // column E
                    row.Append(StringCell(tutor.ClassStartTime.ToString(@"hh\:mm"), 2));
                    // column F
                    row.Append(NumberCell(tutor.ClassDuration.TotalMinutes.ToString(), 2));
                    // column G (blank column)
                    row.Append(EmptyCell(6));
                    // column H
                    row.Append(StringCell(tutor.TutorFullName, 2));
                    // column I (blank column)
                    row.Append(EmptyCell(6));
                    // column J
                    row.Append(StringCell(string.Join(',', teachingWeeks.ToArray()), 2));
                    // column K
                    row.Append(StringCell(tutor.PayrateCode, 2));
                    // column L (blank column)
                    row.Append(EmptyCell(6));
                    // column M 
                    row.Append(StringCell(tutor.StaffStatus, 2));
                    // column N (blank column)
                    row.Append(EmptyCell(6));
                    // column O
                    Payrate payrate = _context.Payrate.Where(c => c.Code == tutor.PayrateCode).FirstOrDefault();
                    row.Append(NumberCell(payrate.Rate.ToString(), 11));
                    // add formating to next 5 rows
                    for (int i = 0; i < 5; i++)
                    {
                        row.Append(EmptyCell(8));
                    }
                    sheetDataTeachingEvents.AppendChild(row);

                    // new staff
                    if (tutor.NewStaff)
                    {
                        Row rowNewStaff = new Row();
                        // Column A
                        rowNewStaff.Append(StringCell(tutor.TutorLastName));
                        // Column B
                        rowNewStaff.Append(StringCell(tutor.TutorFirstName));
                        // Column C
                        rowNewStaff.Append(StringCell(tutor.TutorEmail));
                        // Column D
                        rowNewStaff.Append(StringCell(tutor.TutorAddress));
                        // Column E
                        rowNewStaff.Append(StringCell(tutor.TutorSuburb));
                        // Column F
                        rowNewStaff.Append(StringCell(tutor.TutorPostCode));
                        // Column G
                        rowNewStaff.Append(EmptyCell());
                        // Column H
                        rowNewStaff.Append(EmptyCell());
                        // Column I
                        rowNewStaff.Append(StringCell(tutor.TutorMobileNumber));
                        sheetDataNewStaff.AppendChild(rowNewStaff);
                    }
                }
                // empty row between staff details and total stuff
                Row emptyRow = new Row();
                sheetDataTeachingEvents.AppendChild(emptyRow);

                Row TotalRow = new Row();
                for (int i = 0; i < 14; i++) // column A-N
                {
                    TotalRow.Append(EmptyCell());
                }
                // column O
                TotalRow.Append(StringCell("TOTAL", 9));
                // column P
                Cell noSessions = NumberCell("0.00", 10);
                int totalStaff = model.Tutors.Count;
                CellFormula noSessionsFormula = new CellFormula();
                noSessionsFormula.Text = "SUM(P3:P" + (3 + totalStaff - 1) + ")";
                noSessions.CellFormula = noSessionsFormula;
                TotalRow.Append(noSessions);
                // column Q
                Cell hours = NumberCell("0.00", 10);
                CellFormula hoursFormula = new CellFormula();
                hoursFormula.Text = "SUM(Q3:Q" + (3 + totalStaff - 1) + ")";
                hours.CellFormula = hoursFormula;
                TotalRow.Append(hours);
                // column R
                Cell cost = NumberCell("0.00", 12);
                CellFormula costFormula = new CellFormula();
                costFormula.Text = "SUM(R3:R" + (3 + totalStaff - 1) + ")";
                cost.CellFormula = costFormula;
                TotalRow.Append(cost);
                // column S
                Cell costInc = NumberCell("0.00", 12);
                CellFormula costIncFormula = new CellFormula();
                costIncFormula.Text = "SUM(S3:S" + (3 + totalStaff - 1) + ")";
                costInc.CellFormula = costIncFormula;
                TotalRow.Append(costInc);

                sheetDataTeachingEvents.AppendChild(TotalRow);
            }

            // Download the created file
            if (fileName != null)
            {
                var memory = new MemoryStream();
                using (var stream = new FileStream(filePath, FileMode.Open))
                {
                    await stream.CopyToAsync(memory);
                }
                memory.Position = 0;
                System.IO.File.Delete(filePath);
                return File(memory, "application/vnd.openxmlformats-officedocument.spreadsheetml.sheet", fileName);
            }
            return RedirectToAction(nameof(Index));
        }

        private Cell StringCell(string value, UInt32 styleIndex = 0)
        {
            Cell cell = new Cell();
            cell.DataType = CellValues.String;
            cell.CellValue = new CellValue(value);
            cell.StyleIndex = styleIndex;
            return cell;
        }

        private Cell NumberCell(string value, UInt32 styleIndex = 0)
        {
            Cell cell = new Cell();
            cell.DataType = CellValues.Number;
            cell.CellValue = new CellValue(value);
            cell.StyleIndex = styleIndex;
            return cell;
        }

        private Cell EmptyCell(UInt32 styleIndex = 0)
        {
            return new Cell() { StyleIndex = styleIndex };
        }

        private Columns TeachingEventsColumns()
        {
            Columns cols = new Columns();

            cols.Append(new Column() { Min = 1, Max = 1, Width = 10, CustomWidth = true });
            cols.Append(new Column() { Min = 2, Max = 2, Width = 16, CustomWidth = true });
            cols.Append(new Column() { Min = 3, Max = 3, Width = 0.5, CustomWidth = true });
            cols.Append(new Column() { Min = 4, Max = 4, Width = 12.5, CustomWidth = true });
            cols.Append(new Column() { Min = 5, Max = 5, Width = 7.5, CustomWidth = true });
            cols.Append(new Column() { Min = 6, Max = 6, Width = 8.6, CustomWidth = true });
            cols.Append(new Column() { Min = 7, Max = 7, Width = 0.5, CustomWidth = true });
            cols.Append(new Column() { Min = 8, Max = 8, Width = 34.5, CustomWidth = true });
            cols.Append(new Column() { Min = 9, Max = 9, Width = 0.5, CustomWidth = true });
            cols.Append(new Column() { Min = 10, Max = 10, Width = 33.5, CustomWidth = true });
            cols.Append(new Column() { Min = 11, Max = 11, Width = 8, CustomWidth = true });
            cols.Append(new Column() { Min = 12, Max = 12, Width = 0.5, CustomWidth = true });
            cols.Append(new Column() { Min = 13, Max = 13, Width = 28, CustomWidth = true });
            cols.Append(new Column() { Min = 14, Max = 14, Width = 2.5, CustomWidth = true });
            cols.Append(new Column() { Min = 15, Max = 15, Width = 12, CustomWidth = true });
            cols.Append(new Column() { Min = 16, Max = 16, Width = 18, CustomWidth = true });
            cols.Append(new Column() { Min = 17, Max = 17, Width = 6, CustomWidth = true });
            cols.Append(new Column() { Min = 18, Max = 18, Width = 10.5, CustomWidth = true });
            cols.Append(new Column() { Min = 19, Max = 19, Width = 12, CustomWidth = true });
            cols.Append(new Column() { Min = 20, Max = 20, Width = 16.5, CustomWidth = true });

            return cols;
        }

        private Columns NewStaffColumns()
        {
            Columns cols = new Columns();

            cols.Append(new Column() { Min = 1, Max = 1, Width = 14, CustomWidth = true });
            cols.Append(new Column() { Min = 2, Max = 2, Width = 29.5, CustomWidth = true });
            cols.Append(new Column() { Min = 3, Max = 3, Width = 14, CustomWidth = true });
            cols.Append(new Column() { Min = 4, Max = 4, Width = 15, CustomWidth = true });
            cols.Append(new Column() { Min = 5, Max = 5, Width = 15.5, CustomWidth = true });
            cols.Append(new Column() { Min = 6, Max = 6, Width = 13.5, CustomWidth = true });
            cols.Append(new Column() { Min = 7, Max = 7, Width = 14.5, CustomWidth = true });
            cols.Append(new Column() { Min = 8, Max = 8, Width = 14.5, CustomWidth = true });
            cols.Append(new Column() { Min = 9, Max = 9, Width = 14.5, CustomWidth = true });

            return cols;
        }

        //https://blogs.msdn.microsoft.com/chrisquon/2009/11/30/stylizing-your-excel-worksheets-with-open-xml-2-0/
        private Stylesheet GenerateStyleSheet()
        {
            return new Stylesheet(
                new Fonts(
                    new Font(                                                               // Index 0 - The default font.
                        new FontSize() { Val = 10 },
                        new Color() { Rgb = new HexBinaryValue() { Value = "000000" } },
                        new FontName() { Val = "Arial" }),
                    new Font(                                                               // Index 1 - The bold font. Used for faculty stuff headers
                        new Bold(),
                        new FontSize() { Val = 10 },
                        new Color() { Rgb = new HexBinaryValue() { Value = "000000" } },
                        new FontName() { Val = "Arial" }),
                    new Font(                                                               // Index 2 - The Italic font.
                        new Italic(),
                        new FontSize() { Val = 10 },
                        new Color() { Rgb = new HexBinaryValue() { Value = "000000" } },
                        new FontName() { Val = "Arial" }),
                    new Font(
                        new Italic(),                                                      // Index 3 - Bold and Italic and blue, used for headers
                        new Bold(),
                        new FontSize() { Val = 10 },
                        new Color() { Rgb = new HexBinaryValue() { Value = "0000CC" } },
                        new FontName() { Val = "Arial" }),
                    new Font(
                        new Bold(),                                                       // Index 4 - Bold and red
                        new FontSize() { Val = 10 },
                        new Color() { Rgb = new HexBinaryValue() { Value = "EE0000" } },
                        new FontName() { Val = "Arial" }),
                    new Font(
                        new Bold(),                                                       // Index 5 - Bold and Blue
                        new FontSize() { Val = 10 },
                        new Color() { Rgb = new HexBinaryValue() { Value = "0000CC" } },
                        new FontName() { Val = "Arial" })
                ),
                new Fills(
                    new Fill(                                                           // Index 0 - The default fill.
                        new PatternFill() { PatternType = PatternValues.None }),
                    new Fill(                                                           // Index 1 - The default fill of gray 125 (required)
                        new PatternFill() { PatternType = PatternValues.Gray125 }),
                    new Fill(                                                           // Index 2 - The blue fill.
                        new PatternFill(
                            new ForegroundColor() { Rgb = new HexBinaryValue() { Value = "CCFFFF" } }
                        )
                        { PatternType = PatternValues.Solid }),
                    new Fill(
                        new PatternFill(                                                // Index 3 - The black fill.
                            new ForegroundColor() { Rgb = new HexBinaryValue() { Value = "000000" } }
                        )
                        { PatternType = PatternValues.Solid })
                ),
                new Borders(
                    new Border(                                                         // Index 0 - The default border.
                        new LeftBorder(),
                        new RightBorder(),
                        new TopBorder(),
                        new BottomBorder(),
                        new DiagonalBorder()),
                    new Border(                                                         // Index 1 - Applies a Left, Right, Top, Bottom border to a cell, used in Headers
                        new LeftBorder(
                            new Color() { Auto = true }
                        )
                        { Style = BorderStyleValues.Thin },
                        new RightBorder(
                            new Color() { Auto = true }
                        )
                        { Style = BorderStyleValues.Thin },
                        new TopBorder(
                            new Color() { Auto = true }
                        )
                        { Style = BorderStyleValues.Thin },
                        new BottomBorder(
                            new Color() { Auto = true }
                        )
                        { Style = BorderStyleValues.Thin },
                        new DiagonalBorder()),
                    new Border(
                        new LeftBorder(                                               // Index 2 - Dotted border for table data
                            new Color() { Auto = true }
                        )
                        { Style = BorderStyleValues.Thin },
                        new RightBorder(
                            new Color() { Auto = true }
                        )
                        { Style = BorderStyleValues.Thin },
                        new TopBorder(
                            new Color() { Auto = true }
                        )
                        { Style = BorderStyleValues.Hair },
                        new BottomBorder(
                            new Color() { Auto = true }
                        )
                        { Style = BorderStyleValues.Hair },
                        new DiagonalBorder()),
                    new Border(                                                         // Index 3, used for TOTAL line
                        new LeftBorder(),
                        new RightBorder(),
                        new TopBorder(
                            new Color() { Auto = true }
                        )
                        { Style = BorderStyleValues.Thin },
                        new BottomBorder(
                            new Color() { Auto = true }
                        )
                        { Style = BorderStyleValues.Medium },
                        new DiagonalBorder())

                ),
                new CellFormats(
                    new CellFormat() { FontId = 0, FillId = 0, BorderId = 0 },                                          // Index 0 - The default cell style.  If a cell does not have a style index applied it will use this style combination instead
                    new CellFormat(                                                                                     // Index 1 - Bold - Faculty Support headers
                        new Alignment() { Horizontal = HorizontalAlignmentValues.Center, Vertical = VerticalAlignmentValues.Bottom, WrapText = true }
                        )
                    { FontId = 1, FillId = 2, BorderId = 1, ApplyFont = true, ApplyFill = true },
                    new CellFormat() { FontId = 0, FillId = 0, BorderId = 2, ApplyFont = true },                        // Index 2 - Standard cell in table
                    new CellFormat(                                                                                     // Index 3 - Bold, Italic, blue - Other Headers
                        new Alignment() { Horizontal = HorizontalAlignmentValues.Center, Vertical = VerticalAlignmentValues.Bottom, WrapText = true }
                        )
                    { FontId = 3, FillId = 0, BorderId = 1, ApplyFont = true, ApplyAlignment = true, ApplyBorder = true },
                    new CellFormat(
                        new Alignment() { Vertical = VerticalAlignmentValues.Bottom, WrapText = true })
                    { FontId = 5, FillId = 0, BorderId = 0, ApplyFill = true },                        // Index 4 - New Staff format
                    new CellFormat(                                                                                     // Index 5 - Alignment
                        new Alignment() { Horizontal = HorizontalAlignmentValues.Center, Vertical = VerticalAlignmentValues.Center }
                    )
                    { FontId = 0, FillId = 0, BorderId = 0, ApplyAlignment = true },
                    new CellFormat() { FontId = 0, FillId = 3, BorderId = 1, ApplyBorder = true, ApplyFill = true },      // Index 6 - Border, black fill
                    new CellFormat(                                                                                       // Index 7 - Bold and red, used for Faculty Support Staff only warning
                        new Alignment() { Horizontal = HorizontalAlignmentValues.Center, Vertical = VerticalAlignmentValues.Center, WrapText = true }
                        )
                    { FontId = 4, FillId = 2, BorderId = 0, ApplyFill = true },
                    new CellFormat() { FontId = 0, FillId = 2, BorderId = 2, ApplyFont = true, ApplyFill = true },         // Index 8 - Standard cell in table Faculty
                    new CellFormat() { FontId = 0, FillId = 0, BorderId = 3, ApplyFont = true },                             // Index 9 - Cell used for TOTAL line stuff
                    new CellFormat() { FontId = 0, FillId = 0, BorderId = 3, ApplyFont = true, NumberFormatId = 2, ApplyNumberFormat = true },   // Index 10 - formatted number '0.00'
                    new CellFormat() { FontId = 0, FillId = 2, BorderId = 2, ApplyFont = true, ApplyFill = true, NumberFormatId = 2, ApplyNumberFormat = true },          // Index 11 - formatted number '0.00' "FacultyStaff" cell
                    new CellFormat() { FontId = 0, FillId = 0, BorderId = 3, ApplyFont = true, NumberFormatId = 44, ApplyNumberFormat = true },   // Index 12 - formatted accounting '0.00' //43
                    new CellFormat() { FontId = 4, FillId = 0, BorderId = 0 }                   // Index 13, Warning on newstaff sheet
                )
            ); ; // return
        }
    }
}