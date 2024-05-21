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
            bool verbose = false;

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
                    case "-v":
                    case "--verbose":
                        verbose = true;
                        break;
                    case "-h":
                    case "--help":
                        PrintHelp();
                        return;
                    case "-s":
                    case "--syntax":
                        Organizer.CheckSyntax(Path.Combine(directoryPath, ".organizer"));
                        return;
                }
            }

            // Create an organizer object and organize the files
            Organizer organizer = new Organizer(directoryPath, verbose);
            organizer.LoadRules();
            organizer.LoadFiles();
            organizer.Organize();

            // Remove empty folders
            if (removeEmptyFolders)
            {
                organizer.RemoveEmptyFolders(directoryPath);
            }
        }

        static void PrintHelp()
        {
            Console.WriteLine("Usage: organize [OPTIONS]");
            Console.WriteLine("Options:");
            Console.WriteLine("  -d, --directory <path>  Set the directory to organize (default: current directory)");
            Console.WriteLine("  -re, --remove-empty     Remove empty folders after organizing");
            Console.WriteLine("  -v, --verbose           Print verbose output");
            Console.WriteLine("  -h, --help              Show this help message");
            Console.WriteLine("  -s, --syntax            Check the syntax of the .organizer file");
        }
    }

    class Organizer
    {
        // Fields
        private string directoryPath;
        private string organizerFilePath;
        private bool verbose;
        private List<string> files = new List<string>();
        private Dictionary<string, List<string>> rules = new Dictionary<string, List<string>>();
        private List<string> negativeFolders = new List<string>();
        private List<string> negativeFiles = new List<string>();

        // Constructor
        public Organizer(string directoryPath, bool verbose = false)
        {
            this.directoryPath = directoryPath;
            organizerFilePath = Path.Combine(directoryPath, ".organizer");
            this.verbose = verbose;
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
                Message(organizerFilePath, "create");
            }
            // Load rules from file
            else
            {
                if (!CheckSyntax(organizerFilePath))
                {
                    Environment.Exit(1);
                }
                foreach (string line in File.ReadAllLines(organizerFilePath))
                {
                    line.Trim();
                    if (line == "") continue;
                    // Load negative rules
                    if (line.StartsWith('!')){
                        // Negative folder
                        if (line.EndsWith('/')){
                            negativeFolders.Add(line.Substring(1).Trim());
                        }
                        else {
                            // Negative file
                            negativeFiles.Add(line.Substring(1).Trim());
                        }
                    }
                    else{
                        // Normal rule
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

        private bool CheckNegative(string file)
        {
            foreach (string folder in negativeFolders)
            {
                if (file.Contains(folder))
                {
                    Message(file, "ignore");
                    return true;
                }
            }
            if (negativeFiles.Contains(Path.GetFileName(file)))
            {
                Message(file, "ignore");
                return true;
            }
            return false;
        }

        private void Message(string message, string type)
        {
            if (!verbose) return;
            switch(type)
            {
                case "move":
                    Console.ForegroundColor = ConsoleColor.DarkCyan;
                    Console.Write("[->] ");
                    Console.ResetColor();
                    Console.WriteLine(message);
                    break;
                case "create":
                    Console.ForegroundColor = ConsoleColor.Green;
                    Console.Write("[+] ");
                    Console.ResetColor();
                    Console.WriteLine(message);
                    break;
                case "ignore":
                    Console.ForegroundColor = ConsoleColor.Yellow;
                    Console.Write("[!] ");
                    Console.ResetColor();
                    Console.WriteLine(message);
                    break;
                case "delete":
                    Console.ForegroundColor = ConsoleColor.Red;
                    Console.Write("[-] ");
                    Console.ResetColor();
                    Console.WriteLine(message);
                    break;
            }
        }

        // Move a file to a folder
        private void MoveFile(string file, string folder)
        {
            // Check file not already in folder and not negative
            if (file.StartsWith(folder) || CheckNegative(file))
            {
                return;
            }
            if (!Directory.Exists(folder))
            {
                Directory.CreateDirectory(folder);
                Console.ForegroundColor = ConsoleColor.Green;
                Message(folder, "create");
            }
            string newFile = Path.Combine(folder, Path.GetFileName(file));
            File.Move(file, newFile);
            Message(file + " -> " + newFile, "move");
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
                    Message(dir, "delete");
                }
            }
        }
        public static bool CheckSyntax(string organizerFilePath)
        {
            if (!File.Exists(organizerFilePath))
            {
                Console.WriteLine(".organizer file not found.");
                return false;
            }

            string[] lines = File.ReadAllLines(organizerFilePath);
            List<string> errors = new List<string>();

            for (int i = 0; i < lines.Length; i++)
            {
                string line = lines[i].Trim();

                // Skip empty lines
                if (string.IsNullOrWhiteSpace(line))
                    continue;

                if (line.StartsWith("!"))
                {
                    // Exclusion rule syntax check
                    if (!line.EndsWith('/') && !line.Contains('.'))
                    {
                        errors.Add($"Line {i + 1}: Exclusion rule '{line}' should end with '/' (folder) or be a file name");
                    }
                    else if (line.EndsWith('/') && line.Length == 1)
                    {
                        errors.Add($"Line {i + 1}: Exclusion rule '{line}' should not be empty");
                    }
                }
                else
                {
                    // Folder rule syntax check
                    int slashIndex = line.LastIndexOf('/');
                    if (slashIndex == -1)
                    {
                        errors.Add($"Line {i + 1}: Missing '/' in folder rule '{line}'");
                        continue;
                    }

                    string folder = line.Substring(0, slashIndex + 1);
                    string extensions = line.Substring(slashIndex + 1).Trim();

                    if (string.IsNullOrEmpty(extensions))
                    {
                        errors.Add($"Line {i + 1}: No extensions specified in folder rule '{line}'");
                        continue;
                    }

                    string[] patterns = extensions.Split(',');

                    foreach (string pattern in patterns)
                    {
                        string trimmedPattern = pattern.Trim();

                        if (string.IsNullOrEmpty(trimmedPattern))
                        {
                            errors.Add($"Line {i + 1}: Empty extension pattern in folder rule '{line}'");
                        }
                        else if (!IsValidPattern(trimmedPattern))
                        {
                            errors.Add($"Line {i + 1}: Invalid extension pattern '{trimmedPattern}' in folder rule '{line}'");
                        }
                    }
                }
            }

            if (errors.Count > 0)
            {
                Console.ForegroundColor = ConsoleColor.Red;
                Console.WriteLine("Syntax errors found in .organizer file:");
                foreach (string error in errors)
                {
                    Console.WriteLine(error);
                }
                Console.ResetColor();
                return false;
            }
            else
            {
                Console.ForegroundColor = ConsoleColor.Green;
                Console.WriteLine("No syntax errors found in .organizer file.");
                Console.ResetColor();
                return true;
            }
        }

        private static bool IsValidPattern(string pattern)
        {
            // Check for valid wildcard pattern
            if (pattern.Contains("*"))
            {
                // Ensure that wildcard patterns are valid
                if ((pattern.StartsWith("*") && pattern.Contains(".")) || pattern.Contains("*."))
                {
                    return true;
                }
                return false;
            }

            // Check for valid file extension
            if (pattern.Contains(".") && pattern.Length > 1 && pattern.LastIndexOf('.') != 0)
            {
                return true;
            }

            return false;
        }
    
    }
}