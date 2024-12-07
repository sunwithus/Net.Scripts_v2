using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations.Schema;

namespace MudBlazorWeb2.Components.EntityFrameworkCore.Sprutora;

public partial class SprSpeechTable
{
    public short SType { get; set; }

    public long SInckey { get; set; }

    public DateTime? SDatetime { get; set; }

    public string? SStandard { get; set; }

    public string? SNetwork { get; set; }

    public string? SBelong { get; set; }

    public short? SCalltype { get; set; }

    public int? SSourceid { get; set; }

    public string? SSourcename { get; set; }

    public string? SSysnumbertype { get; set; }

    public string? SSysnumber { get; set; }

    public string? SUsernumber { get; set; }

    public string? SUsernumberRev { get; set; }

    public int? SPriority { get; set; }

    public string? STalker { get; set; }

    public string? STalkerRev { get; set; }

    public string? STalkername { get; set; }

    public int? STalkerid { get; set; }

    public string? SBasestation { get; set; }

    public string? SLac { get; set; }

    public string? SCid { get; set; }

    public string? SPostid { get; set; }

    public string? SDeviceid { get; set; }

    public string? SNotice { get; set; }

    public short? SPrelooked { get; set; }

    public string? SSysnumbertype2 { get; set; }

    public string? SSysnumber2 { get; set; }

    public string? SSysnumbertype3 { get; set; }

    public string? SSysnumber3 { get; set; }

    public short? SSelstatus { get; set; }

    public string? SFrequency { get; set; }

    public int? SDchannel { get; set; }

    public int? SRchannel { get; set; }

    public short? SStatus { get; set; }

    public string? SDecryptinfo { get; set; }

    public short? SEvent { get; set; }

    public string? SPostkey { get; set; }

    public string? SEventcode { get; set; }

    public short? SReplicated { get; set; }

    //public TimeSpan? SDuration { get; set; } //durationString = string.Format("+00 {0:D2}:{1:D2}:{2:D2}.000000", duration / 3600, (duration % 3600) / 60, duration % 60);
    public string? SDuration { get; set; } // Duration - это INTERVAL (в C# - TimeSpan), не string (??? уже не помню, почему string а не TimeSpan)
                                          //public TimeSpan? Duration { get; set; }

    public string? STalkerBs { get; set; }

    public string? STalkerLac { get; set; }

    public string? STalkerCid { get; set; }

    public string? STalkerSn { get; set; }

    public string? STalkerSntype { get; set; }

    public string? STalkerSn2 { get; set; }

    public string? STalkerSntype2 { get; set; }

    public string? STalkerSn3 { get; set; }

    public string? STalkerSntype3 { get; set; }

    public string? SEquipment { get; set; }

    public short? DeleteStatus { get; set; }

    /// <summary>
    /// Источник поступления записи
    /// </summary>
    public long? SSourcedataid { get; set; }

    /// <summary>
    /// Привязка записи к Учету(шифр задания)
    /// </summary>
    public long? SCipherid { get; set; }

    public string? SBasestationEnd { get; set; }

    public string? SLacEnd { get; set; }

    public string? SCidEnd { get; set; }

    public string? STalkerBsEnd { get; set; }

    public string? STalkerLacEnd { get; set; }

    public string? STalkerCidEnd { get; set; }

    public string? BsOperators { get; set; }

    public string? SDescription { get; set; }

    public virtual ICollection<DirectionSpeech> DirectionSpeeches { get; set; } = new List<DirectionSpeech>();

    public virtual ICollection<Link> Links { get; set; } = new List<Link>();

    public virtual SprCalltype? SCalltypeNavigation { get; set; }

    public virtual SprEvent? SEventNavigation { get; set; }

    public virtual SprSelstatus? SSelstatusNavigation { get; set; }

    public virtual SprStatus? SStatusNavigation { get; set; }

    public virtual SprCategory STypeNavigation { get; set; } = null!;

    public virtual ICollection<SprSpData1Table> SprSpData1Tables { get; set; } = new List<SprSpData1Table>();

    public virtual ICollection<SprSpFotoTable> SprSpFotoTables { get; set; } = new List<SprSpFotoTable>();

    public virtual ICollection<SprSpMccmnc> SprSpMccmncs { get; set; } = new List<SprSpMccmnc>();
}
