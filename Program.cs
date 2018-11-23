using System;
using System.Collections.Generic;
using System.IO;
using Microsoft.Win32;
using System.Linq;


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

        public List<String> k64List = new List<String>();
        public List<String> k32List = new List<String>();
        public List<String> uk64List = new List<String>();
        public List<String> uk32List = new List<String>();

        public List<String> applicationKeys = new List<String>();
        public List<String> uninstallKeys = new List<String>();

        public List<String> tempList = new List<String>();

        public String inputCommand = "";

        #endregion

        #region Main

        static void Main(string[] args)
        {
            Console.Title = "S2-LocalRegExtractor";
            Console.ForegroundColor = ConsoleColor.Green;
            S2_LocalRegExtractor.Program m = new S2_LocalRegExtractor.Program();
            //Console.WriteLine("Help:\n" +
            //    "\n-sd    Used to seperate data between 32bit and 64bit keys" +
            //    "\n\nFor a default run press the Return key.");

            //Console.Write("\n\nPlease enter a command: ");
            //m.inputCommand = Console.ReadLine().ToUpper();
            //Console.WriteLine("\n\n\n\n");
            m.Run();
        }

        #endregion

        #region Run

        public void Run()
        {
            Console.WriteLine("Running...");
            if (inputCommand.Contains("-SD"))
                
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
            //Sort Lists
            k64List.Sort();
            k32List.Sort();
            uk64List.Sort();
            uk32List.Sort();

            //Remove duplicates from lists
  
            //Application list
            tempList.AddRange(k64List);
            tempList.AddRange(k32List);
            List<String> distinct = tempList.Distinct().ToList();

            foreach (String x in distinct)
                applicationKeys.Add(x);

            tempList.Clear();
            distinct.Clear();

            //Uninstall keys
            tempList.AddRange(uk64List);
            tempList.AddRange(uk32List);
            distinct = tempList.Distinct().ToList();

            foreach (String x in distinct)
                uninstallKeys.Add(x);

            tempList.Clear();
            distinct.Clear();
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
                    "for the application name. Do not enter this as part of any launch command in the\n" +
                    "AppsAnywhere portal! \n\n\n");



                file.WriteLine("                             Applications from App Paths (32bit + 64bit)\n" + stars + "\n");
                foreach (String x in applicationKeys)
                    file.WriteLine(x);
                file.WriteLine("                             Applications from Unisntall keys (32bit + 64bit)\n" + stars + "\n");
                foreach (String x in uninstallKeys)
                    file.WriteLine(x);

            }
        }

        #endregion
    }
}
