using System;
using System.Collections.Generic;

namespace MudBlazorWeb2.Components.EntityFrameworkCore.Sprutora;

public partial class SKwIST
{
    public long SK { get; set; }

    public short SWtp { get; set; }

    public long SIk { get; set; }

    public virtual SprSpeechTable SIkNavigation { get; set; } = null!;

    public virtual SKwIT SKNavigation { get; set; } = null!;
}
