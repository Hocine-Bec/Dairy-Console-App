using System;
using System.Collections.Generic;
using System.IO;

namespace ImprovedDairyProject
{
    public class FileManager
    {
        private readonly string _filePath = "F:\\Coding Journey\\Projects I've done\\Dairy Project\\My Dairies.txt";

        public void UpdateEntryInFiles(List<JournalEntry> allEntries, JournalEntry entry, bool markedForDeletion = false)
        {
            using (FileStream fs = new FileStream(_filePath, FileMode.Truncate, FileAccess.Write))
            using (StreamWriter writer = new StreamWriter(fs))
            {
                foreach (var e in allEntries)
                {
                    if (e.EntryId == entry.EntryId)
                    {
                        allEntries[allEntries.IndexOf(e)] = entry;

                        if (markedForDeletion)
                        {
                            allEntries[allEntries.IndexOf(e)] = null;
                        }
                    }

                    if (e != null)
                    {
                        writer.WriteLine($"EntryId: {e.EntryId}");
                        writer.WriteLine($"Title: {e.Title}");
                        writer.WriteLine($"Date: {e.Date}");
                        writer.WriteLine($"Content: {e.Content}");
                    }
                }
            }
        }

        public bool SaveEntryToFiles(JournalEntry entry, bool markedForDeletion = false)
        {
            FileMode mode = File.Exists(_filePath) ? FileMode.Append : FileMode.CreateNew;

            using (FileStream fs = new FileStream(_filePath, mode, FileAccess.Write))
            using (StreamWriter writer = new StreamWriter(fs))
            {
                if (!markedForDeletion)
                {
                    writer.WriteLine($"EntryId: {entry.EntryId}");
                    writer.WriteLine($"Title: {entry.Title}");
                    writer.WriteLine($"Date: {entry.Date}");
                    writer.WriteLine($"Content: {entry.Content}");
                }
                return true;
            }
        }

        public List<JournalEntry> LoadEntriesFromFiles()
        {
            var allEntries = new List<JournalEntry>();

            using (FileStream fs = new FileStream(_filePath, FileMode.Open, FileAccess.Read))
            using (StreamReader reader = new StreamReader(fs))
            {
                while (!reader.EndOfStream)
                {
                    var entry = new JournalEntry
                    {
                        EntryId = reader.ReadLine().Substring(9),
                        Title = reader.ReadLine().Substring(7),
                        Date = reader.ReadLine().Substring(6),
                        Content = reader.ReadLine().Substring(9)
                    };
                    allEntries.Add(entry);
                }
            }

            return allEntries;
        }
    }

    public class EntryRepository
    {
        private readonly FileManager _fileManager;
        private List<JournalEntry> _allEntries;

        public EntryRepository()
        {
            _fileManager = new FileManager();
            _allEntries = _fileManager.LoadEntriesFromFiles();
        }

        public void CreateEntry(JournalEntry entry)
        {
            _fileManager.SaveEntryToFiles(entry);
            _allEntries.Add(entry);
        }

        public List<JournalEntry> GetAllEntries()
        {
            return _allEntries;
        }

        public void UpdateEntry(JournalEntry entry)
        {
            _fileManager.UpdateEntryInFiles(_allEntries, entry);
        }

        public void DeleteEntry(JournalEntry entry)
        {
            _fileManager.UpdateEntryInFiles(_allEntries, entry, true);
            _allEntries.Remove(entry);
        }

        public JournalEntry GetEntryById(string entryId)
        {
            return _allEntries.Find(e => e.EntryId == entryId);
        }
    }

    public class JournalEntry
    {
        public string EntryId { get; set; }
        public string Title { get; set; }
        public string Date { get; set; }
        public string Content { get; set; }

        public static JournalEntry CreateNew()
        {
            return new JournalEntry
            {
                EntryId = GenerateEntryId(),
                Date = DateTime.Now.ToString("yyyy-MM-dd")
            };
        }

        private static string GenerateEntryId()
        {
            var rndId = new Random();
            return (char)rndId.Next(65, 90) + Convert.ToString(rndId.Next(1, 1000));
        }
    }

    public class JournalApp
    {
        private readonly EntryRepository _entryRepository;

        public JournalApp()
        {
            _entryRepository = new EntryRepository();
        }

        public void Run()
        {
            while (true)
            {
                DisplayMainMenu();
                var choice = GetUserChoice(1, 6);

                switch (choice)
                {
                    case 1:
                        CreateEntry();
                        break;
                    case 2:
                        ViewAllEntries();
                        break;
                    case 3:
                        UpdateEntry();
                        break;
                    case 4:
                        DeleteEntry();
                        break;
                    case 5:
                        SearchEntry();
                        break;
                    case 6:
                        Console.WriteLine("Goodbye!");
                        return;
                }
            }
        }

        private void DisplayMainMenu()
        {
            Console.Clear();
            Console.WriteLine("\n\n\t\t================================================= ");
            Console.WriteLine("\t\t                My Personal Diary                ");
            Console.WriteLine("\t\t================================================= \n");

            Console.WriteLine("\t\t\t[1] Create New Entry");
            Console.WriteLine("\t\t\t[2] View All Entries");
            Console.WriteLine("\t\t\t[3] Update Existing Entry");
            Console.WriteLine("\t\t\t[4] Delete Entry");
            Console.WriteLine("\t\t\t[5] Search Entries");
            Console.WriteLine("\t\t\t[6] Exit");
            Console.WriteLine("\t\t================================================= \n");
        }

