using UnityEngine;

namespace TowerDefense.Presentation
{
    /// <summary>"Dusk Garden" palette from docs/03 v2 (to be validated in the mood shot).</summary>
    public static class Palette
    {
        public static readonly Color Background = Hex(0x1E2146);
        public static readonly Color ArenaLine = Hex(0x3B3F72);
        public static readonly Color Petal = Hex(0x33376A);
        public static readonly Color PetalSelected = Hex(0x5A5FA0);
        public static readonly Color Core = Hex(0xF3E9D7);
        public static readonly Color Weapon = Hex(0x5CC8C0);
        public static readonly Color Booster = Hex(0xE9B872);
        public static readonly Color Economy = Hex(0x9AD9A1);
        public static readonly Color Enemy = Hex(0xF07A6A);
        public static readonly Color EnemyHeavy = Hex(0xC8507A);
        public static readonly Color Text = Hex(0xF3E9D7);
        public static readonly Color TextDim = new Color(0.95f, 0.91f, 0.84f, 0.6f);

        public static Color WithAlpha(Color color, float alpha)
        {
            color.a = alpha;
            return color;
        }

        private static Color Hex(int rgb)
        {
            return new Color(((rgb >> 16) & 0xFF) / 255f, ((rgb >> 8) & 0xFF) / 255f, (rgb & 0xFF) / 255f, 1f);
        }
    }
}
