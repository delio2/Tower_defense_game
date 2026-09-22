using UnityEngine;

namespace TowerDefense.Presentation
{
    /// <summary>Colours of one three-surface material (docs/03 A3): lit body, fresnel rim, low emissive inside.</summary>
    public struct SurfaceStyle
    {
        public Color Body;
        public Color Rim;
        public Color Inside;
        public float RimStrength;
        public float InsideStrength;

        public SurfaceStyle(Color body, Color rim, Color inside, float rimStrength, float insideStrength)
        {
            Body = body;
            Rim = rim;
            Inside = inside;
            RimStrength = rimStrength;
            InsideStrength = insideStrength;
        }
    }

    /// <summary>Sky of one act (design-system themes dusk / twilight / night): floor gradient, key light, ambient.</summary>
    public struct SkyTheme
    {
        public Color Far;
        public Color Mid;
        public Color Near;
        public Color Glow;
        public Color Key;
        public float KeyIntensity;
        public Color AmbientSky;
        public Color AmbientGround;
        public Color Ring;
    }

    /// <summary>
    /// "Dusk Garden" palette v2 — mirrors the design system's tokens.json (docs/03 A4, docs/09 §5). Enemy hues
    /// (coral, rose) never appear on the Core, modules or UI; allies use <see cref="Ally"/>.
    /// </summary>
    public static class Palette
    {
        // Tokens (sRGB hex, as in tokens.json).
        public static readonly Color Background = Hex(0x0E1030); // sky-near, Act I
        public static readonly Color ArenaLine = Hex(0x8C8FD0);  // range-band rings, drawn at 15–20%
        public static readonly Color Petal = Hex(0x6A6FB0);
        public static readonly Color PetalSelected = Hex(0x8F94D8);
        public static readonly Color Core = Hex(0xF3E9D7);
        public static readonly Color Weapon = Hex(0x5CC8C0);
        public static readonly Color Booster = Hex(0xE9B872);
        public static readonly Color Economy = Hex(0x9AD9A1);
        public static readonly Color Enemy = Hex(0xF07A6A);
        public static readonly Color EnemyHeavy = Hex(0xE8709A); // rose: elites and Guardians
        public static readonly Color Ally = Hex(0xC9C2FF);
        public static readonly Color Glow = Hex(0xFFD9A8);
        public static readonly Color Text = Hex(0xF3E9D7);
        public static readonly Color TextDim = new Color(0.95f, 0.91f, 0.84f, 0.6f);

        // Three-surface styles (mood shot v1 values, docs/03 A9).
        public static readonly SurfaceStyle CoreSurface = new SurfaceStyle(Core, Hex(0xFFE7C2), Glow, 0.9f, 0.35f);
        public static readonly SurfaceStyle PetalSurface = new SurfaceStyle(Petal, Hex(0xB8BCF0), Hex(0x3B3F72), 1.1f, 0.18f);
        public static readonly SurfaceStyle PetalLitSurface = new SurfaceStyle(PetalSelected, Core, Hex(0x4A4E86), 1.4f, 0.25f);
        public static readonly SurfaceStyle WeaponSurface = new SurfaceStyle(Weapon, Hex(0xBFF3EE), Weapon, 1.2f, 0.3f);
        public static readonly SurfaceStyle BoosterSurface = new SurfaceStyle(Booster, Hex(0xFFE6B8), Booster, 1.2f, 0.3f);
        public static readonly SurfaceStyle EconomySurface = new SurfaceStyle(Economy, Hex(0xDDF7E0), Economy, 1.0f, 0.25f);
        public static readonly SurfaceStyle EnemySurface = new SurfaceStyle(Enemy, Hex(0xFFB9A8), Enemy, 1.2f, 0.35f);
        public static readonly SurfaceStyle EnemyHeavySurface = new SurfaceStyle(EnemyHeavy, Hex(0xFF9CC0), EnemyHeavy, 1.3f, 0.45f);

        /// <summary>The glowing inside of a model (Arc's ring, Echo's inner ring, Splitter's crack): emissive above the bloom threshold.</summary>
        public static readonly SurfaceStyle AccentSurface = new SurfaceStyle(Glow, Glow, Glow, 0.4f, 1.1f);

        /// <summary>Act skies: 0 = Act I Dusk, 1 = Act II Twilight, 2 = Act III Night (enemy contrast checked per theme).</summary>
        public static readonly SkyTheme[] Acts =
        {
            new SkyTheme
            {
                Far = Hex(0x3B3F7A), Mid = Hex(0x1C2050), Near = Hex(0x0E1030), Glow = Hex(0x2E3478),
                Key = Hex(0xFFE2BC), KeyIntensity = 1.15f, AmbientSky = Hex(0x5A5E9A), AmbientGround = Hex(0x1A1C3E), Ring = ArenaLine,
            },
            new SkyTheme
            {
                Far = Hex(0x224A60), Mid = Hex(0x12324A), Near = Hex(0x081A26), Glow = Hex(0x1F4E66),
                Key = Hex(0xE8F0FF), KeyIntensity = 1.05f, AmbientSky = Hex(0x4A7A90), AmbientGround = Hex(0x10222E), Ring = Hex(0x7FB0C0),
            },
            new SkyTheme
            {
                Far = Hex(0x27265A), Mid = Hex(0x10123A), Near = Hex(0x06071A), Glow = Hex(0x1E1F55),
                Key = Hex(0xC9D4FF), KeyIntensity = 0.95f, AmbientSky = Hex(0x4A4C88), AmbientGround = Hex(0x0E0F2A), Ring = Hex(0x8A8CD0),
            },
        };

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
