using System;
using System.Collections.Generic;

namespace MudBlazorWeb2.Components.EntityFrameworkCore.Sprutora;

public partial class SprSpWorkhistoryTable
{
    public long SInckey { get; set; }

    public DateTime? SDatetime { get; set; }

    public string? SUsernic { get; set; }

    public string? SUserreal { get; set; }

    public string? SAction { get; set; }

    public TimeSpan? SDuration { get; set; }

    public string? SComp { get; set; }

    public virtual SprSpeechTable SInckeyNavigation { get; set; } = null!;
}
