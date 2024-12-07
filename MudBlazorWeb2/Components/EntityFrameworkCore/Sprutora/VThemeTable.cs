using System;
using System.Collections.Generic;

namespace MudBlazorWeb2.Components.EntityFrameworkCore.Sprutora;

public partial class VThemeTable
{
    public long? Id { get; set; }

    public long? SInckey { get; set; }

    public long? DirectionId { get; set; }

    public string? Theme { get; set; }

    public short? Prelooked { get; set; }
}
