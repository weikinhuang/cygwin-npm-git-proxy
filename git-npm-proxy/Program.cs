using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Diagnostics;
using System.IO;
using System.Linq;
using System.Text;
using System.Text.RegularExpressions;
using System.Threading.Tasks;

namespace git_npm_proxy
{
    class Program
    {
        static StreamWriter w = File.AppendText(@"C:\npm.log.txt");

        static void Main(string[] args)
        {
            w.WriteLine("88888888888888888888888888888888888888888888888");
            //
            // Setup the process with the ProcessStartInfo class.
            //
            ProcessStartInfo start = new ProcessStartInfo();
            start.FileName = @"C:\cygwin\bin\git.exe"; // Specify exe name.

            Regex rmQuotes = new Regex("[\"]");
            List<string> argList = new List<string>();
            // escape arguments, assume '"' is not allowed
            w.WriteLine("XXXXXXXXXXXXXXXXXXXXXXXXXXXXXXXXXXXXXXXXXXXXXXXXXXXXXXXXX");
            foreach (string arg in args)
            {
                //Console.Error.WriteLine(arg);
                string addArg = arg;
                string[] addArgParts = new string[2];
                string addArgPrefix = "";
                if (addArg[0] == '-')
                {
                    argList.Add(addArg);
                    continue;
                }
                if (Regex.Match(arg, "[\"]").Success)
                {
                    addArg = rmQuotes.Replace(arg, "").ToString();
                }
                if (addArg.IndexOf('\\') == -1)
                {
                    argList.Add(addArg);
                }
                else
                {
                    w.WriteLine(ConvertToCygpath(addArg));
                    argList.Add("'" + ConvertToCygpath(addArg) + "'");
                }
            }
            w.WriteLine("XXXXXXXXXXXXXXXXXXXXXXXXXXXXXXXXXXXXXXXXXXXXXXXXXXXXXXXXX");

            start.Arguments = String.Join(" ", argList.ToArray());
            if (args[1] == "clone")
            {
                Console.Error.WriteLine(start.Arguments);
            }

            // remove the current implementation of the proxy from the path, since we assume
            // that this proxy application is in a separate PATH than the actual git binary
            string thisProcessPath = Path.GetDirectoryName(System.Diagnostics.Process.GetCurrentProcess().MainModule.FileName);
            string[] pathEnv = Environment.GetEnvironmentVariable("PATH").Split(';');
            List<string> list = new List<string>();
            for (int i = 0; i < pathEnv.Length; i++)
            {
                if (thisProcessPath != pathEnv[i] && (thisProcessPath + @"\") != pathEnv[i])
                {
                    list.Add(pathEnv[i]);
                }
            }
            start.EnvironmentVariables["PATH"] = String.Join(";", list.ToArray());
            start.EnvironmentVariables["HOME"] = Environment.GetEnvironmentVariable("HOME");
            start.EnvironmentVariables["CYGWIN"] = Environment.GetEnvironmentVariable("CYGWIN");
            start.EnvironmentVariables["TMP"] = Environment.GetEnvironmentVariable("TMP");
            start.EnvironmentVariables["TEMP"] = Environment.GetEnvironmentVariable("TEMP");
            start.EnvironmentVariables["APPDATA"] = Environment.GetEnvironmentVariable("APPDATA");

            w.WriteLine(Directory.GetCurrentDirectory());

            start.ErrorDialog = false;
            start.UseShellExecute = false;
            start.RedirectStandardOutput = true;
            start.RedirectStandardError = true;
            start.RedirectStandardInput = true;

            Process process = Process.Start(start);
            process.WaitForExit();
            Console.Write(process.StandardOutput.ReadToEnd().Trim());
            Console.Error.Write(process.StandardError.ReadToEnd().Trim());
            w.WriteLine("EXITING---------------------------------------");
            Environment.Exit(0);
        }

        /// <summary>
        /// Invokes 'cygpath' to convert a cygwin path to a windows path. 
        /// Note the cygpath executable must be in your PATH, or an exception will 
        /// silently occur and your original unconverted path will be returned
        /// </summary>
        /// <param name="path">The input path, potentially in cygwin format</param>
        /// <returns>If the path does not contain the / character, it is simply returned.
        /// If it does, then cygpath is invoked, and the value returned from cygpath is returned.
        /// If an error occurs invoking cygpath, then the error is swallowed and the input path is simply returned.</returns>
        public static string ConvertToCygpath(string path)
        {
            if (!path.Contains("\\"))
                return path;

            ProcessStartInfo startInfo = new ProcessStartInfo("cygpath", "-u '" + path.Replace("'", "\\'") + "'")
            {
                UseShellExecute = false,
                WindowStyle = ProcessWindowStyle.Hidden,
                CreateNoWindow = true,
                RedirectStandardOutput = true
            };

            try
            {
                w.WriteLine("CYGXXXXXXXXXXXXXXXXXXXXXXXXXXXXXXXXXXXXXXXXX");
                var s = Process.Start(startInfo).StandardOutput.ReadToEnd().Trim();
                w.WriteLine(path);
                w.WriteLine(s);
                w.WriteLine("CYGXXXXXXXXXXXXXXXXXXXXXXXXXXXXXXXXXXXXXXXXX");
                return s;
            }
            catch (Win32Exception) // file not found - can't find cygpath.exe. Don't care about being more specific with our error
            { }
            return path;
        }
    }
}
