using System;
using System.Collections.Generic;

namespace MudBlazorWeb2.Components.EntityFrameworkCore.Sprutora;

public partial class SprSpCommentTable
{
    public long SInckey { get; set; }

    public byte[]? SComment { get; set; }

    public virtual SprSpeechTable SInckeyNavigation { get; set; } = null!;
}
