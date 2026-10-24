using UnityEngine;
using System;

public class WaveEnemyTracker : MonoBehaviour
{

    private Action onRemoved;

    private bool reported;

    public void Initialize(Action callback)
    {
        onRemoved = callback;
        reported = false;
    }

    public void ReportRemoved()
    {
        if (reported) return;

        reported=true;
        onRemoved?.Invoke();
        onRemoved = null;
    }
 
}
