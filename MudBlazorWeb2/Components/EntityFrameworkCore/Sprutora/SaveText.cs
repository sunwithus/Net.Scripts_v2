using System;
using System.Collections.Generic;

namespace MudBlazorWeb2.Components.EntityFrameworkCore.Sprutora;

public partial class SaveText
{
    public long Id { get; set; }

    public long DataId { get; set; }

    public long? SummaryId { get; set; }

    public string? SName { get; set; }

    public string? FileName { get; set; }

    public short? Status { get; set; }

    public byte[]? TextOriginal { get; set; }

    public byte[]? TextTransfer { get; set; }

    public virtual SprSpData1Table Data { get; set; } = null!;
}
