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

            string directoryPath = Directory.GetCurrentDirectory();

            foreach (string arg in args)
            {
                switch (arg)
                {
                    case "-d":
                    case "--directory":
                        directoryPath = args[Array.IndexOf(args, arg) + 1];
                        break;
                }
            }

            Organizer organizer = new Organizer(directoryPath);
            organizer.LoadRules();
            organizer.LoadFiles();
            organizer.Organize();
        }
    }

    class Organizer
    {
        private string directoryPath;
        private string organizerFilePath;
        private List<string> files = new List<string>();
        private Dictionary<string, List<string>> rules = new Dictionary<string, List<string>>();

        public Organizer(string directoryPath)
        {
            this.directoryPath = directoryPath;
            this.organizerFilePath = Path.Combine(directoryPath, ".organizer");
        }

        public void LoadRules(){

            // Load rules from file or create default rules
            if (!File.Exists(this.organizerFilePath))
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
            else
            {
                foreach (string line in File.ReadAllLines(organizerFilePath))
                {
                    string folder = line.IndexOf('/') == -1 ? line : line.Substring(0, line.IndexOf('/'));
                    string[] parts = line.Substring(folder.Length + 1).Split(',');
                    List<string> extensions = new List<string>();
                    foreach (string part in parts)
                    {
                        extensions.Add(part.Trim());
                    }
                    rules.Add(folder, extensions);
                }
            }
        }

        public void LoadFiles()
        {
            string[] allFiles = Directory.GetFiles(this.directoryPath, "*", SearchOption.AllDirectories);
            foreach (string file in allFiles)
            {
                files.Add(file);
            }
        }

        private bool ExtensionIncluded(string extension)
        {
            return files.Any(file => file.EndsWith(extension));
        }

        public void Organize()
        {
            foreach (string file in files)
            {
                string extension = Path.GetExtension(file);
                foreach (KeyValuePair<string, List<string>> rule in rules)
                {
                    if (rule.Value.Contains("*" + extension) && ExtensionIncluded(extension))
                    {
                        string folder = Path.Combine(this.directoryPath, rule.Key);
                        if (!Directory.Exists(folder))
                        {
                            Directory.CreateDirectory(folder);
                        }
                        string newFile = Path.Combine(folder, Path.GetFileName(file));
                        File.Move(file, newFile);
                        Console.WriteLine("Moved " + file + " to " + newFile);
                        break;
                    }
                }
            }
        }
    }
}