using UnityEngine;

// Describes one level: its name, which scene to load, and what unlocks it.
// Make one per level with Right-click > Create > Skyline Rush > Level Data.
// This never changes while playing. What the player has DONE on a level lives in the save file (see SaveManager).
[CreateAssetMenu(fileName = "NewLevel", menuName = "Skyline Rush/Level Data")]
public class LevelData : ScriptableObject
{
    [Header("Identity")]
    [Tooltip("The save file finds this level by this ID. Keep it unique, and don't change it once people have save files.")]
    public string levelId = "level_01";
    public string displayName = "New Level";
    [Tooltip("Exact scene name. The scene also has to be in the scene list under File > Build Profiles.")]
    public string sceneName;
    [Tooltip("Picture shown in the level details popup.")]
    public Sprite previewImage;

    [Header("Unlocking")]
    [Tooltip("Every level in this list must be beaten (on any temperament) before this one opens. Leave empty for the first level.")]
    public LevelData[] requiredLevels;
    [Tooltip("How many keys the player needs. Only the boss level uses this.")]
    public int keysRequired = 0;
    [Tooltip("Beating this level gives the player a key.")]
    public bool givesKey = false;
}
