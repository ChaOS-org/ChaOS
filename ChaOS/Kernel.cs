﻿using System;
using System.IO;
using Sys = Cosmos.System;
using Cosmos.System.FileSystem;
using Cosmos.System.FileSystem.VFS;
using IL2CPU.API.Attribs;
using PrismAPI.Graphics.Fonts;
using PrismAPI.Hardware.GPU;
using static ChaOS.Core;
using static ChaOS.DiskManager;
using static SVGAIIColor;

namespace ChaOS
{
    public class Kernel : Sys.Kernel
    {
        public const string MajorVer = "1"; 
        public const string MinorVer = "2";
        public const string FixVer = "0";
        public const string PreVer = "Prerelease 1";
        
        public const string ver = MajorVer + "." + MinorVer + "." + FixVer + " " + PreVer;
        
        
        
        public static bool devMode = false;
        public static bool hasUpdatedConsoleSize = false;

        public static ushort Width = 800, Height = 600;

        [ManifestResourceStream(ResourceName = "ChaOS.Resources.Font_1x.btf")]
        static byte[] font_1x_raw;

        public static Font Font_1x = new Font(font_1x_raw, 16);
        
        public static string us = String.Empty;

        public static Display Screen;
        public static SVGAIITerminal Console;
        
        public const string copyright = "Copyright (c) 2023 ekeleze. All Rights Reserved.";

        public static string username = "usr";

        public static string input;
        public static string inputBeforeLower;
        public static string inputCapitalized;

        public static CosmosVFS fs = new CosmosVFS();

        protected override void BeforeRun()
        {
            try
            {
                Screen = Display.GetDisplay(Width, Height);
                Console = new SVGAIITerminal(Width, Height, Font_1x, FallbackTerminalUpdate);

                //log("Starting up ChaOS...");
                InitFS(fs);
                LoadSettings();

                Console.Clear();
                //log("Welcome to...\n");
                clog(
                    "  ______   __                   ______    ______  \n /      \\ |  \\                 /      \\  /      \\ \n|  $$$$$$\\| $$____    ______  |  $$$$$$\\|  $$$$$$\\\n| $$   \\$$| $$    \\  |      \\ | $$  | $$| $$___\\$$\n| $$      | $$$$$$$\\  \\$$$$$$\\| $$  | $$ \\$$    \\ \n| $$   __ | $$  | $$ /      $$| $$  | $$ _\\$$$$$$\\\n| $$__/  \\| $$  | $$|  $$$$$$$| $$__/ $$|  \\__| $$\n \\$$    $$| $$  | $$ \\$$    $$ \\$$    $$ \\$$    $$\n  \\$$$$$$  \\$$   \\$$  \\$$$$$$$  \\$$$$$$   \\$$$$$$ ",
                    DarkGreen);
                //log("\n" + ver + "\n" + copyright + "\nType \"help\" to get started!");
                //if (!disk) log("No hard drive detected, ChaOS will continue without disk support.");

                log();
                log("ChaOS Version info:");
                log("   ChaOS " + ver);
                log();

                log("Here are some planned features:");
                log("   GoOS compatability: \n      GoCode\n      9xCode");
                log("   Custom Java compiler and runtime");
                log("   and more");

                Console.SetCursorPosition(0, 32);
                log("Currently ChaOS is unavailable.");
                log("You may enter dev mode to test ChaOS, however it may be unstable and may corrupt data or not work.\n");
                write("To enter dev mode, press ESC...");

                while (!devMode)
                {
                    ConsoleKeyInfo key = Console.ReadKey();

                    if (key.Key == ConsoleKey.Escape)
                    {
                        Width = 1280;
                        Height = 960;
                        
                        UpdateResolution();
                        devMode = true;
                    }
                }
            }
            catch (Exception ex)
            {
                Crash(ex);
            }
        }

