using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using FeedThisDev.GSPuppy;
using SpreadsheetLight;

namespace FeedThisDev.GSPuppyRunner
{
    class Program
    {
        static async Task Main(string[] args)
        {
            HashSet<string> exludeSites = new HashSet<string>();

            var searchConfig = new PuppyConfiguration()
            {
                ExludeSites = exludeSites,
                Headless = false,
                MaxResults = 20, //-1 for unlimited
                WaitMSBeforeNextPage = 1000
            };

            Puppy searchDog = new Puppy();
            
            var results = (
                await searchDog
                .DoSearchAsync("searchterm1")
                ).Select(x => ExtendedSearchResult.FromSearchResult(x))
                .ToList();

            results.AddRange(
                        (await searchDog
                         .DoSearchAsync("searchterm2")
                        ).Select(x => ExtendedSearchResult.FromSearchResult(x))
                        .ToList()
                );

            string filename = "output.xlsx";
            CreateExcel(results, filename);

            Console.WriteLine($"Wrote {results.Count} records to {filename}. Press any key to close.");
            Console.ReadKey();
        }

        private static void CreateExcel(IEnumerable<ExtendedSearchResult> searchResults, string filename = "output.xlsx")
        {
            SLDocument sl = new SLDocument();
            SLStyle style = sl.CreateStyle();
            style.SetWrapText(true);

            int rowCounter = 1;
            foreach (var searchResult in searchResults)
            {
                sl.SetCellValue(rowCounter, 1, searchResult.Site);
                sl.SetCellValue(rowCounter, 2, searchResult.Headline);
                sl.SetCellValue(rowCounter, 3, searchResult.Snippet);
                if (searchResult.Date != null)
                    sl.SetCellValue(rowCounter, 4, searchResult.Date.Value.ToShortDateString());
                sl.SetCellValue(rowCounter, 5, searchResult.URI);
                rowCounter++;
            }
            sl.SetColumnStyle(1, 5, style);
            sl.AutoFitColumn(1, 5, 50d);
            sl.AutoFitRow(1, rowCounter);

            sl.SaveAs(filename);
        }

    }
}
