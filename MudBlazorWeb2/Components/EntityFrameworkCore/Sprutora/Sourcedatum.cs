using System;
using System.Collections.Generic;

namespace MudBlazorWeb2.Components.EntityFrameworkCore.Sprutora;

/// <summary>
/// Источники внешних данных
/// </summary>
public partial class Sourcedatum
{
    public long Id { get; set; }

    /// <summary>
    /// Имя источника данных
    /// </summary>
    public string? Sourcename { get; set; }

    public string? Commentary { get; set; }
}
