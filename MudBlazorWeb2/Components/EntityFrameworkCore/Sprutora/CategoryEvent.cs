using System;
using System.Collections.Generic;

namespace MudBlazorWeb2.Components.EntityFrameworkCore.Sprutora;

public partial class CategoryEvent
{
    public int Id { get; set; }

    public short CategoryId { get; set; }

    public short EventId { get; set; }
}
