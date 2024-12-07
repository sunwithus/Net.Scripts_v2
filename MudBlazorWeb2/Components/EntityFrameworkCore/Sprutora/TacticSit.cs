using System;
using System.Collections.Generic;

namespace MudBlazorWeb2.Components.EntityFrameworkCore.Sprutora;

public partial class TacticSit
{
    public long Id { get; set; }

    public byte[] Polygon { get; set; } = null!;

    public double? SLatitude { get; set; }

    public double? SLongitude { get; set; }

    public string? SName { get; set; }

    public short SColor { get; set; }

    public short? ObjectType { get; set; }

    public short? PrimitiveType { get; set; }

    public short? Isround { get; set; }

    public short TypeLine { get; set; }
}
