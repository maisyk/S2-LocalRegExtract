//using System;
//using System.Collections.Generic;
//using System.IO;
//using Microsoft.Win32;
//using System.Linq;
//using System.Diagnostics;


//namespace S2_LocalRegExtractor
//{
//    class Program
//    {
//        #region Variables

//        public String k64 = @"SOFTWARE\Microsoft\Windows\CurrentVersion\App Paths\";
//        public String k32 = @"SOFTWARE\WOW6432Node\Microsoft\Windows\CurrentVersion\App Paths\";
//        public String uk64 = @"SOFTWARE\Microsoft\Windows\CurrentVersion\Uninstall\";
//        public String uk32 = @"SOFTWARE\WOW6432Node\Microsoft\Windows\CurrentVersion\Uninstall\";

//        public String outputFile = "S2-LocalKeyExtractor.log";
//        public String csvFile = "S2-LocalKeyExtractor.csv";

//        public List<String> k64List = new List<String>();
//        public List<String> k32List = new List<String>();
//        public List<String> uk64List = new List<String>();
//        public List<String> uk32List = new List<String>();

//        public List<String> applicationKeys = new List<String>();
//        public List<String> uninstallKeys = new List<String>();

//        public List<String> tempList = new List<String>();

//        public List<String> csvData = new List<String>();

//        public String inputCommand = "";


//        //Time
//        public Stopwatch watch = new Stopwatch();
//        public String timeTaken;


//        //Misc
//        public Boolean createCSV = false;
//        #endregion

//        #region Main

//        //static void Main(string[] args)
//        //{
//        //    Console.Title = "S2-LocalRegExtractor";
//        //    Console.ForegroundColor = ConsoleColor.Green;
//        //    S2_LocalRegExtractor.Program m = new S2_LocalRegExtractor.Program();
//        //    m.watch.Start();
//        //    Console.WriteLine("Help:\n" +
//        //        "\n-csv    Create a CSV file of gathered data" +
//        //        "\n\nFor a default run press the Return key.");

//        //    Console.Write("\n\nPlease enter a command: ");
//        //    m.inputCommand = Console.ReadLine().ToUpper();
//        //    Console.WriteLine("\n\n\n\n");
//        //    m.SetupCheck();
//        //    m.Run();
//        //}

//        #endregion

//        #region Setup

//        public void SetupCheck()
//        {
//            if (inputCommand.Contains("-CSV"))
//                createCSV = true;
//        }

//        #endregion

//        #region Run

//        public void Run()
//        {
//            Console.WriteLine("Running...");

//            Get64BitKeys();
//            Get32BitKeys();
//            Get64BitUninstallKeys();
//            Get32BitUninstallKeys();

//            SortLists();
//            Filtering();
//            watch.Stop();
//            timeTaken = (((double)watch.ElapsedMilliseconds / (double)1000)).ToString();
//            Console.WriteLine("Writing to log file...");

//            WriteToLogFile();

//            if (createCSV)
//            {
//                CreateCSVFile();
//                Console.WriteLine("Generating CSV file");
//            }

//            Console.WriteLine("\n\nFinished reading keys. Please check log file for details." +
//                                "\n\nTotal time taken: " + timeTaken + " Seconds\n" +
//                                "Total Applications found from App Paths: " + applicationKeys.Count() +
//                                "\nTotal Applications found in Uninstall keys: " + uninstallKeys.Count() +
//                "\n\nThe log file can be found at the following location:\n\n" +

//                Directory.GetCurrentDirectory() + @"\" + outputFile);

//            if (createCSV)
//                Console.WriteLine("\nThe CSV file can be found at the following location:\n\n" +
//                   Directory.GetCurrentDirectory() + @"\" + csvFile + "\n\n\nPress any key to exit...");

//            Console.ReadKey();

//        }

//        #endregion

//        #region Get 64bit Keys

//        public void Get64BitKeys()
//        {

//            using (Microsoft.Win32.RegistryKey key = Registry.LocalMachine.OpenSubKey(k64))
//            {
//                foreach (String subKeyName in key.GetSubKeyNames())
//                {
//                    String keyPath = "";
//                    if (!subKeyName.Contains(".dll")) //If subkey doesn't contain .dll in string
//                    {
//                        k64List.Add(subKeyName);

//                        if (createCSV) //If the create CSV option is set to true
//                        {
//                            //If the "Path" subkey exists
//                            if (key.OpenSubKey(subKeyName).GetValue("Path") != null)
//                            {
//                                keyPath = "" + key.OpenSubKey(subKeyName).GetValue("Path");

