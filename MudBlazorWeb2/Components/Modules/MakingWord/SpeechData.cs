//SpeechData.cs
namespace MudBlazorWeb2.Components.Modules.MakingWord
{
    public class SpeechData
    {
        public long? Id { get; set; }
        public string? Deviceid { get; set; }
        public string? Duration { get; set; }
        public DateTime? Datetime { get; set; }
        public string? Belong { get; set; }
        public string? Sourcename { get; set; }
        public string? Talker { get; set; }
        public string? Usernumber { get; set; }
        public string? Cid { get; set; } = "";
        public string? Lac { get; set; } = "";
        public string? Basestation { get; set; } = "";
        public int? Calltype { get; set; } = 2;
        public byte[]? Comment { get; set; }
        public byte[]? AudioF { get; set; }
        public byte[]? AudioR { get; set; }
    }
}