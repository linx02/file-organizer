using System;
using System.Collections.Generic;
using System.IO;
using System.Security.Cryptography.X509Certificates;
using System.Linq;

namespace FileOrganizer
{
    class Program
    {
        static void Main(string[] args)
        {

            // Initially set the directory path to the current directory
            string directoryPath = Directory.GetCurrentDirectory();
            bool removeEmptyFolders = false;

            // Parse command line arguments
            foreach (string arg in args)
            {
                switch (arg)
                {
                    case "-d":
                    case "--directory":
                        directoryPath = args[Array.IndexOf(args, arg) + 1];
                        break;
                    case "-re":
                    case "--remove-empty":
                        removeEmptyFolders = true;
                        break;
                }
            }

            // Create an organizer object and organize the files
            Organizer organizer = new Organizer(directoryPath);
            organizer.LoadRules();
            organizer.LoadFiles();
            organizer.Organize();

            // Remove empty folders
            if (removeEmptyFolders)
            {
                organizer.RemoveEmptyFolders(directoryPath);
            }
        }
    }

    class Organizer
    {
        // Fields
        private string directoryPath;
        private string organizerFilePath;
        private List<string> files = new List<string>();
        private Dictionary<string, List<string>> rules = new Dictionary<string, List<string>>();

        // Constructor
        public Organizer(string directoryPath)
        {
            this.directoryPath = directoryPath;
            organizerFilePath = Path.Combine(directoryPath, ".organizer");
        }

        // Load rules from the organizer file
        public void LoadRules(){

            // Create default rules if organizer file does not exist
            if (!File.Exists(organizerFilePath))
            {
                rules = new Dictionary<string, List<string>>{
                    {"Images/", new List<string>{"*.jpg", "*.jpeg", "*.png", "*.gif", "*.bmp", "*.tiff", "*.webp"}},
                    {"Videos/", new List<string>{"*.mp4", "*.mkv", "*.avi", "*.mov", "*.wmv", "*.flv", "*.webm"}},
                    {"Music/", new List<string>{"*.mp3", ".wav", ".flac", ".ogg", ".m4a", ".wma"}},
                    {"Documents/", new List<string>{"*.pdf", "*.doc", "*.docx", "*.xls", "*.xlsx", "*.ppt", "*.pptx", "*.txt", "*.rtf", "*.csv"}},
                    {"Archives/", new List<string>{"*.zip", "*.rar", "*.7z", "*.tar", "*.gz", "*.bz2"}},
                    {"Executables/", new List<string>{"*.exe", "*.msi", "*.apk", "*.app", "*.bat", "*.sh"}},
                    {"Scripts/", new List<string>{"*.py", "*.js", "*.ts", "*.cs", "*.java", "*.cpp", "*.c", "*.h", "*.html", "*.css", "*.php", "*.rb", "*.pl", "*.lua", "*.sh", "*.bat"}},
                    {"Programs/", new List<string>{"*.dll", "*.so", "*.dylib"}},
                    {"Fonts/", new List<string>{"*.ttf", "*.otf", "*.woff", "*.woff2"}},
                    {"Shortcuts/", new List<string>{"*.lnk"}},
                    {"Torrents/", new List<string>{"*.torrent"}},
                    {"Databases/", new List<string>{"*.db", "*.sqlite", "*.sql", "*.json"}},
                    {"VM/", new List<string>{"*.vdi", "*.vmdk", "*.vhd", "*.vhdx", "*.iso"}},
                    {"Config/", new List<string>{"*.ini", "*.cfg", "*.conf", "*.json", "*.xml", "*.yml", "*.yaml"}},
                    {"Logs/", new List<string>{"*.log"}},
                    {"Temp/", new List<string>{"*.tmp", "*.temp"}},
                    {"Other/", new List<string>{"*.*"}}
                };
                // Write default rules to file
                List<string> lines = new List<string>();
                foreach (KeyValuePair<string, List<string>> rule in rules)
                {
                    string line = rule.Key + " ";
                    foreach (string extension in rule.Value)
                    {
                        line += extension + ", ";
                    }
                    lines.Add(line.TrimEnd(',', ' '));
                }
                File.WriteAllLines(organizerFilePath, lines);
            }
            // Load rules from file
            else
            {
                foreach (string line in File.ReadAllLines(organizerFilePath))
                {
                    // Split the line into folder and extensions
                    string folder = line.LastIndexOf('/') == -1 ? line : line.Substring(0, line.LastIndexOf('/'));
                    string[] parts = line.Substring(folder.Length + 1).Split(',');

                    // Add the folder and extensions to the rules dictionary
                    List<string> extensions = new List<string>();
                    foreach (string part in parts)
                    {
                        extensions.Add(part.Trim());
                    }
                    rules.Add(folder, extensions);
                }
            }
        }

