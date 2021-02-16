using DocumentFormat.OpenXml;
using DocumentFormat.OpenXml.Packaging;
using DocumentFormat.OpenXml.Spreadsheet;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Hosting;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using SessionalAllocation.Data;
using SessionalAllocation.Models;
using System;
using System.Collections.Generic;
using System.Data;
using System.Globalization;
using System.IO;
using System.Linq;
using System.Threading.Tasks;

// For more information on enabling MVC for empty projects, visit https://go.microsoft.com/fwlink/?LinkID=397860

namespace SessionalAllocation.Controllers
{
    // may need to change, currently assuming convenors can import timetable data
    [Authorize(Roles = "Admin")]
    public class TimetableImporterController : Controller
    {
        private readonly ApplicationDbContext _context;
        private readonly IHostingEnvironment _hostingEnvironment;

        public TimetableImporterController(ApplicationDbContext context, IHostingEnvironment hostingEnvironment)
        {
            _context = context;
            _hostingEnvironment = hostingEnvironment;
        }

        // GET: /<controller>/
        public IActionResult Index()
        {
            return View();
        }

        [HttpPost("UploadFiles")]
        public async Task<IActionResult> UploadFile(IFormFile excelFile)
        {
            if (excelFile != null)
            {
                // check if file is xlsx file
                if (string.Equals(excelFile.ContentType, "application/vnd.openxmlformats-officedocument.spreadsheetml.sheet", StringComparison.OrdinalIgnoreCase))
                {
                    if (excelFile.Length > 0)
                    {
                        var filePath = Path.Combine(_hostingEnvironment.WebRootPath, excelFile.FileName); // need to do some checks on fileName
                        using (var stream = new FileStream(filePath, FileMode.Create))
                        {
                            await excelFile.CopyToAsync(stream);
                        }
                        await ImportExcelToDatabase(filePath);
                        System.IO.File.Delete(filePath);
                    }
                }
                else
                {
                    TempData["StatusMessage"] = "Incorrect filetype, file must be .xlsx";
                }
            }
            return RedirectToAction(nameof(Index));
        }

