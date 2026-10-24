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
}