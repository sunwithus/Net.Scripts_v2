using System;
using System.Collections.Generic;

namespace MudBlazorWeb2.Components.EntityFrameworkCore.Sprutora;

public partial class ImgArea
{
    public long Id { get; set; }

    public long IdTable { get; set; }

    public short TableType { get; set; }

    public string? SName { get; set; }

    public string? FileName { get; set; }

    public double? SHeight { get; set; }

    public double? SWidth { get; set; }

    public double? STop { get; set; }

    public double? SLeft { get; set; }
}
