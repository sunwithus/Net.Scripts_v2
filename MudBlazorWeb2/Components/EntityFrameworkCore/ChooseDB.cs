namespace MudBlazorWeb2.Components.EntityFrameworkCore
{
    public class ChooseDB
    {
        public enum Database
        {
            Oracle,
            Interbase,
            Postgres
        }

        public string ChooseDatabase (Database db)
        {
            string result = db switch
            {
                Database.Oracle => "OracleDbContext",
                Database.Interbase => "InterbaseDbContext",
                Database.Postgres => "PostgresDbContext",
                _ => "OracleDbContext"
            };
            return result;
        }
    }


}