        protected override void Run()
        {
            var CanContinue = true;

            try
            {
                if (!Directory.GetCurrentDirectory().StartsWith(rootdir))
                    Directory.SetCurrentDirectory(rootdir); // Directory error correction

                if (disk) write(username + " (" + Directory.GetCurrentDirectory() + "): ");
                else write(username + " > ");

                inputBeforeLower = Console.ReadLine(); // Input
                inputCapitalized = inputBeforeLower.ToUpper(); // Input converted to uppercase
                input = inputBeforeLower.ToLower().Trim(); // Input converted to lowercase

                log();

                if (input.StartsWith("help"))
                {
                    clog("Functions:", DarkGreen);
                    log(" help - Shows all functions, do \"help (page)\" for more commands\n" + 
                        " cls/clear - Clears the screen\n" + 
                        " time - Tells you the time\n" + 
                        " echo - Echoes what you say\n" + 
                        " calc - Allows you to do simple math\n" + 
                        " sysinfo - Gives info about the system\n" + 
                        " username - Username related functions\n" + 
                        " color - Allows you to change the color scheme\n" + 
                        " sd/shutdown - Shuts down ChaOS\n" + 
                        " rb/reboot - Reboots the system\n" + 
                        " diskinfo - Gives info about the disk" + us + "\n" +
                        " cd - Opens directory" + us + "\n" +
                        " cd.. - Opens last directory" + us + "\n" +
                        " dir - Lists files in the current directory" + us + "\n" + 
                        " mkdir - Creates a directory" + us + "\n" +
                        " mkfile - Creates a file" + us + "\n" +
                        " deldir - Deletes a directory" + us + "\n" +
                        " delfile - Deletes a file" + us + "\n" +
                        " lb - Relabels the disk" + us + "\n" +
                        " notepad - Launches MIV (Minimalistic Vi)" + us + "\n");
                }

                #region Username functions

                else if (input.StartsWith("username"))
                {
                    if (input.Contains("set"))
                    {
                        try
                        {
                            username = input.Split("set ")[1].Trim();
                        }
                        catch
                        {
                            clog("No arguments\n", Red);
                        }

                        clog("Done! (" + username + ")\n", Yellow);
                    }
                    else if (input.EndsWith("current")) clog("Current username: " + username + "\n", Yellow);
                    else
                    {
                        clog("Username subfunctions:", DarkGreen);
                        log(" username set (username) - Changes the username");
                        log(" username current - Displays current username\n");
                    }
                }

                #endregion

                #region Color functions

                else if (input.Contains("color"))
                {
                    if (input.EndsWith("list"))
                    {
                        var OldBack = Console.BackgroundColor;
                        var OldFore = Console.ForegroundColor;
                        clog("Color list:", Green);
                        write(" ");
                        SetScreenColor(White, Black, false);
                        write("black - Pure light mode, will make you blind");
                        SetScreenColor(OldBack, OldFore, false);
                        clog("\n dark blue - Dark blue with black background", DarkBlue);
                        clog(" dark green - Dark green with black background", DarkGreen);
                        clog(" dark cyan - Dark cyan with black background", DarkCyan);
                        clog(" dark gray - Dark gray with black background", DarkGray);
                        clog(" blue - Normal blue with black background", Blue);
                        clog(" green - Green with black background", Green);
                        clog(" cyan - Cyan with black background", Cyan);
                        clog(" dark red - Dark red with black background", DarkRed);
                        clog(" dark magenta - Dark magenta with black background", DarkMagenta);
                        clog(" dark yellow - Dark yellow/orange with black background", DarkYellow);
                        clog(" gray - Gray with black background", Gray);
                        clog(" red - Red with black background", Red);
                        clog(" magenta - Magenta with black background", Magenta);
                        clog(" yellow - Light yellow with black background", Yellow);
                        clog(" white - Pure white with black background\n", White);
                    } // "Cosmos is built on else if blocks"
                    else if (input.EndsWith("black")) SetScreenColor(White, Black);
                    else if (input.EndsWith("dark blue")) SetScreenColor(Black, DarkBlue);
                    else if (input.EndsWith("dark green")) SetScreenColor(Black, DarkGreen);
                    else if (input.EndsWith("dark cyan")) SetScreenColor(Black, DarkCyan);
                    else if (input.EndsWith("dark gray")) SetScreenColor(Black, DarkGray);
                    else if (!input.Contains("dark") && input.EndsWith("blue")) SetScreenColor(Black, Blue);
                    else if (!input.Contains("dark") && input.EndsWith("green")) SetScreenColor(Black, Green);
                    else if (!input.Contains("dark") && input.EndsWith("cyan")) SetScreenColor(Black, Cyan);
                    else if (input.EndsWith("dark red")) SetScreenColor(Black, DarkRed);
                    else if (input.EndsWith("dark magenta")) SetScreenColor(Black, DarkMagenta);
                    else if (input.EndsWith("dark yellow")) SetScreenColor(Black, DarkYellow);
                    else if (!input.Contains("dark") && input.EndsWith("gray")) SetScreenColor(Black, Gray);
                    else if (!input.Contains("dark") && input.EndsWith("red")) SetScreenColor(Black, Red);
                    else if (!input.Contains("dark") && input.EndsWith("magenta")) SetScreenColor(Black, Magenta);
                    else if (!input.Contains("dark") && input.EndsWith("yellow")) SetScreenColor(Black, Yellow);
                    else if (input.EndsWith("white")) SetScreenColor(Black, White);
                    else
                        clog(
                            "Please list colors by doing \"opt color list\" or set a color by doing \"opt color (color)\"\n",
                            Gray);
                }

                #endregion

                else if (input == "clear" || input == "cls")
                    Console.Clear();

                else if (input == "time")
                {
                    string Hour = Cosmos.HAL.RTC.Hour.ToString();
                    string Minute = Cosmos.HAL.RTC.Minute.ToString();
                    if (Minute.Length < 2) Minute = "0" + Minute;
                    clog("Current time is " + Hour + ":" + Minute + "\n", Yellow);
                }

                else if (input == "shutdown" || input == "sd")
                {
                    if (disk) SaveChangesToDisk();
                    clog("Shutting down...", Gray);
                    Sys.Power.Shutdown();
                }

                else if (input == "reboot" || input == "rb")
                {
                    if (disk) SaveChangesToDisk();
                    clog("Rebooting...", Gray);
                    Sys.Power.Reboot();
                }

                else if (input.StartsWith("echo"))
                    clog(inputBeforeLower.Split("echo ")[1] + "\n", Gray);

                else if (input.StartsWith("calc"))
                {
                    log(Apps.Calc.Single(inputBeforeLower.Replace("calc ", "")));
                }

                else if (input == "sysinfo")
                {
                    clog("System info:", DarkGreen);
                    log(" CPU: " + Cosmos.Core.CPU.GetCPUBrandString());
                    log(" CPU speed: " + Cosmos.Core.CPU.GetCPUCycleSpeed() / 1e6 + "MHz");
                    log(" System RAM: " + Cosmos.Core.CPU.GetAmountOfRAM() + "MiB used out of " +
                        Cosmos.Core.GCImplementation.GetAvailableRAM() + "\n");
                }

                else if (input == "diskinfo")
                {
                    long availableSpace = VFSManager.GetAvailableFreeSpace(@"0:\");
                    long diskSpace = VFSManager.GetTotalSize(@"0:\");
                    string fsType = VFSManager.GetFileSystemType("0:\\");
                    clog("Disk info for " + fs.GetFileSystemLabel(rootdir), DarkGreen);
                    if (diskSpace < 1e6)
                        log(" Disk space: " + availableSpace / 1e3 + " KB free out of " + diskSpace / 1000 +
                            " KB total");
                    else if (diskSpace > 1e6)
                        log(" Disk space: " + availableSpace / 1e6 + " MB free out of " + diskSpace / 1e+6 +
                            " MB total");
                    else if (diskSpace > 1e9)
                        log(" Disk space: " + availableSpace / 1e9 + " GB free out of " + diskSpace / 1e+9 +
                            " GB total");
                    log(" Filesystem type: " + fsType + "\n");
                }

                #region Disk commands

                else if (input.StartsWith("mkdir"))
                {
                    try
                    {
                        inputCapitalized = inputCapitalized.Split("MKDIR ")[1];
                    }
                    catch
                    {
                        clog("No arguments\n", Red);
                        CanContinue = false;
                    }

                    if (inputCapitalized.Contains("0:\\"))
                    {
                        inputCapitalized.Replace("0:\\", "");
                    }

                    if (inputCapitalized.Contains(" "))
                    {
                        clog("Directory name cannot contain spaces!\n", Red);
                        CanContinue = false;
                    }

                    if (CanContinue)
                    {
                        if (!Directory.Exists(inputCapitalized))
                            Directory.CreateDirectory(Directory.GetCurrentDirectory() + @"\" + inputCapitalized);
                        else
                            clog("Directory already exists!\n", Red);
                    }
                }

                else if (input.StartsWith("mkfile"))
                {
                    try
                    {
                        inputCapitalized = inputCapitalized.Split("MKFILE ")[1];
                    }
                    catch
                    {
                        clog("No arguments\n", Red);
                        CanContinue = false;
                    }

                    if (inputCapitalized.Contains("0:\\"))
                    {
                        input.Replace("0:\\", "");
                    }

                    if (inputCapitalized.Contains(" "))
                    {
                        clog("Filename cannot contain spaces!\n", Red);
                        CanContinue = false;
                    }

                    if (CanContinue)
                    {
                        if (!File.Exists(inputCapitalized))
                            File.Create(Directory.GetCurrentDirectory() + @"\" + inputCapitalized);
                        else
                            clog("File already exists!\n", Red);
                    }
                }

                else if (input.StartsWith("deldir"))
                {
                    try
                    {
                        inputCapitalized = inputCapitalized.Split("DELDIR ")[1];
                    }
                    catch
                    {
                        clog("No arguments\n", Red);
                        CanContinue = false;
                    }

                    if (inputCapitalized.Contains("0:\\"))
                    {
                        input.Replace("0:\\", "");
                    }

                    if (inputCapitalized.Contains(" "))
                    {
                        clog("Filename cannot contain spaces!\n", Red);
                        CanContinue = false;
                    }

                    if (CanContinue)
                    {
                        if (Directory.Exists(inputCapitalized))
                            Directory.Delete(Directory.GetCurrentDirectory() + @"\" + inputCapitalized, true);
                        else
                            clog("Directory not found!\n", Red);
                    }
                }

                else if (input.StartsWith("delfile"))
                {
                    try
                    {
                        inputCapitalized = inputCapitalized.Split("DELFILE ")[1];
                    }
                    catch
                    {
                        clog("No arguments\n", Red);
                        CanContinue = false;
                    }

                    if (inputCapitalized.Contains("0:\\"))
                    {
                        input.Replace("0:\\", "");
                    }

                    if (inputCapitalized.Contains(" "))
                    {
                        clog("Filename cannot contain spaces!\n", Red);
                        CanContinue = false;
                    }

                    if (CanContinue)
                    {
                        if (File.Exists(inputCapitalized))
                            File.Delete(Directory.GetCurrentDirectory() + @"\" + inputCapitalized);
                        else
                            clog("File not found!\n", Red);
                    }
                }

                else if (input.StartsWith("cd") && disk)
                {
                    if (input == "cd..")
                    {
                        try
                        {
                            Directory.SetCurrentDirectory(Directory.GetCurrentDirectory().TrimEnd('\\')
                                .Remove(Directory.GetCurrentDirectory().LastIndexOf('\\') + 1));
                            Directory.SetCurrentDirectory(Directory.GetCurrentDirectory()
                                .Remove(Directory.GetCurrentDirectory().Length - 1));
                        }
                        catch
                        {
                        }
                    }

                    else if (input.StartsWith("cd "))
                    {
                        try
                        {
                            inputCapitalized = inputCapitalized.Split("CD ")[1];
                        }
                        catch
                        {
                            clog("No arguments\n", Red);
                            CanContinue = false;
                        }

                        if (inputCapitalized.Trim() != string.Empty) CanContinue = true;
                        if (CanContinue)
                        {
                            if (inputCapitalized.Contains(@"0:\"))
                            {
                                inputCapitalized.Replace(@"0:\", "");
                            }

                            if (Directory.GetCurrentDirectory() != rootdir)
                            {
                                inputCapitalized = @"\" + inputCapitalized;
                            }

                            if (Directory.Exists(Directory.GetCurrentDirectory() + inputCapitalized))
                                Directory.SetCurrentDirectory(Directory.GetCurrentDirectory() + inputCapitalized);
                            else clog("Directory not found!\n", Red);
                        }
                    }

                    else
                    {
                        clog("Cd subfunctions:", DarkGreen);
                        log(" cd (path) - Browses to directory");
                        log(" cd.. - Browses to last directory\n");
                    }
                }

                else if (input == "dir" && disk)
                {
                    clog("Directory listing at " + Directory.GetCurrentDirectory(), Yellow);
                    var directoryList = VFSManager.GetDirectoryListing(Directory.GetCurrentDirectory());
                    var files = 0;
                    var dirs = 0;
                    foreach (var directoryEntry in directoryList)
                    {
                        if (Directory.Exists(Directory.GetCurrentDirectory() + "\\" + directoryEntry.mName))
                            clog("<Dir> " + directoryEntry.mName, Gray);
                        dirs += 1;
                    }

                    foreach (var directoryEntry in directoryList)
                    {
                        if (File.Exists(Directory.GetCurrentDirectory() + "\\" + directoryEntry.mName))
                            clog("<File> " + directoryEntry.mName, Gray);
                        files += 1;
                    }

                    clog("\nFound " + files + " files and " + dirs + " directories.\n", Yellow);
                }

                else if (input.StartsWith("copy"))
                {
                    var potato = string.Empty;
                    var potato1 = string.Empty;
                    try
                    {
                        potato = inputBeforeLower.Split(" ")[1];
                        potato1 = inputBeforeLower.Split(" ")[2];
                    }
                    catch
                    {
                        clog("No arguments\n", Red);
                        CanContinue = false;
                    }

                    if (CanContinue)
                    {
                        var Contents = File.ReadAllText(potato);
                        File.Create(potato1);
                        File.WriteAllText(potato1, Contents);
                        clog("Copy process finished successfully!\n", Gray);
                    }
                }

                else if (input.StartsWith("lb") && disk)
                    fs.SetFileSystemLabel(rootdir, inputBeforeLower.Split("lb ")[1]);

                #endregion

                else
                {
                    //Console.Beep();
                    // I commented it out because it isn't async and freezes the os until the sound is done. -ekeleze
                    clog("Unknown command.\n", Red);
                }
            }
            catch (Exception ex)
            {
                Crash(ex);
                return; // remove if it didn't do anything
            }
        }

        public static void UpdateResolution()
        {
            Screen = Display.GetDisplay(Width, Height);
            Console = new SVGAIITerminal(Width, Height, Font_1x, FallbackTerminalUpdate);
        }

        private static void FallbackTerminalUpdate()
        {
            Screen.DrawImage(0, 0, Console.Contents, false);
            Screen.Update();
        }
    }
}