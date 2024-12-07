using System;
using System.Collections.Generic;

namespace MudBlazorWeb2.Components.EntityFrameworkCore.Sprutora;

/// <summary>
/// Учеты Шифр заданий
/// </summary>
public partial class Jobcipher
{
    public long Id { get; set; }

    /// <summary>
    /// Шифр задания
    /// </summary>
    public string? Cipher { get; set; }
}
