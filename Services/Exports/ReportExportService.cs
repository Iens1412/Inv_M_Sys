using CsvHelper;
using CsvHelper.Configuration;
using Inv_M_Sys.Models;
using PdfSharpCore.Drawing;
using PdfSharpCore.Pdf;
using ClosedXML.Excel;
using Serilog;
using System;
using System.Collections.Generic;
using System.Globalization;
using System.IO;

namespace Inv_M_Sys.Services.Exports
{
    public static class ReportExportService
    {
        //Export the report as CSV file
        public static void ExportAsCsv(IEnumerable<Order> orders, string filePath)
        {
            try
            {
                using var writer = new StreamWriter(filePath);
                using var csv = new CsvWriter(writer, new CsvConfiguration(CultureInfo.InvariantCulture)
                {
                    HasHeaderRecord = true,
                });

                csv.WriteField("Order ID");
                csv.WriteField("Customer Name");
                csv.WriteField("Delivery Date");
                csv.WriteField("Total Price");
                csv.WriteField("Status");
                csv.NextRecord();

                foreach (var order in orders)
                {
                    csv.WriteField(order.Id);
                    csv.WriteField(order.CustomerName);
                    csv.WriteField(order.DeliveryDate.ToString("yyyy-MM-dd"));
                    csv.WriteField(order.TotalPrice.ToString("C", CultureInfo.CurrentCulture));
                    csv.WriteField(order.Status.ToString());
                    csv.NextRecord();
                }
            }
            catch (Exception ex)
            {
                Log.Error(ex, "CSV export failed.");
                throw;
            }
        }

        //Export the report as Excel file
        public static void ExportAsExcel(IEnumerable<Order> orders, string filePath)
        {
            try
            {
                using var workbook = new XLWorkbook();
                var worksheet = workbook.Worksheets.Add("Sales Report");

                worksheet.Cell("A1").Value = "Sales Report";
                worksheet.Cell("A1").Style.Font.Bold = true;
                worksheet.Cell("A1").Style.Font.FontSize = 16;
                worksheet.Range("A1:E1").Merge().Style.Alignment.Horizontal = XLAlignmentHorizontalValues.Center;

                worksheet.Cell(3, 1).Value = "Order ID";
                worksheet.Cell(3, 2).Value = "Customer Name";
                worksheet.Cell(3, 3).Value = "Delivery Date";
                worksheet.Cell(3, 4).Value = "Total Price";
                worksheet.Cell(3, 5).Value = "Status";
                worksheet.Range("A3:E3").Style.Font.Bold = true;

                int row = 4;
                foreach (var order in orders)
                {
                    worksheet.Cell(row, 1).Value = order.Id;
                    worksheet.Cell(row, 2).Value = order.CustomerName;
                    worksheet.Cell(row, 3).Value = order.DeliveryDate.ToString("yyyy-MM-dd");
                    worksheet.Cell(row, 4).Value = order.TotalPrice;
                    worksheet.Cell(row, 5).Value = order.Status.ToString();
                    row++;
                }

                worksheet.Columns().AdjustToContents();
                workbook.SaveAs(filePath);
            }
            catch (Exception ex)
            {
                Log.Error(ex, "Excel export failed.");
                throw;
            }
        }

        //Export the report as Pdf file
        public static void ExportAsPdf(IEnumerable<Order> orders, string filePath, string reportTitle)
        {
            try
            {
                var doc = new PdfDocument();
                doc.Info.Title = reportTitle;
                var page = doc.AddPage();
                var gfx = XGraphics.FromPdfPage(page);
                var headerFont = new XFont("Verdana", 14, XFontStyle.Bold);
                var font = new XFont("Verdana", 10);
                double y = 40;

                gfx.DrawString(reportTitle, headerFont, XBrushes.Black, new XRect(0, y, page.Width, 30), XStringFormats.TopCenter);
                y += 40;

                gfx.DrawString("Generated On: " + DateTime.Now.ToString("yyyy-MM-dd HH:mm"), font, XBrushes.Gray, new XRect(40, y, page.Width - 80, 20), XStringFormats.TopLeft);
                y += 30;

                foreach (var order in orders)
                {
                    string line = $"#{order.Id} - {order.CustomerName} - {order.DeliveryDate:yyyy-MM-dd} - {order.TotalPrice:C} - {order.Status}";
                    gfx.DrawString(line, font, XBrushes.Black, new XRect(40, y, page.Width - 80, 20), XStringFormats.TopLeft);
                    y += 20;

                    if (y > page.Height - 60)
                    {
                        page = doc.AddPage();
                        gfx = XGraphics.FromPdfPage(page);
                        y = 40;
                    }
                }

                doc.Save(filePath);
            }
            catch (Exception ex)
            {
                Log.Error(ex, "PDF export failed.");
                throw;
            }
        }
    }
}