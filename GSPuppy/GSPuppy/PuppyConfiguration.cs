using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace FeedThisDev.GSPuppy
{
    public class PuppyConfiguration
    {
        /// <summary>
        /// open chromium in headless mode (invisible)
        /// </summary>
        public bool Headless { get; set; }

        /// <summary>
        /// stop scraping result pages after <code>MaxResults</code> found
        /// </summary>
        public int MaxResults { get; set; }

        /// <summary>
        /// Wait <code>WaitMSBeforeNextPage</code> milliseconds before clicking on "next page"
        /// </summary>
        /// <remarks>
        /// Google might ask you to complete captures if you set this value to low
        /// </remarks>
        public int WaitMSBeforeNextPage { get; set; }

        /// <summary>
        /// Don't include sites that are listed in this HashSet. 
        /// This is useful for doing several searches with different search queries that cumulate into the same result set.
        /// Set this to <code>null</code> to ignore this parameter.
        /// </summary>
        /// <note>
        /// during execution of <code>Puppy.DoSearchAsync</code> this set will autopopulate
        /// </note>
        /// <example>
        /// 
        /// </example>
        public HashSet<string> ExludeSites { get; set; }

    }
}
