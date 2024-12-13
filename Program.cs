using System.Security.Cryptography;
using System.Text;

class Program
{
    static void Main(string[] args)
    {
        Console.WriteLine("Taakbeheer App - Gebruikmakend van .NET 9 Functionaliteiten\n");

        var taskManager = new TaskManager(); // Maak een nieuwe instantie van TaskManager.

        while (true)
        {
            // Toon het menu met opties voor de gebruiker.
            Console.WriteLine("Menu:");
            Console.WriteLine("1. Voeg een taak toe");
            Console.WriteLine("2. Bekijk taken");
            Console.WriteLine("3. Verwijder een taak");
            Console.WriteLine("4. Sla taken op");
            Console.WriteLine("5. Laad taken");
            Console.WriteLine("6. Afsluiten");

            Console.Write("Selecteer een optie: ");
            string? choice = Console.ReadLine(); // Lees de keuze van de gebruiker.

            switch (choice)
            {
                case "1":
                    // Voeg een taak toe.
                    Console.Write("Voer de taaknaam in: ");
                    string? taskName = Console.ReadLine();
                    if (string.IsNullOrWhiteSpace(taskName))
                    {
                        Console.WriteLine("Taaknaam mag niet leeg zijn.\n");
                        continue;
                    }

                    Console.Write("Voer de prioriteit in (lager is belangrijker): ");
                    string? priorityInput = Console.ReadLine();
                    if (!int.TryParse(priorityInput, out int priority))
                    {
                        Console.WriteLine("Ongeldige prioriteit. Voer een geldig getal in.\n");
                        continue;
                    }
                    taskManager.AddTask(taskName, priority); // Voeg de taak toe aan TaskManager.
                    break;

                case "2":
                    // Bekijk de taken.
                    taskManager.ViewTasks();
                    break;

                case "3":
                    // Verwijder een taak.
                    Console.Write("Voer de naam van de te verwijderen taak in: ");
                    string? taskToRemove = Console.ReadLine();
                    if (string.IsNullOrWhiteSpace(taskToRemove))
                    {
                        Console.WriteLine("Taaknaam mag niet leeg zijn.\n");
                        continue;
                    }
                    taskManager.RemoveTask(taskToRemove);
                    break;

                case "4":
                    // Sla de taken op naar een bestand.
                    taskManager.SaveTasks("tasks.txt");
                    break;

                case "5":
                    // Laad de taken uit een bestand.
                    taskManager.LoadTasks("tasks.txt");
                    break;

                case "6":
                    // Stop het programma.
                    return;

                default:
                    Console.WriteLine("Ongeldige keuze, probeer opnieuw.\n");
                    break;
            }
        }
    }
}

// Beheer van taken met prioriteiten en opslag.
class TaskManager
{
    private PriorityQueue<string, int> taskQueue = new PriorityQueue<string, int>(); // Een priority queue om taken te beheren op basis van prioriteit.
    private OrderedDictionary<string, int> taskDictionary = new OrderedDictionary<string, int>(); // Een gesorteerde dictionary voor consistente volgorde.

    public void AddTask(string task, int priority)
    {
        // Voeg een taak toe aan de queue en dictionary.
        taskQueue.Enqueue(task, priority);
        taskDictionary.Add(task, priority);
        Console.WriteLine("Taak toegevoegd.\n");
    }

    public void ViewTasks()
    {
        // Toon alle taken in de dictionary met hun prioriteiten.
        Console.WriteLine("Takenlijst:");
        foreach (var task in taskDictionary)
        {
            Console.WriteLine($"Taak: {task.Key}, Prioriteit: {task.Value}");
        }
        Console.WriteLine();
    }

