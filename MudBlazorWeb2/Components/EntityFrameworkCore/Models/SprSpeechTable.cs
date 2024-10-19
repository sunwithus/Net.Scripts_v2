using System.ComponentModel.DataAnnotations.Schema;
using System.ComponentModel.DataAnnotations;

namespace MudBlazorWeb2.Components.EntityFrameworkCore.Models
{
    public class SprSpeechTable
    {
        [Key]
        [Column("S_INCKEY")]
        public long Id { get; set; }
        public long SprSpData1TableID { get; set; }//внешний ключ
        public long SprSpCommentTableID { get; set; }//внешний ключ
        public SprSpData1Table? SprSpData1Table { get; set; }
        public SprSpCommentTable? SprSpCommentTable { get; set; }

        [Column("S_TYPE")] //1 - текстовое сообщение, 0 - сеанс связи
        public int Type { get; set; } = -1;

        [Column("S_PRELOOKED")] //Признак просмотра (0/1)
        public int? Prelooked { get; set; }

        [Column("S_DEVICEID")] //Имя устройства регистрации (MEDIUM_R)
        public string? Deviceid { get; set; }

        [Column("S_DURATION")] //durationString = string.Format("+00 {0:D2}:{1:D2}:{2:D2}.000000", duration / 3600, (duration % 3600) / 60, duration % 60);
        public string? Duration { get; set; }

        [Column("S_DATETIME")] //DateTime timestamp = DateTime.ParseExact(timestampString, "dd-MM-yyyy HH:mm:ss", null); || DateTime timestamp = DateTime.Now
        public DateTime? Datetime { get; set; }

        [Column("S_EVENT")] //Тип события (Событие: -1 -  неизвестно, 0 - назначение трафик-канала, 1 - отключение трафик-канала...)
        public int? Event { get; set; } = -1;

        [Column("S_EVENTCODE")] //Событие (оригинал) - GSM
        public string? Eventcode { get; set; }

        [Column("S_STANDARD")] //стандарт системы связи - GSM_ABIS
        public string? Standard { get; set; }

        [Column("S_NETWORK")] //
        public string? Network { get; set; }

        [Column("S_SYSNUMBER3")] //imei
        public string? Sysnumber3 { get; set; }

        [Column("S_SOURCEID")] //??? Номер источника сообщения по базе отбора - 0
        public int? Sourseid { get; set; }

        [Column("S_STATUS")] //??? статус завершения сеанса - 0
        public int? Status { get; set; }

        [Column("S_BELONG")] //приндалежность - язык оригинала
        public string? Belong { get; set; }

        [Column("S_SOURCENAME")] //Имя источника - оператор
        public string? Sourcename { get; set; }

        [Column("S_NOTICE")] //примечение
        public string? Notice { get; set; }

        [Column("S_DCHANNEL")] //номер прямого канала комплекса регистрации (-1, если нет)
        public int? Dchannel { get; set; } = 2; //0 - по описанию

        [Column("S_RCHANNEL")] //номер обратного канала комплекса регистрации (-1, если нет)
        public int? Rchannel { get; set; } = 2; //0 - по описанию

        [Column("S_TALKER")] //пользовательский номер собеседника (тот, кому звонят)
        public string? Talker { get; set; }

        [Column("S_USERNUMBER")] //пользовательский номер источника (тот, кто звонит)
        public string? Usernumber { get; set; }

        [Column("S_CALLTYPE")] //тип вызова 0-входящий, 1-исходящий, 2-неизвестный...
        public string? Calltype { get; set; }


    }

}
