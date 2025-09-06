using MillenniumWebFixed.Models.Projects;
using System.ComponentModel.DataAnnotations.Schema;
using System.Data.Entity;
using System.Diagnostics;


namespace MillenniumWebFixed.Models
{
    public class AppDbContext : DbContext
    {
        public AppDbContext() : base("MillRoofDb")
        {
            this.Database.Log = s => Debug.WriteLine(s);
        }

        // Contacts
        public DbSet<Contact> Contacts { get; set; }
        public DbSet<Enquiry> Enquiries { get; set; }

        // Customers
        public DbSet<Customer> Customers { get; set; }

        // Mapping For Quotes --> JSON Project to prod_Product
        public DbSet<ProductMapping> ProductMappings { get; set; }

        // Quote
        public DbSet<Quote> Quotes { get; set; }
        public DbSet<QuoteVersion> QuoteVersions { get; set; }
        public DbSet<QuoteLineItem> QuoteLineItems { get; set; }
        public DbSet<QuoteAttachment> QuoteAttachments { get; set; }
        public DbSet<QuoteHistory> QuoteHistories { get; set; }
        public DbSet<ManualQuote> ManualQuotes { get; set; }

        // Project Data (Import EXCEL)
        public DbSet<GeneralProjectData> GeneralProjectDatas { get; set; } // MAIN TABLE
        public DbSet<xls_AtticRooms> xls_AtticRooms { get; set; }
        public DbSet<xls_Boards> xls_Boards { get; set; }
        public DbSet<xls_Bracing> xls_Bracing { get; set; }
        public DbSet<xls_Cladding> xls_Cladding { get; set; }
        public DbSet<xls_CO2material> xls_CO2material { get; set; }
        public DbSet<xls_ConnectorPlates> xls_ConnectorPlates { get; set; }
        public DbSet<xls_Decking> xls_Decking { get; set; }
        public DbSet<xls_DoorsandWindows> xls_DoorsandWindows { get; set; }
        public DbSet<xls_Fasteners> xls_Fasteners { get; set; }
        public DbSet<xls_Frames> xls_Frames { get; set; }
        public DbSet<xls_FramingZones> xls_FramingZones { get; set; }
        public DbSet<xls_Manufactureframes> xls_Manufactureframes { get; set; }
        public DbSet<xls_Metalwork> xls_Metalwork { get; set; }
        public DbSet<xls_PosiStruts> xls_PosiStruts { get; set; }
        public DbSet<xls_RoofingData> xls_RoofingData { get; set; }
        public DbSet<xls_Sheeting> xls_Sheeting { get; set; }
        public DbSet<xls_SundryItems> xls_SundryItems { get; set; }
        public DbSet<xls_Surfaces> xls_Surfaces { get; set; }
        public DbSet<xls_Timber> xls_Timber { get; set; }
        public DbSet<xls_Walls> xls_Walls { get; set; }

        // Project Data (Import JSON)
        public DbSet<GeneralQuoteData> GeneralQuoteDatas { get; set; } // MAIN TABLE
        public DbSet<AppUser> AppUsers { get; set; }
        public DbSet<FramingZones> FramingZones { get; set; }
        public DbSet<Frames> Frames { get; set; }
        public DbSet<ManufacturingFrames> ManufacturingFrames { get; set; }
        public DbSet<Timber> Timbers { get; set; }
        public DbSet<Connectors> Connectors { get; set; }
        public DbSet<Metalwork> Metalworks { get; set; }
        public DbSet<Bracing> Bracings { get; set; }
        public DbSet<Walls> Walls { get; set; }
        public DbSet<Surfaces> Surfaces { get; set; }
        public DbSet<RoofingData> RoofingData { get; set; }
        public DbSet<Fasteners> Fasteners { get; set; }
        public DbSet<Sheeting> Sheetings { get; set; }
        public DbSet<Co2Material> Co2Materials { get; set; }
        public DbSet<ProjectImage> ProjectImages { get; set; }
        public DbSet<ImportFailure> ImportFailures { get; set; }
        public DbSet<AtticRoom> AtticRooms { get; set; }
        public DbSet<Board> Boards { get; set; }
        public DbSet<Cladding> Claddings { get; set; }
        public DbSet<Decking> Deckings { get; set; }
        public DbSet<OpeningFrameLabel> OpeningFrameLabels { get; set; }
        public DbSet<PosiStrut> PosiStruts { get; set; }
        public DbSet<SundryItem> SundryItems { get; set; }

        // Stock Item + Supporting Tables
        public DbSet<StockItem> StockItems { get; set; }
        public DbSet<ItemType> ItemTypes { get; set; }
        public DbSet<ItemTypeProperty> ItemTypeProperties { get; set; }
        public DbSet<UOM> UOMs { get; set; }
        public DbSet<UOMConversion> UOMConversions { get; set; }
        public DbSet<Product> Products { get; set; }
        public DbSet<ProductProperty> ProductProperties { get; set; }
        public DbSet<ProductAssembly> ProductAssemblies { get; set; }
        public DbSet<ProductAssemblyComponent> ProductAssemblyComponents { get; set; }
        public DbSet<ManualQuoteLineItem> ManualQuoteLineItems { get; set; }
        public DbSet<QuoteImage> QuoteImages { get; set; }
        public DbSet<Project> Projects { get; set; }
        public DbSet<ProjectVersion> ProjectVersions { get; set; }
        public DbSet<ProjectImportBatch> ProjectImportBatches { get; set; }
        public DbSet<ProjectDocument> ProjectDocuments { get; set; }
        public DbSet<ProjectAudit> ProjectAudits { get; set; }
        public DbSet<ProjectQuote> ProjectQuotes { get; set; }
        public DbSet<ProjectQuoteLineHeader> ProjectQuoteLineHeaders { get; set; }
        public DbSet<ProjectQuoteLineItem> ProjectQuoteLineItems { get; set; }
        public DbSet<ProjectQuoteLineAggregate> ProjectQuoteLineAggregates { get; set; }

