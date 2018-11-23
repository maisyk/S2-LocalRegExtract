using System;
using System.Collections.Generic;
using System.IO;
using Microsoft.Win32;

namespace S2_LocalRegExtractor
{
    class Program
    {
        #region Variables

        public String k64 = @"SOFTWARE\Microsoft\Windows\CurrentVersion\App Paths\";
        public String k32 = @"SOFTWARE\WOW6432Node\Microsoft\Windows\CurrentVersion\App Paths\";
        public String uk64 = @"SOFTWARE\Microsoft\Windows\CurrentVersion\Uninstall\";
        public String uk32 = @"SOFTWARE\WOW6432Node\Microsoft\Windows\CurrentVersion\Uninstall\";

        public String outputFile = "S2-LocalKeyExtractor.log";

        List<String> k64List = new List<String>();
        List<String> k32List = new List<String>();
        List<String> uk64List = new List<String>();
        List<String> uk32List = new List<String>();
        #endregion

        #region Main

        static void Main(string[] args)
        {
            Console.Title = "S2-LocalRegExtractor";
            Console.ForegroundColor = ConsoleColor.Green;
            S2_LocalRegExtractor.Program m = new S2_LocalRegExtractor.Program();
            m.Run();
        }

        #endregion

        #region Run

        public void Run()
        {
            Console.WriteLine("Running...");
            Get64BitKeys();
            Get32BitKeys();
            Get64BitUninstallKeys();
            Get32BitUninstallKeys();

            SortLists();

            Console.WriteLine("Writing to log file...");
            WriteToLogFile();
            Console.WriteLine("\n\nFinished reading keys. Please check log file for details.\nThe log file can be found at the following location:\n\n" +
                Directory.GetCurrentDirectory() + @"\" + outputFile + "\n\n\nPress any key to exit...");
            Console.ReadKey();

        }

        #endregion

        #region Get 64bit Keys

        public void Get64BitKeys()
        {

            using (Microsoft.Win32.RegistryKey key = Registry.LocalMachine.OpenSubKey(k64))
            {
                foreach (String subKeyName in key.GetSubKeyNames())
                {
                    k64List.Add(subKeyName);
                }
            }
        }

        #endregion

        #region Get 32bit keys

        public void Get32BitKeys()
        {
            using (Microsoft.Win32.RegistryKey key = Registry.LocalMachine.OpenSubKey(k32))
            {
                foreach (String subKeyName in key.GetSubKeyNames())
                {
                    k32List.Add(subKeyName);
                }
            }
        }

        #endregion

        #region Get 64bit Uninstall keys

        public void Get64BitUninstallKeys()
        {
            using (Microsoft.Win32.RegistryKey key = Registry.LocalMachine.OpenSubKey(uk64))
            {
                foreach (String subKeyName in key.GetSubKeyNames())
                {
                    String x = "\nKey Name: " + subKeyName;

                    if (key.OpenSubKey(subKeyName).GetValue("DisplayName") != null)
                        x = x + "\nDisplay Name: " + key.OpenSubKey(subKeyName).GetValue("DisplayName");

                    if (key.OpenSubKey(subKeyName).GetValue("DisplayIcon") != null)
                        x = x + "\nDisplay Icon: " + key.OpenSubKey(subKeyName).GetValue("DisplayIcon");

                    if (key.OpenSubKey(subKeyName).GetValue("InstallLocation") != null && key.OpenSubKey(subKeyName).GetValue("InstallLocation") != "")
                        x = x + "\nInstall Location: " + key.OpenSubKey(subKeyName).GetValue("InstallLocation");
                    uk64List.Add(x);
                }
            }
        }

        #endregion

        #region Get 32bit Uninstall keys

        public void Get32BitUninstallKeys()
        {

            using (Microsoft.Win32.RegistryKey key = Registry.LocalMachine.OpenSubKey(uk32))
            {
                foreach (String subKeyName in key.GetSubKeyNames())
                {
                    String x = "\nKey Name: " + subKeyName;

                    if (key.OpenSubKey(subKeyName).GetValue("DisplayName") != null)
                        x = x + "\nDisplay Name: " + key.OpenSubKey(subKeyName).GetValue("DisplayName");

                    if (key.OpenSubKey(subKeyName).GetValue("DisplayIcon") != null)
                        x = x + "\nDisplay Icon: " + key.OpenSubKey(subKeyName).GetValue("DisplayIcon");

                    if (key.OpenSubKey(subKeyName).GetValue("InstallLocation") != null && key.OpenSubKey(subKeyName).GetValue("InstallLocation") != "")
                        x = x + "\nInstall Location: " + key.OpenSubKey(subKeyName).GetValue("InstallLocation");
                    uk32List.Add(x);
                }
            }
        }

        #endregion

        #region Sort lists

        public void SortLists()
        {
            k64List.Sort();
            k32List.Sort();
            uk64List.Sort();
            uk32List.Sort();
        }

        #endregion

        #region Write to log file

        public void WriteToLogFile()
        {
            if (File.Exists(outputFile))
            {
                File.Delete(outputFile);
            }

            String stars = "************************************************************************************************";
            using (System.IO.StreamWriter file = new System.IO.StreamWriter(outputFile, true))
            {
                file.WriteLine("" +
                    "Note: The display names from the uninstall keys are for refference purposes only\n" +
                    "for the application name. Do not enter this as part of any launch command in the\n " +
                    "AppsAnywhere portal! \n\n\n");
                file.WriteLine("                                     64bit Applications\n" + stars + "\n");

                foreach (String k in k64List)
                    file.WriteLine(k);
                file.WriteLine("Size: " + k64List.Count);

                file.WriteLine("\n\n                                     32bit Applications\n" + stars + "\n");

                foreach (String k in k32List)
                    file.WriteLine(k);
                file.WriteLine("Size: " + k32List.Count);

                file.WriteLine("\n\n                                     64bit Applications (From uninstall keys)\n" + stars + "\n");

                foreach (String k in uk64List)
                    file.WriteLine(k);
                file.WriteLine("Size: " + uk64List.Count);

                file.WriteLine("\n\n                                     32bit Applications (From uninstall keys)\n" + stars + "\n");

                foreach (String k in uk32List)
                    file.WriteLine(k);
                file.WriteLine("Size: " + uk32List.Count);
            }
        }

        #endregion
    }
}
