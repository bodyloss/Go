using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.IO;
using System.Diagnostics;
using System.ComponentModel;
using System.Text.RegularExpressions;

/*
 * Go lets you add any system executable identifier to a key/value file so that you can then run commands super quick
 * i.e. go add paint c:\windows\system32\paint.exe
 * then you can do go paint and it will run paint.exe
 */
namespace Go
{
    class Program
    {
        // Change to path where you want to store your identifiers:commands
        private const string FILENAME = @"c:\users\jciechanowicz\GoCommands.dat";
        private const string LINE_SEPERATOR = "\t";

        private static string[] _reserved = new string[] {"add", "-a", "list", "-l", "remove", "-r", "clear", "-c"};

        public static Dictionary<Regex, Func<string[], string>> argIdents = new Dictionary<Regex, Func<string[], string>>
        {
            /*
             * Adds a new command to the commands file
             */
            {new Regex("add|-a"), x => {
                if (x.Length < 2)
                    return "Invalid number of options provided";

                // Check they aren't trying to add a identifier in our reserved word list
                if (_reserved.Contains(x[1].ToLower()))
                {
                    return "Commands can not be identified using '" + x[1] + "'";
                }

                // Check it doesn't exist
                string line;
                int gotLine = TryGetCommandLine(x[1], out line);
                if (gotLine == 0)
                    return line;
                else if (gotLine == 1)
                    return "Command file already contains entry for '" + x[1] + "' with command: " + line.Substring((x[1].Length + LINE_SEPERATOR.Length) - 1);

                // Need to concatenate all arguments after the identifier
                StringBuilder sb = new StringBuilder();
                for (int i = 2; i < x.Length - 1; i++)
                {
                    sb.Append(x[i]).Append(" ");
                }
                sb.Append(x[x.Length - 1]);

                try
                {
                    using (var sw = new StreamWriter(FILENAME, true))
                    {
                        sw.WriteLine(x[1] + LINE_SEPERATOR + sb.ToString());
                    }
                    return "Command: " + sb.ToString() + "\nAdded under identifier:" + x[1];
                }
                catch (DirectoryNotFoundException)
                {
                    return "File not found: " + FILENAME;
                }
                catch (IOException ioe)
                {
                    return "Error opening file \"" + FILENAME + "\" for writing: " + ioe.Message;
                }
                catch (UnauthorizedAccessException)
                {
                    return "You do not have access to write to " + FILENAME;
                }
                catch (Exception e)
                {
                    return "Unknown exception while trying to open \"" + FILENAME + "\" for writing: " + e.Message;
                }
            }},
            /*
             * Lists all commands in the file
             */
            {new Regex("list|-l"), x => {
                ICollection<string> lines = x.Length > 1 && x[1] != null ? (ICollection<string>)new SortedSet<string>() : new List<string>();
                try
                {
                    using (var sr = new StreamReader(FILENAME))
                    {
                        string line;
                        while ((line = sr.ReadLine()) != null)
                        {
                            lines.Add(line);
                        }
                    }
                    return "Contents of " + Path.GetFullPath(FILENAME) + "\n" + String.Join("\n", lines);
                }
                catch (FileNotFoundException)
                {
                    return "Could not find file: " + FILENAME;
                }
                catch (IOException ioe)
                {
                    return "IO exception while reading file: " + ioe.Message;
                }
            }},
            /*
             * Removes a command from the file
             */
            {new Regex("remove|-r"), x => {
                return "This doesn't work, clear it or manually do it."; 
            }},
            { new Regex("clear|-c"), x => {
                try {
                    if (File.Exists(FILENAME)) {
                        File.Delete(FILENAME);
                    }
                } catch (IOException ioe) {
                    return "Error clearing commands file: " + ioe;
                }
                return "";
            }},
            { new Regex(@"help|/\?|-h"), x => {
                return 
@"Usage: go identifier
go add|-a identifier command
go list|-l [order]
go clear|-c
go remove-r command            
";
            }},
            /*
             * Default catch to try and execute commands
             */
            {new Regex(".*"), x => {
                string line;
                int gotLine = TryGetCommandLine(x[0], out line);
                if (gotLine == 0)
                    return line;
                else if (gotLine == 1)
                    return "Command '" + x[0] + "' not found";

                string cmd = line.Substring(x[0].Length + LINE_SEPERATOR.Length);

                // TODO: work out whether to execute command though cmd shell using /C or 
                // whether it can just be started as a process straight up.
                try
                {
                    ProcessStartInfo pinfo = new ProcessStartInfo(cmd);
                    pinfo.CreateNoWindow = false;
                    pinfo.UseShellExecute = true;

                    Process.Start(pinfo);
                    return "";
                }
                catch (Win32Exception)
                {
                    // This is pretty bad ....
                    try {
                        Process.Start("cmd", "/C " + cmd);
                        return "Shelled command";
                    }
                    catch (Win32Exception w32e)
                    {
                        return "Error executing command in shell '" + cmd + "': " + w32e.Message;
                    }
                }
            }},
        };

        /// <summary>
        /// Tries to get the relevant command line from the file,
        /// If an error occurs then str contains the error
        /// </summary>
        /// <param name="identifier"></param>
        /// <param name="str"></param>
        /// <returns>0 for error, 1 for could not found, 2 for anything else. I like error codes, so sue me</returns>
        private static byte TryGetCommandLine(string identifier, out string str)
        {
            string ident = identifier + LINE_SEPERATOR;

            try
            {
                using (var sr = new StreamReader(FILENAME) )
                {
                    string line;
                    while ((line = sr.ReadLine()) != null)
                    {
                        // Check if we have our line
                        if (line.StartsWith(ident))
                        {
                            str = line;
                            return 2;
                        }
                    }
                    str = "";
                    return 1;
                }
            }
            catch (FileNotFoundException)
            {
                str = "Could not find file: " + FILENAME;
                return 0;
            }
            catch (IOException ioe)
            {
                str = "IO exception while reading file: " + ioe.Message;
                return 0;
            }
            catch (System.ComponentModel.Win32Exception w32e)
            {
                str = "Win32 exception while executing command: " + w32e.Message;
                return 0;
            }
        }

        static void Main(string[] args)
        {
            if (args.Length == 0) {
                Console.WriteLine("Invalid number of arguments");
                return;
            }

            string message = argIdents.First(x => x.Key.IsMatch(args[0])).Value(args);
            if (message != "")
            {
                Console.WriteLine(message);
                Console.Read();
            }
        }
    }
}
