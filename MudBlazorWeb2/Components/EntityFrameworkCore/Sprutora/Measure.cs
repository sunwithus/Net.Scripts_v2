using System;
using System.Collections.Generic;

namespace MudBlazorWeb2.Components.EntityFrameworkCore.Sprutora;

/// <summary>
/// Измерения соседних БС
/// </summary>
public partial class Measure
{
    public long Id { get; set; }

    public DateTime? Measuredt { get; set; }

    public string Standard { get; set; } = null!;

    public string Imsi { get; set; } = null!;

    public int? Mcc { get; set; }

    public string? Mnc { get; set; }

    public int? Lac { get; set; }

    public int? Cid { get; set; }

    public short? Rxlevel { get; set; }

    public string? Arfcn { get; set; }

    public string? Addition { get; set; }
}
