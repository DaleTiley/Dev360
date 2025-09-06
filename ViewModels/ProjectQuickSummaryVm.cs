namespace MillenniumWebFixed.Models
{
    public class ProjectQuickSummaryVm
    {
        public int Id { get; set; }
        public string ProjectCode { get; set; }
        public string ProjectName { get; set; }

        public string ClientName { get; set; }
        public string ContactName { get; set; }

        public string SiteAddress1 { get; set; }
        public string SiteCity { get; set; }
        public string SiteProvince { get; set; }
        public string SitePostcode { get; set; }

        public string Status { get; set; }
        public string Designer { get; set; }

        public int QuotesCount { get; set; }
        public int TendersCount { get; set; }
        public int AttachmentsCount { get; set; }

        public string CreatedBy { get; set; }
        public string CreatedAtDisplay { get; set; }

        public string ERPNumber { get; set; }
        public string SalesPerson { get; set; }
        public string Township { get; set; }
        public string StandNumber { get; set; }
        public string PortionNumber { get; set; }
    }
}
