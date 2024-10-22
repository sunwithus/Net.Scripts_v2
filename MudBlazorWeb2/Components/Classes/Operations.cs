using Microsoft.AspNetCore.DataProtection.KeyManagement;

namespace MudBlazorWeb2.Components.Classes
{
    public class Operations
    {
        public void ClearFolder(string FolderName)
        {
            DirectoryInfo dir = new DirectoryInfo(FolderName);

            foreach (FileInfo file in dir.GetFiles())
            {
                file.Delete();
            }

            foreach (DirectoryInfo di in dir.GetDirectories())
            {
                ClearFolder(di.FullName);
                di.Delete();
            }
        }

        public void DeleteFolder(string FolderName)
        {
            DirectoryInfo dir = new DirectoryInfo(FolderName);
            if (dir.Exists)
            {
                dir.Delete(true);
            }
        }

        public int GetLineIndex (string[] lines, string findLineIndex)
        {
            for (int line=0; line<lines.Length; line++)
            {
                if (lines[line].StartsWith($"{findLineIndex}="))
                {
                    return line;
                }
            }
            return lines.Length - 1;
        }
    }



}
