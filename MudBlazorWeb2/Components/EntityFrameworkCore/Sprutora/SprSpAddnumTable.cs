using System;
using System.Collections.Generic;

namespace MudBlazorWeb2.Components.EntityFrameworkCore.Sprutora;

public partial class SprSpAddnumTable
{
    public long SInckey { get; set; }

    public string? SNumber { get; set; }

    public string? SNumberRev { get; set; }

    public string? SNumbertype { get; set; }

    public short SNumberclass { get; set; }

    public int? SId { get; set; }

    public string? SName { get; set; }

    public short SSt { get; set; }

    public virtual SprSpeechTable SInckeyNavigation { get; set; } = null!;
}
