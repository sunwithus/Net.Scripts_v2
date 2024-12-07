using System;
using System.Collections.Generic;

namespace MudBlazorWeb2.Components.EntityFrameworkCore.Sprutora;

public partial class SprSpFotoTable
{
    public long Id { get; set; }

    public long SInckey { get; set; }

    public DateTime? FDatetime { get; set; }

    public double? FBearing { get; set; }

    public double? FElevation { get; set; }

    public byte[]? FImage { get; set; }

    public int? FCamera { get; set; }

    public string? FPname { get; set; }

    public virtual SprSpeechTable SInckeyNavigation { get; set; } = null!;
}
