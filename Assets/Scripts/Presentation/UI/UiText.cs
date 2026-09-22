using TowerDefense.Simulation;

namespace TowerDefense.Presentation.UI
{
    /// <summary>
    /// Player-facing sentences built from simulation state. Every word comes from the string table through
    /// <see cref="Loc"/> (EN + IT); this class only chooses which key and fills the numbers in.
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
                return Loc.T("shop.dps", from, to);
            }

            long percent = (after - before) * 100 / before;
            string sign = percent >= 0 ? "+" : string.Empty;
            return Loc.T("shop.dps_change", from, to, sign + percent);
        }

        public static string Describe(ModuleKind kind) => Loc.T("module." + kind + ".desc");

        private static string Seconds(int ticks) => (ticks / (float)SimConstants.TicksPerSecond).ToString("0.00", System.Globalization.CultureInfo.InvariantCulture);

        /// <summary>Title of a module's tooltip: what it is and how far it has been levelled.</summary>
        public static string ModuleTitle(ModuleInstance module)
        {
            string name = Loc.ModuleName(module.Kind);
            return module.Level > 1 ? Loc.T("inspect.title_level", name, module.Level) : name;
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

            return Loc.T("inspect.weapon", NumberFormat.CompactHundredths(module.EffectiveDamage),
                Seconds(module.EffectiveCooldown), NumberFormat.CompactHundredths(module.DamageThisWave));
        }

        /// <summary>
        /// The module sheet of the shop (docs/09 §2.3): what it does, what it is worth back, and — for a booster —
        /// which neighbours it is actually lifting right now, which is the whole point of the ring.
        /// </summary>
        public static string ModuleSheet(ModuleInstance module, Ring ring, int sellValue)
        {
            string head = module.Category == ModuleCategory.Weapon
                ? Loc.T("inspect.weapon_head", NumberFormat.CompactHundredths(module.EffectiveDamage), Seconds(module.EffectiveCooldown))
                : Describe(module.Kind);

            string neighbours = string.Empty;
            if (module.Category == ModuleCategory.Booster)
            {
                ModuleInstance left = ring.At(ring.LeftOf(module.Slot));
                ModuleInstance right = ring.At(ring.RightOf(module.Slot));
                string lifted = Join(left, right);
                neighbours = lifted.Length > 0 ? Loc.T("inspect.lifting", lifted) : Loc.T("inspect.no_neighbours");
            }

            return Loc.T("inspect.sheet", head, sellValue, neighbours);
        }

        private static string Join(ModuleInstance left, ModuleInstance right)
        {
            if (left != null && right != null)
            {
                return Loc.T("inspect.and", Loc.ModuleName(left.Kind), Loc.ModuleName(right.Kind));
            }

            ModuleInstance only = left ?? right;
            return only != null ? Loc.ModuleName(only.Kind) : string.Empty;
        }

        /// <summary>An offer card held down: what buying it would give and what it costs.</summary>
        public static string OfferSheet(ModuleDefinition definition) => Loc.T("inspect.offer", Describe(definition.Kind), definition.Cost);

        public static string EnemyTitle(Enemy enemy)
        {
            string name = Loc.EnemyName(enemy.Kind);
            return enemy.IsElite ? Loc.T("inspect.elite", name) : name;
        }

        /// <summary>The enemy card: health left, what it shrugs off, and how fast it closes in.</summary>
        public static string EnemyCard(Enemy enemy)
        {
            string hp = NumberFormat.CompactHundredths(enemy.Hp);
            string maxHp = NumberFormat.CompactHundredths(enemy.MaxHp);
            string speed = (enemy.Definition.SpeedMilli / 1000f).ToString("0.0", System.Globalization.CultureInfo.InvariantCulture);
            string armour = enemy.Armor > 0 ? Loc.T("inspect.armour", NumberFormat.CompactHundredths(enemy.Armor)) : string.Empty;
            return Loc.T("inspect.enemy", hp, maxHp, speed, armour);
        }

        public static string DescribeRejection(CommandResult result)
        {
            return result switch
            {
                CommandResult.NotEnoughCredits => Loc.T("reject.credits"),
                CommandResult.SlotOccupied => Loc.T("reject.slot_taken"),
                CommandResult.PulseNotReady => Loc.T("reject.pulse"),
                CommandResult.OfferAlreadyBought => Loc.T("reject.bought"),
                CommandResult.InvalidSlot => Loc.T("reject.slot"),
                _ => result.ToString(),
            };
        }
    }
}
