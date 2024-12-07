using System;
using System.Collections.Generic;

namespace MudBlazorWeb2.Components.EntityFrameworkCore.Sprutora;

public partial class SprAutodelete
{
    public short DType { get; set; }

    public short DEnabled { get; set; }

    public short DDays { get; set; }

    public short DFromlast { get; set; }

    public short BEnabled { get; set; }

    public short BDays { get; set; }

    public short BFromlast { get; set; }
}
