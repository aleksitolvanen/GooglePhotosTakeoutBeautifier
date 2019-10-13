using System;
using System.Globalization;
using System.IO;
using System.Linq;
using System.Text;
using System.Text.RegularExpressions;
using System.Threading.Tasks;

namespace GooglePhotosTakeoutBeautifier
{
    public class Beautifier
    {
        public readonly string directoryRegex = "^[0-9]{4}-[0-9]{2}-[0-9]{2}";
        public readonly string rootPath;

        public Beautifier(string rootPath)
        {
            this.rootPath = rootPath;
        }

        public string Beautify(bool dryRun, Action<string> log)
        {
            if (!Directory.Exists(rootPath))
            {
                return "Directory does not exist. Aborting...";
            }

            var sb = new StringBuilder();

            var rootDir = Directory.EnumerateDirectories(rootPath);

            foreach (var dirString in rootDir)
            {
                var dir = new DirectoryInfo(dirString);
                DeleteEmptyTakeoutDirectories(dir, dryRun, log);
            }

            return sb.ToString();
        }

        // Recursively delete empty Google Photos takeout directories
        // Means directories with only one .json file
        private void DeleteEmptyTakeoutDirectories(DirectoryInfo dir, bool dryRun, Action<string> log)
        {
            foreach (var subDir in dir.GetDirectories())
            {
                DeleteEmptyTakeoutDirectories(subDir, dryRun, log);
            }

            if (ShouldBeDeleted(dir))
            {
                var files = dir.GetFiles();

                foreach (var file in files)
                {
                    log.Invoke($"Deleting file     : {file.FullName}");

                    if (!dryRun)
                    {
                        try
                        {
                            file.Delete();
                        }
                        catch (Exception ex)
                        {
                            log.Invoke($"WARNING! Failed to delete file: {file.FullName}. Exception: {ex.ToString()}");
                        }
                    }
                }

                if (dir.GetDirectories().Length == 0 && dir.GetFiles().Length == 0)
                {
                    log.Invoke($"Deleting directory: {dir.FullName}");

                    if (!dryRun)
                    {
                        try
                        {
                            dir.Delete();
                        }
                        catch (Exception ex)
                        {
                            log.Invoke($"WARNING! Failed to delete directory: {dir.FullName}. Exception: {ex.ToString()}");
                        }
                    }
                }
            }
        }
        private bool ShouldBeDeleted(DirectoryInfo dir)
        {
            // More lenient towards directories starting with yyyy-MM-dd
            if (dir.Name.Length >= 10 && Regex.IsMatch(dir.Name.Substring(0, 10), directoryRegex))
            {
                var files = dir.GetFiles().Where(f => f.Extension.ToLower() != ".json").ToList();
                return files.Count == 0 || files.All(f => f.Name.StartsWith("folder", true, CultureInfo.InvariantCulture));
            }
            else
            {
                return !dir.GetFiles().Any(f => f.Name.StartsWith("IMG_", true, CultureInfo.InvariantCulture));
            }
        }

        public string GetInfo(bool verbose = true)
        {
            if (!Directory.Exists(rootPath))
            {
                return "Warning! Directory does not exist.";
            }

            var sb = new StringBuilder();
            var rootDir = Directory.EnumerateDirectories(rootPath).ToList();

            if (verbose)
            {
                foreach (var dir in rootDir)
                {
                    sb.AppendLine(dir.ToString());
                }
            }

            sb.AppendLine($"Directory count: {rootDir.Count}");

            return sb.ToString();
        }
    }
}