        public DbSet<QuoteWorkflowLog> QuoteWorkflowLogs { get; set; }


        protected override void OnModelCreating(DbModelBuilder modelBuilder)
        {
            // ── Core keys ──────────────────────────────────────────────────────────────
            modelBuilder.Entity<UOM>()
                .HasKey(u => u.UOMCode);

            // ── ManualQuote relationships (unchanged) ─────────────────────────────────
            modelBuilder.Entity<ManualQuote>()
                .HasOptional(m => m.PotentialCustomer)
                .WithMany()
                .HasForeignKey(m => m.PotentialCustomerId);

            modelBuilder.Entity<ManualQuote>()
                .HasOptional(m => m.SalesRep)
                .WithMany()
                .HasForeignKey(m => m.SalesRepId);

            // ── Project* table -> explicit table names (unchanged) ─────────────────────
            modelBuilder.Entity<Project>()
                .ToTable("Projects");

            modelBuilder.Entity<ProjectVersion>()
                .ToTable("ProjectVersions");

            modelBuilder.Entity<ProjectImportBatch>()
                .ToTable("ProjectImportBatch");

            modelBuilder.Entity<ProjectDocument>()
                .ToTable("ProjectDocuments");

            // ── ProjectQuote (existing) ────────────────────────────────────────────────
            modelBuilder.Entity<ProjectQuote>()
                .HasKey(pq => pq.QuoteId)
                .ToTable("ProjectQuotes");

            // optional FK to Project (only if you added ProjectId)
            modelBuilder.Entity<ProjectQuote>()
                .HasOptional(pq => pq.Project)
                .WithMany() // or .WithMany(p => p.ProjectQuotes)
                .HasForeignKey(pq => pq.ProjectId);

            // decimal precision for money/areas on ProjectQuote if present
            modelBuilder.Entity<ProjectQuote>()
                .Property(pq => pq.TotalAmount)
                .HasPrecision(18, 2);

            modelBuilder.Entity<ProjectQuote>()
                .Property(p => p.RoofArea)
                .HasPrecision(18, 2);

            modelBuilder.Entity<ProjectQuote>()
                .Property(p => p.FloorArea)
                .HasPrecision(18, 2);

            // ── NEW: Quote import tables ───────────────────────────────────────────────
            // ProjectQuoteLineHeader (from Quote.xlsx)
            var h = modelBuilder.Entity<ProjectQuoteLineHeader>();
            h.ToTable("ProjectQuoteLineHeader");
            h.Property(p => p.RoofPitch).HasPrecision(9, 3);
            h.Property(p => p.RoofOverhang).HasPrecision(18, 3);
            h.Property(p => p.RoofGableOverhang).HasPrecision(18, 3);
            h.Property(p => p.MaxBattenCentres).HasPrecision(18, 3);
            h.Property(p => p.MaxTrussCentres).HasPrecision(18, 3);
            h.Property(p => p.FloorArea).HasPrecision(18, 3);
            h.Property(p => p.RoofArea).HasPrecision(18, 3);
            h.Property(p => p.EFinksWorkUnits).HasPrecision(18, 3);
            h.Property(p => p.EFinksCost).HasPrecision(18, 2);
            h.Property(p => p.TransportCost).HasPrecision(18, 2);
            // ImportedDate is a computed column in SQL (DEFAULT SYSUTCDATETIME())
            h.Property(p => p.ImportedDate)
             .HasDatabaseGeneratedOption(DatabaseGeneratedOption.Computed);

            // ProjectQuoteLineItem (from QuoteLines.xlsx)
            var i = modelBuilder.Entity<ProjectQuoteLineItem>();
            i.ToTable("ProjectQuoteLineItem");
            i.Property(p => p.Quantity).HasPrecision(18, 3);
            i.Property(p => p.PricePerUnit).HasPrecision(18, 2);
            i.Property(p => p.CostPerUnit).HasPrecision(18, 2);
            i.Property(p => p.Vat).HasPrecision(9, 4);
            i.Property(p => p.ImportedDate)
             .HasDatabaseGeneratedOption(DatabaseGeneratedOption.Computed);

            // ProjectQuoteLineAggregate (from QuoteLineAggregate.xlsx)
            var a = modelBuilder.Entity<ProjectQuoteLineAggregate>();
            a.ToTable("ProjectQuoteLineAggregate");
            a.Property(p => p.Quantity).HasPrecision(18, 3);
            a.Property(p => p.Price).HasPrecision(18, 2);
            a.Property(p => p.PricePerUnit).HasPrecision(18, 2);
            a.Property(p => p.CostPerUnit).HasPrecision(18, 2);
            a.Property(p => p.Vat).HasPrecision(9, 4);
            a.Property(p => p.ImportedDate)
             .HasDatabaseGeneratedOption(DatabaseGeneratedOption.Computed);

            base.OnModelCreating(modelBuilder);
        }

    }
}