    public void RemoveTask(string task)
    {
        if (!taskDictionary.ContainsKey(task))
        {
            Console.WriteLine("Taak niet gevonden in de lijst.\n");
            return;
        }

        // Verwijder de taak uit de dictionary en de priority queue.
        taskDictionary.Remove(task);

        var tempQueue = new PriorityQueue<string, int>(); // Maak een tijdelijke queue aan.
        while (taskQueue.TryDequeue(out string? removedTask, out int priority))
        {
            if (removedTask != task)
            {
                tempQueue.Enqueue(removedTask, priority);
            }
        }
        taskQueue = tempQueue; // Vervang de originele queue door de tijdelijke queue.
        Console.WriteLine($"Taak '{task}' verwijderd uit de queue.\n");
    }

    public void SaveTasks(string filePath)
    {
        // Sla de taken op in een bestand, geëncodeerd en gehasht voor integriteit.
        StringBuilder sb = new StringBuilder();
        foreach (var task in taskDictionary)
        {
            sb.AppendLine($"{task.Key},{task.Value}");
        }

        string encodedTasks = Base64UrlEncode(Encoding.UTF8.GetBytes(sb.ToString()));
        byte[] hash = CryptographicOperations.HashData(HashAlgorithmName.SHA256, Encoding.UTF8.GetBytes(encodedTasks));

        File.WriteAllText(filePath, encodedTasks + "\n" + BitConverter.ToString(hash).Replace("-", ""));
        Console.WriteLine("Taken opgeslagen.\n");
    }

    public void LoadTasks(string filePath)
    {
        // Laad taken vanuit een bestand en controleer integriteit met hash.
        if (!File.Exists(filePath))
        {
            Console.WriteLine("Bestand niet gevonden.\n");
            return;
        }

        string[] fileContents = File.ReadAllLines(filePath);
        if (fileContents.Length < 2)
        {
            Console.WriteLine("Ongeldig bestand.\n");
            return;
        }

        string encodedTasks = fileContents[0];
        string storedHash = fileContents[1];

        byte[] hash = CryptographicOperations.HashData(HashAlgorithmName.SHA256, Encoding.UTF8.GetBytes(encodedTasks));
        string computedHash = BitConverter.ToString(hash).Replace("-", "");

        if (storedHash != computedHash)
        {
            Console.WriteLine("Gegevens zijn gewijzigd of corrupt.\n");
            return;
        }

        string decodedTasks = Encoding.UTF8.GetString(Base64UrlDecode(encodedTasks));
        string[] tasks = decodedTasks.Split(new[] { '\n' }, StringSplitOptions.RemoveEmptyEntries);

        taskQueue.Clear();
        taskDictionary.Clear();

        foreach (string taskLine in tasks)
        {
            string[] parts = taskLine.Split(',');
            string task = parts[0];
            int priority = int.Parse(parts[1]);
            taskQueue.Enqueue(task, priority);
            taskDictionary.Add(task, priority);
        }

        Console.WriteLine("Taken geladen.\n");
    }

    private string Base64UrlEncode(byte[] input)
    {
        // Converteer bytes naar Base64 URL-veilige string.
        return Convert.ToBase64String(input).TrimEnd('=')
            .Replace('+', '-')
            .Replace('/', '_');
    }

    private byte[] Base64UrlDecode(string input)
    {
        // Decodeer een Base64 URL-veilige string naar bytes.
        string base64 = input.Replace('-', '+').Replace('_', '/');
        switch (base64.Length % 4)
        {
            case 2: base64 += "=="; break;
            case 3: base64 += "="; break;
        }
        return Convert.FromBase64String(base64);
    }

    // Custom OrderedDictionary implementatie om items in volgorde van toevoeging te bewaren.
    class OrderedDictionary<TKey, TValue> : Dictionary<TKey, TValue>
    {
        private readonly List<TKey> keys = new List<TKey>();

        public new void Add(TKey key, TValue value)
        {
            base.Add(key, value);
            keys.Add(key);
        }

        public new IEnumerator<KeyValuePair<TKey, TValue>> GetEnumerator()
        {
            foreach (var key in keys)
            {
                yield return new KeyValuePair<TKey, TValue>(key, this[key]);
            }
        }
    }
}
