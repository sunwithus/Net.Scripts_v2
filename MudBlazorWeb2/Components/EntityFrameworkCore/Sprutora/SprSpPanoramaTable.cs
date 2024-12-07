using System;
using System.Collections.Generic;

namespace MudBlazorWeb2.Components.EntityFrameworkCore.Sprutora;

public partial class SprSpPanoramaTable
{
    public DateTime? PDatetime { get; set; }

    public double? PBmin { get; set; }

    public double? PBmax { get; set; }

    public double? PEmin { get; set; }

    public double? PEmax { get; set; }

    public byte[]? PImage { get; set; }

    public double? PLatitude { get; set; }

    public double? PLongitude { get; set; }

    public string PName { get; set; } = null!;

    public string? PDesc { get; set; }
}
