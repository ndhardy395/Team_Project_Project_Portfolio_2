using System.Collections.Generic;
using UnityEngine;

// Anything the Tower Manu can select: it shows the tower's range ring and offers its next tier.
// Gun and MineTower both are one, so TowerUpgrad doesn't care which kind of tower was clicked.
public interface ITower
{
    Transform transform { get; }        // every MonoBehaviour already has this
    GameObject UpgradePrefab { get; }   // the next tier, or null on the last tier
    int UpgradeCost { get; }
    void ShowRange(bool show);

    // For the Tower Stats window (TowerStatsPanel), which opens with the Tower Manu.
    string DisplayName { get; }         // e.g. "Turret", the same on every tier
    string Description { get; }         // one line on what the tower does
    int Level { get; }                  // 1 on the tower the build menu places, 2 after its first upgrade, ...
    void ListStats(List<(string label, string value)> stats);   // adds this tier's numbers, e.g. ("Damage", "45")
}
