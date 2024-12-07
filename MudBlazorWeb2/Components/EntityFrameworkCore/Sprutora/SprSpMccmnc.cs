using System;
using System.Collections.Generic;

namespace MudBlazorWeb2.Components.EntityFrameworkCore.Sprutora;

public partial class SprSpMccmnc
{
    public long Id { get; set; }

    public long SInckey { get; set; }

    public int? Mcc { get; set; }

    public string? Mnc { get; set; }

    public virtual SprSpeechTable SInckeyNavigation { get; set; } = null!;
}
