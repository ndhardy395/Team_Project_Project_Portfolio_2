using UnityEngine;

// Anything the Tower Manu can select: it shows the tower's range ring and offers its next tier.
// Gun and MineTower both are one, so TowerUpgrad doesn't care which kind of tower was clicked.
public interface ITower
{
    Transform transform { get; }        // every MonoBehaviour already has this
    GameObject UpgradePrefab { get; }   // the next tier, or null on the last tier
    int UpgradeCost { get; }
    void ShowRange(bool show);
}
