
using MillenniumWebFixed.Hubs;
using MillenniumWebFixed.Models;
using System;
using System.Collections.Generic;
using System.Linq;

public class QuoteBuilderService
{
    private readonly AppDbContext db;

    public QuoteBuilderService(AppDbContext context)
    {
        db = context;
    }

    public int GenerateQuoteLineItems(int quoteVersionId, int projectId)
    {
        var version = db.QuoteVersions.Find(quoteVersionId);
        if (version == null) return 0;

        int addedCount = 0;

        // Define a dictionary mapping table names to LINQ queries for each section
        var sectionQueries = new Dictionary<string, IEnumerable<dynamic>>
    {
        { "TIMBER_TABLE", db.Timbers.Where(t => t.GeneralProjectDataId == projectId).ToList() },
        { "FRAMES_TABLE", db.Frames.Where(f => f.GeneralProjectDataId == projectId).ToList() },
        { "BRACING_TABLE", db.Bracings.Where(b => b.GeneralProjectDataId == projectId).ToList() },
        { "MANUFACTURING_FRAMES_TABLE", db.ManufacturingFrames.Where(mf => mf.GeneralProjectDataId == projectId).ToList() },
        { "CONNECTORS_TABLE", db.Connectors.Where(c => c.GeneralProjectDataId == projectId).ToList() },
        { "METALWORK_TABLE", db.Metalworks.Where(m => m.GeneralProjectDataId == projectId).ToList() },
        { "WALLS_TABLE", db.Walls.Where(w => w.GeneralProjectDataId == projectId).ToList() },
        { "SURFACES_TABLE", db.Surfaces.Where(s => s.GeneralProjectDataId == projectId).ToList() },
        { "ROOFING_DATA_TABLE", db.RoofingData.Where(r => r.GeneralProjectDataId == projectId).ToList() },
        { "FASTENERS_TABLE", db.Fasteners.Where(f => f.GeneralProjectDataId == projectId).ToList() },
        { "SHEETING_TABLE", db.Sheetings.Where(s => s.GeneralProjectDataId == projectId).ToList() },
        { "CO2_MATERIAL_TABLE", db.Co2Materials.Where(c => c.GeneralProjectDataId == projectId).ToList() },
    };

        // Loop through each section
        foreach (var section in sectionQueries)
        {
            string sectionName = section.Key;
            var records = section.Value;

            QuoteProgressHub.Send($"🔨 Processing {sectionName}...");

            foreach (var record in records)
            {
                string materialKey = TryGetMaterialKey(record);

                if (string.IsNullOrWhiteSpace(materialKey))
                {
                    QuoteProgressHub.Send("⚠️ Skipping blank material name...");
                    continue;
                }

                var mapping = db.ProductMappings.FirstOrDefault(m => m.JsonItemName == materialKey);
                if (mapping == null)
                {
                    QuoteProgressHub.Send($"❌ No mapping found for: '{materialKey}'");
                    continue;
                }

                var product = db.Products.FirstOrDefault(p => p.Name == mapping.ProductDesc);
                if (product == null)
                {
                    QuoteProgressHub.Send($"❌ No product found for mapped desc: '{mapping.ProductDesc}'");
                    continue;
                }

                // Determine quantity property dynamically
                decimal quantity = TryGetQuantity(record);

                var line = new QuoteLineItem
                {
                    QuoteVersionId = version.Id,
                    JsonItemName = materialKey,
                    ProductId = product.Id,
                    ProductDesc = product.Name,
                    Quantity = quantity,
                    Unit = product.BaseUOM,
                    UnitCost = product.CostPrice,
                    UnitSell = product.SellingPrice,
                    Notes = $"Matched from {sectionName}"
                };

                db.QuoteLineItems.Add(line);
                addedCount++;

                QuoteProgressHub.Send($"✅ Added: '{materialKey}' → '{product.Name}'");
            }
        }

        db.SaveChanges();

        db.QuoteHistories.Add(new QuoteHistory
        {
            QuoteId = version.QuoteId,
            Action = "Generated line items from all sections",
            PerformedBy = "System",
            PerformedAt = DateTime.Now,
            Notes = $"Auto-created from project ID {projectId}"
        });

        db.SaveChanges();

        QuoteProgressHub.Send("✅ DONE — Quote line item generation complete.");
        return addedCount;
    }

    // Helper to get Material Key dynamically
    private string TryGetMaterialKey(dynamic record)
    {
        try
        {
            return record.MATERIAL_NAME ?? record.TYPE ?? record.LABEL ?? string.Empty;
        }
        catch
        {
            return string.Empty;
        }
    }

    // Helper to get Quantity dynamically
    private decimal TryGetQuantity(dynamic record)
    {
        try
        {
            return record.QUANTITY ?? 0;
        }
        catch
        {
            return 0;
        }
    }

}
