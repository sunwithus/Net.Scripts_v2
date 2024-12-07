using System;
using System.Collections.Generic;

namespace MudBlazorWeb2.Components.EntityFrameworkCore.Sprutora;

public partial class SprHistoryTable
{
    public long HInckey { get; set; }

    public DateTime? HDatetime { get; set; }

    public string? HPostid { get; set; }

    public string? HEvent { get; set; }

    public byte[]? HComment { get; set; }

    public short? HPrelooked { get; set; }

    public short? HReplicated { get; set; }
}
