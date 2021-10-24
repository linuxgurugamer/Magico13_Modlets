using System;
using System.IO;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;

namespace DatedQuickSaves
{
    [KSPAddon(KSPAddon.Startup.FlightAndKSC, false)]
    public class DatedQuickSaves : MonoBehaviour
    {
        private List<string> SavedQSFiles = new List<string>();
        private List<string> SavedASFiles = new List<string>();

        private readonly string SaveFolder = Path.Combine(Path.GetFullPath(KSPUtil.ApplicationRootPath), "saves", HighLogic.SaveFolder) + Path.DirectorySeparatorChar;

        private Configuration config;
        Settings settings;
        //private DQSSettings settings;

        void Start()
        {
            GameEvents.OnGameSettingsApplied.Add(OnGameSettingsApplied);
            
            settings = new Settings();

            if (settings.QuickSaveEnable || settings.AutoSaveEnable)
            {
                config = new Configuration();

                GetKnownFiles();

                if (settings.AutoSaveEnable)
                {
                    var freq_sec = settings.AutoSaveFreq * 60;

                    // scale period in Physic Warp
                    if (TimeWarp.WarpMode == TimeWarp.Modes.LOW)
                        freq_sec = (int)(freq_sec * Math.Max(TimeWarp.CurrentRate, 1));

                    if (settings.AutoSaveOnStart)
                        InvokeRepeating("DoAutoSave", 1, freq_sec);
                    else
                        InvokeRepeating("DoAutoSave", freq_sec, freq_sec);
                }
            }
        }

        void OnGameSettingsApplied()
        {
            Logger.Log("OnGameSettingsApplied");

            if (config != null && HighLogic.CurrentGame.Parameters.CustomParams<DQSSettings>().ReloadExtra)
            {
                config.ReLoad();
                HighLogic.CurrentGame.Parameters.CustomParams<DQSSettings>().ReloadExtra = false;
            }

            var new_settings = new Settings();

            if (new_settings != settings)
            {

                if (settings.StockNeedUpdate(new_settings))
                {
                    GameSettings.AUTOSAVE_INTERVAL = new_settings.StockAutosaveInterval;
                    GameSettings.AUTOSAVE_SHORT_INTERVAL = new_settings.StockAutosaveShortInterval;
                    GameSettings.SaveSettings();
                }

                if (settings.NeedUpdateAndEnabling(new_settings))
                {
                    config = new Configuration();
                    GetKnownFiles();
                }

                if (settings.AutoSaveNeedUpdate(new_settings))
                {
                    CancelInvoke("DoAutoSave");

                    if (new_settings.AutoSaveEnable)
                    {
                        var freq_sec = new_settings.AutoSaveFreq * 60;

                        // preventing shrinking period in Physic Warp
                        if (TimeWarp.WarpMode == TimeWarp.Modes.LOW)
                            freq_sec = (int)(freq_sec * TimeWarp.CurrentRate);

                        if (new_settings.AutoSaveOnStart)
                            InvokeRepeating("DoAutoSave", 1, freq_sec);
                        else
                            InvokeRepeating("DoAutoSave", freq_sec, freq_sec);
                    }
                }
                settings = new_settings;
            }
        }

        public void OnDisable()
        {
            GameEvents.OnGameSettingsApplied.Remove(OnGameSettingsApplied);
        }
        bool invokeOnce = false;
        void Update()
        {
            if (settings.QuickSaveEnable)
            {
                if (GameSettings.QUICKSAVE.GetKey() && !GameSettings.MODIFIER_KEY.GetKey()) //F5 but not Alt-F5
                {
                    invokeOnce = true;
                }

                if (invokeOnce)
                {
                    Invoke("DoQuickSaveWork", settings.QuickSaveDelay);
                    invokeOnce = false;
                }
            }
        }

        void DoQuickSaveWork()
        {
            //Logger.LogColor("DoQuickSaveWork");
            
            string quicksave = SaveFolder + "quicksave";

            if (!File.Exists(quicksave + ".sfs"))
            {
                Logger.Log("No quicksave file");
                return;
            }

            string newName = MagiCore.StringTranslation.AddFormatInfo(config.quickSaveTemplate, "DatedQuickSaves", config.dateFormat);
            string fname = SaveFolder + newName;
            if (System.IO.File.Exists(fname))
            {
                int cnt = 0;
                while (File.Exists(SaveFolder + newName + "-" + cnt.ToString() + ".sfs") ||
                    File.Exists(SaveFolder + newName + "-" + cnt.ToString() + ".loadmeta"))
                    cnt++;
                newName = newName + "-" + cnt.ToString();
                fname = SaveFolder + newName;
            }
            if (settings.StockQuickSaveRename)
            {
                RunFileOperation(File.Move, quicksave, fname);
                Logger.Log($"Renamed quicksave.sfs to {newName}.sfs");
            }
            else
            {
                RunFileOperation(File.Copy, quicksave, fname);
                Logger.Log($"Copied quicksave.sfs to {newName}.sfs");
            }

            ScreenMessages.PostScreenMessage("Quicksaved to " + newName);

            SavedQSFiles.Add(newName);
            PurgeExtraneousFiles();
            SaveKnownFiles();
        }
        /// <summary>
        /// Run fileoperation for on extenshion-less src and dest for (.sfs, .loadmeta)
        /// </summary>
        /// <param name="fileoperation">copy or move</param>
        /// <param name="src">source path without extension</param>
        /// <param name="dest">destination path without extension</param>
        void RunFileOperation(Action<string, string> fileoperation, string src, string dest)
        {
            fileoperation(src + ".sfs", dest + ".sfs");
            fileoperation(src + ".loadmeta", dest + ".loadmeta");
        }