        // Load all files in the directory and subdirectories
        public void LoadFiles()
        {
            string[] allFiles = Directory.GetFiles(directoryPath, "*", SearchOption.AllDirectories);
            foreach (string file in allFiles)
            {
                files.Add(file);
            }
        }

        // Check if the file matches a specific rule (file name)
        private string SpecificRule(string file)
        {
            foreach (KeyValuePair<string, List<string>> rule in rules)
            {
                if (rule.Value.Contains(Path.GetFileName(file)))
                {
                    return rule.Key;
                }
            }
            return "";
        }

        // Check if the file matches a wildcard rule
        private string WildcardRule(string file)
        {
            foreach (KeyValuePair<string, List<string>> rule in rules)
            {
                foreach (string spec in rule.Value)
                {
                    if (!(spec.Substring(0, 2) == "*.")){
                        if (MatchesPattern(file, spec))
                        {
                            return rule.Key;
                        }
                    }
                }
            }
            return "";
        }

        // Check if the file matches an extension rule
        private string ExtensionRule(string file)
        {
            string extension = Path.GetExtension(file);
            foreach (KeyValuePair<string, List<string>> rule in rules)
            {
                if (rule.Value.Contains("*" + extension))
                {
                    return rule.Key;
                }
            }
            return "";
        }

        // Check if the file matches a pattern
        private bool MatchesPattern(string file, string pattern)
        {
            string fileName = Path.GetFileName(file);

            if (pattern.StartsWith("*") && pattern.EndsWith(Path.GetExtension(file)))
            {
                // Pattern: *file.txt
                string endPattern = pattern.Substring(1); // Remove leading '*'
                return fileName.EndsWith(endPattern, StringComparison.OrdinalIgnoreCase);
            }
            else if (pattern.EndsWith("*" + Path.GetExtension(file)))
            {
                // Pattern: file*.txt
                string startPattern = pattern.Substring(0, pattern.IndexOf("*")); // Get part before '*'
                return fileName.StartsWith(startPattern);
            }

            return false;
        }

        // Move a file to a folder
        private void MoveFile(string file, string folder)
        {
            // Check file not already in folder
            if (file.StartsWith(folder))
            {
                return;
            }
            if (!Directory.Exists(folder))
            {
                Directory.CreateDirectory(folder);
            }
            string newFile = Path.Combine(folder, Path.GetFileName(file));
            File.Move(file, newFile);
            Console.WriteLine("Moved " + file + " to " + newFile);
        }

        // Organize the files
        public void Organize()
        {
            foreach (string file in files)
            {
                string folder;
                if ((folder = SpecificRule(file)) != "")
                {
                    MoveFile(file, Path.Combine(directoryPath, folder));
                }
                else if ((folder = WildcardRule(file)) != "")
                {
                    MoveFile(file, Path.Combine(directoryPath, folder));
                }
                else if ((folder = ExtensionRule(file)) != "")
                {
                    MoveFile(file, Path.Combine(directoryPath, folder));
                }

            }
        }

        // Remove empty folders
        public void RemoveEmptyFolders(string directory)
        {
            foreach (string dir in Directory.GetDirectories(directory))
            {
                RemoveEmptyFolders(dir);
                if (Directory.GetFiles(dir).Length == 0 && Directory.GetDirectories(dir).Length == 0)
                {
                    Directory.Delete(dir);
                    Console.WriteLine("Deleted empty folder: " + dir);
                }
            }
        }
    }
}