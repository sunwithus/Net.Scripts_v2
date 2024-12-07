using System;
using System.Collections.Generic;

namespace MudBlazorWeb2.Components.EntityFrameworkCore.Sprutora;

/// <summary>
/// Склейки фрагментов разговоров spr_speech
/// </summary>
public partial class Link
{
    public long Id { get; set; }

    public long SInckey { get; set; }

    /// <summary>
    /// Сквозной номер склейки
    /// </summary>
    public long NumLink { get; set; }

    /// <summary>
    /// Номер пп фрагмента в склейке
    /// </summary>
    public int? OrderLink { get; set; }

    /// <summary>
    /// Фрагмент часть разговора =0 или в кач-ве фрагмента выбран весь разговор =1
    /// </summary>
    public short? SpeechFull { get; set; }

    /// <summary>
    /// Какой фрагмент р-ра: из поля s_fspeech =1, из поля s_rspeech =2, микширование полей s_fspeech+s_rspeech =3
    /// </summary>
    public short? SpeechType { get; set; }

    /// <summary>
    /// Сам фрагмент s_fspeech (если фрагмент=весь разговор не заполняется)
    /// </summary>
    public byte[]? Fspeech { get; set; }

    /// <summary>
    /// Сам фрагмент s_rspeech (если фрагмент=весь разговор не заполняется)
    /// </summary>
    public byte[]? Rspeech { get; set; }

    /// <summary>
    /// Комментарий
    /// </summary>
    public string? Comment { get; set; }

    /// <summary>
    /// Начало фрагмента в разговоре
    /// </summary>
    public DateTime? BeginDt { get; set; }

    /// <summary>
    /// Окончание фрагмента в разговоре
    /// </summary>
    public DateTime? EndDt { get; set; }

    /// <summary>
    /// Продолжительность фрагмента
    /// </summary>
    public TimeSpan? Duration { get; set; }

    /// <summary>
    /// Время последнего изменения записи
    /// </summary>
    public DateTime? UpdDt { get; set; }

    /// <summary>
    /// Наименование фрагмента
    /// </summary>
    public string? Name { get; set; }

    /// <summary>
    /// Статус фрагмента
    /// </summary>
    public short? Status { get; set; }

    public virtual SprSpeechTable SInckeyNavigation { get; set; } = null!;
}
