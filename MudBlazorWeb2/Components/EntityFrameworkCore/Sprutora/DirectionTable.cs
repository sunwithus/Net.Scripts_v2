using System;
using System.Collections.Generic;

namespace MudBlazorWeb2.Components.EntityFrameworkCore.Sprutora;

public partial class DirectionTable
{
    public long DirectionId { get; set; }

    public DateTime? SDatetime { get; set; }

    public string? Theme { get; set; }

    public virtual ICollection<DirectionSpeech> DirectionSpeeches { get; set; } = new List<DirectionSpeech>();
}
