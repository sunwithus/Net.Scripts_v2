//SpeechDataService.cs
using Microsoft.EntityFrameworkCore;
using MudBlazorWeb2.Components.EntityFrameworkCore;

namespace MudBlazorWeb2.Components.Modules.MakingWord
{
    public class DbQuery
    {
        public static async Task<List<SpeechData>> GetSpeechDataByIdAsync(long? id, string schema, string conStringDBA)
        {
            using (var context = new OracleDbContext(new DbContextOptionsBuilder<OracleDbContext>().UseOracle(conStringDBA).Options))
            {
                await context.Database.OpenConnectionAsync();
                await context.Database.ExecuteSqlRawAsync($"ALTER SESSION SET CURRENT_SCHEMA = {schema}");

                var speechDataList = await context.SprSpeechTable.Where(x => x.Id == id).ToListAsync();

                if (speechDataList == null || !speechDataList.Any() || speechDataList?.FirstOrDefault()?.Id != id)
                {
                    await context.Database.CloseConnectionAsync();
                    return null;
                }

                var speechData = speechDataList?.Select(speech => new SpeechData
                {
                    Id = speech.Id,
                    Deviceid = speech.Deviceid,
                    Duration = speech.Duration,
                    Datetime = speech.Datetime,
                    Belong = speech.Belong,
                    Sourcename = speech.Sourcename,
                    Talker = speech.Talker,
                    Usernumber = speech.Usernumber,
                    Calltype = speech.Calltype,
                    Cid = speech.Cid,
                    Lac = speech.Lac,
                    Basestation = speech.Basestation,
                    Comment = context.SprSpCommentTable.Where(x => x.Id == id).ToListAsync().Result.FirstOrDefault(c => c.Id == speech.Id)?.Comment,
                    AudioF = context.SprSpData1Table.Where(x => x.Id == id).ToListAsync().Result.FirstOrDefault(c => c.Id == speech.Id)?.Fspeech,
                    AudioR = context.SprSpData1Table.Where(x => x.Id == id).ToListAsync().Result.FirstOrDefault(c => c.Id == speech.Id)?.Rspeech
                }).ToList();

                await context.Database.CloseConnectionAsync();

                return speechData;
            }
        }
    }
}