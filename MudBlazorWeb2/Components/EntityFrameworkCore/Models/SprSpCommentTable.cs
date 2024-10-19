using System.ComponentModel.DataAnnotations.Schema;
using System.ComponentModel.DataAnnotations;

namespace MudBlazorWeb2.Components.EntityFrameworkCore.Models
{
    public class SprSpCommentTable
    {
        [Key]
        [Column("S_INCKEY")]
        public long Id { get; set; }

        [Column("S_COMMENT")]
        public byte[]? Comment { get; set; }

        public ICollection<SprSpeechTable> SprSpeechTables { get; set; }
    }
}
