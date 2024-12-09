//OracleDbContext.cs

using Microsoft.EntityFrameworkCore;
using MudBlazorWeb2.Components.EntityFrameworkCore;
using MudBlazorWeb2.Components.EntityFrameworkCore.Sprutora;

namespace MudBlazorWeb2.Components.EntityFrameworkCore
{
    // Определение контекста базы данных для работы с Entity Framework Core
    public class OracleDbContext : BaseDbContext
    {
        public OracleDbContext(DbContextOptions<OracleDbContext> options) : base(options)
        {
        }

        // Определение DbSet - коллекций таблиц базы данных
        /*
        public override DbSet<SPR_SPEECH_TABLE> SprSpeechTables { get; set; }
        public override DbSet<SPR_SP_DATA_1_TABLE> SprSpData1Tables { get; set; }
        public override DbSet<SPR_SP_COMMENT_TABLE> SprSpCommentTables { get; set; }
        */
        public override DbSet<SprSpeechTable> SprSpeechTables { get; set; }
        public override DbSet<SprSpData1Table> SprSpData1Tables { get; set; }
        public override DbSet<SprSpCommentTable> SprSpCommentTables { get; set; }
        
        public static DbContextOptionsBuilder<OracleDbContext> ConfigureOptionsBuilder(string connectionString)
        {
            var optionsBuilder = new DbContextOptionsBuilder<OracleDbContext>();
            optionsBuilder.UseOracle(connectionString, providerOptions =>
            {
                providerOptions.CommandTimeout(60);
                providerOptions.UseRelationalNulls(true);
                providerOptions.MinBatchSize(2);
            })
            .EnableDetailedErrors(false)
            .EnableSensitiveDataLogging(false)
            .UseQueryTrackingBehavior(QueryTrackingBehavior.NoTracking);
            return optionsBuilder;
        }

        // Переопределение метода для настройки моделей (сопоставления сущностей с таблицами)
        protected override void OnModelCreating(ModelBuilder modelBuilder)
        {
            // Указываем, что сущность SPR_SPEECH_TABLE сопоставляется с таблицей "SPR_SPEECH_TABLE"
            modelBuilder.Entity<SprSpeechTable>().ToTable("SPR_SPEECH_TABLE");

            // Настраиваем свойства сущности SPR_SP_DATA_1_TABLE для полей Fspeech и Rspeech, указывая их тип как BLOB
            modelBuilder.Entity<SprSpData1Table>()
                .ToTable("SPR_SP_DATA_1_TABLE")  // Указываем имя таблицы
                .Property(b => b.SFspeech)       // Указываем свойство Fspeech
                .HasColumnType("BLOB");         // Устанавливаем тип данных для колонки как BLOB

            modelBuilder.Entity<SprSpData1Table>()
                .Property(b => b.SRspeech)       // Указываем свойство Rspeech
                .HasColumnType("BLOB");         // Устанавливаем тип данных для колонки как BLOB

            // Сопоставляем сущность SPR_SP_COMMENT_TABLE с таблицей "SPR_SP_COMMENT_TABLE"
            modelBuilder.Entity<SprSpCommentTable>().ToTable("SPR_SP_COMMENT_TABLE");
            //.Property(b=>b.SComment).HasColumnType("BLOB");

            // Вызов базовой реализации метода
            base.OnModelCreating(modelBuilder);
        }
    }

}

