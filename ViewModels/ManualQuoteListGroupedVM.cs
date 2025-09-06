using System;
using System.Collections.Generic;

namespace MillenniumWebFixed.ViewModels
{
    public class ManualQuoteRevisionVM
    {
        public int Id { get; set; }          // ManualQuote Id for that revision
        public string Rev { get; set; }      // e.g. "A", "B", "C"
        public DateTime Saved { get; set; }  // when that revision was saved
    }

    public class ManualQuoteSummaryVM
    {
        public string QuoteNo { get; set; }

        // “latest” helpers for the main row
        public int LatestQuoteId { get; set; }
        public string LatestRevision { get; set; }
        public int RevCount { get; set; }
        public DateTime LastSaved { get; set; }

        // extra display fields
        public string CustomerName { get; set; }
        public string SiteName { get; set; }
        public string SalesRepName { get; set; }

        // all revisions for the child row
        public List<ManualQuoteRevisionVM> Revisions { get; set; } = new List<ManualQuoteRevisionVM>();
    }
}