        public async Task<IActionResult> DownloadFile(string fileName)
        {
            if (fileName != null)
            {
                var filePath = Path.Combine(_hostingEnvironment.WebRootPath, fileName);
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

        public async Task ImportExcelToDatabase(string filePath)
        {
            // store excel data in datatable
            DataTable data = ExcelToDatatable(filePath);

            // storage for failed rows
            DataTable failedRows = new DataTable();
            // store error messages in a list
            List<string> failedErrorMsgs = new List<string>();
            // set column headers for failedRows
            foreach (DataColumn column in data.Columns)
            {
                failedRows.Columns.Add(column.ColumnName, column.DataType);
            }
            await _context.Database.EnsureCreatedAsync();

            int successful = 0;
            int duplicate = 0;
            int failed = 0;
            foreach (DataRow row in data.Rows)
            {
                try
                {
                    // add Faculty Information
                    Faculty faculty = new Faculty();
                     faculty.Name = row["Availability Owning Org Unit Name"].ToString();
                    //Above COlumn does not exists in the excel provided and exception is thrown
                   //faculty.Name = row["Faculty"].ToString();   

                    int latestFacultyId;
                    if (!_context.Faculty.Any(x => x.Name == faculty.Name))                        
                    {
                        _context.Faculty.Add(faculty);
                        await _context.SaveChangesAsync();
                        latestFacultyId = faculty.Id;
                    }
                    else
                    {
                        latestFacultyId = _context.Faculty.ToList<Faculty>().Find(x => x.Name.Equals(faculty.Name)).Id;
                    }

                    // Add school information
                    School school = new School();
                    school.Faculty = latestFacultyId;
                    school.Name = row["Availability Teaching Parent Org Unit Name"].ToString();
                    int latestSchoolId;
                    if (!_context.School.Any(x => x.Name == school.Name))
                    {
                        _context.School.Add(school);
                        await _context.SaveChangesAsync();
                        latestSchoolId = school.Id;
                    }
                    else
                    {
                        latestSchoolId = _context.School.ToList<School>().Find(x => x.Name.Equals(school.Name)).Id;
                    }

                    // Add Department information
                    Department department = new Department();
                    department.School = latestSchoolId;
                    department.Name = row["Availability Teaching Org Unit Name"].ToString();
                    int latestDepartmentId;

                    if (!_context.Department.Any(x => x.Name == department.Name))
                    {
                        _context.Department.Add(department);
                        await _context.SaveChangesAsync();
                        latestDepartmentId = department.Id;
                    }
                    else
                    {
                        latestDepartmentId = _context.Department.ToList<Department>().Find(x => x.Name.Equals(department.Name)).Id;
                    }

                    // Add Unit information.
                    Unit unit = new Unit();
                    unit.Department = latestDepartmentId;
                    unit.UnitCode = row["Study Package Code"].ToString();
                    unit.UnitName = row["Unit Title"].ToString();
                    unit.UnitLink = row["Unit Link"].ToString();
                    int latestUnitId;
                    if (!_context.Unit.Any(x => x.UnitCode == unit.UnitCode))
                    {
                        _context.Unit.Add(unit);
                        await _context.SaveChangesAsync();
                        latestUnitId = unit.Id;
                    }
                    else
                    {
                        latestUnitId = _context.Unit.ToList<Unit>().Find(x => x.UnitCode.Equals(unit.UnitCode)).Id;
                    }

                    // Add Class information.
                    Class aClass = new Class();
                    aClass.UnitId = latestUnitId;
                    aClass.ClassType = row["Activities Activity Type"].ToString();
                    aClass.Location = row["Activities Location"].ToString();
                    aClass.Year = row["Activities Availability Year"].ToString();
                    aClass.StudyPeriod = row["Activities Study Period"].ToString();
                    aClass.DayOfWeek = row["Activities Class Start Day"].ToString();
                    CultureInfo culture = new CultureInfo("es-ES");
                    aClass.StartDate = DateTime.Parse(row["Activities Class Start Date"].ToString(),culture);
                    aClass.StartTimeScheduled = TimeSpan.Parse(row["Activities Class Start Time"].ToString());
                    aClass.EndTimeScheduled = TimeSpan.Parse(row["Activities Class Session Class End Time"].ToString());
                    aClass.roomDetails = row["Activities Class Building Id"].ToString() + row["Activities Class Room Id"].ToString();
                    
                    // Duplicate entry check. If this row already exist in the database, skip this row.
                    if (!_context.Class.Any(x =>
                        x.UnitId == aClass.UnitId
                        && x.ClassType == aClass.ClassType
                        && x.Location == aClass.Location
                        && x.Year == aClass.Year
                        && x.StudyPeriod == aClass.StudyPeriod
                        && x.DayOfWeek == aClass.DayOfWeek
                        && x.StartDate == aClass.StartDate
                        && x.StartTimeScheduled == aClass.StartTimeScheduled
                        && x.EndTimeScheduled == aClass.EndTimeScheduled 
                        && x.roomDetails == aClass.roomDetails))
                    {
                        _context.Class.Add(aClass);
                        await _context.SaveChangesAsync();
                        successful++;
                    }
                    else
                    {
                        duplicate++;
                    }

                }
                catch (Exception ex)
                {
                    failedRows.Rows.Add(row.ItemArray);
                    failed++;
                    failedErrorMsgs.Add(ex.Message);
                }
            }
            TempData["StatusMessage"] = successful + " rows entered successfully, " + duplicate + " duplicate entries ignored, " + failed + " failed.";
            if (failed > 0) // used to set link to download failed data
            {
                var fileName = FailedDataToExcel(failedRows, failedErrorMsgs);
                TempData["FileName"] = fileName;
            }
        }

        #region maincodeexceltodb
        private static DataTable ExcelToDatatable(string fileName)
        {
            DataTable dt = new DataTable();

            using (SpreadsheetDocument spreadSheetDocument = SpreadsheetDocument.Open(fileName, false))
            {
                WorkbookPart workbookPart = spreadSheetDocument.WorkbookPart;
                // should get user to select sheet, currently assuming correct sheet is the last, can change later

                //IEnumerable<Sheet> sheets = spreadSheetDocument.WorkbookPart.Workbook.GetFirstChild<Sheets>().Elements<Sheet>();
                //string relationshipId = sheets.First().Id.Value;
                //WorksheetPart worksheetPart = (WorksheetPart)spreadSheetDocument.WorkbookPart.GetPartById(relationshipId);
                WorksheetPart worksheetPart = workbookPart.WorksheetParts.Last();
                Worksheet workSheet = worksheetPart.Worksheet;
                SheetData sheetData = workSheet.GetFirstChild<SheetData>();
                IEnumerable<Row> rows = sheetData.Descendants<Row>();
               
                foreach (Cell cell in rows.ElementAt(0)) // header row for dt columns [it was 0 before and was reading header]
                {
                    dt.Columns.Add(GetCellValue(spreadSheetDocument, cell));
                }

                foreach (Row row in rows) // includes header row
                {
                    if (row.RowIndex != 1)
                    {

                        DataRow tempRow = dt.NewRow();
                        // 05/05/2020 replacing old

                        //for (int i = 0; i < row.Descendants<Cell>().Count(); i++)
                        //{
                        //    tempRow[i] = GetCellValue(spreadSheetDocument, row.Descendants<Cell>().ElementAt(i));
                        //}
                        for (int i = 0; i < row.Descendants<Cell>().Count(); i++)
                        {
                            Cell cell = row.Descendants<Cell>().ElementAt(i);
                            int actualCellIndex = CellReferenceToIndex(cell);
                            tempRow[actualCellIndex] = GetCellValue(spreadSheetDocument, cell);
                        }
                        dt.Rows.Add(tempRow);
                    }
                }
            }
            dt.Rows.RemoveAt(2); // remove header data from first row
            return dt;
        }
        #endregion

        #region CellReferenceToIndex
        private static int CellReferenceToIndex(Cell cell)
        {
            int index = 0;
            string reference = cell.CellReference.ToString().ToUpper();
            foreach (char ch in reference)
            {
                if (Char.IsLetter(ch))
                {
                    int value = (int)ch - (int)'A';
                    index = (index == 0) ? value : ((index + 1) * 26) + value;
                }
                else
                {
                    return index;
                }
            }
            return index;
        }
        #endregion
        private static string GetCellValue(SpreadsheetDocument document, Cell cell)
        {
           // string value = string.Empty;
            SharedStringTablePart stringTablePart = document.WorkbookPart.SharedStringTablePart;
            if (cell.CellValue == null)
            {
                return "";
            }
            string value = cell.CellValue.InnerXml;
            if (cell.DataType != null && cell.DataType == CellValues.SharedString)
            {
                return stringTablePart.SharedStringTable.ChildElements[Int32.Parse(value)].InnerText;
            }
           
            else
            {
                // get the cell format to see if it is stored as shortdate in excel
                int styleIndex = (int)cell.StyleIndex.Value;
                CellFormat cellFormat = (CellFormat)document.WorkbookPart.WorkbookStylesPart.Stylesheet.CellFormats.ElementAt(styleIndex);
                uint formatId = cellFormat.NumberFormatId.Value;
                // id for shortdate in excel is 14
                if (formatId == 14)
                {
                    // get date value
                    return DateTime.FromOADate(double.Parse(value)).ToString();
                }
                return value;
            }
        }

        private string FailedDataToExcel(DataTable dt, List<string> errMsgs)
        {
            // set file name to include the current date and time
            var currentTime = DateTime.Now.ToString("dd-MM-yyyy_HH-mm-ss");
            var fileName = "failed_" + currentTime + ".xlsx";
            var filePath = Path.Combine(_hostingEnvironment.WebRootPath, "failed_" + currentTime + ".xlsx");
            using (SpreadsheetDocument spreadSheetDocument = SpreadsheetDocument.Create(filePath, SpreadsheetDocumentType.Workbook))
            {
                // create woorkbookpart
                WorkbookPart workbookPart = spreadSheetDocument.AddWorkbookPart();
                //workbookPart.Workbook = new Workbook();

                spreadSheetDocument.WorkbookPart.Workbook = new Workbook();
                spreadSheetDocument.WorkbookPart.Workbook.Sheets = new Sheets();

                //create worksheetPart
                WorksheetPart worksheetPart = workbookPart.AddNewPart<WorksheetPart>();
                SheetData sheetData = new SheetData();
                worksheetPart.Worksheet = new Worksheet(sheetData);

                // create sheet
                Sheets sheets = spreadSheetDocument.WorkbookPart.Workbook.GetFirstChild<Sheets>();
                Sheet sheet = new Sheet()
                {
                    Id = spreadSheetDocument.WorkbookPart.GetIdOfPart(worksheetPart),
                    SheetId = 1,
                    Name = "Failed entries"
                };
                sheets.Append(sheet);

                Row headers = new Row();
                // first get headers and store in a row
                foreach (DataColumn col in dt.Columns)
                {
                    Cell cell = new Cell();
                    cell.DataType = CellValues.String;// just store as string
                    cell.CellValue = new CellValue(col.ColumnName); // get column headers for each column in datatable
                    headers.Append(cell); // add the cell to headers
                }
                // add error messages header
                Cell errorCell = new Cell();
                errorCell.DataType = CellValues.String;
                errorCell.CellValue = new CellValue("Error message");
                headers.Append(errorCell);
                sheetData.AppendChild(headers); // add the headers to the sheet

                // enumerator for error messages
                IEnumerator<string> errMsgEnumerator = errMsgs.GetEnumerator();
                foreach (DataRow dtRow in dt.Rows)
                {
                    Row row = new Row(); // this is the spreadsheet row that will be stored in sheetData
                    foreach (var item in dtRow.ItemArray)
                    {
                        Cell cell = new Cell();
                        cell.DataType = CellValues.String; // just storing as string

                        cell.CellValue = new CellValue(item.ToString());
                        row.Append(cell); // add the cell to the row
                    }
                    // set errMsg enumerator to next and then get current value and add to row.
                    errMsgEnumerator.MoveNext();
                    Cell errCell = new Cell();
                    errCell.DataType = CellValues.String;
                    errCell.CellValue = new CellValue(errMsgEnumerator.Current);
                    row.Append(errCell);

                    sheetData.AppendChild(row); // add row to the sheet data
                }
            }

            // return the filename so user can download
            return fileName;
        }
    }
}
