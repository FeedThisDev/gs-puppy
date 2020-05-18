using FeedThisDev.GSPuppy.Model;
using HtmlAgilityPack;
using PuppeteerSharp;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace FeedThisDev.GSPuppy
{
    public class Puppy
    {
        private readonly static PuppyConfiguration defaultConfig = new PuppyConfiguration()
        {
            ExludeSites = null,
            Headless = false,
            MaxResults = -1,
            WaitMSBeforeNextPage = 1000
        };

        public PuppyConfiguration Configuration
        {
            get;
            private set;
        }

        public Puppy() : this(defaultConfig)
        {

        }

        public Puppy(PuppyConfiguration config)
        {
            Configuration = config;
        }

        /// <summary>
        /// Browses to google.com, inputs <code>query</code> into the searchfield and emulates return key to start search.
        /// Parses all search results on the first page into <code>SearchResult</code> Collection 
        /// clicks on "next page" link and repeats.
        /// </summary>
        /// <param name="query">the search query" </param>
        /// <returns><code>SearchResult[]</code></returns>
        public async Task<IEnumerable<SearchResult>> DoSearchAsync(string query)
        {
            List<SearchResult> searchResults = new List<SearchResult>();
            // Download the Chromium revision if it does not already exist
            await new BrowserFetcher().DownloadAsync(BrowserFetcher.DefaultRevision);
            // Create an instance of the browser and configure launch options
            Browser browser = await Puppeteer.LaunchAsync(new LaunchOptions
            {
                Headless = Configuration.Headless
            });

            // Create a new page and go to google startpage
            Page page = await browser.NewPageAsync();
            await page.GoToAsync("https://www.google.com");

            // get the searchbox, input query and newline key to start search
            await page.WaitForSelectorAsync("input[name=\"q\"]");
            await page.FocusAsync("input[name=\"q\"");
            await page.Keyboard.TypeAsync(query + '\n'); 
            await page.WaitForNavigationAsync();

            try
            {
                do
                {
                    // get the HTML of the current page
                    string content = await page.GetContentAsync();

                    searchResults.AddRange(parseContent(Configuration.MaxResults, Configuration.ExludeSites, content) );
                    if (Configuration.MaxResults >= 0 && searchResults.Count >= Configuration.MaxResults)
                    {
                        break;
                    }

                    System.Threading.Thread.Sleep(Configuration.WaitMSBeforeNextPage);
                    await page.ClickAsync("table.AaVjTc > tbody > tr > td:last-child");
                    await page.WaitForNavigationAsync();

                } while (await page.WaitForSelectorAsync("table.AaVjTc", new WaitForSelectorOptions() { Timeout = 5000 }) != null);
            }
            catch (Exception ex)
            {
                Console.WriteLine(ex.ToString());
            }

            return searchResults;
        }

        private static List<SearchResult> parseContent(int maxResults, HashSet<string> exludeSites, string content)
        {
            List<SearchResult> searchResults = new List<SearchResult>();
            HtmlDocument doc = new HtmlDocument();
            doc.LoadHtml(content);

            foreach (var searchResultDiv in doc.DocumentNode.QuerySelectorAll("div.rc"))
            {
                SearchResult searchResult = new SearchResult();
                var rElem = searchResultDiv.QuerySelector("div.r");
                searchResult.Site = rElem.QuerySelector("cite").Descendants().First(node => node.NodeType == HtmlNodeType.Text).InnerText.Trim();
                searchResult.Headline = rElem.QuerySelector("a > h3").InnerText.Trim();
                searchResult.URI = rElem.QuerySelector("a").Attributes["href"].Value;
                searchResult.Snippet = searchResultDiv.QuerySelector("div.s > div > span.st").InnerText.Trim();

                if (exludeSites == null || !exludeSites.Contains(searchResult.Site))
                {
                    searchResults.Add(searchResult);
                }

                if (maxResults >= 0 && searchResults.Count >= maxResults)
                {
                    break;
                }
            }
            return searchResults;
        }
    }
}
