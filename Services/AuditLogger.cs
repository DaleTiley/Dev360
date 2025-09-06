// Services/AuditLogger.cs
using MillenniumWebFixed.Models;
using Newtonsoft.Json;
using System;
using System.Linq;

namespace MillenniumWebFixed.Services
{
    public static class AuditLogger
    {
        // Minimal: one-line message
        public static void Log(AppDbContext db, int projectId, int userId, string area, string description, object meta = null, DateTime? whenUtc = null)
        {
            db.ProjectAudits.Add(new ProjectAudit
            {
                ProjectId = projectId,
                Area = area ?? AuditArea.General,
                Description = description?.Trim() ?? "",
                ChangedByUserId = userId,
                ChangedAt = whenUtc ?? DateTime.UtcNow,
                MetaJson = meta == null ? null : JsonConvert.SerializeObject(meta)
            });
            db.SaveChanges();
        }

        // Helper: build a concise “field changes” description
        public static void LogFieldChanges(AppDbContext db, int projectId, int userId, string area, object before, object after)
        {
            if (before == null || after == null) return;

            var props = before.GetType().GetProperties()
                .Where(p => p.CanRead)
                .Where(p =>
                    // whitelist important fields
                    new[]{
                        "ClientName","ContactName","ContactEmail","ContactPhone",
                        "SiteName","StreetAddress","SiteAddress1","SiteAddress2","SiteCity","SiteProvince","SitePostcode",
                        "ERPNumber","Township","PortionNumber","StandNumber",
                        "SalesPerson","SiteContactName","SiteContactPhone",
                        "GoogleMapUrl","SharePointUrl","SiteRentals",
                        "CrmStage","CrmNextAction","CrmFollowUpDate",
                        "Notes",
                        "Status"
                    }.Contains(p.Name))
                .ToList();

            var changes = props
                .Select(p =>
                {
                    var a = (before.GetType().GetProperty(p.Name)?.GetValue(before))?.ToString() ?? "";
                    var b = (after.GetType().GetProperty(p.Name)?.GetValue(after))?.ToString() ?? "";
                    return (Name: p.Name, Old: a, New: b);
                })
                .Where(x => (x.Old ?? "") != (x.New ?? ""))
                .Select(x => $"{x.Name}: '{TrimForAudit(x.Old)}' → '{TrimForAudit(x.New)}'");

            var message = string.Join("; ", changes);
            if (string.IsNullOrEmpty(message)) return;

            Log(db, projectId, userId, area, message);
        }

        private static string TrimForAudit(string s)
        {
            if (string.IsNullOrEmpty(s)) return "";
            s = s.Replace("\r", " ").Replace("\n", " ").Trim();
            return s.Length <= 120 ? s : s.Substring(0, 117) + "...";
        }
    }
}
