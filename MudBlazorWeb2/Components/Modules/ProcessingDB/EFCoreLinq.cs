//EFCoreLinq.cs

using Microsoft.EntityFrameworkCore;
using MudBlazorWeb2.Components.EntityFrameworkCore;
using Oracle.ManagedDataAccess.Client;
using Spire.Doc;
using System.Text;

namespace MudBlazorWeb2.Components.Modules.ProcessingDB
{
    public class EFCoreLinq
    {
        public static bool ConvertDurationStringToSeconds(string durationString)
        {
            durationString = durationString.Replace("+00 ", "");
            durationString = durationString.Replace(".000000", "");

            string[] parts = durationString.Split(':');

            int hours = int.Parse(parts[0]);
            int minutes = int.Parse(parts[1]);
            int seconds = int.Parse(parts[2]);
            return (hours * 3600 + minutes * 60 + seconds) > 10;
        }

        public static List<SPR_SPEECH_TABLE> GetSpeechRecords(DateTime startDate, DateTime endDate, OracleDbContext db)
        {
            //Console.WriteLine($"GetSpeechRecords => startDate: {startDate}, endDate: {endDate}");
            return db.SprSpeechTable.Where(x => x.Datetime >= startDate && x.Datetime <= endDate)
                .Where(x => x.Type == 0) // Тип записи (-1 – неизвестно,0 – сеанс связи, 1 – сообщение, 2 – биллинг,3 – служебное сообщение, 4 – регистрация автотранспорта
                .Where(x => x.Notice == null || x.Notice == "")
                //так не работает выборка по длительности
                //.Where (x => x.Duration > 10) // Duration is string // c Oracle.ManagedDataAccess.Core работало так $"AND S_DURATION > INTERVAL '10' SECOND " 
                .Where (x => !EncodingDecoding._ignoreRecordType.Contains(x.Eventcode))
                .Where (x => !EncodingDecoding._tempIgnoreRecordType.Contains(x.Eventcode))
                .OrderByDescending(x => x.Datetime)
                .AsEnumerable() // Evaluate the query so far on the database
                .Where(x => EFCoreLinq.ConvertDurationStringToSeconds(x.Duration))
                .ToList();
            
            /*var parameters = new OracleParameter[]
            {
                new OracleParameter("startDate", startDate),
                new OracleParameter("endDate", endDate)
            };

            var sqlQuery = $@"
                SELECT * FROM SPR_SPEECH_TABLE
                WHERE S_DATETIME BETWEEN :startDate AND :endDate
                AND S_TYPE = 0
                AND (S_NOTICE IS NULL OR S_NOTICE = '')
                AND S_DURATION > INTERVAL '10' SECOND
                AND S_EVENTCODE NOT IN ({string.Join(",", EncodingDecoding._ignoreRecordType.Select(e => $"'{e}'"))})
                ORDER BY S_DATETIME DESC";

            return db.SprSpeechTable
                .FromSqlRaw(sqlQuery, parameters)
                .ToList();
            */
        }

        public static async Task<(byte[]? audioDataLeft, byte[]? audioDataRight, string? recordType)> GetAudioDataAsync(long? key, string schemeName, OracleDbContext db)
        {
            await Task.Delay(1);
            var result = db.SprSpData1Table
                .Where(x => x.Id == key)
                //.Where(x => !EncodingDecoding._tempIgnoreRecordType.Contains(x.Recordtype))
                .Select(x => new
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
                // AsEnumerable() - обязательно, иначе ошибка Oracle 11.2 (т.к. EFCore использует новый синтаксис SQL)
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
                    speech.Deviceid = "MEDIUM_R";
                    db.SprSpeechTable.Update(speech);
                }
                else
                {
                    speech = new SPR_SPEECH_TABLE
                    {
                        Id = key,
                        Belong = detectedLanguage,
                        Notice = dangerLevelText,
                        Postid = modelName,
                        Deviceid = "MEDIUM_R"
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