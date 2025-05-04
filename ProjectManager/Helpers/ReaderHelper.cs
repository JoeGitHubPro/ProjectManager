using ClosedXML.Excel;
using ProjectManager.Models;
using ProjectManager.Views.Dialogs;
using System.IO;

namespace ProjectManager.Helpers
{
    public static class ReaderHelper
    {
        public static async Task<List<Consumer>> ReadConsumersFromExcelAsync(string filePath)
        {
            if (string.IsNullOrWhiteSpace(filePath))
                throw new ArgumentException("File path cannot be empty", nameof(filePath));

            if (!File.Exists(filePath))
                throw new FileNotFoundException("Excel file not found", filePath);

            try
            {
                return await Task.Run(() =>
                {
                    var consumers = new List<Consumer>();

                    using (var workbook = new XLWorkbook(filePath))
                    {
                        var worksheet = workbook.Worksheet(1) ??
                            throw new InvalidOperationException("Worksheet not found");

                        // Get the actual rows (not range rows)
                        var rows = worksheet.RowsUsed().Skip(1); // Skip header

                        foreach (var row in rows)
                        {
                            try
                            {
                                var consumer = new Consumer
                                {
                                    Name = GetCellValue(row, 1),
                                    Description = GetCellValue(row, 2),
                                    Phone = GetCellValue(row, 3),
                                    Whatsapp = GetCellValue(row, 4),
                                    Facebook = GetCellValue(row, 5),
                                    TikTok = GetCellValue(row, 6),
                                    X = GetCellValue(row, 7),
                                    Instagram = GetCellValue(row, 8),
                                    Site = GetCellValue(row, 9),
                                    Notes = GetCellValue(row, 10),
                                    Location = GetCellValue(row, 11),
                                    CommercialRegisterNo = GetCellValue(row, 12)
                                };

                                if (!string.IsNullOrWhiteSpace(consumer.Name))
                                {
                                    consumers.Add(consumer);
                                }
                            }
                            catch (Exception ex)
                            {
                                new ErrorDialog($"Error processing row {row.RowNumber()}: ", $"{ex.Message}");
                                continue;
                            }
                        }
                    }

                    return consumers;
                });
            }
            catch (Exception ex)
            {
                new ErrorDialog($"Error reading Excel file: ", $"{ex.Message}");
                return new List<Consumer>();
            }
        }

        private static string GetCellValue(IXLRow row, int column)
        {
            try
            {
                return row.Cell(column).GetString().Trim();
            }
            catch
            {
                return string.Empty;
            }
        }
    }
}