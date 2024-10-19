//OraSettings.cs
namespace MudBlazorWeb2.Components.Modules.Replicator
{
    public class ReplOraItems
    {
        public string User { get; set; }
        public string Password { get; set; }
        public string DataSource { get; set; }
        public string Scheme { get; set; }
        public string TryToParse { get; set; }
    }

    public class ReplOraString
    {
        public string OracleDbConnectionString { get; set; }
        public string Scheme { get; set; }
    }

    public class OraSettings
    {
        public ReplOraItems ReplOraItems { get; set; }
        public ReplOraString ReplOraString { get; set; }
    }

}