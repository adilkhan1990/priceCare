using System;
using System.Collections.Generic;
using System.Configuration;
using System.IO;
using System.Linq;
using System.Web;
using ClosedXML.Excel;
using PriceCare.Web.Repository;

namespace PriceCare.Web.Helpers
{
    public static class ClosedXmlHelper
    {
        public static void CreateXlsxFile(string filePath, List<List<string>> data)
        {
            var wb = new XLWorkbook(XLEventTracking.Disabled);
            var ws = wb.Worksheets.Add("Sheet1");

            for (int rowIndex = 0; rowIndex < data.Count; rowIndex++)
            {

                for (int colIndex = 0; colIndex < data[rowIndex].Count; colIndex++)
                {
                    //ws.Column(colIndex+1).Width = 
                    var cellValue = data[rowIndex][colIndex];
                    ws.Cell(rowIndex + 1, colIndex + 1).Value = cellValue;
                }
                ws.Row(rowIndex + 1).Height = 15d;//default heigth in xlsx
            }

            wb.SaveAs(filePath);
        }
        public static void CreateXlsxFile(string filePath, Dictionary<string, List<List<string>>> sheetsAndData)
        {
            var wb = new XLWorkbook(XLEventTracking.Disabled);
            FillXlsxSheets(wb, sheetsAndData);
            wb.SaveAs(filePath);
        }
        public static void CreateXlsxFileFromTemplate(string filePath, string templateFilePath, Dictionary<string, List<List<string>>> sheetsAndData)
        {
            var wb = new XLWorkbook(templateFilePath);
            FillXlsxSheets(wb, sheetsAndData);
            wb.SaveAs(filePath);
        }
        public static void FillXlsxSheets(XLWorkbook wb, Dictionary<string, List<List<string>>> sheetsAndData)
        {
            foreach (var sheetName in sheetsAndData.Keys)
            {
                var ws = wb.Worksheets.FirstOrDefault(s => s.Name == sheetName) ?? wb.Worksheets.Add(sheetName);
                var data = sheetsAndData[sheetName];

                for (int rowIndex = 0; rowIndex < data.Count; rowIndex++)
                {

                    for (int colIndex = 0; colIndex < data[rowIndex].Count; colIndex++)
                    {
                        var cellValue = data[rowIndex][colIndex];
                        ws.Cell(rowIndex + 1, colIndex + 1).Value = cellValue;
                    }
                    ws.Row(rowIndex + 1).Height = 15d;//default heigth in xlsx
                }
            }
        }

        public static MemoryStream AppendXlsxSheetsNoHeaders(byte[] file, Dictionary<string, List<List<object>>> sheetsAndData)
        {
            return AppendXlsxSheet(file, sheetsAndData, initialRecord: 0);
        }

        public static MemoryStream AppendXlsxSheetsHavingHeaders(byte[] file, Dictionary<string, List<List<object>>> sheetsAndData)
        {
            return AppendXlsxSheet(file, sheetsAndData, initialRecord: 1);
        }

        public static MemoryStream AppendXlsxSheetsHavingHeaders(byte[] file, Dictionary<string, List<List<string>>> sheetsAndData)
        {
            return AppendXlsxSheet(file, sheetsAndData, initialRecord: 1);
        }

        //the T is a workaround until everything is migrated to List<object>
        private static MemoryStream AppendXlsxSheet<T>(byte[] file, Dictionary<string, List<List<T>>> sheetsAndData, int initialRecord)
        {
            var memoryStream = new MemoryStream(file);
            var wb = new XLWorkbook(memoryStream);

            foreach (var sheetName in sheetsAndData.Keys)
            {
                var ws = wb.Worksheets.FirstOrDefault(s => s.Name == sheetName) ?? wb.Worksheets.Add(sheetName);
                var data = sheetsAndData[sheetName];

                var lastRowIndex = ws.LastRowUsed() == null ? 0 : ws.LastRowUsed().RowNumber();

                if (lastRowIndex > initialRecord)
                    lastRowIndex++;

                for (int rowIndex = lastRowIndex; rowIndex < data.Count; rowIndex++)
                {
                    for (int colIndex = 0; colIndex < data[rowIndex].Count; colIndex++)
                    {
                        var cellValue = data[rowIndex][colIndex];
                        ws.Cell(rowIndex + 1, colIndex + 1).Value = cellValue;
                    }
                    ws.Row(rowIndex + 1).Height = 15d; //default heigth in xlsx
                }
            }

            MemoryStream fs = new MemoryStream();
            wb.SaveAs(fs);
            fs.Position = 0;
            return fs;
        }

    }

    public class SheetData
    {
        public string Name { get; set; }
        public List<List<string>> Datas { get; set; }
    }
}