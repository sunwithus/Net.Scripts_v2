using System;
using System.Collections.Generic;

namespace MudBlazorWeb2.Components.EntityFrameworkCore.Sprutora;

public partial class SprSpGeoTable
{
    public long SInckey { get; set; }

    public int SOrder { get; set; }

    public short? SCommand { get; set; }

    public double? SLatitude { get; set; }

    public double? SLongitude { get; set; }

    public double? SHeight { get; set; }

    public string? SObject { get; set; }

    public double? SD { get; set; }

    public double? SElevation { get; set; }

    public int? STime { get; set; }

    public short? SSelect { get; set; }

    public string? SGeosource { get; set; }

    public short? SQuality { get; set; }

    public double? SRxlevel { get; set; }

    public int? SSrctype { get; set; }

    public double? SA { get; set; }

    public double? SAw { get; set; }

    public double? SAreasize { get; set; }

    public double? SAntA { get; set; }

    public double? SAntAw { get; set; }

    public virtual SprSpeechTable SInckeyNavigation { get; set; } = null!;

    public virtual SprSrctype? SSrctypeNavigation { get; set; }
}
