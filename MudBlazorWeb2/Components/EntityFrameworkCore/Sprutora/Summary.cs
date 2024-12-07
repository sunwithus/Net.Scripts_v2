using System;
using System.Collections.Generic;

namespace MudBlazorWeb2.Components.EntityFrameworkCore.Sprutora;

public partial class Summary
{
    public long Id { get; set; }

    public DateTime? SDatetime { get; set; }

    public byte[]? SummaryText { get; set; }

    public short? Status { get; set; }
}
