//OracleDbContext.cs

using Microsoft.EntityFrameworkCore;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;


namespace MudBlazorWeb2.Components.EntityFrameworkCore
{
    // Определение контекста базы данных для работы с Entity Framework Core
    public class OracleDbContext : DbContext
    {
        // Конструктор, принимающий параметры конфигурации контекста
        public OracleDbContext(DbContextOptions<OracleDbContext> options) : base(options)
        {
        }
        // Определение DbSet - коллекций таблиц базы данных
        public DbSet<SPR_SPEECH_TABLE> SprSpeechTable { get; set; }
        public DbSet<SPR_SP_DATA_1_TABLE> SprSpData1Table{ get; set; }
        public DbSet<SPR_SP_COMMENT_TABLE> SprSpCommentTable { get; set; }

        protected override void OnConfiguring(DbContextOptionsBuilder optionsBuilder)
        {
            base.OnConfiguring(optionsBuilder);
            optionsBuilder
                 //тут изменять настройки, для другой схемы, другой базы
                 .UseOracle("User Id=SYSDBA;Password=masterkey;Data Source=localhost / sprutora;", providerOptions => providerOptions
                                .CommandTimeout(60)
                                .UseRelationalNulls(true)
                                .MinBatchSize(2))
                 
                 .EnableDetailedErrors(false)
                 .EnableSensitiveDataLogging(false)
                 //.LogTo(System.Console.WriteLine)
                 .UseQueryTrackingBehavior(QueryTrackingBehavior.NoTracking);

        }

        // Переопределение метода для настройки моделей (сопоставления сущностей с таблицами)
        protected override void OnModelCreating(ModelBuilder modelBuilder)
        {
            // Указываем, что сущность SPR_SPEECH_TABLE сопоставляется с таблицей "SPR_SPEECH_TABLE"
            modelBuilder.Entity<SPR_SPEECH_TABLE>().ToTable("SPR_SPEECH_TABLE");

            // Настраиваем свойства сущности SPR_SP_DATA_1_TABLE для полей Fspeech и Rspeech, указывая их тип как BLOB
            modelBuilder.Entity<SPR_SP_DATA_1_TABLE>()
                .ToTable("SPR_SP_DATA_1_TABLE")  // Указываем имя таблицы
                .Property(b => b.Fspeech)       // Указываем свойство Fspeech
                .HasColumnType("BLOB");         // Устанавливаем тип данных для колонки как BLOB

            modelBuilder.Entity<SPR_SP_DATA_1_TABLE>()
                .Property(b => b.Rspeech)       // Указываем свойство Rspeech
                .HasColumnType("BLOB");         // Устанавливаем тип данных для колонки как BLOB

            // Сопоставляем сущность SPR_SP_COMMENT_TABLE с таблицей "SPR_SP_COMMENT_TABLE"
            modelBuilder.Entity<SPR_SP_COMMENT_TABLE>().ToTable("SPR_SP_COMMENT_TABLE");
                //.Property(b=>b.Comment).HasColumnType("BLOB");

            // Вызов базовой реализации метода
            base.OnModelCreating(modelBuilder);
        }
    }

    [Table("SPR_SPEECH_TABLE")]
    public class SPR_SPEECH_TABLE
    {
        [Key]
        [Column("S_INCKEY")]
        public long? Id { get; set; } = 0;

        [Column("S_TYPE")] //1 - текстовое сообщение, 0 - сеанс связи
        public int? Type { get; set; } = 0;

        [Column("S_PRELOOKED")] //Признак просмотра (0/1)
        public int? Prelooked { get; set; } = 0;

        [Column("S_DEVICEID")] //Имя устройства регистрации (MEDIUM_R)
        public string? Deviceid { get; set; } = "MEDIUM_R";

        [Column("S_DURATION")] //durationString = string.Format("+00 {0:D2}:{1:D2}:{2:D2}.000000", duration / 3600, (duration % 3600) / 60, duration % 60);
        public string? Duration { get; set; } // Duration - это INTERVAL (в C# - TimeSpan), не string (??? уже не помню, почему string а не TimeSpan)
        //public TimeSpan? Duration { get; set; }

        [Column("S_DATETIME")] //DateTime timestamp = DateTime.ParseExact(timestampString, "dd-MM-yyyy HH:mm:ss", null); || DateTime timestamp = DateTime.Now
        public DateTime? Datetime { get; set; }

        [Column("S_EVENT")] //Тип события (Событие: -1 -  неизвестно, 0 - назначение трафик-канала, 1 - отключение трафик-канала...)
        public int? Event { get; set; } = -1;

        [Column("S_EVENTCODE")] //Событие (оригинал) - GSM (совпадает с RecordType)
        public string? Eventcode { get; set; } = "GSM";

        [Column("S_STANDARD")] //стандарт системы связи - GSM_ABIS
        public string? Standard { get; set; } = "GSM_ABIS";

        [Column("S_NETWORK")] //
        public string? Network { get; set; }

        [Column("S_SYSNUMBER3")] //imei
        public string? Sysnumber3 { get; set; }

        [Column("S_SOURCEID")] //??? Номер источника сообщения по базе отбора - 0
        public int? Sourceid { get; set; } = 0;

        [Column("S_STATUS")] //??? статус завершения сеанса - 0
        public int? Status { get; set; } = 0;

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

        [Column("S_CID")] //идентификатор соты базовой станции
        public string? Cid { get; set; }

        [Column("S_LAC")] //код зоны базовой станции
        public string? Lac { get; set; }

        [Column("S_BASESTATION")] //код зоны базовой станции
        public string? Basestation { get; set; }

        [Column("S_POSTID")] //bмя поста регистрации
        public string? Postid { get; set; }


        [Column("S_CALLTYPE")] //тип вызова 0-входящий, 1-исходящий, 2-неизвестный...
        public int? Calltype { get; set; } = 2;
    }

    [Table("SPR_SP_DATA_1_TABLE")]
    public class SPR_SP_DATA_1_TABLE
    {
        [Key]
        [Column("S_INCKEY")]
        public long? Id { get; set; } = 0;

        [Column("S_ORDER")] //Номер записи в сеансе (0 - по умолчанию) - обязательный параметр
        public int? Order { get; set; } = 0;
        
        [Column("S_RECORDTYPE")]//Типзаписи (GSM/SMS Text/UCS2/…) - обязательный параметр
        public string? Recordtype { get; set; } = "PCMA"; //поиграться с кодировками
        
        [Column("S_FSPEECH")]
        public byte[]? Fspeech { get; set; }

        [Column("S_RSPEECH")]
        public byte[]? Rspeech { get; set; }
    }

    [Table("SPR_SP_COMMENT_TABLE")]
    public class SPR_SP_COMMENT_TABLE
    {
        [Key]
        [Column("S_INCKEY")]
        public long? Id { get; set; } = 0;

        [Column("S_COMMENT")]
        public byte[]? Comment { get; set; }

    }
    /*
    
    //Более компактная запись, когда название таблицы совпадает с названием класса
    //и название поля совпадает с названием свойства
    public class SPR_SP_COMMENT_TABLE  // Без [Table("SPR_SP_COMMENT_TABLE")]
    {
        [Key] // Указывает, что это первичный ключ
        [Column("S_INCKEY")] // Указывает, что свойство связано с колонкой S_INCKEY
        public long Id { get; set; } = 0; // Поле S_INCKEY
        public byte[]? S_COMMENT { get; set; } // Поле S_COMMENT, без [Column("S_COMMENT")]
    }

    */
}
