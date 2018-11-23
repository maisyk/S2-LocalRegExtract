using System;
using System.Collections.Generic;
using System.IO;
using Microsoft.Win32;
using System.Linq;
using System.Threading.Tasks;
using System.Diagnostics;

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
        public String csvFile = "S2-LocalKeyExtractor.csv";

        public List<String> k64List = new List<String>();
        public List<String> k32List = new List<String>();
        public List<String> uk64List = new List<String>();
        public List<String> uk32List = new List<String>();

        public List<String> applicationKeys = new List<String>();
        public List<String> uninstallKeys = new List<String>();

        public List<String> tempList = new List<String>();

        public List<String> csvData = new List<String>();

        public String inputCommand = "";


        //Time
        public Stopwatch watch = new Stopwatch();
        public String timeTaken;


        //Misc
        public Boolean createCSV = false;
        #endregion

        #region Main

        static void Main(string[] args)
        {
            Console.Title = "S2-LocalRegExtractor";
            Console.ForegroundColor = ConsoleColor.Green;
            S2_LocalRegExtractor.Program m = new S2_LocalRegExtractor.Program();
            m.watch.Start();
            Console.WriteLine("Help:\n" +
                "\n-csv    Create a CSV file of gathered data" +
                "\n\nFor a default run press the Return key.");

            Console.Write("\n\nPlease enter a command: ");
            m.inputCommand = Console.ReadLine().ToUpper();
            Console.WriteLine("\n\n\n\n");
            m.SetupCheck();
            m.Run();
        }

        #endregion

        #region Setup

        public void SetupCheck()
        {
            if (inputCommand.Contains("-CSV"))
                createCSV = true;
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
            Filtering();
            watch.Stop();
            timeTaken = (((double)watch.ElapsedMilliseconds / (double)1000)).ToString();
            Console.WriteLine("Writing to log file...");

            WriteToLogFile();

            if (createCSV)
            {
                CreateCSVFile();
                Console.WriteLine("Generating CSV file");
            }

            Console.WriteLine("\n\nFinished reading keys. Please check log file for details." +
                                "\n\nTotal time taken: " + timeTaken + " Seconds\n" +
                                "Total Applications found from App Paths: " + applicationKeys.Count() +
                                "\nTotal Applications found in Uninstall keys: " + uninstallKeys.Count() +
                "\n\nThe log file can be found at the following location:\n\n" +

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
                    if (!subKeyName.Contains(".dll"))
                    {
                        k64List.Add(subKeyName);

                        if (createCSV)
                        {
                            String x = "," + subKeyName + ",";
                            csvData.Add(x);
                        }
                    }
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
                    if (!subKeyName.Contains(".dll"))
                    {
                        k32List.Add(subKeyName);

                        if (createCSV)
                        {
                            String x = "," + subKeyName + ",";
                            csvData.Add(x);
                        }
                    }
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
                    String displayName = "";
                    String displayIcon = "";
                    if (key.OpenSubKey(subKeyName).GetValue("DisplayName") != null)
                    {
                        x = x + "\nDisplay Name: " + key.OpenSubKey(subKeyName).GetValue("DisplayName");
                        displayName = "" + key.OpenSubKey(subKeyName).GetValue("DisplayName");
                    }


                    if (key.OpenSubKey(subKeyName).GetValue("DisplayIcon") != null)
                    {
                        x = x + "\nDisplay Icon: " + key.OpenSubKey(subKeyName).GetValue("DisplayIcon");
                        displayIcon = "" + key.OpenSubKey(subKeyName).GetValue("DisplayIcon");
                    }


                    if (key.OpenSubKey(subKeyName).GetValue("InstallLocation") != null && key.OpenSubKey(subKeyName).GetValue("InstallLocation") != "")
                    {
                        x = x + "\nInstall Location: " + key.OpenSubKey(subKeyName).GetValue("InstallLocation");
                    }

                    uk64List.Add(x);
                    if (createCSV)
                        csvData.Add(
                            displayName + "," +
                            displayIcon + "," +
                            subKeyName
                            );
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

            //CSV Data
            tempList.AddRange(csvData);
            csvData.Clear();

            distinct = tempList.Distinct().ToList();

            foreach (String x in distinct)
                csvData.Add(x);

            //Sort Lists
            applicationKeys.Sort();
            uninstallKeys.Sort();
            csvData.Sort();
        }

        #endregion

        #region Filtering

        public void Filtering()
        {

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

        #region Create a CSV file

        public void CreateCSVFile()
        {
            using (System.IO.StreamWriter file = new System.IO.StreamWriter(csvFile, true))
            {
                foreach (String line in csvData)
                    file.WriteLine(line);
            }
        }

        #endregion
    }
}
