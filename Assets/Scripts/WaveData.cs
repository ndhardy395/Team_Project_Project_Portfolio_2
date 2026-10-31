[System.Serializable]
public class GameData
{
    public WaveData[] waves;
}

[System.Serializable]
public class WaveData
{
    public int waveNumber;
    public float delayBeforeWave;
    public SpawnGroup[] spawns;
}

[System.Serializable]
public class SpawnGroup
{
    public string enemyId;
    public int count;
    public float spawnInterval;
    public float delayBeforeGroup;
    public int pathID; //if you don't define it, it just uses the default. This is for multi path maps.
    //WILL require a .json file for each individual map however. So that's the path, visuals, AND the spawns that need to be loaded
}