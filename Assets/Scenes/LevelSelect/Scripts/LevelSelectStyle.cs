using UnityEngine;
using UnityEngine.UI;

// The level select's colors, copied from the mockup, plus a couple of helpers every level select script uses.
// Change a color here and every node, trail and popup picks it up.
public static class LevelSelectStyle
{
    public static readonly Color Background = Hex("#0B1119");
    public static readonly Color Text = Hex("#EEF2F6");
    public static readonly Color TextMuted = Hex("#9FB0C3");

    public static readonly Color Gold = Hex("#F2B33D");
    public static readonly Color OnGold = Hex("#1B1405");      // text and icons sitting on gold
    public static readonly Color Star = Hex("#FFCB45");
    public static readonly Color StarEmpty = Hex("#2B3A4C");
    public static readonly Color Pink = Hex("#FF3D7F");         // the 4th star, only for Bloodlust

    public static readonly Color NodeFill = Hex("#1B2838");
    public static readonly Color LockedFill = Hex("#121B26");
    public static readonly Color LockedBorder = Hex("#2B3A4C");
    public static readonly Color LockedIcon = Hex("#6A7D94");
    public static readonly Color BossBorder = Hex("#3B4E66");
    public static readonly Color BossIcon = Hex("#8597AD");

    public static readonly Color Key = Hex("#5CC8FF");
    public static readonly Color KeyFill = Hex("#10283B");
    public static readonly Color KeyLockedBorder = Hex("#2F5670");
    public static readonly Color KeyLockedIcon = Hex("#4F7690");

    public static Color Hex(string hex)
    {
        ColorUtility.TryParseHtmlString(hex, out Color color);
        return color;
    }

    public static Color TemperamentColor(Temperament temperament)
    {
        switch (temperament)
        {
            case Temperament.Annoyance: return Hex("#7FD6A0");
            case Temperament.Anger: return Hex("#FF9A3D");
            case Temperament.Violence: return Hex("#FF5A4E");
            default: return Pink;
        }
    }

    public static string Difficulty(Temperament temperament)
    {
        switch (temperament)
        {
            case Temperament.Annoyance: return "Easy";
            case Temperament.Anger: return "Medium";
            case Temperament.Violence: return "Hard";
            default: return "Super hard";
        }
    }

    // Colors a row of 4 star images for a star count (0-4).
    // The first three are gold, the last is the pink Bloodlust star, drawn as an outline until it's earned.
    public static void PaintStars(Image[] stars, int count, Sprite filled, Sprite outline)
    {
        for (int i = 0; i < stars.Length; i++)
        {
            bool earned = i < count;
            if (i == 3)
            {
                stars[i].sprite = earned ? filled : outline;
                stars[i].color = Pink;
            }
            else
            {
                stars[i].sprite = filled;
                stars[i].color = earned ? Star : StarEmpty;
            }
        }
    }
}
