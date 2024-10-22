using System.ComponentModel.DataAnnotations.Schema;
using System.ComponentModel.DataAnnotations;

namespace MudBlazorWeb2.Components.EntityFrameworkCore.Models
{
    public class SprSpData1Table
    {
        [Key]
        [Column("S_INCKEY")]
        public long Id { get; set; }

        [Column("S_ORDER")] //Номер записи в сеансе (0 - по умолчанию) - обязательный параметр
        public int? Order { get; set; } = 1;

        [Column("S_RECORDTYPE")]//Типзаписи (GSM/SMS Text/UCS2/…) - обязательный параметр
        public string? Recordtype { get; set; } = "PCMA"; //поиграться с кодировками

        [Column("S_FSPEECH")]
        public byte[]? Fspeech { get; set; }

        [Column("S_RSPEECH")]
        public byte[]? Rspeech { get; set; }

        public ICollection<SprSpeechTable> SprSpeechTables { get; set; }
    }
}
