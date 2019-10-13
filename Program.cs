using System;
using System.Threading.Tasks;

namespace GooglePhotosTakeoutBeautifier
{
    public class Program
    {
        public static void Main(string[] args)
        {
            if (args.Length != 2)
            {
                Console.WriteLine("Usage: dotnet run [string:directory] [bool:dryrun]");
                return;
            }

            var directory = args[0];
            var dryRun = Convert.ToBoolean(args[1]);

            var beautifier = new Beautifier(directory);

            Console.WriteLine("Beautify Google Photos takeout by trying remove irrelevant images based on directory and file naming.");
            Console.WriteLine(beautifier.GetInfo(false));

            if (!dryRun)
            {
                Console.WriteLine("Warning! This is NOT dry run and will delete files/folders. Continue? (y/n)");
                var key = Console.ReadKey(true).KeyChar;
                if (!(key.Equals('y') || key.Equals('Y')))
                {
                    Console.WriteLine("Aborted.");
                    return;
                }
            }
            else
            {
                Console.WriteLine("This is dry run so nothing will be deleted.");
            }

            try
            {
                var result = beautifier.Beautify(dryRun, Log);
                if (result == string.Empty)
                {
                    result = "Couldn't find files or folders to delete.";
                }
                Console.WriteLine(result);
            }
            catch (Exception ex)
            {
                Console.WriteLine($"Something went wrong: {ex.ToString()}");
            }
        }

        private static void Log(string log)
        {
            Console.WriteLine(log);
        }
    }
}