//                                keyPath = keyPath.Replace(@"C:\Program Files\", "");
//                                keyPath = keyPath.Replace(@"C:\Program Files (x86)\", "");
//                                keyPath = keyPath.Replace(";", "");
//                            }
//                            else
//                            {   //If the "Path" subkey doesn't exist
//                                keyPath = subKeyName.Replace(".exe", "");
//                            }

//                            String x = keyPath + "," + subKeyName + ",N/A,N/A";
//                            csvData.Add(x);
//                        }
//                    }
//                }
//            }
//        }

//        #endregion

//        #region Get 32bit keys

//        public void Get32BitKeys()
//        {
//            using (Microsoft.Win32.RegistryKey key = Registry.LocalMachine.OpenSubKey(k32))
//            {
//                foreach (String subKeyName in key.GetSubKeyNames())
//                {
//                    String keyPath = "";
//                    if (!subKeyName.Contains(".dll"))
//                    {
//                        k32List.Add(subKeyName);

//                        if (createCSV)
//                        {
//                            if (key.OpenSubKey(subKeyName).GetValue("Path") != null)
//                            {
//                                keyPath = "" + key.OpenSubKey(subKeyName).GetValue("Path");

//                                keyPath = keyPath.Replace(@"C:\Program Files\", "");
//                                keyPath = keyPath.Replace(@"C:\Program Files (x86)\", "");
//                                keyPath = keyPath.Replace(";", "");
//                            }
//                            else
//                            {
//                                keyPath = subKeyName.Replace(".exe", "");
//                            }

//                            String x = keyPath + "," + subKeyName + ",N/A,N/A";
//                            csvData.Add(x);
//                        }
//                    }
//                }
//                Console.WriteLine(csvData.Count());
//            }
//        }

//        #endregion

//        #region Get 64bit Uninstall keys

//        public void Get64BitUninstallKeys()
//        {
//            using (Microsoft.Win32.RegistryKey key = Registry.LocalMachine.OpenSubKey(uk64))
//            {

//                foreach (String subKeyName in key.GetSubKeyNames())
//                {

//                    //Variables used for csv file generation
//                    String x = "\nKey Name: " + subKeyName;
//                    String displayName = "";
//                    String displayIcon = "Please add manually";
//                    String installLocation = "N/A";

//                    //Display name
//                    if (key.OpenSubKey(subKeyName).GetValue("DisplayName") != null)
//                    {
//                        x = x + "\nDisplay Name: " + key.OpenSubKey(subKeyName).GetValue("DisplayName");
//                        displayName = "" + key.OpenSubKey(subKeyName).GetValue("DisplayName");
//                    }

//                    //Display Icon
//                    if (key.OpenSubKey(subKeyName).GetValue("DisplayIcon") != null)
//                    {
//                        x = x + "\nDisplay Icon: " + key.OpenSubKey(subKeyName).GetValue("DisplayIcon");
//                        displayIcon = "" + key.OpenSubKey(subKeyName).GetValue("DisplayIcon");

//                        if (displayIcon.Contains(","))
//                        {
//                            int index = displayIcon.IndexOf(",");

//                            if (index > 0)
//                                displayIcon = displayIcon.Substring(0, index);
//                        }
//                    }

//                    //Install location
//                    if (key.OpenSubKey(subKeyName).GetValue("InstallLocation") != null && key.OpenSubKey(subKeyName).GetValue("InstallLocation") != "")
//                    {
//                        x = x + "\nInstall Location: " + key.OpenSubKey(subKeyName).GetValue("InstallLocation");
//                        installLocation = "" + key.OpenSubKey(subKeyName).GetValue("InstallLocation");
//                    }

//                    uk64List.Add(x);

//                    if (displayName == "")
//                        displayName = "Default Name: " + subKeyName;

//                    if (createCSV)
//                        csvData.Add(
//                            displayName + "," +
//                            displayIcon + "," +
//                            subKeyName + "," +
//                            installLocation
//                            );
//                }
//                Console.WriteLine("Finished adding 64bit uninstall keys: " + csvData.Count());
//            }
//        }

//        #endregion

//        #region Get 32bit Uninstall keys

//        public void Get32BitUninstallKeys()
//        {

//            using (Microsoft.Win32.RegistryKey key = Registry.LocalMachine.OpenSubKey(uk32))
//            {
//                foreach (String subKeyName in key.GetSubKeyNames())
//                {

//                    //Variables used for csv file generation
//                    String x = "\nKey Name: " + subKeyName;
//                    String displayName = "";
//                    String displayIcon = "Please add manually";
//                    String installLocation = "N/A";

