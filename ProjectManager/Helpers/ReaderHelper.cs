using ClosedXML.Excel;
using ProjectManager.Models;

namespace ProjectManager.Helpers
{
    public static class ReaderHelper
    {
        public static List<Consumer> ReadConsumersFromExcel(string filePath)
        {
            var consumers = new List<Consumer>();

            using (var workbook = new XLWorkbook(filePath))
            {
                var worksheet = workbook.Worksheet(1); // First worksheet
                var rows = worksheet.RangeUsed().RowsUsed().Skip(1); // Skip header row

                foreach (var row in rows)
                {
                    var consumer = new Consumer
                    {
                        Name = row.Cell(1).GetString(),
                        Description = row.Cell(2).GetString(),
                        Phone = row.Cell(3).GetString(),
                        Whatsapp = row.Cell(4).GetString(),
                        Facebook = row.Cell(5).GetString(),
                        TikTok = row.Cell(6).GetString(),
                        X = row.Cell(7).GetString(),
                        Instagram = row.Cell(8).GetString(),
                        Site = row.Cell(9).GetString(),
                        Notes = row.Cell(10).GetString(),
                        Location = row.Cell(11).GetString(),
                        CommercialRegisterNo = row.Cell(12).GetString()
                    };

                    consumers.Add(consumer);
                }
            }

            return consumers;
        }
    }
}
