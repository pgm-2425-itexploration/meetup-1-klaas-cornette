using System.Text;
using System.Security.Cryptography;
using TaskManagerApp.Helper;

namespace TaskManagerApp.Services
{
    public class TaskManager
    {
        private readonly List<TodoTask> _tasks = new();

        public IEnumerable<TodoTask> GetTasks() => _tasks;

        public void AddTask(TodoTask task)
        {
            _tasks.Add(task);
        }

        public void RemoveTask(string taskName)
        {
            var task = _tasks.FirstOrDefault(t => t.Name == taskName);
            if (task != null)
            {
                _tasks.Remove(task);
            }
        }

        public string EnCodeTask()
        {
            // Zet taken om naar CSV-indeling
            StringBuilder sb = new StringBuilder();
            foreach (var task in _tasks)
            {
                sb.AppendLine($"{task.Name},{task.Priority}");
            }

            string encodedTasks = Base64UrlEncode(Encoding.UTF8.GetBytes(sb.ToString()));
            byte[] hash = ComputeHash(encodedTasks);

            return encodedTasks + "\n" + BitConverter.ToString(hash).Replace("-", "");
        }

        public void LoadTasks(string fileContent)
        {
            // Splits de content in taken en hash
            var lines = fileContent.Split('\n', StringSplitOptions.RemoveEmptyEntries);
            if (lines.Length < 2) throw new InvalidOperationException("Ongeldig bestand.");

            string encodedTasks = lines[0];
            string storedHash = lines[1];

            // Controleer de integriteit
            byte[] hash = ComputeHash(encodedTasks);
            string computedHash = BitConverter.ToString(hash).Replace("-", "");
            if (storedHash != computedHash)
            {
                throw new InvalidOperationException("Gegevens zijn gewijzigd of corrupt.");
            }

            // Decodeer de taken
            string decodedTasks = Encoding.UTF8.GetString(Base64UrlDecode(encodedTasks));
            _tasks.Clear();
            foreach (var line in decodedTasks.Split('\n', StringSplitOptions.RemoveEmptyEntries))
            {
                var parts = line.Split(',');
                if (parts.Length == 2)
                {
                    Console.WriteLine(parts[0]);
                    Console.WriteLine(parts[1]);
                    _tasks.Add(new TodoTask
                    {
                        Name = parts[0],
                        Priority = int.Parse(parts[1])
                    });
                }
            }

        }

        private static byte[] ComputeHash(string input)
        {
            return SHA256.HashData(Encoding.UTF8.GetBytes(input));
        }

        private static string Base64UrlEncode(byte[] input)
        {
            return Convert.ToBase64String(input).TrimEnd('=')
                .Replace('+', '-')
                .Replace('/', '_');
        }

        private static byte[] Base64UrlDecode(string input)
        {
            string base64 = input.Replace('-', '+').Replace('_', '/');
            switch (base64.Length % 4)
            {
                case 2: base64 += "=="; break;
                case 3: base64 += "="; break;
            }
            return Convert.FromBase64String(base64);
        }
    }
}
