using System;
using System.Collections.Generic;

namespace MudBlazorWeb2.Components.EntityFrameworkCore.Sprutora;

public partial class SprEvent
{
    public short Val { get; set; }

    public string ValName { get; set; } = null!;

    public virtual ICollection<SprSpeechTable> SprSpeechTables { get; set; } = new List<SprSpeechTable>();
}
