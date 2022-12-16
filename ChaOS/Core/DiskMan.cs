﻿using System;
using System.IO;
using Cosmos.System.FileSystem;
using Cosmos.System.FileSystem.VFS;
using static ChaOS.Core;
using static System.ConsoleColor;

namespace ChaOS {
    public class DiskManager {
        public const string systempath = @"0:\SYSTEM";
        public const string rootdir = @"0:\";
        public static bool disk = true;

        public class Files {
            public const string userfile = @"0:\SYSTEM\USERFILE.SYS";
            public const string colorfile = @"0:\SYSTEM\COLOR.SYS";
        };

        public static void InitFS(CosmosVFS fs) {
            VFSManager.RegisterVFS(fs); write(".");
            try { Directory.GetFiles(rootdir); }
            catch { disk = false; } write(".");
            if (disk) LoadSettings();
            else Console.Beep();
        }

        private static void LoadSettings() {
            if (Directory.Exists(systempath)) {
                if (File.Exists(Files.userfile)) Kernel.username = File.ReadAllText(Files.userfile); write(".");
                if (File.Exists(Files.colorfile)) SetScreenColor((ConsoleColor)File.ReadAllBytes(Files.colorfile)[0], (ConsoleColor)File.ReadAllBytes(Files.colorfile)[1], false); write(".");
            }
        }

        public static void SaveChangesToDisk() {
            clog("Writing changes to disk...", Gray);
            Directory.CreateDirectory(systempath);
            File.WriteAllText(Files.userfile, Kernel.username);
            File.WriteAllBytes(Files.colorfile, new byte[]{ (byte)Console.BackgroundColor, (byte)Console.ForegroundColor });
        }
    }
}