        private byte GetUserChoice(byte from, byte to)
        {
            Console.Write($"\t\tPlease Enter your choice! [{from} to {to}]? ");
            byte choice;
            while (!byte.TryParse(Console.ReadLine(), out choice) || choice < from || choice > to)
            {
                Console.Write($"\t\tInvalid choice! Please enter a number between [{from} to {to}]: ");
            }
            return choice;
        }

        private void CreateEntry()
        {
            var newEntry = JournalEntry.CreateNew();

            Console.Write("\t\tEnter Your Entry Title: ");
            newEntry.Title = Console.ReadLine();

            Console.Write("\t\tDump Your Thoughts: ");
            newEntry.Content = Console.ReadLine();

            _entryRepository.CreateEntry(newEntry);
            Console.WriteLine("\n\t\t======= New Entry Created Successfully :-) =======");
            Console.ReadKey();
        }

        private void ViewAllEntries()
        {
            var allEntries = _entryRepository.GetAllEntries();
            byte count = 0;

            foreach (var entry in allEntries)
            {
                count++;
                Console.WriteLine($"\t\t{count}. EntryId: {entry.EntryId} | [{entry.Date}] {entry.Title}");
            }

            Console.ReadKey();
        }

        private void UpdateEntry()
        {
            var entryId = GetUserInput("\t\tPlease Enter The Entry ID! ");
            var entry = _entryRepository.GetEntryById(entryId);

            if (entry == null)
            {
                Console.WriteLine("\n\t\tI'm Sorry, This Entry Doesn't Exist! Try Again.");
                Console.ReadKey();
                return;
            }

            PrintEntry(entry);

            if (!ConfirmAction("\t\tAre You Sure Want To Update This Entry? [Y or N]? "))
            {
                Console.WriteLine("\n\t\tI'm Sorry, An Interruption Occurred. Please Try Again.");
                Console.ReadKey();
                return;
            }

            UpdateEntryInformation(entry);
            _entryRepository.UpdateEntry(entry);
            Console.WriteLine("\n\t\t========= Entry Updated Successfully :-) ========");
            PrintEntry(entry);
            Console.ReadKey();
        }

        private void DeleteEntry()
        {
            var entryId = GetUserInput("\t\tPlease Enter The Entry ID! ");
            var entry = _entryRepository.GetEntryById(entryId);

            if (entry == null)
            {
                Console.WriteLine("\n\t\tI'm Sorry, This Entry Doesn't Exist! Try Again.");
                Console.ReadKey();
                return;
            }

            PrintEntry(entry);

            if (ConfirmAction("\t\tAre You Sure Want To Delete This Entry? [Y or N]? "))
            {
                _entryRepository.DeleteEntry(entry);
                Console.WriteLine("\n\t\t========= Entry Deleted Successfully :-) ========");
            }
            else
            {
                Console.WriteLine("\n\t\tI'm Sorry, An Interruption Occurred. Please Try Again.");
                Console.ReadKey();
            }
        }

        private void SearchEntry()
        {
            var entryId = GetUserInput("\t\tPlease Enter The Entry ID! ");
            var entry = _entryRepository.GetEntryById(entryId);

            if (entry == null)
            {
                Console.WriteLine("\n\t\tI'm Sorry, This Entry Doesn't Exist! Try Again.");
                Console.ReadKey();
                return;
            }

            PrintEntry(entry);
            Console.ReadKey();
        }

        private void UpdateEntryInformation(JournalEntry entry)
        {
            Console.Write("\n\t\tPlease Enter New Title! ");
            entry.Title = Console.ReadLine();

            Console.Write("\n\t\tPlease Enter New Content! ");
            entry.Content = Console.ReadLine();
        }

        private void PrintEntry(JournalEntry entry)
        {
            byte contentSeparator = 0;
            Console.WriteLine("\n\t\t================== Entry Found ==================");
            Console.WriteLine("\t\tEntryId: {0}", entry.EntryId);
            Console.WriteLine("\t\tTitle: {0}", entry.Title);
            Console.WriteLine("\t\tDate: {0}", entry.Date);
            Console.WriteLine("\t\t=================================================");
            Console.WriteLine("\t\tContent: ");
            Console.Write($"\t\t");
            foreach (var c in entry.Content)
            {
                contentSeparator++;
                Console.Write(c);
                if (contentSeparator >= 50 && char.IsWhiteSpace(c))
                {
                    Console.Write(c);
                    Console.WriteLine();
                    Console.Write("\t\t");
                    contentSeparator = 0;
                }
            }
            Console.WriteLine("\n\t\t=================================================");
        }

        private string GetUserInput(string message)
        {
            Console.Write(message);
            return Console.ReadLine();
        }

        private bool ConfirmAction(string message)
        {
            Console.Write(message);
            return Console.ReadLine().ToLower().StartsWith("y");
        }
    }

    internal class Program
    {
        static void Main(string[] args)
        {
            var journalApp = new JournalApp();
            journalApp.Run();
            Console.ReadKey();
        }
    }
}