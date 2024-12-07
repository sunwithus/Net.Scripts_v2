using System;
using System.Collections.Generic;

namespace MudBlazorWeb2.Components.EntityFrameworkCore.Sprutora;

public partial class SprSpLastsvaTable
{
    public long SInckey { get; set; }

    public long? SKc { get; set; }

    public virtual SprSpeechTable SInckeyNavigation { get; set; } = null!;
}
