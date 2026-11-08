using UnityEditor;
using UnityEngine;

// Editor-only testing tools for the save system (anything in a folder named "Editor" never goes into the game build).
// Adds a Tools > Skyline Rush > Save Data menu, plus right-click options on Level Data assets,
// so you can fake wins and check unlocks without playing through a level.
public static class SaveDataMenu
{
    const string ToolsMenu = "Tools/Skyline Rush/Save Data/";
    const string AssetMenu = "Assets/Skyline Rush/";

    [MenuItem(ToolsMenu + "Print Progress")]
    static void PrintProgress()
    {
        // Load from disk so this shows what's really in the file.
        SaveManager.Load();

        string report = $"Total stars: {SaveManager.TotalStars()}   Keys: {SaveManager.KeyCount()}\n";
        foreach (LevelData level in FindAllLevels())
        {
            string state = SaveManager.IsUnlocked(level) ? "unlocked" : "LOCKED";
            report += $"{level.displayName} ({level.levelId}): {state}, stars {SaveManager.GetStars(level)}, best wave {SaveManager.GetBestWave(level)}\n";
        }
        Debug.Log(report);
    }

    [MenuItem(ToolsMenu + "Open Save Folder")]
    static void OpenSaveFolder()
    {
        EditorUtility.RevealInFinder(Application.persistentDataPath);
    }

    [MenuItem(ToolsMenu + "Delete Save File")]
    static void DeleteSaveFile()
    {
        if (EditorUtility.DisplayDialog("Delete save file?", "This wipes all stars, best waves and keys.", "Delete", "Cancel"))
        {
            SaveManager.DeleteSave();
            Debug.Log("Save file deleted.");
        }
    }

    // ---------- Right-click a Level Data asset to fake a result ----------

    [MenuItem(AssetMenu + "Test: Win on Annoyance")]
    static void WinAnnoyance() => FakeWin(Temperament.Annoyance);

    [MenuItem(AssetMenu + "Test: Win on Anger")]
    static void WinAnger() => FakeWin(Temperament.Anger);

    [MenuItem(AssetMenu + "Test: Win on Violence")]
    static void WinViolence() => FakeWin(Temperament.Violence);

    [MenuItem(AssetMenu + "Test: Win on Bloodlust")]
    static void WinBloodlust() => FakeWin(Temperament.Bloodlust);

    [MenuItem(AssetMenu + "Test: Lose on Wave 20")]
    static void LoseOnWave20()
    {
        LevelData level = Selection.activeObject as LevelData;
        SaveManager.RecordLoss(level, 20);
        Debug.Log($"Recorded a loss on wave 20 for {level.displayName}.");
    }

    // A "validate" function decides whether a menu item is clickable. All five test items above
    // share this path prefix, so each needs its own validate method pointing here.
    [MenuItem(AssetMenu + "Test: Win on Annoyance", true)]
    [MenuItem(AssetMenu + "Test: Win on Anger", true)]
    [MenuItem(AssetMenu + "Test: Win on Violence", true)]
    [MenuItem(AssetMenu + "Test: Win on Bloodlust", true)]
    [MenuItem(AssetMenu + "Test: Lose on Wave 20", true)]
    static bool IsLevelSelected() => Selection.activeObject is LevelData;

    static void FakeWin(Temperament temperament)
    {
        LevelData level = Selection.activeObject as LevelData;
        int newStars = SaveManager.RecordWin(level, temperament, 20);
        Debug.Log($"Recorded a win on {temperament} for {level.displayName}: +{newStars} new star(s).");
    }

    static LevelData[] FindAllLevels()
    {
        // FindAssets returns GUIDs (each asset's permanent ID), so each one is turned into a path, then loaded.
        string[] guids = AssetDatabase.FindAssets("t:LevelData");
        LevelData[] levels = new LevelData[guids.Length];
        for (int i = 0; i < guids.Length; ++i)
            levels[i] = AssetDatabase.LoadAssetAtPath<LevelData>(AssetDatabase.GUIDToAssetPath(guids[i]));
        return levels;
    }
}
