using MillenniumWebFixed.Models;
using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations.Schema;

namespace MillenniumWebFixed.Models
{
    public class ManualQuote
    {
        public int Id { get; set; }

        // Generated PDF file path
        public string GeneratedPdfPath { get; set; }

        // --- Quote Info ---
        public string QuoteNo { get; set; }
        public string RevisionID { get; set; }
        public DateTime? ImportDate { get; set; }
        public int? QuantityOfUnits { get; set; }
        public DateTime? DueDate { get; set; }
        public string ImportedPMRJBFileRef { get; set; }

        public int? DesignerId { get; set; }
        [ForeignKey("DesignerId")]
        public virtual AppUser Designer { get; set; } // optional: for reverse access

        // --- Site Info ---
        public string SiteName { get; set; }
        public string StandNumber { get; set; }
        public string ProjectName { get; set; }
        public string StreetAddress { get; set; }
        public string UnitBlockNumber { get; set; }
        public string UnitBlockType { get; set; }

        // --- Sales Info ---
        public int? PotentialCustomerId { get; set; }
        [ForeignKey("PotentialCustomerId")]
        public virtual AppUser PotentialCustomer { get; set; }

        public int? SalesRepId { get; set; }
        [ForeignKey("SalesRepId")]
        public virtual AppUser SalesRep { get; set; }

        public int? ContactId { get; set; }
        [ForeignKey("ContactId")]
        public virtual Contact Contact { get; set; }

        public int? EnquiryId { get; set; }
        [ForeignKey("EnquiryId")]
        public virtual Enquiry Enquiry { get; set; }

        // --- Shipping Address ---
        public string ShipToStreet1 { get; set; }
        public string ShipToStreet2 { get; set; }
        public string ShipToStreet3 { get; set; }
        public string ShipToCity { get; set; }
        public string ShipToState { get; set; }
        public string ShipToPostalCode { get; set; }
        public string ShipToCountry { get; set; }

        // --- Design Info ---
        public string RoofPitch { get; set; }
        public string RoofOverhang { get; set; }
        public string RoofGableOverhang { get; set; }
        public string RoofCovering { get; set; }
        public string MaxBattenCenters { get; set; }
        public string MaxTrustCenters { get; set; }
        public string FloorArea { get; set; }
        public string RoofArea { get; set; }

        // --- Notes ---
        public string QuoteNotes { get; set; }
        public string SalesNotes { get; set; }

        // --- Metadata ---
        public DateTime CreatedAt { get; set; }

        // --- Navigation Properties ---
        //public virtual ICollection<ManualQuoteLineItem> ManualQuoteLineItems { get; set; }
        public virtual ICollection<ManualQuoteLineItem> LineItems { get; set; }

        public string QuoteStage { get; set; }

        //public int? GeneralProjectDataId { get; set; }
    }
}
