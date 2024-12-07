using System;
using System.Collections.Generic;
using Microsoft.EntityFrameworkCore;
using MudBlazorWeb2.Components.EntityFrameworkCore.Sprutora;


namespace MudBlazorWeb2.Components.EntityFrameworkCore;

public partial class PostgresDbContext : DbContext
{
    public PostgresDbContext()
    {
    }

    public PostgresDbContext(DbContextOptions<PostgresDbContext> options)
        : base(options)
    {
    }

    public virtual DbSet<CategoryEvent> CategoryEvents { get; set; }

    public virtual DbSet<DbService> DbServices { get; set; }

    public virtual DbSet<DirectionSpeech> DirectionSpeeches { get; set; }

    public virtual DbSet<DirectionTable> DirectionTables { get; set; }

    public virtual DbSet<ImgArea> ImgAreas { get; set; }

    public virtual DbSet<Jobcipher> Jobciphers { get; set; }

    public virtual DbSet<Link> Links { get; set; }

    public virtual DbSet<Measure> Measures { get; set; }

    public virtual DbSet<Operator> Operators { get; set; }

    public virtual DbSet<SKwIST> SKwISTs { get; set; }

    public virtual DbSet<SKwISW> SKwISWs { get; set; }

    public virtual DbSet<SKwIT> SKwITs { get; set; }

    public virtual DbSet<SaveText> SaveTexts { get; set; }

    public virtual DbSet<Sourcedatum> Sourcedata { get; set; }

    public virtual DbSet<SprArmPlace> SprArmPlaces { get; set; }

    public virtual DbSet<SprAutodelete> SprAutodeletes { get; set; }

    public virtual DbSet<SprCalltype> SprCalltypes { get; set; }

    public virtual DbSet<SprCategory> SprCategories { get; set; }

    public virtual DbSet<SprEvent> SprEvents { get; set; }

    public virtual DbSet<SprHistoryTable> SprHistoryTables { get; set; }

    public virtual DbSet<SprSelstatus> SprSelstatuses { get; set; }

    public virtual DbSet<SprSpAddnumTable> SprSpAddnumTables { get; set; }

    public virtual DbSet<SprSpCommentTable> SprSpCommentTables { get; set; }

    public virtual DbSet<SprSpData1Table> SprSpData1Tables { get; set; }

    public virtual DbSet<SprSpFotoTable> SprSpFotoTables { get; set; }

    public virtual DbSet<SprSpGeoTable> SprSpGeoTables { get; set; }

    public virtual DbSet<SprSpLastsvaTable> SprSpLastsvaTables { get; set; }

    public virtual DbSet<SprSpMccmnc> SprSpMccmncs { get; set; }

    public virtual DbSet<SprSpPanoramaTable> SprSpPanoramaTables { get; set; }

    public virtual DbSet<SprSpThemeTable> SprSpThemeTables { get; set; }

    public virtual DbSet<SprSpWorkhistoryTable> SprSpWorkhistoryTables { get; set; }

    public virtual DbSet<SprSpeechTable> SprSpeechTables { get; set; }

    public virtual DbSet<SprSrctype> SprSrctypes { get; set; }

    public virtual DbSet<SprStatus> SprStatuses { get; set; }

    public virtual DbSet<Summary> Summaries { get; set; }

    public virtual DbSet<TacticSit> TacticSits { get; set; }

    public virtual DbSet<UsersTable> UsersTables { get; set; }

    public virtual DbSet<VCountDirection> VCountDirections { get; set; }

    public virtual DbSet<VSpeechDatetime> VSpeechDatetimes { get; set; }

    public virtual DbSet<VSprSpData1> VSprSpData1s { get; set; }

    public virtual DbSet<VThemeTable> VThemeTables { get; set; }


