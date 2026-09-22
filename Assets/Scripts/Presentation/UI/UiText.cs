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
