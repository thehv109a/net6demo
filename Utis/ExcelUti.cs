using OfficeOpenXml;

namespace netcore6demo.Utis
{
    public class ExcelUti
    {
        public void readFromTemplate()
        {
            var fileInfo = new FileInfo("templates.xlsx");
            using (var excelPackage = new ExcelPackage(fileInfo))
            {
                var excelWorksheetNew = excelPackage.Workbook.Worksheets.Add("Sheet1");
                var excelWorksheetExists = excelPackage.Workbook.Worksheets["Sheet1"];
                excelWorksheetNew.Cells[$"AM{1}"].Value = "CELL_CONTENT";
                excelWorksheetExists.Cells[$"AM{1}"].Value = "CELL_CONTENT";
                excelPackage.SaveAs(new FileInfo("output.xlsx"));
            }
        }
    }
}