// SettingsDb.cs

namespace MudBlazorWeb2.Components.Modules._Shared
{
    public class SettingsDb
    {
        public bool Selected { get; set; } //Replicator //MakingWord
        public string DbType { get; set; }
        public string User { get; set; }
        public string Password { get; set; }
        public string ServerAddress { get; set; }
        public string Scheme { get; set; } = ""; //Database
    }
}

