using UnityEngine;

// Remembers which level the player started from the level select, and on which temperament,
// so the win/lose screens know what to write into the save file.
// Static like SaveManager: nothing to place in a scene.
public static class LevelSession
{
    // Null when a level was started some other way (for example by pressing Play inside Path Scene_Mikey).
    public static LevelData Level { get; private set; }
    public static Temperament Temperament { get; private set; } = Temperament.Annoyance;

    // Domain reload is off in this project, so statics have to be cleared by hand each time Play starts.
    [RuntimeInitializeOnLoadMethod(RuntimeInitializeLoadType.SubsystemRegistration)]
    static void ResetStatics()
    {
        Level = null;
        Temperament = Temperament.Annoyance;
    }

    public static void Begin(LevelData level, Temperament temperament)
    {
        Level = level;
        Temperament = temperament;
    }

    // Returns how many new stars the win earned (0 if the level wasn't started from the level select).
    public static int RecordWin(int waveReached)
    {
        if (Level == null)
            return 0;
        return SaveManager.RecordWin(Level, Temperament, waveReached);
    }

    public static void RecordLoss(int waveReached)
    {
        if (Level != null)
            SaveManager.RecordLoss(Level, waveReached);
    }
}
