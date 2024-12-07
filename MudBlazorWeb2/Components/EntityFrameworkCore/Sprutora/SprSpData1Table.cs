using System;
using System.Collections.Generic;

namespace MudBlazorWeb2.Components.EntityFrameworkCore.Sprutora;

public partial class SprSpData1Table
{
    public long Id { get; set; }

    public long SInckey { get; set; }

    public int SOrder { get; set; }

    public string? SRecordtype { get; set; }

    public byte[]? SFspeech { get; set; }

    public byte[]? SRspeech { get; set; }

    public byte[]? SSpbookmark { get; set; }

    public byte[]? SSpbookmarkrev { get; set; }

    public string? Jsonparam { get; set; }

    public virtual SprSpeechTable SInckeyNavigation { get; set; } = null!;

    public virtual ICollection<SaveText> SaveTexts { get; set; } = new List<SaveText>();
}
