
using System;

public class QuoteAttachment
{
    public int Id { get; set; }
    public int QuoteVersionId { get; set; }

    public string FileName { get; set; }
    public string FileType { get; set; }
    public byte[] FileData { get; set; }
    public DateTime CreatedAt { get; set; } = DateTime.Now;

    public virtual QuoteVersion QuoteVersion { get; set; }
}
