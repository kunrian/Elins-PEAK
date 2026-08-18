using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using BepInEx.Logging;
using Newtonsoft.Json;

namespace PEAKUsageSkills.Persistence
{
    internal sealed class SaveStore
    {
        private const int MaximumBackups = 5;
        private readonly ManualLogSource log;
        private readonly string saveDirectory;
        private readonly string savePath;
        private readonly string backupDirectory;

        public SaveStore(ManualLogSource log)
        {
            this.log = log;
            saveDirectory = Path.Combine(
                Environment.GetFolderPath(Environment.SpecialFolder.LocalApplicationData),
                "LandCrab",
                "PEAK",
                "PEAKUsageSkills");
            savePath = Path.Combine(saveDirectory, "progression.json");
            backupDirectory = Path.Combine(saveDirectory, "backups");
        }

        public ProgressionSave Load()
        {
            Directory.CreateDirectory(saveDirectory);
            Directory.CreateDirectory(backupDirectory);

            if (TryLoad(savePath, out ProgressionSave? save))
            {
                log.LogInfo($"Loaded usage-skill progression from {savePath}");
                return save!;
            }

            foreach (string backup in GetBackupsNewestFirst())
            {
                if (!TryLoad(backup, out save))
                {
                    continue;
                }

                log.LogWarning($"Recovered usage-skill progression from backup {backup}");
                return save!;
            }

            log.LogInfo("No valid usage-skill save found; starting new progression.");
            return new ProgressionSave();
        }

        public void Save(ProgressionSave save)
        {
            Directory.CreateDirectory(saveDirectory);
            Directory.CreateDirectory(backupDirectory);
            save.LastSavedUtc = DateTime.UtcNow;

            string temporaryPath = savePath + ".tmp";
            string json = JsonConvert.SerializeObject(save, Formatting.Indented);
            File.WriteAllText(temporaryPath, json);

            if (File.Exists(savePath))
            {
                string backupPath = Path.Combine(
                    backupDirectory,
                    $"progression-{DateTime.UtcNow:yyyyMMdd-HHmmss-fff}.json");
                File.Replace(temporaryPath, savePath, backupPath, true);
            }
            else
            {
                File.Move(temporaryPath, savePath);
            }

            CleanupBackups();
        }

        private bool TryLoad(string path, out ProgressionSave? save)
        {
            save = null;
            if (!File.Exists(path))
            {
                return false;
            }

            try
            {
                save = JsonConvert.DeserializeObject<ProgressionSave>(File.ReadAllText(path));
                return save != null && save.SchemaVersion == 1 && save.Skills != null;
            }
            catch (Exception exception)
            {
                log.LogWarning($"Could not load progression file {path}: {exception.Message}");
                return false;
            }
        }

        private IEnumerable<string> GetBackupsNewestFirst()
        {
            if (!Directory.Exists(backupDirectory))
            {
                return Array.Empty<string>();
            }

            return Directory.GetFiles(backupDirectory, "progression-*.json")
                .OrderByDescending(File.GetLastWriteTimeUtc);
        }

        private void CleanupBackups()
        {
            string[] backups = GetBackupsNewestFirst().ToArray();
            for (int index = MaximumBackups; index < backups.Length; index++)
            {
                try
                {
                    File.Delete(backups[index]);
                }
                catch (Exception exception)
                {
                    log.LogWarning($"Could not remove old progression backup {backups[index]}: {exception.Message}");
                }
            }
        }
    }
}
