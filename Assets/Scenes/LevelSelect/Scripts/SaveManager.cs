using System.Collections.Generic;
using System.IO;
using UnityEngine;

// Loads and saves the player's campaign progress, and answers questions about it
// ("how many stars on this level?", "is this level unlocked?").
// It's a static class, so nothing needs to be placed in a scene: call SaveManager.TotalStars()
// (or any other method) from any script, and it reads the save file the first time it's needed.
public static class SaveManager
{
    const string FileName = "save.json";

    static SaveData data;
    // Quick way to find a level's record by its ID. Rebuilt from data.levels every time we load.
    static Dictionary<string, LevelProgress> lookup;

    // Where the file lives. On Windows: C:\Users\<you>\AppData\LocalLow\<company>\<product>\save.json
    public static string SavePath => Path.Combine(Application.persistentDataPath, FileName);

    // This project has domain reload turned off (Project Settings > Editor > Enter Play Mode Settings),
    // so static fields would keep their old values between Play sessions. This clears them each time Play starts.
    [RuntimeInitializeOnLoadMethod(RuntimeInitializeLoadType.SubsystemRegistration)]
    static void ResetStatics()
    {
        data = null;
        lookup = null;
    }

    // ---------- Loading and saving ----------

    public static void Load()
    {
        data = null;

        if (File.Exists(SavePath))
        {
            try
            {
                data = JsonUtility.FromJson<SaveData>(File.ReadAllText(SavePath));
            }
            catch (System.Exception e)
            {
                Debug.LogWarning($"Couldn't read the save file, starting fresh. ({e.Message})");
            }
        }

        if (data == null)
            data = new SaveData();

        lookup = new Dictionary<string, LevelProgress>();
        foreach (LevelProgress record in data.levels)
            lookup[record.levelId] = record;
    }

    public static void Save()
    {
        EnsureLoaded();
        // true = pretty-print, so the file is easy to read if you open it.
        File.WriteAllText(SavePath, JsonUtility.ToJson(data, true));
    }

    // Wipes all progress. Used by the Tools menu for testing.
    public static void DeleteSave()
    {
        if (File.Exists(SavePath))
            File.Delete(SavePath);

        data = null;
        lookup = null;
    }

    // ---------- Questions about progress ----------

    public static int GetStars(LevelData level) => FindRecord(level)?.bestStars ?? 0;
    public static int GetBestWave(LevelData level) => FindRecord(level)?.bestWave ?? 0;
    public static bool IsBeaten(LevelData level) => GetStars(level) > 0;

    public static int TotalStars()
    {
        EnsureLoaded();
        int total = 0;
        foreach (LevelProgress record in data.levels)
            total += record.bestStars;
        return total;
    }

    public static int KeyCount()
    {
        EnsureLoaded();
        int keys = 0;
        foreach (LevelProgress record in data.levels)
        {
            if (record.hasKey)
                keys++;
        }
        return keys;
    }

    // A level is open when every level in its requiredLevels list is beaten
    // and the player holds at least keysRequired keys.
    public static bool IsUnlocked(LevelData level)
    {
        if (level.requiredLevels != null)
        {
            foreach (LevelData required in level.requiredLevels)
            {
                if (required != null && !IsBeaten(required))
                    return false;
            }
        }

        return KeyCount() >= level.keysRequired;
    }

    // ---------- Recording results ----------

    // Call this when the player beats a level. Only improvements are kept: beating a level
    // on Anger after already beating it on Violence doesn't take a star away.
    // Returns how many NEW stars were earned, so the victory screen can show "+1 star".
    public static int RecordWin(LevelData level, Temperament temperament, int waveReached)
    {
        LevelProgress record = GetOrCreateRecord(level);
        int stars = (int)temperament;
        int newStars = Mathf.Max(0, stars - record.bestStars);

        record.bestStars = Mathf.Max(record.bestStars, stars);
        record.bestWave = Mathf.Max(record.bestWave, waveReached);
        if (level.givesKey)
            record.hasKey = true;

        Save();
        return newStars;
    }

    // Call this when the player loses. Losing earns no stars, but a new best wave still counts.
    public static void RecordLoss(LevelData level, int waveReached)
    {
        LevelProgress record = GetOrCreateRecord(level);
        record.bestWave = Mathf.Max(record.bestWave, waveReached);
        Save();
    }

    // ---------- Helpers ----------

    static void EnsureLoaded()
    {
        if (data == null)
            Load();
    }

    // Returns null if the player has never finished this level.
    static LevelProgress FindRecord(LevelData level)
    {
        EnsureLoaded();
        lookup.TryGetValue(level.levelId, out LevelProgress record);
        return record;
    }

    static LevelProgress GetOrCreateRecord(LevelData level)
    {
        if (string.IsNullOrEmpty(level.levelId))
            Debug.LogError($"Level '{level.name}' has no levelId, so its progress can't be saved properly.", level);

        LevelProgress record = FindRecord(level);
        if (record == null)
        {
            record = new LevelProgress { levelId = level.levelId };
            data.levels.Add(record);
            lookup[record.levelId] = record;
        }
        return record;
    }


    public static int AvaliableStars() // check to see how much stars user have
    {
        EnsureLoaded();
        return Mathf.Max(0, TotalStars() - data.spentStars);
    }

    public static bool IsSpellUnlocked(string spellId)
    {
        EnsureLoaded();

        // fireball is the free starting spell.
        if (spellId == "fireball")
        {
            return true;
        }

        return data.unlockedSpells != null && data.unlockedSpells.Contains(spellId);
    }


    public static bool TryUnlockSpell(string spellId)
    {
        EnsureLoaded();

        // Prices are defined here so buttons cannot suppy the wrong cost.

        int cost;

        switch (spellId)
        {
            case "ice":
                cost = 3;
                break;

            case "lightning":
                cost = 8;
                break;

            default:
                return false;
        }

        if (IsSpellUnlocked(spellId) || AvaliableStars() < cost)
        {
            return false;
        }

        if (data.unlockedSpells == null)
        {
            data.unlockedSpells = new List<string>();
        }

        data.spentStars += cost;
        data.unlockedSpells.Add(spellId);
        Save();

        return true;
    }

    public static int GetTowerUpgrades(string towerId)
    {
        EnsureLoaded();

        switch (towerId)
        {
            case "turret": return data.turrentUpgrades;
            case "aoe": return data.aoeUpgrades;
            default: return 0;
        }

    }

    public static int GetTowerUpgradesCost(string towerId)
    {
        int upgrades = GetTowerUpgrades(towerId);

        Debug.Log($"Tower: {towerId}, saved upgrades: {upgrades}");

        // Zero means the towe has reached its maxinum level
        return upgrades >= 2 ? 0 : (upgrades + 1) * 2;
    }

    public static bool TryUpgradeTower(string towerId)
    {
        if (towerId != "turret" && towerId != "aoe")
        {
            return false;
        }

        int cost = GetTowerUpgradesCost(towerId);

        if (cost <= 0 || AvaliableStars() < cost)
        {
            return false;
        }

        data.spentStars += cost;

        if(towerId == "turret")
        {
            data.turrentUpgrades++;
        }
        else
        {
            data.aoeUpgrades++;
        }

        Save();
        return true;
    }
}
