using System;
using System.Collections.Generic;
using System.Data;
using System.Text;
using System.Threading.Tasks;
using Oracle.ManagedDataAccess.Client;

public class DatabaseService
{
    //public readonly string _connectionString;
    public readonly OracleSettings _oracleSettings;

    // Конструктор, который принимает OracleSettings через Dependency Injection
    public DatabaseService(OracleSettings oracleSettings)
    {
        _oracleSettings = oracleSettings;
    }

    private string GetConnectionString()
    {
        return $"User Id={_oracleSettings.User};Password={_oracleSettings.Password};Data Source={_oracleSettings.DataSource};";
    }
    // Получение списка номеров ключей для дат между startDate и endDate из таблицы {schemeName}.SPR_SPEECH_TABLE
    public async Task<List<int>> GetInkKeysAsync(DateTime startDate, DateTime endDate, string schemeName)
    {
        using var connection = new OracleConnection(GetConnectionString());
        await connection.OpenAsync();

        using var command = connection.CreateCommand();
        command.CommandText =
            $"SELECT S_INCKEY FROM {schemeName}.SPR_SPEECH_TABLE " +
            $"WHERE S_TYPE=0 " +
            $"AND S_DURATION > INTERVAL '10' SECOND " +
            $"AND S_EVENTCODE NOT IN ('BINARY', 'FAXDATA_GSM', 'DATA_GSM', 'FAXDATA_CDMA') " +
            $"AND S_NOTICE IS NULL " + //Примечание IS NOT NULL
            //$"AND S_NOTICE <> ''" +
            $"AND S_DATETIME BETWEEN :start_date AND :end_date " +
            $"ORDER BY S_DATETIME DESC"; //в порядке уменьшения, //в порядке увеличения $"ORDER BY S_DATETIME ASC" 
        command.Parameters.Add(":start_date", OracleDbType.TimeStamp).Value = startDate;
        command.Parameters.Add(":end_date", OracleDbType.TimeStamp).Value = endDate;

        List<int> inkKeys = new List<int>();
        using var reader = await command.ExecuteReaderAsync();
        while (await reader.ReadAsync())
        {
            inkKeys.Add(reader.GetInt32(0));
            // the same as => inkKeys.Add(Convert.ToInt32(keyReader["S_INCKEY"]));
        }
        Console.WriteLine("inkKeys.Count: " + inkKeys.Count);

        return inkKeys;
    }

    public async Task<(byte[] audioDataLeft, byte[]? audioDataRight, string? recordType)> GetAudioDataAsync(int key, string schemeName)
    {
        using (var connection = new OracleConnection(GetConnectionString()))
        {
            await connection.OpenAsync();

            using var command = connection.CreateCommand();
            command.CommandText = $"SELECT S_RECORDTYPE, S_FSPEECH, S_RSPEECH FROM {schemeName}.SPR_SP_DATA_1_TABLE WHERE S_INCKEY = :key";
            command.Parameters.Add(":key", OracleDbType.Int32).Value = key;

            using var reader = await command.ExecuteReaderAsync();
            if (await reader.ReadAsync())
            {
                byte[] audioDataLeft = (byte[])reader["S_FSPEECH"];
                byte[]? audioDataRight = reader["S_RSPEECH"] as byte[];
                string? recordType = reader["S_RECORDTYPE"] as string;

                Console.WriteLine($"Audio data for key {key} loaded successfully. recordType = " + recordType);
                return (audioDataLeft, audioDataRight, recordType);
            }

            throw new InvalidOperationException("No data found for the given key.");
        }

    }

