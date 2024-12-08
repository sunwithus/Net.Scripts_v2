// SettingsDb.cs

namespace MudBlazorWeb2.Components.Modules._Shared
{
    public class SettingsDb
    {
        public string AimType { get; set; } //Replicator //MakingWord
        public string DbType { get; set; }
        public string User { get; set; } = "sysdba"; //postgres
        public string Password { get; set; } = "masterkey"; //postgres
        public string ServerAddress { get; set; } = "localhost"; //DataSource = "localhost / sprutora"; //Host = "localhost";
        public string Scheme { get; set; } = ""; //Database
    }
}