    public static DbContextOptionsBuilder<PostgresDbContext> ConfigureOptionsBuilder(string connectionString)
    {
        var optionsBuilder = new DbContextOptionsBuilder<PostgresDbContext>();
        optionsBuilder.UseNpgsql(connectionString, providerOptions =>
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
    //protected override void OnConfiguring(DbContextOptionsBuilder optionsBuilder)
    //    => optionsBuilder.UseNpgsql("Host=localhost;Database=test;Username=postgres;Password=postgres");

    protected override void OnModelCreating(ModelBuilder modelBuilder)
    {
        modelBuilder.Entity<CategoryEvent>(entity =>
        {
            entity.HasKey(e => e.Id).HasName("cat_event_key");

            entity.ToTable("category_event", "sprut");

            entity.Property(e => e.Id)
                .ValueGeneratedNever()
                .HasColumnName("id");
            entity.Property(e => e.CategoryId).HasColumnName("category_id");
            entity.Property(e => e.EventId).HasColumnName("event_id");
        });

        modelBuilder.Entity<DbService>(entity =>
        {
            entity.HasKey(e => e.Id).HasName("pk_db_service");

            entity.ToTable("db_service", "sprut", tb => tb.HasComment("Параметры обслуживания базы"));

            entity.Property(e => e.Id)
                .ValueGeneratedNever()
                .HasColumnName("id");
            entity.Property(e => e.BackupDays)
                .HasComment("Время хранения файлов резервных копий (сутки)")
                .HasColumnName("backup_days");
            entity.Property(e => e.BackupFromlast)
                .HasDefaultValue((short)0)
                .HasComment("Отсчет времени хранения резервных копий от времени последего бэкапа (0) или от текущего времени (1)")
                .HasColumnName("backup_fromlast");
            entity.Property(e => e.ExecuteDt)
                .HasComment("Время последнего обслуживания базы")
                .HasColumnName("execute_dt");
            entity.Property(e => e.RecordDays)
                .HasComment("Время хранения записей (сутки)")
                .HasColumnName("record_days");
            entity.Property(e => e.RecordFromlast)
                .HasDefaultValue((short)1)
                .HasComment("Отсчет времени хранения записей от времени последней записи (1) или от текущего времени (0)")
                .HasColumnName("record_fromlast");
            entity.Property(e => e.WorkEnabled)
                .HasDefaultValue((short)0)
                .HasComment("Флаг: обслуживание базы разрешено")
                .HasColumnName("work_enabled");
        });

        modelBuilder.Entity<DirectionSpeech>(entity =>
        {
            entity.HasKey(e => e.Id).HasName("direction_speech_pkey");

            entity.ToTable("direction_speech", "sprut");

            entity.HasIndex(e => e.SInckey, "ind1_dirspeech");

            entity.HasIndex(e => e.DirectionId, "ind2_dirspeech");

            entity.HasIndex(e => new { e.SInckey, e.DirectionId }, "unq_theme").IsUnique();

            entity.Property(e => e.Id)
                .ValueGeneratedNever()
                .HasColumnName("id");
            entity.Property(e => e.DirectionId).HasColumnName("direction_id");
            entity.Property(e => e.Prelooked).HasColumnName("prelooked");
            entity.Property(e => e.SInckey).HasColumnName("s_inckey");

            entity.HasOne(d => d.Direction).WithMany(p => p.DirectionSpeeches)
                .HasForeignKey(d => d.DirectionId)
                .HasConstraintName("fk_direction_id");

            entity.HasOne(d => d.SInckeyNavigation).WithMany(p => p.DirectionSpeeches)
                .HasForeignKey(d => d.SInckey)
                .HasConstraintName("fk_s_inckey");
        });

        modelBuilder.Entity<DirectionTable>(entity =>
        {
            entity.HasKey(e => e.DirectionId).HasName("direction_table_pkey");

            entity.ToTable("direction_table", "sprut");

            entity.HasIndex(e => e.Theme, "ind_direction");

            entity.HasIndex(e => e.Theme, "unq_direction_theme").IsUnique();

            entity.Property(e => e.DirectionId)
                .ValueGeneratedNever()
                .HasColumnName("direction_id");
            entity.Property(e => e.SDatetime).HasColumnName("s_datetime");
            entity.Property(e => e.Theme)
                .HasMaxLength(40)
                .HasColumnName("theme");
        });

        modelBuilder.Entity<ImgArea>(entity =>
        {
            entity.HasKey(e => e.Id).HasName("img_area_key");

            entity.ToTable("img_area", "sprut");

            entity.Property(e => e.Id)
                .ValueGeneratedNever()
                .HasColumnName("id");
            entity.Property(e => e.FileName)
                .HasMaxLength(250)
                .HasColumnName("file_name");
            entity.Property(e => e.IdTable).HasColumnName("id_table");
            entity.Property(e => e.SHeight).HasColumnName("s_height");
            entity.Property(e => e.SLeft).HasColumnName("s_left");
            entity.Property(e => e.SName)
                .HasMaxLength(250)
                .HasColumnName("s_name");
            entity.Property(e => e.STop).HasColumnName("s_top");
            entity.Property(e => e.SWidth).HasColumnName("s_width");
            entity.Property(e => e.TableType).HasColumnName("table_type");
        });

        modelBuilder.Entity<Jobcipher>(entity =>
        {
            entity.HasKey(e => e.Id).HasName("jobcipher_key");

            entity.ToTable("jobcipher", "sprut", tb => tb.HasComment("Учеты Шифр заданий"));

            entity.HasIndex(e => e.Cipher, "ind_jobcipher");

            entity.Property(e => e.Id)
                .ValueGeneratedNever()
                .HasColumnName("id");
            entity.Property(e => e.Cipher)
                .HasMaxLength(50)
                .HasComment("Шифр задания")
                .HasColumnName("cipher");
        });

        modelBuilder.Entity<Link>(entity =>
        {
            entity.HasKey(e => e.Id).HasName("links_key");

            entity.ToTable("links", "sprut", tb => tb.HasComment("Склейки фрагментов разговоров spr_speech"));

            entity.Property(e => e.Id)
                .ValueGeneratedNever()
                .HasColumnName("id");
            entity.Property(e => e.BeginDt)
                .HasComment("Начало фрагмента в разговоре")
                .HasColumnName("begin_dt");
            entity.Property(e => e.Comment)
                .HasComment("Комментарий")
                .HasColumnName("comment");
            entity.Property(e => e.Duration)
                .HasComment("Продолжительность фрагмента")
                .HasColumnName("duration");
            entity.Property(e => e.EndDt)
                .HasComment("Окончание фрагмента в разговоре")
                .HasColumnName("end_dt");
            entity.Property(e => e.Fspeech)
                .HasComment("Сам фрагмент s_fspeech (если фрагмент=весь разговор не заполняется)")
                .HasColumnName("fspeech");
            entity.Property(e => e.Name)
                .HasMaxLength(100)
                .HasComment("Наименование фрагмента")
                .HasColumnName("name");
            entity.Property(e => e.NumLink)
                .HasComment("Сквозной номер склейки")
                .HasColumnName("num_link");
            entity.Property(e => e.OrderLink)
                .HasComment("Номер пп фрагмента в склейке")
                .HasColumnName("order_link");
            entity.Property(e => e.Rspeech)
                .HasComment("Сам фрагмент s_rspeech (если фрагмент=весь разговор не заполняется)")
                .HasColumnName("rspeech");
            entity.Property(e => e.SInckey).HasColumnName("s_inckey");
            entity.Property(e => e.SpeechFull)
                .HasComment("Фрагмент часть разговора =0 или в кач-ве фрагмента выбран весь разговор =1")
                .HasColumnName("speech_full");
            entity.Property(e => e.SpeechType)
                .HasComment("Какой фрагмент р-ра: из поля s_fspeech =1, из поля s_rspeech =2, микширование полей s_fspeech+s_rspeech =3")
                .HasColumnName("speech_type");
            entity.Property(e => e.Status)
                .HasComment("Статус фрагмента")
                .HasColumnName("status");
            entity.Property(e => e.UpdDt)
                .HasComment("Время последнего изменения записи")
                .HasColumnName("upd_dt");

            entity.HasOne(d => d.SInckeyNavigation).WithMany(p => p.Links)
                .HasForeignKey(d => d.SInckey)
                .HasConstraintName("fk_links");
        });

        modelBuilder.Entity<Measure>(entity =>
        {
            entity.HasKey(e => e.Id).HasName("pk_measure");

            entity.ToTable("measure", "sprut", tb => tb.HasComment("Измерения соседних БС"));

            entity.HasIndex(e => e.Measuredt, "ind1_measure");

            entity.HasIndex(e => e.Imsi, "ind2_measure");

            entity.HasIndex(e => e.Mcc, "ind3_measure");

            entity.HasIndex(e => e.Mnc, "ind4_measure");

            entity.HasIndex(e => e.Lac, "ind5_measure");

            entity.HasIndex(e => e.Cid, "ind6_measure");

            entity.Property(e => e.Id)
                .ValueGeneratedNever()
                .HasColumnName("id");
            entity.Property(e => e.Addition)
                .HasMaxLength(100)
                .HasColumnName("addition");
            entity.Property(e => e.Arfcn)
                .HasMaxLength(20)
                .HasColumnName("arfcn");
            entity.Property(e => e.Cid).HasColumnName("cid");
            entity.Property(e => e.Imsi)
                .HasMaxLength(20)
                .HasColumnName("imsi");
            entity.Property(e => e.Lac).HasColumnName("lac");
            entity.Property(e => e.Mcc).HasColumnName("mcc");
            entity.Property(e => e.Measuredt).HasColumnName("measuredt");
            entity.Property(e => e.Mnc)
                .HasMaxLength(5)
                .HasColumnName("mnc");
            entity.Property(e => e.Rxlevel).HasColumnName("rxlevel");
            entity.Property(e => e.Standard)
                .HasMaxLength(20)
                .HasColumnName("standard");
        });

        modelBuilder.Entity<Operator>(entity =>
        {
            entity.HasKey(e => e.Id).HasName("operators_key");

            entity.ToTable("operators", "sprut", tb => tb.HasComment("MCC+MNC Коды операторов сотовой связи"));

            entity.HasIndex(e => new { e.Mcc, e.Mnc }, "unic_operators_mcc").IsUnique();

            entity.Property(e => e.Id)
                .ValueGeneratedNever()
                .HasColumnName("id");
            entity.Property(e => e.Countryname)
                .HasMaxLength(50)
                .HasComment("Страна")
                .HasColumnName("countryname");
            entity.Property(e => e.Mcc)
                .HasComment("MCC Код стран")
                .HasColumnName("mcc");
            entity.Property(e => e.Mnc)
                .HasMaxLength(5)
                .HasComment("MNC Код оператора")
                .HasColumnName("mnc");
            entity.Property(e => e.Operatorname)
                .HasMaxLength(50)
                .HasComment("Оператор")
                .HasColumnName("operatorname");
            entity.Property(e => e.OperatornameMore)
                .HasMaxLength(250)
                .HasColumnName("operatorname_more");
        });

        modelBuilder.Entity<SKwIST>(entity =>
        {
            entity
                .HasNoKey()
                .ToTable("s_kw_i_s_t", "sprut");

            entity.HasIndex(e => e.SK, "foreign27");

            entity.HasIndex(e => e.SIk, "foreign28");

            entity.Property(e => e.SIk).HasColumnName("s_ik");
            entity.Property(e => e.SK).HasColumnName("s_k");
            entity.Property(e => e.SWtp).HasColumnName("s_wtp");

            entity.HasOne(d => d.SIkNavigation).WithMany()
                .HasForeignKey(d => d.SIk)
                .HasConstraintName("s_kw_i_s_t_foreign_sp");

            entity.HasOne(d => d.SKNavigation).WithMany()
                .HasForeignKey(d => d.SK)
                .HasConstraintName("s_kw_i_s_t_foreign");
        });

        modelBuilder.Entity<SKwISW>(entity =>
        {
            entity
                .HasNoKey()
                .ToView("s_kw_i_s_w", "sprut");

            entity.Property(e => e.SIk).HasColumnName("s_ik");
            entity.Property(e => e.SK).HasColumnName("s_k");
            entity.Property(e => e.SKw)
                .HasMaxLength(80)
                .HasColumnName("s_kw");
            entity.Property(e => e.SWtp).HasColumnName("s_wtp");
        });

        modelBuilder.Entity<SKwIT>(entity =>
        {
            entity.HasKey(e => e.SK).HasName("s_kw_i_t_pkey");

            entity.ToTable("s_kw_i_t", "sprut");

            entity.HasIndex(e => e.SK, "primary26").IsUnique();

            entity.HasIndex(e => e.SKw, "spr_kw_itg_keyword");

            entity.Property(e => e.SK)
                .ValueGeneratedNever()
                .HasColumnName("s_k");
            entity.Property(e => e.SKw)
                .HasMaxLength(80)
                .HasColumnName("s_kw");
        });

        modelBuilder.Entity<SaveText>(entity =>
        {
            entity.HasKey(e => e.Id).HasName("save_text_key");

            entity.ToTable("save_text", "sprut");

            entity.Property(e => e.Id)
                .ValueGeneratedNever()
                .HasColumnName("id");
            entity.Property(e => e.DataId).HasColumnName("data_id");
            entity.Property(e => e.FileName)
                .HasMaxLength(250)
                .HasColumnName("file_name");
            entity.Property(e => e.SName)
                .HasMaxLength(250)
                .HasColumnName("s_name");
            entity.Property(e => e.Status).HasColumnName("status");
            entity.Property(e => e.SummaryId).HasColumnName("summary_id");
            entity.Property(e => e.TextOriginal).HasColumnName("text_original");
            entity.Property(e => e.TextTransfer).HasColumnName("text_transfer");

            entity.HasOne(d => d.Data).WithMany(p => p.SaveTexts)
                .HasForeignKey(d => d.DataId)
                .HasConstraintName("fk_data_id");
        });

        modelBuilder.Entity<Sourcedatum>(entity =>
        {
            entity.HasKey(e => e.Id).HasName("sourcedata_key");

            entity.ToTable("sourcedata", "sprut", tb => tb.HasComment("Источники внешних данных"));

            entity.Property(e => e.Id)
                .ValueGeneratedNever()
                .HasColumnName("id");
            entity.Property(e => e.Commentary)
                .HasMaxLength(250)
                .HasColumnName("commentary");
            entity.Property(e => e.Sourcename)
                .HasMaxLength(250)
                .HasComment("Имя источника данных")
                .HasColumnName("sourcename");
        });

        modelBuilder.Entity<SprArmPlace>(entity =>
        {
            entity.HasKey(e => e.AWorkplace).HasName("pr_key");

            entity.ToTable("spr_arm_places", "sprut");

            entity.HasIndex(e => e.AWorkplace, "primary11").IsUnique();

            entity.Property(e => e.AWorkplace)
                .HasMaxLength(100)
                .HasColumnName("a_workplace");
            entity.Property(e => e.AEnabled).HasColumnName("a_enabled");
            entity.Property(e => e.AValue).HasColumnName("a_value");
        });

        modelBuilder.Entity<SprAutodelete>(entity =>
        {
            entity.HasKey(e => e.DType).HasName("spr_autodelete_pkey");

            entity.ToTable("spr_autodelete", "sprut");

            entity.HasIndex(e => e.DType, "primary2").IsUnique();

            entity.Property(e => e.DType)
                .ValueGeneratedNever()
                .HasColumnName("d_type");
            entity.Property(e => e.BDays).HasColumnName("b_days");
            entity.Property(e => e.BEnabled).HasColumnName("b_enabled");
            entity.Property(e => e.BFromlast).HasColumnName("b_fromlast");
            entity.Property(e => e.DDays).HasColumnName("d_days");
            entity.Property(e => e.DEnabled).HasColumnName("d_enabled");
            entity.Property(e => e.DFromlast).HasColumnName("d_fromlast");
        });

        modelBuilder.Entity<SprCalltype>(entity =>
        {
            entity.HasKey(e => e.Val).HasName("spr_calltype_pk");

            entity.ToTable("spr_calltype", "sprut");

            entity.HasIndex(e => e.ValName, "2").IsUnique();

            entity.HasIndex(e => e.Val, "primary1").IsUnique();

            entity.Property(e => e.Val)
                .ValueGeneratedNever()
                .HasColumnName("val");
            entity.Property(e => e.ValName)
                .HasMaxLength(50)
                .HasColumnName("val_name");
        });

        modelBuilder.Entity<SprCategory>(entity =>
        {
            entity.HasKey(e => e.Val).HasName("spr_category_pkey");

            entity.ToTable("spr_category", "sprut");

            entity.HasIndex(e => e.ValName, "10").IsUnique();

            entity.HasIndex(e => e.Val, "primary9").IsUnique();

            entity.Property(e => e.Val)
                .ValueGeneratedNever()
                .HasColumnName("val");
            entity.Property(e => e.ValName)
                .HasMaxLength(50)
                .HasColumnName("val_name");
        });

        modelBuilder.Entity<SprEvent>(entity =>
        {
            entity.HasKey(e => e.Val).HasName("spr_event_pkey");

            entity.ToTable("spr_event", "sprut");

            entity.HasIndex(e => e.ValName, "8").IsUnique();

            entity.HasIndex(e => e.Val, "primary7").IsUnique();

            entity.HasIndex(e => e.ValName, "spr_event_val_name_key").IsUnique();

            entity.Property(e => e.Val)
                .ValueGeneratedNever()
                .HasColumnName("val");
            entity.Property(e => e.ValName)
                .HasMaxLength(50)
                .HasColumnName("val_name");
        });

        modelBuilder.Entity<SprHistoryTable>(entity =>
        {
            entity.HasKey(e => e.HInckey).HasName("spr_history_table_pkey");

            entity.ToTable("spr_history_table", "sprut");

            entity.HasIndex(e => e.HDatetime, "history_datetime");

            entity.HasIndex(e => e.HInckey, "primary25").IsUnique();

            entity.Property(e => e.HInckey)
                .ValueGeneratedNever()
                .HasColumnName("h_inckey");
            entity.Property(e => e.HComment).HasColumnName("h_comment");
            entity.Property(e => e.HDatetime).HasColumnName("h_datetime");
            entity.Property(e => e.HEvent)
                .HasMaxLength(100)
                .HasColumnName("h_event");
            entity.Property(e => e.HPostid)
                .HasMaxLength(20)
                .HasColumnName("h_postid");
            entity.Property(e => e.HPrelooked).HasColumnName("h_prelooked");
            entity.Property(e => e.HReplicated).HasColumnName("h_replicated");
        });

        modelBuilder.Entity<SprSelstatus>(entity =>
        {
            entity.HasKey(e => e.Val).HasName("spr_selstatus_pkey");

            entity.ToTable("spr_selstatus", "sprut");

            entity.HasIndex(e => e.ValName, "4").IsUnique();

            entity.HasIndex(e => e.Val, "primary3").IsUnique();

            entity.HasIndex(e => e.ValName, "spr_selstatus_val_name_key").IsUnique();

            entity.Property(e => e.Val)
                .ValueGeneratedNever()
                .HasColumnName("val");
            entity.Property(e => e.ValName)
                .HasMaxLength(50)
                .HasColumnName("val_name");
        });

        modelBuilder.Entity<SprSpAddnumTable>(entity =>
        {
            entity
                .HasNoKey()
                .ToTable("spr_sp_addnum_table", "sprut");

            entity.HasIndex(e => e.SInckey, "foreign24");

            entity.HasIndex(e => e.SName, "spr_sp_addnum_sourcename");

            entity.HasIndex(e => e.SNumber, "spr_sp_addnum_sysnumber");

            entity.HasIndex(e => e.SNumberRev, "spr_sp_addnum_usernumber_rev");

            entity.Property(e => e.SId).HasColumnName("s_id");
            entity.Property(e => e.SInckey).HasColumnName("s_inckey");
            entity.Property(e => e.SName)
                .HasMaxLength(40)
                .HasColumnName("s_name");
            entity.Property(e => e.SNumber)
                .HasMaxLength(40)
                .HasColumnName("s_number");
            entity.Property(e => e.SNumberRev)
                .HasMaxLength(40)
                .HasColumnName("s_number_rev");
            entity.Property(e => e.SNumberclass).HasColumnName("s_numberclass");
            entity.Property(e => e.SNumbertype)
                .HasMaxLength(20)
                .HasColumnName("s_numbertype");
            entity.Property(e => e.SSt).HasColumnName("s_st");

            entity.HasOne(d => d.SInckeyNavigation).WithMany()
                .HasForeignKey(d => d.SInckey)
                .HasConstraintName("spaddnum_inckey");
        });

        modelBuilder.Entity<SprSpCommentTable>(entity =>
        {
            entity
                .HasNoKey()
                .ToTable("spr_sp_comment_table", "sprut");

            entity.HasIndex(e => e.SInckey, "foreign16");

            entity.HasIndex(e => e.SInckey, "spr_sp_comment_table_s_inckey_key").IsUnique();

            entity.Property(e => e.SComment).HasColumnName("s_comment");
            entity.Property(e => e.SInckey).HasColumnName("s_inckey");

            entity.HasOne(d => d.SInckeyNavigation).WithOne()
                .HasForeignKey<SprSpCommentTable>(d => d.SInckey)
                .HasConstraintName("spcm_inckey");
        });

        modelBuilder.Entity<SprSpData1Table>(entity =>
        {
            entity.HasKey(e => e.Id).HasName("pk_data_1_table");

            entity.ToTable("spr_sp_data_1_table", "sprut");

            entity.HasIndex(e => e.SInckey, "foreign17");

            entity.HasIndex(e => e.SRecordtype, "spdata_rectype");

            entity.HasIndex(e => new { e.SInckey, e.SOrder }, "spdt1_incorder").IsUnique();

            entity.Property(e => e.Id)
                .ValueGeneratedNever()
                .HasColumnName("id");
            entity.Property(e => e.Jsonparam).HasColumnName("jsonparam");
            entity.Property(e => e.SFspeech).HasColumnName("s_fspeech");
            entity.Property(e => e.SInckey).HasColumnName("s_inckey");
            entity.Property(e => e.SOrder).HasColumnName("s_order");
            entity.Property(e => e.SRecordtype)
                .HasMaxLength(30)
                .HasColumnName("s_recordtype");
            entity.Property(e => e.SRspeech).HasColumnName("s_rspeech");
            entity.Property(e => e.SSpbookmark).HasColumnName("s_spbookmark");
            entity.Property(e => e.SSpbookmarkrev).HasColumnName("s_spbookmarkrev");

            entity.HasOne(d => d.SInckeyNavigation).WithMany(p => p.SprSpData1Tables)
                .HasForeignKey(d => d.SInckey)
                .HasConstraintName("spdt1_inckey");
        });

        modelBuilder.Entity<SprSpFotoTable>(entity =>
        {
            entity.HasKey(e => e.Id).HasName("pk_foto_table");

            entity.ToTable("spr_sp_foto_table", "sprut");

            entity.HasIndex(e => e.SInckey, "foreign20");

            entity.HasIndex(e => e.FPname, "foto_panorama_name");

            entity.Property(e => e.Id)
                .ValueGeneratedNever()
                .HasColumnName("id");
            entity.Property(e => e.FBearing).HasColumnName("f_bearing");
            entity.Property(e => e.FCamera).HasColumnName("f_camera");
            entity.Property(e => e.FDatetime).HasColumnName("f_datetime");
            entity.Property(e => e.FElevation).HasColumnName("f_elevation");
            entity.Property(e => e.FImage).HasColumnName("f_image");
            entity.Property(e => e.FPname)
                .HasMaxLength(200)
                .HasColumnName("f_pname");
            entity.Property(e => e.SInckey).HasColumnName("s_inckey");

            entity.HasOne(d => d.SInckeyNavigation).WithMany(p => p.SprSpFotoTables)
                .HasForeignKey(d => d.SInckey)
                .HasConstraintName("spfoto_inckey");
        });

        modelBuilder.Entity<SprSpGeoTable>(entity =>
        {
            entity
                .HasNoKey()
                .ToTable("spr_sp_geo_table", "sprut");

            entity.HasIndex(e => e.SInckey, "foreign19");

            entity.HasIndex(e => e.SLatitude, "sp_geo_latitude");

            entity.HasIndex(e => e.SLongitude, "sp_geo_longitude");

            entity.HasIndex(e => e.SObject, "sp_geo_object");

            entity.Property(e => e.SA).HasColumnName("s_a");
            entity.Property(e => e.SAntA).HasColumnName("s_ant_a");
            entity.Property(e => e.SAntAw).HasColumnName("s_ant_aw");
            entity.Property(e => e.SAreasize).HasColumnName("s_areasize");
            entity.Property(e => e.SAw).HasColumnName("s_aw");
            entity.Property(e => e.SCommand).HasColumnName("s_command");
            entity.Property(e => e.SD).HasColumnName("s_d");
            entity.Property(e => e.SElevation).HasColumnName("s_elevation");
            entity.Property(e => e.SGeosource)
                .HasMaxLength(20)
                .HasColumnName("s_geosource");
            entity.Property(e => e.SHeight).HasColumnName("s_height");
            entity.Property(e => e.SInckey).HasColumnName("s_inckey");
            entity.Property(e => e.SLatitude).HasColumnName("s_latitude");
            entity.Property(e => e.SLongitude).HasColumnName("s_longitude");
            entity.Property(e => e.SObject)
                .HasMaxLength(250)
                .HasColumnName("s_object");
            entity.Property(e => e.SOrder).HasColumnName("s_order");
            entity.Property(e => e.SQuality).HasColumnName("s_quality");
            entity.Property(e => e.SRxlevel).HasColumnName("s_rxlevel");
            entity.Property(e => e.SSelect).HasColumnName("s_select");
            entity.Property(e => e.SSrctype).HasColumnName("s_srctype");
            entity.Property(e => e.STime).HasColumnName("s_time");

            entity.HasOne(d => d.SInckeyNavigation).WithMany()
                .HasForeignKey(d => d.SInckey)
                .HasConstraintName("spgeo_inckey");

            entity.HasOne(d => d.SSrctypeNavigation).WithMany()
                .HasForeignKey(d => d.SSrctype)
                .OnDelete(DeleteBehavior.Cascade)
                .HasConstraintName("spgeo_srctype");
        });

        modelBuilder.Entity<SprSpLastsvaTable>(entity =>
        {
            entity
                .HasNoKey()
                .ToTable("spr_sp_lastsva_table", "sprut");

            entity.HasIndex(e => e.SInckey, "foreign21");

            entity.HasIndex(e => e.SKc, "lastsva_kc");

            entity.Property(e => e.SInckey).HasColumnName("s_inckey");
            entity.Property(e => e.SKc).HasColumnName("s_kc");

            entity.HasOne(d => d.SInckeyNavigation).WithMany()
                .HasForeignKey(d => d.SInckey)
                .HasConstraintName("lastsva_inckey");
        });

        modelBuilder.Entity<SprSpMccmnc>(entity =>
        {
            entity.HasKey(e => e.Id).HasName("pk_mccmnc");

            entity.ToTable("spr_sp_mccmnc", "sprut");

            entity.HasIndex(e => e.SInckey, "ind1_mccmnc");

            entity.Property(e => e.Id)
                .ValueGeneratedNever()
                .HasColumnName("id");
            entity.Property(e => e.Mcc).HasColumnName("mcc");
            entity.Property(e => e.Mnc)
                .HasMaxLength(5)
                .HasColumnName("mnc");
            entity.Property(e => e.SInckey).HasColumnName("s_inckey");

            entity.HasOne(d => d.SInckeyNavigation).WithMany(p => p.SprSpMccmncs)
                .HasForeignKey(d => d.SInckey)
                .HasConstraintName("fk_sp_mccmnc");
        });

        modelBuilder.Entity<SprSpPanoramaTable>(entity =>
        {
            entity
                .HasNoKey()
                .ToTable("spr_sp_panorama_table", "sprut");

            entity.HasIndex(e => e.PName, "panorama_name").IsUnique();

            entity.Property(e => e.PBmax).HasColumnName("p_bmax");
            entity.Property(e => e.PBmin).HasColumnName("p_bmin");
            entity.Property(e => e.PDatetime).HasColumnName("p_datetime");
            entity.Property(e => e.PDesc)
                .HasMaxLength(128)
                .HasColumnName("p_desc");
            entity.Property(e => e.PEmax).HasColumnName("p_emax");
            entity.Property(e => e.PEmin).HasColumnName("p_emin");
            entity.Property(e => e.PImage).HasColumnName("p_image");
            entity.Property(e => e.PLatitude).HasColumnName("p_latitude");
            entity.Property(e => e.PLongitude).HasColumnName("p_longitude");
            entity.Property(e => e.PName)
                .HasMaxLength(200)
                .HasColumnName("p_name");
        });

        modelBuilder.Entity<SprSpThemeTable>(entity =>
        {
            entity
                .HasNoKey()
                .ToTable("spr_sp_theme_table", "sprut");

            entity.HasIndex(e => new { e.SInckey, e.STheme }, "23").IsUnique();

            entity.HasIndex(e => e.SInckey, "foreign22");

            entity.HasIndex(e => e.STheme, "sp_theme_theme");

            entity.HasIndex(e => new { e.SInckey, e.STheme }, "sptm_inctheme").IsUnique();

            entity.Property(e => e.SInckey).HasColumnName("s_inckey");
            entity.Property(e => e.SPrelooked).HasColumnName("s_prelooked");
            entity.Property(e => e.STheme)
                .HasMaxLength(40)
                .HasColumnName("s_theme");

            entity.HasOne(d => d.SInckeyNavigation).WithMany()
                .HasForeignKey(d => d.SInckey)
                .HasConstraintName("sptm_inckey");
        });

        modelBuilder.Entity<SprSpWorkhistoryTable>(entity =>
        {
            entity
                .HasNoKey()
                .ToTable("spr_sp_workhistory_table", "sprut");

            entity.HasIndex(e => e.SInckey, "foreign14");

            entity.Property(e => e.SAction)
                .HasMaxLength(250)
                .HasColumnName("s_action");
            entity.Property(e => e.SComp)
                .HasMaxLength(50)
                .HasColumnName("s_comp");
            entity.Property(e => e.SDatetime).HasColumnName("s_datetime");
            entity.Property(e => e.SDuration).HasColumnName("s_duration");
            entity.Property(e => e.SInckey).HasColumnName("s_inckey");
            entity.Property(e => e.SUsernic)
                .HasMaxLength(31)
                .HasColumnName("s_usernic");
            entity.Property(e => e.SUserreal)
                .HasMaxLength(100)
                .HasColumnName("s_userreal");

            entity.HasOne(d => d.SInckeyNavigation).WithMany()
                .HasForeignKey(d => d.SInckey)
                .HasConstraintName("spwh_inckey");
        });

        modelBuilder.Entity<SprSpeechTable>(entity =>
        {
            entity.HasKey(e => e.SInckey).HasName("speech_key");

            entity.ToTable("spr_speech_table", "sprut");

            entity.HasIndex(e => e.SInckey, "primary13").IsUnique();

            entity.HasIndex(e => e.SBasestation, "speech_basestation");

            entity.HasIndex(e => e.SCid, "speech_cid");

            entity.HasIndex(e => e.SDatetime, "speech_datetime");

            entity.HasIndex(e => e.SEventcode, "speech_eventcode");

            entity.HasIndex(e => e.SLac, "speech_lac");

            entity.HasIndex(e => new { e.SLac, e.SCid }, "speech_lac_cid");

            entity.HasIndex(e => e.SNotice, "speech_notice");

            entity.HasIndex(e => e.SPostkey, "speech_postkey");

            entity.HasIndex(e => e.SSourcename, "speech_sourcename");

            entity.HasIndex(e => e.SSysnumber, "speech_sysnumber");

            entity.HasIndex(e => e.SSysnumber2, "speech_sysnumber2");

            entity.HasIndex(e => e.SSysnumber3, "speech_sysnumber3");

            entity.HasIndex(e => e.STalker, "speech_talker");

            entity.HasIndex(e => e.STalkerBs, "speech_talker_bs");

            entity.HasIndex(e => e.STalkerCid, "speech_talker_cid");

            entity.HasIndex(e => e.STalkerLac, "speech_talker_lac");

            entity.HasIndex(e => e.STalkerRev, "speech_talker_rev");

            entity.HasIndex(e => e.STalkerSn, "speech_talker_sn");

            entity.HasIndex(e => e.STalkerSn2, "speech_talker_sn2");

            entity.HasIndex(e => e.STalkerSn3, "speech_talker_sn3");

            entity.HasIndex(e => e.STalkername, "speech_talkername");

            entity.HasIndex(e => e.SUsernumber, "speech_usernumber");

            entity.HasIndex(e => e.SUsernumberRev, "speech_usernumber_rev");

            entity.Property(e => e.SInckey)
                .ValueGeneratedNever()
                .HasColumnName("s_inckey");
            entity.Property(e => e.BsOperators)
                .HasMaxLength(250)
                .HasColumnName("bs_operators");
            entity.Property(e => e.DeleteStatus).HasColumnName("delete_status");
            entity.Property(e => e.SBasestation)
                .HasMaxLength(250)
                .HasColumnName("s_basestation");
            entity.Property(e => e.SBasestationEnd)
                .HasMaxLength(250)
                .HasColumnName("s_basestation_end");
            entity.Property(e => e.SBelong)
                .HasMaxLength(50)
                .HasColumnName("s_belong");
            entity.Property(e => e.SCalltype).HasColumnName("s_calltype");
            entity.Property(e => e.SCid)
                .HasMaxLength(30)
                .HasColumnName("s_cid");
            entity.Property(e => e.SCidEnd)
                .HasMaxLength(30)
                .HasColumnName("s_cid_end");
            entity.Property(e => e.SCipherid)
                .HasComment("Привязка записи к Учету(шифр задания)")
                .HasColumnName("s_cipherid");
            entity.Property(e => e.SDatetime).HasColumnName("s_datetime");
            entity.Property(e => e.SDchannel).HasColumnName("s_dchannel");
            entity.Property(e => e.SDecryptinfo)
                .HasMaxLength(10)
                .HasColumnName("s_decryptinfo");
            entity.Property(e => e.SDescription)
                .HasMaxLength(128)
                .HasColumnName("s_description");
            entity.Property(e => e.SDeviceid)
                .HasMaxLength(20)
                .HasColumnName("s_deviceid");
            entity.Property(e => e.SDuration).HasColumnName("s_duration");
            entity.Property(e => e.SEquipment)
                .HasMaxLength(128)
                .HasColumnName("s_equipment");
            entity.Property(e => e.SEvent).HasColumnName("s_event");
            entity.Property(e => e.SEventcode)
                .HasMaxLength(30)
                .HasColumnName("s_eventcode");
            entity.Property(e => e.SFrequency)
                .HasMaxLength(20)
                .HasColumnName("s_frequency");
            entity.Property(e => e.SLac)
                .HasMaxLength(30)
                .HasColumnName("s_lac");
            entity.Property(e => e.SLacEnd)
                .HasMaxLength(30)
                .HasColumnName("s_lac_end");
            entity.Property(e => e.SNetwork)
                .HasMaxLength(50)
                .HasColumnName("s_network");
            entity.Property(e => e.SNotice)
                .HasMaxLength(100)
                .HasColumnName("s_notice");
            entity.Property(e => e.SPostid)
                .HasMaxLength(20)
                .HasColumnName("s_postid");
            entity.Property(e => e.SPostkey)
                .HasMaxLength(64)
                .HasColumnName("s_postkey");
            entity.Property(e => e.SPrelooked).HasColumnName("s_prelooked");
            entity.Property(e => e.SPriority).HasColumnName("s_priority");
            entity.Property(e => e.SRchannel).HasColumnName("s_rchannel");
            entity.Property(e => e.SReplicated).HasColumnName("s_replicated");
            entity.Property(e => e.SSelstatus).HasColumnName("s_selstatus");
            entity.Property(e => e.SSourcedataid)
                .HasComment("Источник поступления записи")
                .HasColumnName("s_sourcedataid");
            entity.Property(e => e.SSourceid).HasColumnName("s_sourceid");
            entity.Property(e => e.SSourcename)
                .HasMaxLength(250)
                .HasColumnName("s_sourcename");
            entity.Property(e => e.SStandard)
                .HasMaxLength(20)
                .HasColumnName("s_standard");
            entity.Property(e => e.SStatus).HasColumnName("s_status");
            entity.Property(e => e.SSysnumber)
                .HasMaxLength(20)
                .HasColumnName("s_sysnumber");
            entity.Property(e => e.SSysnumber2)
                .HasMaxLength(20)
                .HasColumnName("s_sysnumber2");
            entity.Property(e => e.SSysnumber3)
                .HasMaxLength(20)
                .HasColumnName("s_sysnumber3");
            entity.Property(e => e.SSysnumbertype)
                .HasMaxLength(20)
                .HasColumnName("s_sysnumbertype");
            entity.Property(e => e.SSysnumbertype2)
                .HasMaxLength(20)
                .HasColumnName("s_sysnumbertype2");
            entity.Property(e => e.SSysnumbertype3)
                .HasMaxLength(20)
                .HasColumnName("s_sysnumbertype3");
            entity.Property(e => e.STalker)
                .HasMaxLength(40)
                .HasColumnName("s_talker");
            entity.Property(e => e.STalkerBs)
                .HasMaxLength(250)
                .HasColumnName("s_talker_bs");
            entity.Property(e => e.STalkerBsEnd)
                .HasMaxLength(250)
                .HasColumnName("s_talker_bs_end");
            entity.Property(e => e.STalkerCid)
                .HasMaxLength(30)
                .HasColumnName("s_talker_cid");
            entity.Property(e => e.STalkerCidEnd)
                .HasMaxLength(30)
                .HasColumnName("s_talker_cid_end");
            entity.Property(e => e.STalkerLac)
                .HasMaxLength(30)
                .HasColumnName("s_talker_lac");
            entity.Property(e => e.STalkerLacEnd)
                .HasMaxLength(30)
                .HasColumnName("s_talker_lac_end");
            entity.Property(e => e.STalkerRev)
                .HasMaxLength(40)
                .HasColumnName("s_talker_rev");
            entity.Property(e => e.STalkerSn)
                .HasMaxLength(20)
                .HasColumnName("s_talker_sn");
            entity.Property(e => e.STalkerSn2)
                .HasMaxLength(20)
                .HasColumnName("s_talker_sn2");
            entity.Property(e => e.STalkerSn3)
                .HasMaxLength(20)
                .HasColumnName("s_talker_sn3");
            entity.Property(e => e.STalkerSntype)
                .HasMaxLength(20)
                .HasColumnName("s_talker_sntype");
            entity.Property(e => e.STalkerSntype2)
                .HasMaxLength(20)
                .HasColumnName("s_talker_sntype2");
            entity.Property(e => e.STalkerSntype3)
                .HasMaxLength(20)
                .HasColumnName("s_talker_sntype3");
            entity.Property(e => e.STalkerid).HasColumnName("s_talkerid");
            entity.Property(e => e.STalkername)
                .HasMaxLength(40)
                .HasColumnName("s_talkername");
            entity.Property(e => e.SType).HasColumnName("s_type");
            entity.Property(e => e.SUsernumber)
                .HasMaxLength(40)
                .HasColumnName("s_usernumber");
            entity.Property(e => e.SUsernumberRev)
                .HasMaxLength(40)
                .HasColumnName("s_usernumber_rev");

            entity.HasOne(d => d.SCalltypeNavigation).WithMany(p => p.SprSpeechTables)
                .HasForeignKey(d => d.SCalltype)
                .HasConstraintName("speech_calltype");

            entity.HasOne(d => d.SEventNavigation).WithMany(p => p.SprSpeechTables)
                .HasForeignKey(d => d.SEvent)
                .HasConstraintName("speech_event");

            entity.HasOne(d => d.SSelstatusNavigation).WithMany(p => p.SprSpeechTables)
                .HasForeignKey(d => d.SSelstatus)
                .HasConstraintName("speech_selstatus");

            entity.HasOne(d => d.SStatusNavigation).WithMany(p => p.SprSpeechTables)
                .HasForeignKey(d => d.SStatus)
                .HasConstraintName("speech_status");

            entity.HasOne(d => d.STypeNavigation).WithMany(p => p.SprSpeechTables)
                .HasForeignKey(d => d.SType)
                .OnDelete(DeleteBehavior.ClientSetNull)
                .HasConstraintName("speech_type");
        });

        modelBuilder.Entity<SprSrctype>(entity =>
        {
            entity.HasKey(e => e.SrctypeId).HasName("spr_srctype_pkey");

            entity.ToTable("spr_srctype", "sprut");

            entity.Property(e => e.SrctypeId)
                .ValueGeneratedNever()
                .HasColumnName("srctype_id");
            entity.Property(e => e.Name)
                .HasMaxLength(30)
                .HasColumnName("name");
        });

        modelBuilder.Entity<SprStatus>(entity =>
        {
            entity.HasKey(e => e.Val).HasName("spr_status_pkey");

            entity.ToTable("spr_status", "sprut");

            entity.HasIndex(e => e.ValName, "6").IsUnique();

            entity.HasIndex(e => e.Val, "primary5").IsUnique();

            entity.HasIndex(e => e.ValName, "spr_status_val_name_key").IsUnique();

            entity.Property(e => e.Val)
                .ValueGeneratedNever()
                .HasColumnName("val");
            entity.Property(e => e.ValName)
                .HasMaxLength(50)
                .HasColumnName("val_name");
        });

        modelBuilder.Entity<Summary>(entity =>
        {
            entity.HasKey(e => e.Id).HasName("summary_pkey");

            entity.ToTable("summary", "sprut");

            entity.Property(e => e.Id)
                .ValueGeneratedNever()
                .HasColumnName("id");
            entity.Property(e => e.SDatetime).HasColumnName("s_datetime");
            entity.Property(e => e.Status).HasColumnName("status");
            entity.Property(e => e.SummaryText).HasColumnName("summary_text");
        });

        modelBuilder.Entity<TacticSit>(entity =>
        {
            entity.HasKey(e => e.Id).HasName("tactic_sit_pkey");

            entity.ToTable("tactic_sit", "sprut");

            entity.Property(e => e.Id)
                .ValueGeneratedNever()
                .HasColumnName("id");
            entity.Property(e => e.Isround).HasColumnName("isround");
            entity.Property(e => e.ObjectType).HasColumnName("object_type");
            entity.Property(e => e.Polygon).HasColumnName("polygon");
            entity.Property(e => e.PrimitiveType).HasColumnName("primitive_type");
            entity.Property(e => e.SColor).HasColumnName("s_color");
            entity.Property(e => e.SLatitude).HasColumnName("s_latitude");
            entity.Property(e => e.SLongitude).HasColumnName("s_longitude");
            entity.Property(e => e.SName)
                .HasMaxLength(250)
                .HasColumnName("s_name");
            entity.Property(e => e.TypeLine).HasColumnName("type_line");
        });

        modelBuilder.Entity<UsersTable>(entity =>
        {
            entity
                .HasNoKey()
                .ToTable("users_table", "sprut");

            entity.Property(e => e.Uuid)
                .HasMaxLength(40)
                .HasColumnName("uuid");
        });

        modelBuilder.Entity<VCountDirection>(entity =>
        {
            entity
                .HasNoKey()
                .ToView("v_count_direction", "sprut");

            entity.Property(e => e.CountInckey).HasColumnName("count_inckey");
            entity.Property(e => e.DirectionId).HasColumnName("direction_id");
            entity.Property(e => e.SEventcode)
                .HasMaxLength(30)
                .HasColumnName("s_eventcode");
            entity.Property(e => e.SType).HasColumnName("s_type");
            entity.Property(e => e.Theme).HasColumnName("theme");
            entity.Property(e => e.ValName).HasColumnName("val_name");
        });

        modelBuilder.Entity<VSpeechDatetime>(entity =>
        {
            entity
                .HasNoKey()
                .ToView("v_speech_datetime", "sprut");

            entity.Property(e => e.BsOperators)
                .HasMaxLength(250)
                .HasColumnName("bs_operators");
            entity.Property(e => e.DeleteStatus).HasColumnName("delete_status");
            entity.Property(e => e.SBasestation)
                .HasMaxLength(250)
                .HasColumnName("s_basestation");
            entity.Property(e => e.SBasestationEnd)
                .HasMaxLength(250)
                .HasColumnName("s_basestation_end");
            entity.Property(e => e.SBelong)
                .HasMaxLength(50)
                .HasColumnName("s_belong");
            entity.Property(e => e.SCalltype).HasColumnName("s_calltype");
            entity.Property(e => e.SCid)
                .HasMaxLength(30)
                .HasColumnName("s_cid");
            entity.Property(e => e.SCidEnd)
                .HasMaxLength(30)
                .HasColumnName("s_cid_end");
            entity.Property(e => e.SCipherid).HasColumnName("s_cipherid");
            entity.Property(e => e.SDate).HasColumnName("s_date");
            entity.Property(e => e.SDatetime).HasColumnName("s_datetime");
            entity.Property(e => e.SDay).HasColumnName("s_day");
            entity.Property(e => e.SDchannel).HasColumnName("s_dchannel");
            entity.Property(e => e.SDecryptinfo)
                .HasMaxLength(10)
                .HasColumnName("s_decryptinfo");
            entity.Property(e => e.SDescription)
                .HasMaxLength(128)
                .HasColumnName("s_description");
            entity.Property(e => e.SDeviceid)
                .HasMaxLength(20)
                .HasColumnName("s_deviceid");
            entity.Property(e => e.SDuration).HasColumnName("s_duration");
            entity.Property(e => e.SEquipment)
                .HasMaxLength(128)
                .HasColumnName("s_equipment");
            entity.Property(e => e.SEvent).HasColumnName("s_event");
            entity.Property(e => e.SEventcode)
                .HasMaxLength(30)
                .HasColumnName("s_eventcode");
            entity.Property(e => e.SFrequency)
                .HasMaxLength(20)
                .HasColumnName("s_frequency");
            entity.Property(e => e.SHour).HasColumnName("s_hour");
            entity.Property(e => e.SInckey).HasColumnName("s_inckey");
            entity.Property(e => e.SLac)
                .HasMaxLength(30)
                .HasColumnName("s_lac");
            entity.Property(e => e.SLacEnd)
                .HasMaxLength(30)
                .HasColumnName("s_lac_end");
            entity.Property(e => e.SMinute).HasColumnName("s_minute");
            entity.Property(e => e.SMonth).HasColumnName("s_month");
            entity.Property(e => e.SNetwork)
                .HasMaxLength(50)
                .HasColumnName("s_network");
            entity.Property(e => e.SNotice)
                .HasMaxLength(100)
                .HasColumnName("s_notice");
            entity.Property(e => e.SPostid)
                .HasMaxLength(20)
                .HasColumnName("s_postid");
            entity.Property(e => e.SPostkey)
                .HasMaxLength(64)
                .HasColumnName("s_postkey");
            entity.Property(e => e.SPrelooked).HasColumnName("s_prelooked");
            entity.Property(e => e.SPriority).HasColumnName("s_priority");
            entity.Property(e => e.SRchannel).HasColumnName("s_rchannel");
            entity.Property(e => e.SReplicated).HasColumnName("s_replicated");
            entity.Property(e => e.SSelstatus).HasColumnName("s_selstatus");
            entity.Property(e => e.SSourcedataid).HasColumnName("s_sourcedataid");
            entity.Property(e => e.SSourceid).HasColumnName("s_sourceid");
            entity.Property(e => e.SSourcename)
                .HasMaxLength(250)
                .HasColumnName("s_sourcename");
            entity.Property(e => e.SStandard)
                .HasMaxLength(20)
                .HasColumnName("s_standard");
            entity.Property(e => e.SStatus).HasColumnName("s_status");
            entity.Property(e => e.SSysnumber)
                .HasMaxLength(20)
                .HasColumnName("s_sysnumber");
            entity.Property(e => e.SSysnumber2)
                .HasMaxLength(20)
                .HasColumnName("s_sysnumber2");
            entity.Property(e => e.SSysnumber3)
                .HasMaxLength(20)
                .HasColumnName("s_sysnumber3");
            entity.Property(e => e.SSysnumbertype)
                .HasMaxLength(20)
                .HasColumnName("s_sysnumbertype");
            entity.Property(e => e.SSysnumbertype2)
                .HasMaxLength(20)
                .HasColumnName("s_sysnumbertype2");
            entity.Property(e => e.SSysnumbertype3)
                .HasMaxLength(20)
                .HasColumnName("s_sysnumbertype3");
            entity.Property(e => e.STalker)
                .HasMaxLength(40)
                .HasColumnName("s_talker");
            entity.Property(e => e.STalkerBs)
                .HasMaxLength(250)
                .HasColumnName("s_talker_bs");
            entity.Property(e => e.STalkerBsEnd)
                .HasMaxLength(250)
                .HasColumnName("s_talker_bs_end");
            entity.Property(e => e.STalkerCid)
                .HasMaxLength(30)
                .HasColumnName("s_talker_cid");
            entity.Property(e => e.STalkerCidEnd)
                .HasMaxLength(30)
                .HasColumnName("s_talker_cid_end");
            entity.Property(e => e.STalkerLac)
                .HasMaxLength(30)
                .HasColumnName("s_talker_lac");
            entity.Property(e => e.STalkerLacEnd)
                .HasMaxLength(30)
                .HasColumnName("s_talker_lac_end");
            entity.Property(e => e.STalkerRev)
                .HasMaxLength(40)
                .HasColumnName("s_talker_rev");
            entity.Property(e => e.STalkerSn)
                .HasMaxLength(20)
                .HasColumnName("s_talker_sn");
            entity.Property(e => e.STalkerSn2)
                .HasMaxLength(20)
                .HasColumnName("s_talker_sn2");
            entity.Property(e => e.STalkerSn3)
                .HasMaxLength(20)
                .HasColumnName("s_talker_sn3");
            entity.Property(e => e.STalkerSntype)
                .HasMaxLength(20)
                .HasColumnName("s_talker_sntype");
            entity.Property(e => e.STalkerSntype2)
                .HasMaxLength(20)
                .HasColumnName("s_talker_sntype2");
            entity.Property(e => e.STalkerSntype3)
                .HasMaxLength(20)
                .HasColumnName("s_talker_sntype3");
            entity.Property(e => e.STalkerid).HasColumnName("s_talkerid");
            entity.Property(e => e.STalkername)
                .HasMaxLength(40)
                .HasColumnName("s_talkername");
            entity.Property(e => e.STime).HasColumnName("s_time");
            entity.Property(e => e.SType).HasColumnName("s_type");
            entity.Property(e => e.SUsernumber)
                .HasMaxLength(40)
                .HasColumnName("s_usernumber");
            entity.Property(e => e.SUsernumberRev)
                .HasMaxLength(40)
                .HasColumnName("s_usernumber_rev");
            entity.Property(e => e.SWeekdate).HasColumnName("s_weekdate");
            entity.Property(e => e.SYear).HasColumnName("s_year");
            entity.Property(e => e.SYearday).HasColumnName("s_yearday");
        });

        modelBuilder.Entity<VSprSpData1>(entity =>
        {
            entity
                .HasNoKey()
                .ToView("v_spr_sp_data_1", "sprut");

            entity.Property(e => e.Id).HasColumnName("id");
            entity.Property(e => e.Jsonparam).HasColumnName("jsonparam");
            entity.Property(e => e.SFspeech).HasColumnName("s_fspeech");
            entity.Property(e => e.SFspeechText).HasColumnName("s_fspeech_text");
            entity.Property(e => e.SInckey).HasColumnName("s_inckey");
            entity.Property(e => e.SOrder).HasColumnName("s_order");
            entity.Property(e => e.SRecordtype)
                .HasMaxLength(30)
                .HasColumnName("s_recordtype");
            entity.Property(e => e.SRspeech).HasColumnName("s_rspeech");
            entity.Property(e => e.SRspeechText).HasColumnName("s_rspeech_text");
            entity.Property(e => e.SSpbookmark).HasColumnName("s_spbookmark");
            entity.Property(e => e.SSpbookmarkrev).HasColumnName("s_spbookmarkrev");
        });

        modelBuilder.Entity<VThemeTable>(entity =>
        {
            entity
                .HasNoKey()
                .ToView("v_theme_table", "sprut");

            entity.Property(e => e.DirectionId).HasColumnName("direction_id");
            entity.Property(e => e.Id).HasColumnName("id");
            entity.Property(e => e.Prelooked).HasColumnName("prelooked");
            entity.Property(e => e.SInckey).HasColumnName("s_inckey");
            entity.Property(e => e.Theme)
                .HasMaxLength(40)
                .HasColumnName("theme");
        });
        modelBuilder.HasSequence("category_event_seq", "sprut").HasMin(0L);
        modelBuilder.HasSequence("data_1_seq", "sprut");
        modelBuilder.HasSequence("direction_seq", "sprut");
        modelBuilder.HasSequence("direction_speech_seq", "sprut");
        modelBuilder.HasSequence("foto_seq", "sprut");
        modelBuilder.HasSequence("gen_operators", "sprut");
        modelBuilder.HasSequence("history_seq", "sprut");
        modelBuilder.HasSequence("img_area_seq", "sprut");
        modelBuilder.HasSequence("jobcipher_seq", "sprut");
        modelBuilder.HasSequence("kw_seq", "sprut");
        modelBuilder.HasSequence("links_num_seq", "sprut").HasMin(0L);
        modelBuilder.HasSequence("links_seq", "sprut").HasMin(0L);
        modelBuilder.HasSequence("mccmnc_seq", "sprut");
        modelBuilder.HasSequence("measure_seq", "sprut");
        modelBuilder.HasSequence("save_text_seq", "sprut");
        modelBuilder.HasSequence("sourcedata_seq", "sprut");
        modelBuilder.HasSequence("speech_seq", "sprut");
        modelBuilder.HasSequence("srctype_seq", "sprut").HasMin(0L);
        modelBuilder.HasSequence("summary_seq", "sprut");
        modelBuilder.HasSequence("tactic_sit_seq", "sprut");
        modelBuilder.HasSequence("tmp_coords_seq", "sprut").HasMin(0L);

        OnModelCreatingPartial(modelBuilder);
    }

    partial void OnModelCreatingPartial(ModelBuilder modelBuilder);
}
