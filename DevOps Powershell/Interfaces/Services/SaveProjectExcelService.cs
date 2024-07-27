using OfficeOpenXml;
using System.Reflection;

namespace DevOps_Powershell.Interfaces.Services
 
{
    public class SaveProjectExcelService : ISaveProjectExcel
    {
        public async Task<string> SaveObjectsToExcelAsync<T>(IEnumerable<T> objects)
        {
            // Ensure that EPPlus can work without a license
            ExcelPackage.LicenseContext = LicenseContext.NonCommercial;

            string filePath = Path.Combine(Directory.GetCurrentDirectory(), "data.xlsx");

            using (var package = new ExcelPackage())
            {
                var worksheet = package.Workbook.Worksheets.Add("Data");

                // Get properties of the object type
                var properties = typeof(T).GetProperties(BindingFlags.Public | BindingFlags.Instance);

                // Add headers
                for (int i = 0; i < properties.Length; i++)
                {
                    worksheet.Cells[1, i + 1].Value = properties[i].Name;
                }

                // Add object data
                int row = 2;
                foreach (var obj in objects)
                {
                    for (int i = 0; i < properties.Length; i++)
                    {
                        var value = properties[i].GetValue(obj);
                        worksheet.Cells[row, i + 1].Value = value != null ? value.ToString() : string.Empty;
                    }
                    row++;
                }

                // Save the file
                var fileInfo = new FileInfo(filePath);
                await package.SaveAsAsync(fileInfo);
            }

            return filePath;
        }

    }
}
