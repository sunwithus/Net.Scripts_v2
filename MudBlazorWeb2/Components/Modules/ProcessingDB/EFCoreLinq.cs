//EFCoreLinq.cs

using MudBlazorWeb2.Components.EntityFrameworkCore;
using System.Text;

namespace MudBlazorWeb2.Components.Modules.ProcessingDB
{
    public class EFCoreLinq
    {
        public static async Task<(byte[]? audioDataLeft, byte[]? audioDataRight, string? recordType)> GetAudioDataAsync(long? key, string schemeName, OracleDbContext db)
        {
            await Task.Delay(1);
            var result = db.SprSpData1Table.Where(x => x.Id == key).Where(x => !(EncodingDecoding._ignoreRecordType.Contains(x.Recordtype))).Select(x => new
            {
                AudioDataLeft = x.Fspeech,
                AudioDataRight = x.Rspeech,
                RecordType = x.Recordtype
            }).ToList().FirstOrDefault();

            if (result == null)
                return (null, null, null);

            return (result.AudioDataLeft, result.AudioDataRight, result.RecordType);
        }

        public static async Task InsertCommentAsync(long? key, string text, string detectedLanguage, string responseOllamaText, OracleDbContext db, string schemeName, string modelName)
        {

            // Register the code pages encoding provider
            Encoding.RegisterProvider(CodePagesEncodingProvider.Instance);
            // Use the appropriate encoding for the comment
            text = responseOllamaText + "\n##############################\n" + text;

            byte[] commentBytes = Encoding.GetEncoding(1251).GetBytes(text); // т.к. в БД доступны только кирилица и латиница, поэтому обязательно текст будем переводить

            int dangerLevel;
            bool success = int.TryParse(responseOllamaText.Substring(0, 1), out dangerLevel);
            string dangerLevelText = success ? dangerLevel.ToString() : "unknown";

            try
            {
                // Проверка существования и обновление/добавление записи в SPR_SP_COMMENT_TABLE
                // Использование AsEnumerable() приводит к выполнению запроса и загрузке данных в память, а затем LastOrDefault() выполняется уже в памяти.
                // AsEnumerable() - обязательно, иначе ошибка Oracle 11.2 (EFCore использует новый синтаксис SQL)
                var comment = db.SprSpCommentTable.Where(c => c.Id == key).AsEnumerable().FirstOrDefault();
                if (comment != null)
                {
                    comment.Comment = commentBytes;
                    db.SprSpCommentTable.Update(comment);
                }
                else
                {
                    comment = new SPR_SP_COMMENT_TABLE
                    {
                        Id = key,
                        Comment = commentBytes
                    };
                    await db.SprSpCommentTable.AddAsync(comment);
                }

                // Проверка существования и обновление/добавление записи в SPR_SPEECH_TABLE
                var speech = db.SprSpeechTable.Where(c => c.Id == key).AsEnumerable().FirstOrDefault();
                if (speech != null)
                {
                    speech.Belong = detectedLanguage;
                    speech.Notice = dangerLevelText;
                    speech.Postid = modelName;
                    db.SprSpeechTable.Update(speech);
                }
                else
                {
                    speech = new SPR_SPEECH_TABLE
                    {
                        Id = key,
                        Belong = detectedLanguage,
                        Notice = dangerLevelText,
                        Postid = modelName
                    };
                    await db.SprSpeechTable.AddAsync(speech);
                }

                // Сохранение всех изменений
                await db.SaveChangesAsync();
            }
            catch
            {
                throw;
            }
        }

    }

}