        void DoAutoSave()
        {
            string newName = MagiCore.StringTranslation.AddFormatInfo(config.autoSaveTemplate, "DatedQuickSaves", config.dateFormat);
            string relpath = Path.Combine("saves", HighLogic.SaveFolder, newName);

            Logger.LogFormat("AutoSaving started to {0}. Tracking {1} autosaves.", relpath, SavedASFiles.Count);

            GamePersistence.SaveGame(newName, HighLogic.SaveFolder, SaveMode.OVERWRITE);
            SavedASFiles.Add(newName);

            Logger.LogFormat("AutoSaved to {0}. Tracking {1} autosaves.", relpath, SavedASFiles.Count);

            ScreenMessages.PostScreenMessage("Autosaved to " + newName);

            PurgeExtraneousFiles();
            SaveKnownFiles();
        }

        void GetKnownFiles()
        {
            SavedQSFiles.Clear();
            SavedASFiles.Clear();
            string saveFolder = SaveFolder;
            if (File.Exists(saveFolder + "DQS_DataBase.cfg"))
            {
                ConfigNode database = ConfigNode.Load(saveFolder + "DQS_DataBase.cfg");
                ConfigNode QSDB, ASDB;
                if (database.HasNode("QuickSaves"))
                {
                    QSDB = database.GetNode("QuickSaves");
                    SavedQSFiles = QSDB.GetValues("file").ToList();
                }
                if (database.HasNode("AutoSaves"))
                {
                    ASDB = database.GetNode("AutoSaves");
                    SavedASFiles = ASDB.GetValues("file").ToList();
                }
            }
        }

        void SaveKnownFiles()
        {
            string saveFolder = SaveFolder;

            ConfigNode database = new ConfigNode();
            ConfigNode QSDB = new ConfigNode("QuickSaves"), ASDB = new ConfigNode("AutoSaves");

            foreach (string file in SavedQSFiles)
            {
                QSDB.AddValue("file", file);
            }
            foreach (string file in SavedASFiles)
            {
                ASDB.AddValue("file", file);
            }

            database.AddNode("QuickSaves", QSDB);
            database.AddNode("AutoSaves", ASDB);

            database.Save(saveFolder + "DQS_DataBase.cfg");
        }

        void DeleteIfExists(string fname)
        {
            if (System.IO.File.Exists(fname))
                System.IO.File.Delete(fname);
        }

        void DeleteIfExistsAllExt(string fname_no_ext)
        {
            DeleteIfExists(fname_no_ext + ".sfs");
            DeleteIfExists(fname_no_ext + ".loadmeta");

            // BetterLoadSaveGame supports
            DeleteIfExists(fname_no_ext + "-thumb.png");
        }

        void PurgeExtraneousFiles()
        {
            int tgtQS = settings.MaxQuickSaveCount;
            int tgtAS = settings.MaxAutoSaveCount;


            string saveFolder = SaveFolder;
            int purgedQS = 0, purgedAS = 0;
            if (tgtQS >= 0) //if negative, then keep all files
            {
                while (SavedQSFiles.Count > tgtQS)
                {
                    //purge oldest (top one)
                    string oldest = SavedQSFiles[0];
                    DeleteIfExistsAllExt(saveFolder + oldest);
                    SavedQSFiles.RemoveAt(0);
                    purgedQS++;
                }
            }
            if (tgtAS >= 0) //if negative, then keep all files
            {
                while (SavedASFiles.Count > tgtAS)
                {
                    //purge oldest (top one)
                    string oldest = SavedASFiles[0];
                    DeleteIfExistsAllExt(saveFolder + oldest);
                    SavedASFiles.RemoveAt(0);
                    purgedAS++;
                }
            }
            if (purgedQS > 0 || purgedAS > 0)
                Logger.Log($"Purged {purgedQS} of {SavedQSFiles.Count} QuickSaves and {purgedAS} of {SavedASFiles.Count} AutoSaves.");
        }
    }

    public class Configuration
    {
        public string dateFormat = "yyyy-MM-dd--HH-mm-ss";
        public string quickSaveTemplate = "quicksave_Y[year0]D[day0]H[hour0]M[min0]S[sec0]";
        public string autoSaveTemplate = "autosave_Y[year0]D[day0]H[hour0]M[min0]S[sec0]";

