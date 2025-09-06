using System;
using System.Configuration;
using System.Linq;
using System.Net;
using System.Net.Mail;
using System.Text;

namespace MillenniumWebFixed.Services
{
    public static class EmailService
    {
        /// <summary>
        /// Sends a quote approval request email to a single approver.
        /// </summary>
        public static bool SendQuoteApprovalRequest(
            string toEmail, string approverName, int quoteId, string projectName,
            string approveUrl, string requestedByName, string requestedByEmail, string requesterFullname,
            out string error, string cc = null, string bcc = null, string plainUrl = null)
        {
            error = null;
            try
            {
                var sendFlag = ConfigurationManager.AppSettings["SendQuoteEmail"];
                if (!string.Equals(sendFlag, "true", StringComparison.OrdinalIgnoreCase))
                {
                    error = "Email sending disabled by config (SendQuoteEmail != true).";
                    return false;
                }

                if (string.IsNullOrWhiteSpace(toEmail))
                {
                    error = "Recipient email is empty.";
                    return false;
                }

                var fromEmail = ConfigurationManager.AppSettings["SmtpSenderEmail"];
                var fromName = ConfigurationManager.AppSettings["SmtpSenderName"] ?? "Millennium Roofing";

                if (string.IsNullOrWhiteSpace(fromEmail))
                {
                    error = "Missing SmtpSenderEmail in config.";
                    return false;
                }

                var safeApprover = WebUtility.HtmlEncode(approverName ?? "");
                var safeProject = WebUtility.HtmlEncode(projectName ?? "");
                var safeReqName = WebUtility.HtmlEncode(requestedByName ?? "");
                var safeReqEmail = WebUtility.HtmlEncode(requestedByEmail ?? "");
                var safeApproveUrl = WebUtility.HtmlEncode(approveUrl ?? "");
                var env = ConfigurationManager.AppSettings["EnvName"] ?? "PROD";
                var displayUrl = string.IsNullOrWhiteSpace(plainUrl) ? approveUrl : plainUrl;
                var subject = $"[{env}] Approval requested • Quote #{quoteId} • {projectName}";
                var safeDisplayUrl = WebUtility.HtmlEncode(displayUrl ?? "");

                var htmlBody = $@"
                    <div style=""font-family:Segoe UI,Arial,sans-serif;font-size:14px;color:#111"">
                      <p>Hi {WebUtility.HtmlEncode(approverName)},</p>
                      <p><strong>{WebUtility.HtmlEncode(requesterFullname)}</strong> ({WebUtility.HtmlEncode(requestedByEmail)}) requested your approval:</p>
                      <table style=""border-collapse:collapse;margin:10px 0 16px 0"">
                        <tr><td style=""padding:2px 8px;color:#555"">Quote #</td><td><strong>{quoteId}</strong></td></tr>
                        <tr><td style=""padding:2px 8px;color:#555"">Project</td><td>{WebUtility.HtmlEncode(projectName)}</td></tr>
                      </table>
                      <p>
                        <a href=""{safeApproveUrl}"" style=""background:#0d6efd;color:#fff;text-decoration:none;padding:10px 14px;border-radius:4px;display:inline-block"">
                          Review &amp; Approve
                        </a>
                      </p>
                      <p style=""margin-top:16px;color:#666"">
                        Or copy this direct link:<br>
                        <span style=""word-break:break-all"">{safeDisplayUrl}</span>
                      </p>
                      <hr style=""border:none;border-top:1px solid #eee;margin:18px 0"">
                      <p style=""color:#999"">Generated {DateTime.Now:yyyy-MM-dd HH:mm} • {env} • Millennium Roofing</p>
                    </div>";

                var textBody =
                    $"Hi {approverName},\r\n\r\n" +
                    $"{requestedByName} ({requestedByEmail}) has requested your approval on the following quote:\r\n" +
                    $"Quote #: {quoteId}\r\nProject: {projectName}\r\n\r\n" +
                    $"Review & Approve:\r\n{approveUrl}\r\n\r\n" +
                    $"This message was sent by Millennium Roofing.";

                using (var message = new MailMessage())
                {
                    message.From = new MailAddress(fromEmail, fromName);
                    message.To.Add(toEmail);

                    if (!string.IsNullOrWhiteSpace(cc))
                        foreach (var addr in cc.Split(',').Select(x => x.Trim()).Where(x => x.Length > 0))
                            message.CC.Add(addr);

                    if (!string.IsNullOrWhiteSpace(bcc))
                        foreach (var addr in bcc.Split(',').Select(x => x.Trim()).Where(x => x.Length > 0))
                            message.Bcc.Add(addr);

                    if (!string.IsNullOrWhiteSpace(requestedByEmail))
                        message.ReplyToList.Add(new MailAddress(requestedByEmail, requestedByName));

                    message.Subject = subject;
                    message.IsBodyHtml = true;
                    message.Body = htmlBody;
                    message.BodyEncoding = Encoding.UTF8;
                    message.SubjectEncoding = Encoding.UTF8;
                    message.AlternateViews.Add(AlternateView.CreateAlternateViewFromString(textBody, Encoding.UTF8, "text/plain"));

                    using (var smtp = new SmtpClient())
                    {
                        smtp.Host = "smtp-relay.brevo.com";
                        smtp.Port = 587;
                        smtp.EnableSsl = true;

                        var user = ConfigurationManager.AppSettings["SmtpUsername"];
                        var pass = ConfigurationManager.AppSettings["SmtpPassword"];
                        if (string.IsNullOrWhiteSpace(user) || string.IsNullOrWhiteSpace(pass))
                        {
                            error = "Missing SMTP credentials in config.";
                            return false;
                        }

                        smtp.Credentials = new NetworkCredential(user, pass);
                        smtp.Send(message);
                    }
                }

                return true;
            }
            catch (Exception ex)
            {
                error = ex.Message;
                return false;
            }
        }


        public static bool SendQuoteImportNotification(string subject, string htmlBody)
        {
            try
            {
                var sendFlag = ConfigurationManager.AppSettings["SendQuoteEmail"];
                if (!string.Equals(sendFlag, "true", StringComparison.OrdinalIgnoreCase))
                    return false;

                var toList = ConfigurationManager.AppSettings["QuoteEmailRecipients"]
                    ?.Split(new[] { ',' }, StringSplitOptions.RemoveEmptyEntries)
                    .Select(e => e.Trim())
                    .ToList();

                var bccList = ConfigurationManager.AppSettings["QuoteEmailBcc"]
                    ?.Split(new[] { ',' }, StringSplitOptions.RemoveEmptyEntries)
                    .Select(e => e.Trim())
                    .ToList();

                if (toList == null || !toList.Any())
                    return false;

                var message = new MailMessage();
                message.From = new MailAddress(ConfigurationManager.AppSettings["SmtpSenderEmail"], "Millennium Roofing");

                foreach (var to in toList)
                    message.To.Add(to);

                if (bccList != null)
                {
                    foreach (var bcc in bccList)
                        message.Bcc.Add(bcc);
                }

                message.Subject = subject;
                message.Body = htmlBody;
                message.IsBodyHtml = true;

                using (var smtp = new SmtpClient())
                {
                    smtp.Host = "smtp-relay.brevo.com";
                    smtp.Port = 587;
                    smtp.EnableSsl = true;

                    smtp.Credentials = new NetworkCredential(
                        ConfigurationManager.AppSettings["SmtpUsername"],
                        ConfigurationManager.AppSettings["SmtpPassword"]);

                    smtp.Send(message); // Synchronous call
                }

                return true;
            }
            catch
            {
                return false;
            }
        }

    }
}
