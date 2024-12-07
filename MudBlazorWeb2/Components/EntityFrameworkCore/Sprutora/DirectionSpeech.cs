using System;
using System.Collections.Generic;

namespace MudBlazorWeb2.Components.EntityFrameworkCore.Sprutora;

public partial class DirectionSpeech
{
    public long Id { get; set; }

    public long SInckey { get; set; }

    public long DirectionId { get; set; }

    public short? Prelooked { get; set; }

    public virtual DirectionTable Direction { get; set; } = null!;

    public virtual SprSpeechTable SInckeyNavigation { get; set; } = null!;
}
