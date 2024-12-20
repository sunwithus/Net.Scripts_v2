//EFCoreQuery.cs

using Microsoft.EntityFrameworkCore;
using MudBlazorWeb2.Components.EntityFrameworkCore.Sprutora;
using MudBlazorWeb2.Components.EntityFrameworkCore;
using System.Text;
using System.Data;


namespace MudBlazorWeb2.Components.Modules.AiEstimateDb
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
        //Todo if needed
        public static int ConvertTimeIntervalStringToSeconds(string TimeInterval)
        {
            return 10;
        }

        public static async Task<List<SprSpeechTable>> GetSpeechRecords(DateTime StartDateTime, DateTime EndDateTime, int TimeInterval, BaseDbContext context, List<string> _ignoreRecordType)
        {
            //Todo TimeInterval TimeSpan.FromSeconds(TimeInterval) //string, int ???
            return await context.SprSpeechTables
               .Where (x => x.SDuration > TimeSpan.FromSeconds(TimeInterval)) 
               .Where(x => x.SDatetime >= StartDateTime && x.SDatetime <= EndDateTime
               && x.SType == 0 // Тип записи (-1 – неизвестно,0 – сеанс связи, 1 – сообщение, 2 – биллинг,3 – служебное сообщение, 4 – регистрация автотранспорта
               && (x.SNotice == null || x.SNotice == "")
               && !_ignoreRecordType.Contains(x.SEventcode))
               .OrderByDescending(x => x.SDatetime)
               //.AsEnumerable() // Evaluate the query so far on the database
               //.Where(x => EFCoreQuery.ConvertDurationStringToSeconds(x.SDuration) > EFCoreQuery.ConvertDurationStringToSeconds(TimeInterval))
               .ToListAsync();
        }

        public static async Task<(byte[]? audioDataLeft, byte[]? audioDataRight, string? recordType)> GetAudioDataAsync(long? key, BaseDbContext db)
        {
            var result = db.SprSpData1Tables
                .Where(x => x.SInckey == key)
                .Select(x => new
            {
                AudioDataLeft = x.SFspeech,
                AudioDataRight = x.SRspeech,
                RecordType = x.SRecordtype
            }).ToList().FirstOrDefault();

            if (result == null)
                return (null, null, null);

            return (result.AudioDataLeft, result.AudioDataRight, result.RecordType);
        }

        public static async Task InsertCommentAsync(long? key, string text, string detectedLanguage, string responseOllamaText, string modelName, BaseDbContext db, int backLight)
        {
                // Register the encoding provider
                Encoding.RegisterProvider(CodePagesEncodingProvider.Instance);
                var sb = new StringBuilder();
                sb.Append(responseOllamaText);
                sb.Append("\n##############################\n");
                sb.Append(text);

                // т.к. в БД доступны только кирилица и латиница, поэтому обязательно текст будем переводить
                byte[] commentBytes = Encoding.GetEncoding(1251).GetBytes(sb.ToString());

                string dangerLevelString = int.TryParse(responseOllamaText.Substring(0, 1), out int dangerLevel) ? dangerLevel.ToString() : "unknown";
                //Selstatus //1 - собеседник, 2 - слово в тексте, 3 - геофильтр, 4 - номер в тексте
                short selStatus = -1; //без признака
                if (dangerLevel > 0 && dangerLevel - backLight >= 0)
                {
                    selStatus = 4; // 4 - номер в тексте
                }

                try
                {
                    // Проверка существования и обновление/добавление записи в SPR_SP_COMMENT_TABLE
                    // Использование AsEnumerable() приводит к выполнению запроса и загрузке данных в память, а затем LastOrDefault() выполняется уже в памяти.
                    // AsEnumerable() - обязательно, иначе ошибка Oracle 11.2 (т.к. EFCore использует новый синтаксис SQL)
                    var comment = db.SprSpCommentTables.Where(c => c.SInckey == key).AsEnumerable().FirstOrDefault();
                    if (comment != null)
                    {
                        comment.SComment = commentBytes;
                        db.SprSpCommentTables.Update(comment);
                    }
                    else
                    {
                        comment = new SprSpCommentTable
                        {
                            SInckey = key,
                            SComment = commentBytes
                        };
                        await db.SprSpCommentTables.AddAsync(comment);
                    }

                    // Проверка существования и обновление/добавление записи в SPR_SPEECH_TABLE
                    var speech = db.SprSpeechTables.Where(c => c.SInckey == key).AsEnumerable().FirstOrDefault();
                    if (speech != null)
                    {
                        speech.SBelong = detectedLanguage;
                        speech.SNotice = dangerLevelString;
                        speech.SPostid = modelName;
                        speech.SDeviceid = "MEDIUM_R";
                        speech.SSelstatus = selStatus;
                        db.SprSpeechTables.Update(speech);
                    }
                    else
                    {
                        speech = new SprSpeechTable
                        {
                            SInckey = key,
                            SBelong = detectedLanguage,
                            SNotice = dangerLevelString,
                            SPostid = modelName,
                            SDeviceid = "MEDIUM_R",
                            SSelstatus = selStatus
                        };
                        await db.SprSpeechTables.AddAsync(speech);
                    }

                    // Сохранение всех изменений
                    await db.SaveChangesAsync();
                }
                catch(Exception ex)
                {
                ConsoleCol.WriteLine($"InsertCommentAsync => {ex.Message}", ConsoleColor.Red);
                    throw;
                }            
        }

        public static async Task UpdateManyNoticeValuesAsync(List<long?> keys, BaseDbContext db, string? value = null)
        {
            await db.SprSpeechTables
                .Where(s => keys.Contains(s.SInckey))
                .ExecuteUpdateAsync(s => s.SetProperty(x => x.SNotice, value));
        }
        public static async Task UpdateNoticeValueAsync(long? key, BaseDbContext db, string? value = null)
        {
            try
            {
                SprSpeechTable speech = db.SprSpeechTables.Where(c => c.SInckey == key).ToList().FirstOrDefault();
                if (speech != null)
                {
                    speech.SNotice = value;
                    db.Entry(speech).State = EntityState.Modified; // Use Entry to set the state explicitly
                    await db.SaveChangesAsync().ConfigureAwait(false); // Use ConfigureAwait(false) to avoid deadlocks
                }
                
            }
            catch (Exception ex)
            {
                ConsoleCol.WriteLine("Ошибка в InsertNullToNoticeAsync => " + ex.Message, ConsoleColor.Red);
            }
        }
        public static async Task<List<long?>> GetSInckeyRecordsForNoticeNull(DateTime StartDateTime, DateTime EndDateTime, BaseDbContext db)
        {
            return await db.SprSpeechTables
                .Where(x => x.SDatetime >= StartDateTime && x.SDatetime <= EndDateTime
                && (x.SNotice != null || x.SNotice != ""))
                .Select(x => x.SInckey)
                .ToListAsync();
        }

    }
}

