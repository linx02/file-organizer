using System;
using System.Collections.Generic;
using System.IO;
using System.Security.Cryptography.X509Certificates;

namespace FileOrganizer {
    class Program {
        static void Main(string[] args){

            string directoryPath = Directory.GetCurrentDirectory();

            foreach (string arg in args) {
                switch (arg) {
                    case "-d":
                    case "--directory":
                        directoryPath = args[Array.IndexOf(args, arg) + 1];
                        break;
                }
            }

            Organizer organizer = new Organizer(directoryPath);

        }
    }

    class Organizer {
        private string directoryPath;
        private string organizerFilePath;
        private List<string> files = new List<string>();
        private Dictionary<string, List<string>> rules = new Dictionary<string, List<string>>{
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

        public Organizer(string directoryPath) {
            this.directoryPath = directoryPath;
            this.organizerFilePath = Path.Combine(directoryPath, ".organizer");

            if (!File.Exists(organizerFilePath)) {
                List<string> lines = new List<string>();
                foreach (KeyValuePair<string, List<string>> rule in rules) {
                    string line = rule.Key + " ";
                    foreach (string extension in rule.Value) {
                        line += extension + ", ";
                    }
                    lines.Add(line.TrimEnd(',', ' '));
            }
            File.WriteAllLines(organizerFilePath, lines);
        }
    }
}
}