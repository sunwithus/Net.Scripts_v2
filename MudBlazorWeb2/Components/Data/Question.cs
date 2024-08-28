namespace MudBlazorWeb2.Components.Data
{
    public class Question
    {
        public string model { get; set; } = "gemma2";
        public string prompt { get; set; } = string.Empty;
        public bool stream { get; set; } = false; //stream не работает пока
    }
}
