//InterbaseDbContext.cs

using Microsoft.EntityFrameworkCore;
using MudBlazorWeb2.Components.EntityFrameworkCore;
using MudBlazorWeb2.Components.EntityFrameworkCore.Sprutora;

namespace MudBlazorWeb2.Components.EntityFrameworkCore
{
    // Определение контекста базы данных для работы с Entity Framework Core
    public abstract class BaseDbContext : DbContext
    {
        protected BaseDbContext(DbContextOptions options) : base(options)
        {
        }
        
        public abstract DbSet<SprSpeechTable> SprSpeechTables { get; set; }
        public abstract DbSet<SprSpData1Table> SprSpData1Tables { get; set; }
        public abstract DbSet<SprSpCommentTable> SprSpCommentTables { get; set; }

        /*
        public abstract DbSet<SPR_SPEECH_TABLE> SprSpeechTables { get; set; }
        public abstract DbSet<SPR_SP_DATA_1_TABLE> SprSpData1Tables { get; set; }
        public abstract DbSet<SPR_SP_COMMENT_TABLE> SprSpCommentTables { get; set; }
        */

    }
}

