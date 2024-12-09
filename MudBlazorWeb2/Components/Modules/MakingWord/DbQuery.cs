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

                var speechDataList = await context.SprSpeechTables.Where(x => x.SInckey == id).ToListAsync();

                if (speechDataList == null || !speechDataList.Any() || speechDataList?.FirstOrDefault()?.SInckey != id)
                {
                    await context.Database.CloseConnectionAsync();
                    return null;
                }

                var speechData = speechDataList?.Select(speech => new SpeechData
                {
                    
                    Id = speech.SInckey,
                    Deviceid = speech.SDeviceid,
                    //Duration = speech.SDuration.ToString(),
                    Datetime = speech.SDatetime,
                    Belong = speech.SBelong,
                    Sourcename = speech.SSourcename,
                    Talker = speech.STalker,
                    Usernumber = speech.SUsernumber,
                    Calltype = speech.SCalltype,
                    Cid = speech.SCid,
                    Lac = speech.SLac,
                    Basestation = speech.SBasestation,
                    Comment = context.SprSpCommentTables.Where(x => x.SInckey == id).ToListAsync().Result.FirstOrDefault(c => c.SInckey == speech.SInckey)?.SComment,
                    AudioF = context.SprSpData1Tables.Where(x => x.SInckey == id).ToListAsync().Result.FirstOrDefault(c => c.SInckey == speech.SInckey)?.SFspeech,
                    AudioR = context.SprSpData1Tables.Where(x => x.SInckey == id).ToListAsync().Result.FirstOrDefault(c => c.SInckey == speech.SInckey)?.SRspeech
                }).ToList();

                await context.Database.CloseConnectionAsync();

                return speechData;
            }
        }
    }
}