        public Configuration()
        {
            ConfigNode[] configs = GameDatabase.Instance.GetConfigNodes("DQS");

            if (configs != null && configs.Length != 0)
            {
                if (configs.Length > 1)
                    Logger.Log("More than 1 DQS node found. Loading the first one");

                ConfigNode cfg = configs[0];

                cfg.TryGetValue("DateString", ref dateFormat);
                cfg.TryGetValue("QuickSaveTemplate", ref quickSaveTemplate);
                cfg.TryGetValue("AutoSaveTemplate", ref autoSaveTemplate);
            }
        }


        public void ReLoad()
        {
            string filename = KSPUtil.ApplicationRootPath + "/GameData/DatedQuickSaves/extra_settings.cfg";

            ConfigNode cfg = ConfigNode.Load(filename).GetNode("DQS");

            cfg.TryGetValue("DateString", ref dateFormat);
            cfg.TryGetValue("QuickSaveTemplate", ref quickSaveTemplate);
            cfg.TryGetValue("AutoSaveTemplate", ref autoSaveTemplate);

            Logger.Log("Reload DQS Config");
            ScreenMessages.PostScreenMessage("Reload DQS Config");
        }
    }
    

    public class Settings
    {
        public bool QuickSaveEnable;
        public int MaxQuickSaveCount;
        public bool StockQuickSaveRename;
        public int QuickSaveDelayMS;
        public float QuickSaveDelay;

        public bool AutoSaveEnable;
        public bool AutoSaveOnStart;
        public int AutoSaveFreq;
        public int MaxAutoSaveCount;

        public int StockAutosaveInterval;
        public int StockAutosaveShortInterval;

        public Settings()
        {
            var settings = HighLogic.CurrentGame.Parameters.CustomParams<DQSSettings>();
            var settings2 = HighLogic.CurrentGame.Parameters.CustomParams<DQSSettings2>();

            QuickSaveEnable = settings.QuickSaveEnable;
            StockQuickSaveRename = settings.StockQuickSaveRename;
            MaxQuickSaveCount = settings.MaxQuickSaveCount;
            QuickSaveDelayMS = settings.QuickSaveDelayMS;
            QuickSaveDelay = QuickSaveDelayMS / 1000.0f;

            AutoSaveEnable = settings.AutoSaveEnable;
            AutoSaveFreq = settings.AutoSaveFreq;
            AutoSaveOnStart = settings.AutoSaveOnStart;
            MaxAutoSaveCount = settings.MaxAutoSaveCount;

            StockAutosaveInterval = settings2.StockAutosaveInterval * 60;
            StockAutosaveShortInterval = settings2.StockAutosaveShortInterval;
        }

        public bool StockNeedUpdate(Settings new_settings)
        {
            if (new_settings == null) return false;

            return StockAutosaveInterval != new_settings.StockAutosaveInterval
                || StockAutosaveShortInterval != new_settings.StockAutosaveShortInterval;
        }

        public bool QuickSaveNeedUpdate(Settings new_settings)
        {
            if (new_settings == null) return false;

            return QuickSaveEnable != new_settings.QuickSaveEnable;
        }

        public bool QuickSaveNeedUpdateAndEnabling(Settings new_settings)
        {
            if (new_settings == null) return false;

            return QuickSaveNeedUpdate(new_settings) 
                && new_settings.QuickSaveEnable;
        }

        public bool AutoSaveNeedUpdate(Settings new_settings)
        {
            if (new_settings == null) return false;

            return AutoSaveEnable != new_settings.AutoSaveEnable
                || AutoSaveFreq != new_settings.AutoSaveFreq
                || AutoSaveOnStart != new_settings.AutoSaveOnStart;
        }

        public bool AutoSaveNeedUpdateAndEnabling(Settings new_settings)
        {
            if (new_settings == null) return false;

            return QuickSaveNeedUpdate(new_settings)
                && new_settings.AutoSaveEnable;
        }

        public bool NeedUpdateAndEnabling(Settings new_settings)
        {
            return QuickSaveNeedUpdateAndEnabling(new_settings) 
                || AutoSaveNeedUpdateAndEnabling(new_settings);
        }
    }
    public static class Logger
    {
        public static void Log(string msg)
        {
            Debug.Log("<color=green>[DQS]</color> " + msg);
        }

        public static void LogFormat(string format, params object[] args)
        {
            Debug.LogFormat("<color=green>[DQS]</color> " + format, args);
        }
    }
}
/*
Copyright (C) 2017  Michael Marvin

    This program is free software: you can redistribute it and/or modify
    it under the terms of the GNU General Public License as published by
    the Free Software Foundation, either version 3 of the License, or
    (at your option) any later version.

    This program is distributed in the hope that it will be useful,
    but WITHOUT ANY WARRANTY; without even the implied warranty of
    MERCHANTABILITY or FITNESS FOR A PARTICULAR PURPOSE.  See the
    GNU General Public License for more details.

    You should have received a copy of the GNU General Public License
    along with this program.  If not, see <http://www.gnu.org/licenses/>.
*/
