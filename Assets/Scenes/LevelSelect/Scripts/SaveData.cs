using System;
using System.Collections.Generic;

// These two classes are exactly what gets written to save.json.
// [Serializable] is what lets Unity's JsonUtility turn them into text and back.

// What the player has done on one level.
[Serializable]
public class LevelProgress
{
    public string levelId;
    public int bestStars;   // 0 = never beaten, otherwise the stars from the hardest temperament cleared (1-4)
    public int bestWave;    // highest wave reached on this level, win or lose
    public bool hasKey;     // true once this level has been beaten, if it's a key level
}

// The whole save file.
[Serializable]
public class SaveData
{
    public int version = 1;   // bump this if the save format ever changes, so old files can be upgraded
    public List<LevelProgress> levels = new List<LevelProgress>();
}
