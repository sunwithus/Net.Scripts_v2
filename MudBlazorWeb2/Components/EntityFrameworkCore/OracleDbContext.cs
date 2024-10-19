//OracleDbContext.cs

using Microsoft.EntityFrameworkCore;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;


namespace MudBlazorWeb2.Components.EntityFrameworkCore
{
    public class OracleDbContext : DbContext
    {
        public OracleDbContext(DbContextOptions<OracleDbContext> options) : base(options)
        {
        }

        public DbSet<SPR_SPEECH_TABLE> SprSpeechTable { get; set; }
        public DbSet<SPR_SP_COMMENT_TABLE> SprSpCommentTable { get; set; }
        public DbSet<SPR_SP_COMMENT_TABLE> SprSpData1Table { get; set; }

        protected override void OnModelCreating(ModelBuilder modelBuilder)
        {
            modelBuilder.Entity<SPR_SPEECH_TABLE>().ToTable("SPR_SPEECH_TABLE");
            modelBuilder.Entity<SPR_SP_COMMENT_TABLE>().ToTable("SPR_SP_COMMENT_TABLE");
            modelBuilder.Entity<SPR_SP_DATA_1_TABLE>().ToTable("SPR_SP_DATA_1_TABLE");

            base.OnModelCreating(modelBuilder);
        }
    }

    [Table("SPR_SPEECH_TABLE")]
    public class SPR_SPEECH_TABLE
    {
        [Key]
        [Column("S_INCKEY")]
        public long Id { get; set; }

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

    [Table("SPR_SP_COMMENT_TABLE")]
    public class SPR_SP_COMMENT_TABLE
    {
        [Key]
        [Column("S_INCKEY")]
        public long Id { get; set; }

        [Column("S_COMMENT")]
        public byte[]? Comment { get; set; }

    }

    [Table("SPR_SP_DATA_1_TABLE")]
    public class SPR_SP_DATA_1_TABLE
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
    }
}