    public async Task InsertCommentAsync(int key, string text, string detectedLanguage, string responseOllamaText, OracleTransaction transaction, string schemeName, string modelName)
    {
        
        // Register the code pages encoding provider
        Encoding.RegisterProvider(CodePagesEncodingProvider.Instance);
        // Use the appropriate encoding for the comment
        text = responseOllamaText + "\n ############################## \n" + text;

        byte[] commentBytes = Encoding.GetEncoding(1251).GetBytes(text); // т.к. доступны только кирилица и латиница, текст будем переводить
        //byte[] commentBytes = Encoding.GetEncoding(1251).GetBytes(text); // китайский текст - знаками вопроса
        //byte[] commentBytes = Encoding.UTF8.GetBytes(text); // всё - закарючки
        //byte[] commentBytes = Encoding.GetEncoding("utf-8").GetBytes(text); // всё - закарючки
        //byte[] commentBytes = Encoding.ASCII.GetBytes(text); // всё - закарючки
        //byte[] commentBytes = Encoding.Unicode.GetBytes(text); // всё пусто кроме цифр
        //byte[] commentBytes = Encoding.GetEncoding("ISO-8859-1").GetBytes(text); // всё - вопросы


        //Console.WriteLine("text: " + text);
        //responseOllamaText = responseOllamaText.Length > 99 ? responseOllamaText.Substring(0, 99) : responseOllamaText;
        int dangerLevel;
        bool success = int.TryParse(responseOllamaText.Substring(0, 1), out dangerLevel);
        string dangerLevelText = success ? dangerLevel.ToString() : "unknown";

        using var connection = transaction.Connection;

        using (var insertCommentCommand = connection.CreateCommand())
        {
            insertCommentCommand.Transaction = transaction;
            insertCommentCommand.CommandText = @$"
            MERGE INTO {schemeName}.SPR_SP_COMMENT_TABLE T1
            USING (SELECT S_INCKEY FROM {schemeName}.SPR_SPEECH_TABLE WHERE S_INCKEY = :S_INCKEY) T2
            ON (T1.S_INCKEY = T2.S_INCKEY)
            WHEN MATCHED THEN
                UPDATE SET S_COMMENT = :S_COMMENT
            WHEN NOT MATCHED THEN
                INSERT (S_INCKEY, S_COMMENT) VALUES (:S_INCKEY, :S_COMMENT)";
            insertCommentCommand.Parameters.Add(":S_INCKEY", OracleDbType.Int32).Value = key;
            insertCommentCommand.Parameters.Add(":S_COMMENT", OracleDbType.Blob).Value = commentBytes;
            await insertCommentCommand.ExecuteNonQueryAsync();

            using var insertSpeechCommand = connection.CreateCommand();
            insertSpeechCommand.Transaction = transaction;
            insertSpeechCommand.CommandText = @$"
            MERGE INTO {schemeName}.SPR_SPEECH_TABLE T1
            USING (SELECT S_INCKEY FROM {schemeName}.SPR_SPEECH_TABLE WHERE S_INCKEY = :S_INCKEY) T2
            ON (T1.S_INCKEY = T2.S_INCKEY)
            WHEN MATCHED THEN
                UPDATE SET T1.S_BELONG = :S_BELONG, T1.S_NOTICE = :S_NOTICE, T1.S_POSTID = :S_POSTID
            WHEN NOT MATCHED THEN
                INSERT (S_INCKEY, S_BELONG, S_NOTICE, S_POSTID) VALUES (:S_INCKEY, :S_BELONG, :S_NOTICE, :S_POSTID)";
            insertSpeechCommand.Parameters.Add(":S_INCKEY", OracleDbType.Int32).Value = key;
            insertSpeechCommand.Parameters.Add(":S_BELONG", OracleDbType.Varchar2).Value = detectedLanguage;
            insertSpeechCommand.Parameters.Add(":S_NOTICE", OracleDbType.Varchar2).Value = dangerLevelText;
            insertSpeechCommand.Parameters.Add(":S_POSTID", OracleDbType.Varchar2).Value = modelName;
            //S_POSTKEY - ключ поста
            //S_POSTID	VARCHAR(20)			Имя поста регистрации
            //S_BELONG	VARCHAR(30)			сеть принадлежности (домашняя сеть)
            //S_NOTICE VARCHAR(100)			Примечание
            await insertSpeechCommand.ExecuteNonQueryAsync();
            //Console.WriteLine("InsertComment " + key + " is Ok \n");
            await transaction.CommitAsync();
        }
    }

    public async Task<OracleTransaction> BeginTransactionAsync()
    {
        var connection = new OracleConnection(GetConnectionString());
        await connection.OpenAsync();
        return (OracleTransaction)await connection.BeginTransactionAsync();
    }
}
