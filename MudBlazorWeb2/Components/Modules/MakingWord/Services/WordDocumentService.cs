//WordDocumentService.cs
using Spire.Doc;
using Spire.Doc.Documents;
using System.Text;

using MudBlazorWeb2.Components.Modules.MakingWord.Modules;

namespace MudBlazorWeb2.Components.Modules.MakingWord.Services
{
    public class WordDocumentService
    {
        public MemoryStream GenerateWord(SpeechData data, string templatePath)
        {
            var document = new Document();
            document.LoadFromFile(templatePath);

            var replacements = new Dictionary<string, string>
            {
                { "#JournalNumber#", data.Id.ToString() ?? "" },
                { "#Complex#", data.Deviceid ?? "" },
                { "#OperatorName#", data.Sourcename ?? "" },
                { "#DateTime#", data.Datetime?.ToString("dd-MM-yyyy HH:mm:ss") ?? "" },
                { "#Duration#", data.Duration?.Trim('+').Trim('0').Trim('.') ?? "" },
                { "#RegistrationTime#", DateTime.Now.ToString("dd.MM HH:mm") },
                { "#Direction#", GetCallDirection(data.Calltype) },
                { "#Direction2#", GetCallDirectionReversed(data.Calltype) },
                { "#Caller#", data.Usernumber ?? "" },
                { "#Talker#", data.Talker ?? "" },
                { "#BaseStation#", FormatBaseStation(data.Basestation) },
                { "#LacCid#", FormatLacCid(data.Lac, data.Cid) },
                { "#DialogFromComment#", GetDecodedComment(data.Comment) }
            };

            foreach (var replacement in replacements)
            {
                document.Replace(replacement.Key, replacement.Value, true, true);
            }

            var stream = new MemoryStream();
            document.SaveToFile(stream, FileFormat.Docx);
            stream.Position = 0;
            return stream;
        }

        private string GetCallDirection(int? callType) => callType switch
        {
            1 => "исходящий звонок с",
            0 => "входящий звонок на",
            _ => "неизвестно направление вызова"
        };

        private string GetCallDirectionReversed(int? callType) => callType switch
        {
            1 => "на",
            0 => "с",
            _ => ""
        };

        private string FormatBaseStation(string? baseStation) => string.IsNullOrEmpty(baseStation) ? "" : $"({baseStation})";

        private string FormatLacCid(string? lac, string? cid) =>
            string.IsNullOrEmpty(lac) && string.IsNullOrEmpty(cid)
            ? ""
            : $"({(lac != null ? $"LAC: {lac}; " : "")}{(cid != null ? $"CID: {cid}" : "")})";

        private string GetDecodedComment(byte[]? comment)
        {
            if (comment == null) return string.Empty;
            return Encoding.GetEncoding("windows-1251").GetString(comment)
                           .TrimStart('0', '1', '2', '3', '4', '5', '.')
                           .Trim()
                           .Replace("\n\n", "\n");
        }
    }
}