//                    //Display Name
//                    if (key.OpenSubKey(subKeyName).GetValue("DisplayName") != null)
//                    {
//                        x = x + "\nDisplay Name: " + key.OpenSubKey(subKeyName).GetValue("DisplayName");
//                        displayName = "" + key.OpenSubKey(subKeyName).GetValue("DisplayName");
//                    }

//                    //Display Icon
//                    if (key.OpenSubKey(subKeyName).GetValue("DisplayIcon") != null)
//                    {
//                        x = x + "\nDisplay Icon: " + key.OpenSubKey(subKeyName).GetValue("DisplayIcon");
//                        displayIcon = "" + key.OpenSubKey(subKeyName).GetValue("DisplayIcon");

//                        if (displayIcon.Contains(","))
//                        {
//                            int index = displayIcon.IndexOf(",");
//                            if (index > 0)
//                                displayIcon = displayIcon.Substring(0, index);
//                        }
//                    }

//                    //Install location
//                    if (key.OpenSubKey(subKeyName).GetValue("InstallLocation") != null && key.OpenSubKey(subKeyName).GetValue("InstallLocation") != "")
//                    {
//                        x = x + "\nInstall Location: " + key.OpenSubKey(subKeyName).GetValue("InstallLocation");
//                        installLocation = "" + key.OpenSubKey(subKeyName).GetValue("InstallLocation");
//                    }

//                    uk64List.Add(x);

//                    if (displayName == "")
//                        displayName = "Default Name: " + subKeyName;

//                    if (createCSV)
//                        csvData.Add(
//                            displayName + "," +
//                            displayIcon + "," +
//                            subKeyName + "," +
//                            installLocation
//                            );
//                }
//                Console.WriteLine("Finished adding 64bit uninstall keys: " + csvData.Count());
//            }
//        }

//        #endregion

//        #region Sort lists

//        public void SortLists()
//        {
//            //Sort Lists
//            k64List.Sort();
//            k32List.Sort();
//            uk64List.Sort();
//            uk32List.Sort();

//            //Remove duplicates from lists

//            //Application list
//            tempList.AddRange(k64List);
//            tempList.AddRange(k32List);
//            List<String> distinct = tempList.Distinct().ToList();

//            foreach (String x in distinct)
//                applicationKeys.Add(x);

//            tempList.Clear();
//            distinct.Clear();

//            //Uninstall keys
//            tempList.AddRange(uk64List);
//            tempList.AddRange(uk32List);
//            distinct = tempList.Distinct().ToList();

//            foreach (String x in distinct)
//                uninstallKeys.Add(x);

//            tempList.Clear();
//            distinct.Clear();

//            //CSV Data
//            tempList.AddRange(csvData);
//            csvData.Clear();

//            distinct = tempList.Distinct().ToList();

//            foreach (String x in distinct)
//                csvData.Add(x);

//            //Sort Lists
//            applicationKeys.Sort();
//            uninstallKeys.Sort();
//            //csvData.Sort();
//        }

//        #endregion

//        #region Filtering

//        public void Filtering()
//        {

//        }

//        #endregion

//        #region Write to log file

//        public void WriteToLogFile()
//        {
//            if (File.Exists(outputFile))
//            {
//                File.Delete(outputFile);
//            }

//            String stars = "************************************************************************************************";
//            using (System.IO.StreamWriter file = new System.IO.StreamWriter(outputFile, true))
//            {
//                file.WriteLine("" +
//                    "Note: The display names from the uninstall keys are for refference purposes only\n" +
//                    "for the application name. Do not enter this as part of any launch command in the\n" +
//                    "AppsAnywhere portal! \n\n\n");



//                file.WriteLine("                             Applications from App Paths (32bit + 64bit)\n" + stars + "\n");
//                foreach (String x in applicationKeys)
//                    file.WriteLine(x);
//                file.WriteLine("                             Applications from Unisntall keys (32bit + 64bit)\n" + stars + "\n");
//                foreach (String x in uninstallKeys)
//                    file.WriteLine(x);

//            }
//        }

//        #endregion

//        #region Create a CSV file

//        public void CreateCSVFile()
//        {
//            if (File.Exists(csvFile))
//            {
//                File.Delete(csvFile);
//            }
//            using (System.IO.StreamWriter file = new System.IO.StreamWriter(csvFile, true))
//            {
//                file.WriteLine("Application Name,Executable Name/ Path + Exe,Uninstall Key,Install Location\n");
//                foreach (String line in csvData)
//                    file.WriteLine(line);
//            }
//        }

//        #endregion
//    }
//}
