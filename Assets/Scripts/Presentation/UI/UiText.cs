using TowerDefense.Simulation;

namespace TowerDefense.Presentation.UI
{
    /// <summary>
    /// Player-facing strings of the prototype. To move to keyed localization (EN + IT) in Phase 3c; ASCII/Latin-1
    /// only, device fonts lack symbols (presentation rules).
    /// </summary>
    internal static class UiText
    {
        public static string FormatDamage(long hundredths) => NumberFormat.CompactHundredths(hundredths);

        public static string DpsChange(long before, long after)
        {
            string from = NumberFormat.CompactHundredths(before);
            string to = NumberFormat.CompactHundredths(after);
            if (before <= 0)
            {
                return $"DPS {from} → {to}";
            }

            long percent = (after - before) * 100 / before;
            string sign = percent >= 0 ? "+" : string.Empty;
            return $"DPS {from} → {to} ({sign}{percent}%)";
        }

        public static string Describe(ModuleKind kind)
        {
            return kind switch
            {
                ModuleKind.Emitter => "Shoots the enemy closest to the core",
                ModuleKind.Scatter => "Hits 3 enemies at once",
                ModuleKind.Amplifier => "Neighbours deal x1.5 damage",
                ModuleKind.Lens => "Neighbours: +1.5 range, +2 damage",
                ModuleKind.Overclock => "Neighbours fire 25% faster",
                ModuleKind.Arc => "Chains over 4 enemies, -10% per jump",
                ModuleKind.Lance => "Pierces every enemy on a line",
                ModuleKind.Mortar => "Explodes on the farthest enemy",
                ModuleKind.Echo => "Neighbour hits echo at 50%",
                ModuleKind.Bank => "+1 interest cap, +1 credit per wave",
                ModuleKind.Salvage => "+1 credit per 10 kills",
                ModuleKind.Bulwark => "+25 integrity, repairs 5 per wave",
                ModuleKind.Frost => "Enemies near the core move 25% slower",
                ModuleKind.Capacitor => "Pulse: -20% cooldown, +50% damage",
                _ => kind.ToString(),
            };
        }

        /// <summary>Title of a module's tooltip: what it is and how far it has been levelled.</summary>
        public static string ModuleTitle(ModuleInstance module)
        {
            return module.Level > 1 ? $"{module.Kind} L{module.Level}" : module.Kind.ToString();
        }

        /// <summary>
        /// What a module is doing right now (docs/09 §2.2): its damage after neighbours, how often it fires, and how
        /// much health it has taken off this wave — the number that answers "is this one pulling its weight?".
        /// </summary>
        public static string ModuleTooltip(ModuleInstance module)
        {
            if (module.Category != ModuleCategory.Weapon)
            {
                return Describe(module.Kind);
            }

            string damage = NumberFormat.CompactHundredths(module.EffectiveDamage);
            string every = (module.EffectiveCooldown / (float)SimConstants.TicksPerSecond).ToString("0.00");
            string dealt = NumberFormat.CompactHundredths(module.DamageThisWave);
            return $"{damage} every {every} s\nthis wave: {dealt}";
        }

        /// <summary>
        /// The module sheet of the shop (docs/09 §2.3): what it does, what it is worth back, and — for a booster —
        /// which neighbours it is actually lifting right now, which is the whole point of the ring.
        /// </summary>
        public static string ModuleSheet(ModuleInstance module, Ring ring, int sellValue)
        {
            string head = module.Category == ModuleCategory.Weapon
                ? $"{NumberFormat.CompactHundredths(module.EffectiveDamage)} every {(module.EffectiveCooldown / (float)SimConstants.TicksPerSecond):0.00} s"
                : Describe(module.Kind);

            string neighbours = string.Empty;
            if (module.Category == ModuleCategory.Booster)
            {
                ModuleInstance left = ring.At(ring.LeftOf(module.Slot));
                ModuleInstance right = ring.At(ring.RightOf(module.Slot));
                string lifted = Join(left, right);
                neighbours = lifted.Length > 0 ? $"\nlifting {lifted}" : "\nno neighbours to lift";
            }

            return $"{head}\nsells back for {sellValue}{neighbours}";
        }

        private static string Join(ModuleInstance left, ModuleInstance right)
        {
            if (left != null && right != null)
            {
                return $"{left.Kind} and {right.Kind}";
            }

            return left?.Kind.ToString() ?? right?.Kind.ToString() ?? string.Empty;
        }

        /// <summary>An offer card held down: what buying it would give and what it costs.</summary>
        public static string OfferSheet(ModuleDefinition definition)
        {
            return $"{Describe(definition.Kind)}\ncosts {definition.Cost}";
        }

        public static string EnemyTitle(Enemy enemy) => enemy.IsElite ? $"{enemy.Kind} (elite)" : enemy.Kind.ToString();

        /// <summary>The enemy card: health left, what it shrugs off, and how fast it closes in.</summary>
        public static string EnemyCard(Enemy enemy)
        {
            string hp = NumberFormat.CompactHundredths(enemy.Hp);
            string maxHp = NumberFormat.CompactHundredths(enemy.MaxHp);
            string speed = (enemy.Definition.SpeedMilli / 1000f).ToString("0.0");
            string armour = enemy.Armor > 0 ? $"\narmour {NumberFormat.CompactHundredths(enemy.Armor)}" : string.Empty;
            return $"{hp} / {maxHp}\n{speed} units per second{armour}";
        }

        public static string DescribeRejection(CommandResult result)
        {
            return result switch
            {
                CommandResult.NotEnoughCredits => "Not enough credits",
                CommandResult.SlotOccupied => "That slot is taken",
                CommandResult.PulseNotReady => "Pulse is recharging",
                CommandResult.OfferAlreadyBought => "Already bought",
                CommandResult.InvalidSlot => "Pick a slot on the ring",
                _ => result.ToString(),
            };
        }
    }
}
