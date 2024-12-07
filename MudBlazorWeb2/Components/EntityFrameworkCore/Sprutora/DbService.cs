using System;
using System.Collections.Generic;

namespace MudBlazorWeb2.Components.EntityFrameworkCore.Sprutora;

/// <summary>
/// Параметры обслуживания базы
/// </summary>
public partial class DbService
{
    public long Id { get; set; }

    /// <summary>
    /// Время хранения записей (сутки)
    /// </summary>
    public short RecordDays { get; set; }

    /// <summary>
    /// Отсчет времени хранения записей от времени последней записи (1) или от текущего времени (0)
    /// </summary>
    public short? RecordFromlast { get; set; }

    /// <summary>
    /// Время хранения файлов резервных копий (сутки)
    /// </summary>
    public short BackupDays { get; set; }

    /// <summary>
    /// Отсчет времени хранения резервных копий от времени последего бэкапа (0) или от текущего времени (1)
    /// </summary>
    public short? BackupFromlast { get; set; }

    /// <summary>
    /// Флаг: обслуживание базы разрешено
    /// </summary>
    public short? WorkEnabled { get; set; }

    /// <summary>
    /// Время последнего обслуживания базы
    /// </summary>
    public DateTime? ExecuteDt { get; set; }
}
