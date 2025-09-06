using MillenniumWebFixed.Models;
using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Web.Mvc;

namespace MillenniumWebFixed.ViewModels
{
    public class ManualQuoteViewModel
    {
        // Quote Information
        public int Id { get; set; } // Optional for editing later
        [Display(Name = "Quote No")]
        public string QuoteNo { get; set; }
        public string RevisionID { get; set; }
        [DataType(DataType.Date)]
        public DateTime? ImportDate { get; set; }
        public int? QuantityOfUnits { get; set; }
        [DataType(DataType.Date)]
        public DateTime? DueDate { get; set; }
        public string DesignerId { get; set; } // or string DesignerId if using user GUIDs
        public IEnumerable<SelectListItem> DesignerOptions { get; set; } // For dropdown

        public string SelectedSalesRepName { get; set; }
        public string SelectedDesignerName { get; set; }

        public string BaseUOM { get; set; }
        public string ProductDescription { get; set; }
        public string ImportedPMRJBFileRef { get; set; }

        // Site Details
        public string SiteName { get; set; }
        public string StandNumber { get; set; }
        public string ProjectName { get; set; } // Read-only
        public string UnitBlockNumber { get; set; }
        public string UnitBlockType { get; set; }
        public string StreetAddress { get; set; } // If this replaces StandNumber


        // Sales Information
        public int? PotentialCustomerId { get; set; }
        public string PotentialCustomerName { get; set; }

        public string SalesRepId { get; set; }
        public IEnumerable<SelectListItem> SalesRepOptions { get; set; }

        public int? SelectedContactId { get; set; }
        public string SelectedContactName { get; set; }

        public int? SelectedEnquiryId { get; set; }
        public string SelectedEnquiryNumber { get; set; }


        // Addresses
        [Display(Name = "Ship To Street 1")]
        public string ShipToStreet1 { get; set; }

        [Display(Name = "Ship To Street 2")]
        public string ShipToStreet2 { get; set; }

        [Display(Name = "Ship To Street 3")]
        public string ShipToStreet3 { get; set; }

        [Display(Name = "Ship To City")]
        public string ShipToCity { get; set; }

        [Display(Name = "Ship To State/Province")]
        public string ShipToState { get; set; }

        [Display(Name = "Ship To ZIP/Postal Code")]
        public string ShipToPostalCode { get; set; }

        [Display(Name = "Ship To Country/Region")]
        public string ShipToCountry { get; set; }


        // Design Details
        [Display(Name = "Roof Pitch")]
        public string RoofPitch { get; set; }

        [Display(Name = "Roof Overhang")]
        public string RoofOverhang { get; set; }

        [Display(Name = "Roof Gable Overhang")]
        public string RoofGableOverhang { get; set; }

        [Display(Name = "Roof Covering")]
        public string RoofCovering { get; set; }

        [Display(Name = "Max Batten Centers")]
        public string MaxBattenCenters { get; set; }

        [Display(Name = "Max Truss Centers")]
        public string MaxTrustCenters { get; set; }

        [Display(Name = "Floor Area")]
        public string FloorArea { get; set; }

        [Display(Name = "Roof Area")]
        public string RoofArea { get; set; }

        [Display(Name = "Quote Notes")]
        public string QuoteNotes { get; set; }

        [Display(Name = "Sales Notes")]
        public string SalesNotes { get; set; }


        // Customers dropdown
        public List<CustomerDropdownItem> Customers { get; set; } = new List<CustomerDropdownItem>();

        // Contacts
        public List<ContactDropdownItem> Contacts { get; set; } = new List<ContactDropdownItem>();

        // Enquiry
        public List<EnquiryDropdownItem> Enquiries { get; set; } = new List<EnquiryDropdownItem>();

        // Stock Items
        public List<StockItemViewModel> StockItems { get; set; } = new List<StockItemViewModel>();

        // Manual Line Items / Stock On Quote
        public string LineItemsJson { get; set; }
        public List<QuoteLineItemViewModel> LineItems { get; set; } = new List<QuoteLineItemViewModel>();

        public string QuoteStage { get; set; }

        public int? GeneralProjectDataId { get; set; }

        public List<QuoteImage> QuoteImages { get; set; } = new List<QuoteImage>();

        public int QuoteImageCount { get; set; }

    }

    public class QuoteLineItemViewModel
    {
        public int ProductId { get; set; }         // required for database insert
        public string ProductCode { get; set; }    // used for display and mapping
        public int Qty { get; set; }
        public decimal? CostPrice { get; set; }
        public decimal? MarginPercent { get; set; }
        public decimal? SellingPrice { get; set; }
        public string BaseUOM { get; set; }
        public string ProductDescription { get; set; }

    }

    public class CustomerDropdownItem
    {
        public int Id { get; set; }
        public string CustomerName { get; set; }
        public string ContactPerson { get; set; }
        public string Email { get; set; }
        public string CustomerCode { get; set; }
        public string AddressLine1 { get; set; }
        public string AddressLine2 { get; set; }
        public string City { get; set; }
        public string StateProvince { get; set; }
        public string Phone { get; set; }
    }

    public class ContactDropdownItem
    {
        public int ContactId { get; set; }
        public string Name { get; set; }
        public string Designation { get; set; }
        public string PhoneNumber { get; set; }
        public string Email { get; set; }
        public int CustomerId { get; set; }
    }

    public class EnquiryDropdownItem
    {
        public int EnquiryId { get; set; }
        public string EnquiryNumber { get; set; }
        public string CustomerName { get; set; }
    }

    public class StockItemViewModel
    {
        public int ProductId { get; set; }
        public string ProductCode { get; set; }
        public string Name { get; set; }
        public string ItemTypeName { get; set; }
        public string BaseUOM { get; set; }
    }
}
