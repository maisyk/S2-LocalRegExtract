using System;
using System.Collections.Generic;
using System.IO;
using Microsoft.Win32;
using System.Linq;
using System.Diagnostics;

namespace S2_LocalRegExtractor
{
    class LKEX
    {

        #region Variables


        //Key locations
        public String k64 = @"SOFTWARE\Microsoft\Windows\CurrentVersion\App Paths\";
        public String k32 = @"SOFTWARE\WOW6432Node\Microsoft\Windows\CurrentVersion\App Paths\";
        public String uk64 = @"SOFTWARE\Microsoft\Windows\CurrentVersion\Uninstall\";
        public String uk32 = @"SOFTWARE\WOW6432Node\Microsoft\Windows\CurrentVersion\Uninstall\";

        //Logs
        public String csvFile = "S2-LocalKeyExtractor.csv";

        //Temp list to be used by various parts of the application
        public List<String> tempList = new List<String>();

        //Uninstall key lists
        public List<String> keyNameUK = new List<String>();
        public List<String> displayNameUK = new List<String>();
        public List<String> installLocationUK = new List<String>();
        public List<String> displayIconUK = new List<String>();

        //AppPath key lists
        public List<String> appKeyList = new List<String>(); //Main key name
        public List<String> defaultKeyList = new List<String>(); //Key/(Default)
        public List<String> pathKeyList = new List<String>(); //Key/Pathd

        //Output lists
        public List<String> appNameOutput = new List<String>();
        public List<String> deliveryMethOutput = new List<String>();
        public List<String> winLaunchExeOutput = new List<String>();
        public List<String> winUninstallKeyOutput = new List<String>();


        public List<String> output = new List<String>();

        public List<String> csvData = new List<String>();

        public String inputCommand = "";

        //Default variables for uninstall keys - You can edit these to set the default output for when 
        //the requested data field is blank. 
        const String DISPLAY_NAME = "";
        const String DISPLAY_ICON = "";
        const String INSTALL_LOCATION = "";
        const String UNINSTALL_KEY = "";

        //Default variables for AppPath keys - You can edit these to set the default output for when 
        //the requested data field is blank. 
        const String DEFAULT_KEY = "";
        const String PATH = "";

        //Time
        public Stopwatch watch = new Stopwatch();

        #endregion

        #region Main
        static void Main(string[] args)
        {
            S2_LocalRegExtractor.LKEX m = new S2_LocalRegExtractor.LKEX(); //New instance of this class

            m.Run(); //Execute the Run() method
        }

        #endregion

