using System;
using System.Collections.Generic;

namespace MudBlazorWeb2.Components.EntityFrameworkCore.Sprutora;

public partial class SprSpThemeTable
{
    public long SInckey { get; set; }

    public string STheme { get; set; } = null!;

    public short? SPrelooked { get; set; }

    public virtual SprSpeechTable SInckeyNavigation { get; set; } = null!;
}
