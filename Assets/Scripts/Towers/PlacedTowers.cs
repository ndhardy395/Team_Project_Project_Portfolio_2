using UnityEngine;

// Added to a tower when the player builds it, so the Tower Manu can sell it later.
// Not put on the prefabs by hand: ClickCode adds it on build, TowerUpgrad carries it over on upgrade.
public class PlacedTower : MonoBehaviour
{
    public GameObject baseSpot;   // the base it was built on; hidden while the tower stands, shown again when it's sold
    public int spent;             // build price plus every upgrade bought, so the refund covers all of it
}