        #region Run 
        /* This method is used to control the flow of the application*/
        public void Run()
        {
            Console.ForegroundColor = ConsoleColor.Green;
            Console.Title = "S2-LocalRegExtractor 1.0.2";
            watch.Start(); //Start stop watch

            GetUninstallkeys();
            GetAppPathKeys();
            ChkUKvAppPaths();
            ProcessRemainingAP();
            ProcessRemainingUK();

            PrintData();
            CreateCSVFile();
            watch.Stop();
            Console.WriteLine("\n\nTime taken: " + (((double)watch.ElapsedMilliseconds / (double)1000)).ToString() + " Seconds");
            Console.Write("\nThe CSV file can be found in the following location:\n" + Directory.GetCurrentDirectory() + @"\" + csvFile);
            Console.WriteLine("\n\nDone, press any key to continue....");

            Console.ReadKey(); //Pause console window at end of execution
        }
        #endregion

        #region Get uninstall keys
        /* This method is used to attain all data required from the uninstall keys. It works by querying the
         * following two hives: 
         * - HKLM\SOFTWARE\Microsoft\Windows\CurrentVersion\Uninstall\
         * - HKLM\SOFTWARE\WOW6432Node\Microsoft\Windows\CurrentVersion\Uninstall\
         * */

        public void GetUninstallkeys()
        {

            //Declare variables for data requests
            var displayName = DISPLAY_NAME;
            var displayIcon = DISPLAY_ICON;
            var installLocation = INSTALL_LOCATION;


            //64bit Uninstall keys
            using (Microsoft.Win32.RegistryKey key = Registry.LocalMachine.OpenSubKey(uk64))
            {

                foreach (String subKeyName in key.GetSubKeyNames())
                {
                    //Reset variable data
                    displayName = DISPLAY_NAME;
                    displayIcon = DISPLAY_ICON;
                    installLocation = INSTALL_LOCATION;

                    //Get display name
                    if (key.OpenSubKey(subKeyName).GetValue("DisplayName") != null)
                    {
                        displayName = "" + key.OpenSubKey(subKeyName).GetValue("DisplayName");
                    }

                    //Get display icon
                    if (key.OpenSubKey(subKeyName).GetValue("DisplayIcon") != null)
                    {
                        displayIcon = "" + key.OpenSubKey(subKeyName).GetValue("DisplayIcon");
                    }

                    //Get install location
                    if (key.OpenSubKey(subKeyName).GetValue("InstallLocation") != null)
                    {
                        installLocation = "" + key.OpenSubKey(subKeyName).GetValue("InstallLocation");
                    }

                    //Will not execute nested code if all 3 checks return false
                    if (displayName != DISPLAY_NAME || displayIcon != DISPLAY_ICON || installLocation != INSTALL_LOCATION)
                    {
                        //Add data to lists
                        keyNameUK.Add(subKeyName);
                        displayNameUK.Add(displayName);
                        displayIconUK.Add(displayIcon);
                        installLocationUK.Add(installLocation);
                    }
                }
            }

            //32bit Uninstall keys
            using (Microsoft.Win32.RegistryKey key = Registry.LocalMachine.OpenSubKey(uk32))
            {

                foreach (String subKeyName in key.GetSubKeyNames())
                {
                    //Reset variable data
                    displayName = DISPLAY_NAME;
                    displayIcon = DISPLAY_ICON;
                    installLocation = INSTALL_LOCATION;

                    //Get display name
                    if (key.OpenSubKey(subKeyName).GetValue("DisplayName") != null)
                    {
                        displayName = "" + key.OpenSubKey(subKeyName).GetValue("DisplayName");
                    }

                    //Get display icon
                    if (key.OpenSubKey(subKeyName).GetValue("DisplayIcon") != null)
                    {
                        displayIcon = "" + key.OpenSubKey(subKeyName).GetValue("DisplayIcon");
                    }

                    //Get install location
                    if (key.OpenSubKey(subKeyName).GetValue("InstallLocation") != null)
                    {
                        installLocation = "" + key.OpenSubKey(subKeyName).GetValue("InstallLocation");
                    }

                    //Will not execute nested code if all 3 checks return false
                    if (displayName != DISPLAY_NAME || displayIcon != DISPLAY_ICON || installLocation != INSTALL_LOCATION)
                    {
                        //Add data to lists
                        keyNameUK.Add(subKeyName);
                        displayNameUK.Add(displayName);
                        displayIconUK.Add(displayIcon);
                        installLocationUK.Add(installLocation);
                    }
                }
            }
        }

        #endregion

        #region Get AppPath Keys

        public void GetAppPathKeys()
        {
            //Declare variables for data requests
            var defaultKey = DEFAULT_KEY;
            var pathKey = PATH;

            //64bit AppPath keys
            using (Microsoft.Win32.RegistryKey key = Registry.LocalMachine.OpenSubKey(k64))
            {

                foreach (String subKeyName in key.GetSubKeyNames())
                {
                    defaultKey = DEFAULT_KEY;
                    pathKey = PATH;

                    //Get (Default)
                    if (key.OpenSubKey(subKeyName).GetValue(null) != null)
                    {
                        //If not equal to null then convert the (Default) subkey to string 
                        //And then assign value to defaultKey
                        defaultKey = key.OpenSubKey(subKeyName).GetValue(null).ToString();
                    }

                    //Get Path
                    if (key.OpenSubKey(subKeyName).GetValue("Path") != null)
                    {
                        pathKey = "" + key.OpenSubKey(subKeyName).GetValue("Path");
                    }

                    //If data has been returned from the (Default) subkey
                    if (defaultKey != DEFAULT_KEY)
                    {
                        //Add to lists
                        appKeyList.Add(subKeyName);
                        defaultKeyList.Add(defaultKey);
                        pathKeyList.Add(pathKey);

                    }

                }
            }

            //32bit AppPath keys
            using (Microsoft.Win32.RegistryKey key = Registry.LocalMachine.OpenSubKey(k32))
            {

                foreach (String subKeyName in key.GetSubKeyNames())
                {
                    defaultKey = DEFAULT_KEY;
                    pathKey = PATH;

                    //Get (Default)
                    if (key.OpenSubKey(subKeyName).GetValue(null) != null)
                    {
                        //If not equal to null then convert the (Default) subkey to string 
                        //And then assign value to defaultKey
                        defaultKey = key.OpenSubKey(subKeyName).GetValue(null).ToString();
                    }

                    //Get Path
                    if (key.OpenSubKey(subKeyName).GetValue("Path") != null)
                    {
                        pathKey = "" + key.OpenSubKey(subKeyName).GetValue("Path");
                    }

                    //If data has been returned from the (Default) subkey
                    if (defaultKey != DEFAULT_KEY)
                    {
                        //Add to lists
                        appKeyList.Add(subKeyName);
                        defaultKeyList.Add(defaultKey);
                        pathKeyList.Add(pathKey);

                    }

                }
            }
        }

        #endregion

        #region Check Uninstall Keys against AppPaths

        public void ChkUKvAppPaths()
        {
            var UK = 0;
            var AP = 0;

            List<int> UKtoRemove = new List<int>();
            List<int> APtoRemove = new List<int>();

            //Compare InstallLocation to each path in AppPaths
            foreach (String i in installLocationUK)
            {
                foreach (String j in pathKeyList)
                {
                    //If i is not equal to "" and i is equal to J
                    if (i != "" && i == j)
                    {
                        //Get index of item in Uninstall Key list and AppPath list
                        UK = installLocationUK.IndexOf(i);
                        AP = pathKeyList.IndexOf(j);

                        appNameOutput.Add(displayNameUK[UK]);
                        deliveryMethOutput.Add("Locally Installed (OK)"); //1
                        winLaunchExeOutput.Add(appKeyList[AP]);
                        winUninstallKeyOutput.Add("");

                        UKtoRemove.Add(UK);
                        APtoRemove.Add(AP);
                    }
                }
            }

            #region Remove items

            //Remove items from UK

            foreach (int i in UKtoRemove)
            {
                if (keyNameUK[i] != "")
                {
                    keyNameUK[i] = "";
                }

                if (displayNameUK[i] != "")
                {
                    displayNameUK[i] = "";
                }

                if (displayIconUK[i] != "")
                {
                    displayIconUK[i] = "";
                }

                if (installLocationUK[i] != "")
                {
                    installLocationUK[i] = "";
                }
            }

            //Remove items form AP
            foreach(int i in APtoRemove)
            {
                if (appKeyList[i] != "")
                {
                    appKeyList[i] = "";
                }

                if (defaultKeyList[i] != "")
                {
                    defaultKeyList[i] = "";
                }
                   

                if (pathKeyList[i] != "")
                {
                    pathKeyList[i] = "";
                }
                    
            }

            UKtoRemove.Clear();
            APtoRemove.Clear();

            #endregion

            //Compare DisplayIcon to each Default value in the AppPaths array
            foreach (String i in displayIconUK)
            {

                foreach (String j in defaultKeyList)
                {
                    //Console.WriteLine(j);
                    //If i is not equal to "" and i is equal to J
                    if (i != "" && i == j)
                    {

                        //Get index of item in Uninstall Key list and AppPath list
                        UK = displayIconUK.IndexOf(i);
                        AP = defaultKeyList.IndexOf(j);

                        appNameOutput.Add(displayNameUK[UK]);
                        deliveryMethOutput.Add("Locally Installed (OK)"); //2
                        winLaunchExeOutput.Add(commaEncapsulation(appKeyList[AP]));
                        winUninstallKeyOutput.Add("");

                        UKtoRemove.Add(UK);
                        APtoRemove.Add(AP);

                    }
                }
            }

            #region Remove items

            //Remove items from UK

            foreach (int i in UKtoRemove)
            {
                if (keyNameUK[i] != "")
                {
                    keyNameUK[i] = "";
                }

                if (displayNameUK[i] != "")
                {
                    displayNameUK[i] = "";
                }

                if (displayIconUK[i] != "")
                {
                    displayIconUK[i] = "";
                }

                if (installLocationUK[i] != "")
                {
                    installLocationUK[i] = "";
                }
            }

            //Remove items form AP
            foreach (int i in APtoRemove)
            {
                if (appKeyList[i] != "")
                {
                    appKeyList[i] = "";
                }

                if (defaultKeyList[i] != "")
                {
                    defaultKeyList[i] = "";
                }


                if (pathKeyList[i] != "")
                {
                    pathKeyList[i] = "";
                }

            }

            UKtoRemove.Clear();
            APtoRemove.Clear();
            #endregion

        }

        #endregion


        #region Process remaining AppPaths

        public void ProcessRemainingAP()
        {

            List<int> APtoRemove = new List<int>();

            foreach (String s in defaultKeyList)
            {
                if(s != "")
                {
                    var item = defaultKeyList.IndexOf(s);
                    appNameOutput.Add(s);
                    deliveryMethOutput.Add("Locally Installed (OK)"); //3
                    winLaunchExeOutput.Add(appKeyList[item]);
                    winUninstallKeyOutput.Add(UNINSTALL_KEY);

                    //appNameOutput.Insert(0, s);
                    //deliveryMethOutput.Insert(0, "Locally Installed (OK)");
                    //winLaunchExeOutput.Insert(0, appKeyList[item]);
                    //winUninstallKeyOutput.Insert(0, UNINSTALL_KEY);

                    APtoRemove.Add(item);
                }
            }

            #region Remove items


            //Remove items form AP
            foreach (int i in APtoRemove)
            {
                if (appKeyList[i] != "")
                {
                    appKeyList[i] = "";
                }

                if (defaultKeyList[i] != "")
                {
                    defaultKeyList[i] = "";
                }


                if (pathKeyList[i] != "")
                {
                    pathKeyList[i] = "";
                }

            }

            APtoRemove.Clear();

            #endregion

            foreach (String s in defaultKeyList)
            {
                var i = defaultKeyList.IndexOf(s);

                //If application key exists but the coresponding default subkey is empty 
                if(appKeyList[i] != "" && s == "" && !s.Contains(".dll"))
                {
                    appNameOutput.Add(appKeyList[i]);
                    deliveryMethOutput.Add("Locally Deployed"); //4
                    winLaunchExeOutput.Add(DEFAULT_KEY);
                    winUninstallKeyOutput.Add(UNINSTALL_KEY);
                }
            }
        }

        #endregion

        #region Process remaining uninstall keys

        public void ProcessRemainingUK()
        {

            List<int> UKtoRemove = new List<int>();
            List<String> temp1 = new List<String>();
            List<String> temp2 = new List<String>();
            List<String> temp3 = new List<String>();
            List<String> temp4 = new List<String>();


            foreach (String s in displayIconUK)
            {
                var item = displayIconUK.IndexOf(s);
                if (s.Contains(".exe"))
                {
                    var a = s.Replace(",0", "");
                    appNameOutput.Add(displayNameUK[item]);
                    deliveryMethOutput.Add("Locally Installed (Check .exe)");//5
                    winLaunchExeOutput.Add(a);
                    winUninstallKeyOutput.Add(keyNameUK[item]);

                    UKtoRemove.Add(item);

                }
            }

            #region Remove items

            //Remove items from UK

            foreach (int i in UKtoRemove)
            {
                if (keyNameUK[i] != "")
                {
                    keyNameUK[i] = "";
                }

                if (displayNameUK[i] != "")
                {
                    displayNameUK[i] = "";
                }

                if (displayIconUK[i] != "")
                {
                    displayIconUK[i] = "";
                }

                if (installLocationUK[i] != "")
                {
                    installLocationUK[i] = "";
                }
            }

            UKtoRemove.Clear();
            #endregion

            foreach (String s in installLocationUK)
            {
                var item = installLocationUK.IndexOf(s);
                if (s != "")
                {
                    appNameOutput.Add(displayNameUK[item]);
                    deliveryMethOutput.Add("Locally Installed (Add .exe)"); //6
                    winLaunchExeOutput.Add(displayIconUK[item]);
                    winUninstallKeyOutput.Add(keyNameUK[item]);

                    UKtoRemove.Add(item);
                }
            }

            #region Remove items

            //Remove items from UK

            foreach (int i in UKtoRemove)
            {
                if (keyNameUK[i] != "")
                {
                    keyNameUK[i] = "";
                }

                if (displayNameUK[i] != "")
                {
                    displayNameUK[i] = "";
                }

                if (displayIconUK[i] != "")
                {
                    displayIconUK[i] = "";
                }

                if (installLocationUK[i] != "")
                {
                    installLocationUK[i] = "";
                }
            }

            UKtoRemove.Clear();
            #endregion

            foreach (String s in keyNameUK)
            {
                var item = keyNameUK.IndexOf(s);
                if (s != "")
                {
                    appNameOutput.Add(displayNameUK[item]);
                    deliveryMethOutput.Add("Locally Deployed"); //7
                    winLaunchExeOutput.Add(displayIconUK[item]);
                    winUninstallKeyOutput.Add(keyNameUK[item]);

                    UKtoRemove.Add(item);
                }
            }

            #region Remove items

            //Remove items from UK

            foreach (int i in UKtoRemove)
            {
                if (keyNameUK[i] != "")
                {
                    keyNameUK[i] = "";
                }

                if (displayNameUK[i] != "")
                {
                    displayNameUK[i] = "";
                }

                if (displayIconUK[i] != "")
                {
                    displayIconUK[i] = "";
                }

                if (installLocationUK[i] != "")
                {
                    installLocationUK[i] = "";
                }
            }

            UKtoRemove.Clear();
            #endregion

        }

        #endregion


        #region Print data for testing 

        public void PrintData()
        {
            var item = 0;

            foreach (String i in appNameOutput)
            {
                item = appNameOutput.IndexOf(i);

                tempList.Add(commaEncapsulation(i) + "," + commaEncapsulation(deliveryMethOutput[item]) + "," + commaEncapsulation(winLaunchExeOutput[item]) + "," + commaEncapsulation(winUninstallKeyOutput[item]));
            }

            List<String> distinct = tempList.Distinct().ToList();
            output.AddRange(distinct);

            foreach(String i in output)
            {
                Console.WriteLine(i);
            }
        }
        #endregion

        #region Comma encapsulation

        public String commaEncapsulation(String s)
        {
            if (s.Contains(",")){
                s = "\"" + s + "\"";
            }


            return s;
        }

        #endregion

        #region Create a CSV file

        public void CreateCSVFile()
        {
            if (File.Exists(csvFile))
            {
                File.Delete(csvFile);
            }
            using (System.IO.StreamWriter file = new System.IO.StreamWriter(csvFile, true))
            {
                file.WriteLine("Application Name,Delivery Method,Windows Executable Name,Windows Uninstall Key\n");
                foreach (String i in output)
                {
                    file.WriteLine(i);
                }
            }
        }

        #endregion
    }
}

