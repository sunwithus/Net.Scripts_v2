using System;
using System.Collections.Generic;

namespace MudBlazorWeb2.Components.EntityFrameworkCore.Sprutora;

/// <summary>
/// MCC+MNC Коды операторов сотовой связи
/// </summary>
public partial class Operator
{
    public long Id { get; set; }

    /// <summary>
    /// MCC Код стран
    /// </summary>
    public int Mcc { get; set; }

    /// <summary>
    /// MNC Код оператора
    /// </summary>
    public string Mnc { get; set; } = null!;

    /// <summary>
    /// Оператор
    /// </summary>
    public string Operatorname { get; set; } = null!;

    /// <summary>
    /// Страна
    /// </summary>
    public string? Countryname { get; set; }

    public string? OperatornameMore { get; set; }
}
