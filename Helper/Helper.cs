using System;
using System.IO;

namespace TaskManagerApp.Helper
{
    public class Downloader
    {
        public void DownloadFiles(string content)
        {
        try
        {
            // Bepaal het bestandspad
            string filePath = "tasks.txt";
            // Schrijf de inhoud naar het bestand
            File.WriteAllText(filePath, content);
        }
        catch (UnauthorizedAccessException ex)
        {
            Console.WriteLine($"Toegang geweigerd: {ex.Message}");
        }
        catch (Exception ex)
        {
            Console.WriteLine($"Er is een fout opgetreden: {ex.Message}");
        }
    }
}

        
    
    public class TodoTask
    {
        public string Name { get; set; } = string.Empty;
        public int Priority { get; set; }
    }
}