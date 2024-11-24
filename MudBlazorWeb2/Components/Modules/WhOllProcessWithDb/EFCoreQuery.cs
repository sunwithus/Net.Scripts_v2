//EFCoreQuery.cs

using Microsoft.EntityFrameworkCore;
using MudBlazorWeb2.Components.EntityFrameworkCore;
using MudBlazorWeb2.Components.Modules.WhOllProcessWithDb.Services;
using Oracle.ManagedDataAccess.Client;
using Spire.Doc;
using System.Configuration;
using System.Text;


namespace MudBlazorWeb2.Components.Modules.WhOllProcessWithDb
{
    public class EFCoreQuery
    {
        public static int ConvertDurationStringToSeconds(string durationString)
        {
            durationString = durationString.Replace("+00 ", "");
            durationString = durationString.Replace(".000000", "");

            string[] parts = durationString.Split(':');

            int hours = int.Parse(parts[0]);
            int minutes = int.Parse(parts[1]);
            int seconds = int.Parse(parts[2]);
            return (hours * 3600 + minutes * 60 + seconds);
        }

        public static async Task<List<SPR_SPEECH_TABLE>> GetSpeechRecords(DateTime StartDate, DateTime EndDate, string TimeInterval, OracleDbContext db, List<string> _ignoreRecordType)
        {
            var parameters = new OracleParameter[]
            {
                new OracleParameter("startDate", StartDate),
                new OracleParameter("endDate", EndDate)
            };

            var sqlQuery = $@"
                SELECT * FROM SPR_SPEECH_TABLE
                WHERE S_DATETIME BETWEEN :startDate AND :endDate
                AND S_TYPE = 0
                AND (S_NOTICE IS NULL OR S_NOTICE = '')
                AND S_DURATION > INTERVAL '{EFCoreQuery.ConvertDurationStringToSeconds(TimeInterval)}' SECOND
                AND S_EVENTCODE NOT IN ({string.Join(",", _ignoreRecordType.Select(e => $"'{e}'"))})
                ORDER BY S_DATETIME DESC";

            return await db.SprSpeechTable
                .FromSqlRaw(sqlQuery, parameters)
                .ToListAsync();

            /*
            return db.SprSpeechTable
               //.Where (x => x.Duration > 10) // так не работает выборка по длительности // Duration is string // c Oracle.ManagedDataAccess.Core работало так $"AND S_DURATION > INTERVAL '10' SECOND " 
               .Where(x => x.Datetime >= startDate && x.Datetime <= endDate
               && x.Type == 0 // Тип записи (-1 – неизвестно,0 – сеанс связи, 1 – сообщение, 2 – биллинг,3 – служебное сообщение, 4 – регистрация автотранспорта
               && (x.Notice == null || x.Notice == "")
               && !_ignoreRecordType.Contains(x.Eventcode))
               .OrderByDescending(x => x.Datetime)
               .AsEnumerable() // Evaluate the query so far on the database
               .Where(x => EFCoreQuery.ConvertDurationStringToSeconds(x.Duration) > EFCoreQuery.ConvertDurationStringToSeconds(TimeInterval))
               .ToList();
            */
        }

        public static async Task<(byte[]? audioDataLeft, byte[]? audioDataRight, string? recordType)> GetAudioDataAsync(long? key, OracleDbContext db)
        {
            await Task.Delay(1);
            var result = db.SprSpData1Table
                .Where(x => x.Id == key)
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

        public static async Task InsertCommentAsync(long? key, string text, string detectedLanguage, string responseOllamaText, string modelName, string schemeName, string conStringDBA)
        {
            using (var db = new OracleDbContext(new DbContextOptionsBuilder<OracleDbContext>().UseOracle(conStringDBA).Options))
            {
                await db.Database.OpenConnectionAsync();
                await db.Database.ExecuteSqlRawAsync($"ALTER SESSION SET CURRENT_SCHEMA = {schemeName}");
                // ORACLE => update entity
                // Register the encoding provider
                Encoding.RegisterProvider(CodePagesEncodingProvider.Instance);
                var sb = new StringBuilder();
                sb.Append(responseOllamaText);
                sb.Append("\n##############################\n");
                sb.Append(text);

                // т.к. в БД доступны только кирилица и латиница, поэтому обязательно текст будем переводить
                byte[] commentBytes = Encoding.GetEncoding(1251).GetBytes(sb.ToString());

                string dangerLevelText = int.TryParse(responseOllamaText.Substring(0, 1), out int dangerLevel) ? dangerLevel.ToString() : "unknown";

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

                await db.Database.CloseConnectionAsync();
            }
            
        }

        public static async Task UpdateNoticeValueAsync(long? key, string schemeName, string conStringDBA, string? value = null)
        {
            using (var db = new OracleDbContext(new DbContextOptionsBuilder<OracleDbContext>().UseOracle(conStringDBA).Options))
            {
                await db.Database.OpenConnectionAsync();
                await db.Database.ExecuteSqlRawAsync($"ALTER SESSION SET CURRENT_SCHEMA = {schemeName}");
                try
                {
                    var speech = db.SprSpeechTable.Where(c => c.Id == key).AsEnumerable().FirstOrDefault();
                    if (speech != null)
                    {
                        speech.Notice = value;
                    }
                    await db.SaveChangesAsync();
                }
                catch (Exception ex)
                {
                    ConsoleCol.WriteLine("Ошибка в InsertNullToNoticeAsync => " + ex.Message, ConsoleColor.Red);
                }
                await db.Database.CloseConnectionAsync();
            }
        }

    }
}

