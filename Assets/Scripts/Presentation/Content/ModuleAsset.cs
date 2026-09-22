using TowerDefense.Simulation;
using UnityEngine;

namespace TowerDefense.Presentation.Content
{
    /// <summary>
    /// Authoring data for one module (GDD v0.2 §6). Integers only: whole HP for damage and integrity, ticks for
    /// cooldowns, milli-units for ranges, permille for ratios. Converted to <see cref="ModuleDefinition"/> by
    /// <see cref="ContentLoader"/>; no floats, so loading is deterministic.
    /// </summary>
    [CreateAssetMenu(menuName = "TowerDefense/Content/Module", fileName = "Module")]
    public sealed class ModuleAsset : ScriptableObject
    {
        [SerializeField] private ModuleKind _kind;
        [SerializeField] private ModuleCategory _category;
        [SerializeField] private Rarity _rarity;
        [SerializeField, Min(0)] private int _cost = 3;
        [SerializeField] private bool _inShop = true;

        [Header("Weapon")]
        [SerializeField, Min(0)] private int _damage;
        [SerializeField, Min(0)] private int _cooldownTicks;
        [SerializeField, Min(0)] private int _rangeMilli;
        [SerializeField, Min(1)] private int _targetCount = 1;

        [Header("Booster (both neighbours)")]
        [SerializeField] private int _damageMultiplierBonusPermille;
        [SerializeField] private int _flatDamageBonus;
        [SerializeField] private int _rangeBonusMilli;
        [SerializeField] private int _cooldownReductionPermille;

        [Header("Economy and utility")]
        [SerializeField] private int _interestCapBonus;
        [SerializeField] private int _creditsPerWave;
        [SerializeField] private int _maxIntegrityBonus;
        [SerializeField] private int _repairPerWave;

        public ModuleKind Kind => _kind;
        public bool InShop => _inShop;

        public ModuleDefinition ToDefinition()
        {
            long hp = SimConstants.HpScale;
            return new ModuleDefinition(_kind, _category, _rarity, _cost)
            {
                Damage = _damage * hp,
                CooldownTicks = _cooldownTicks,
                RangeMilli = _rangeMilli,
                TargetCount = _targetCount,
                DamageMultiplierBonusPermille = _damageMultiplierBonusPermille,
                FlatDamageBonus = _flatDamageBonus * hp,
                RangeBonusMilli = _rangeBonusMilli,
                CooldownReductionPermille = _cooldownReductionPermille,
                InterestCapBonus = _interestCapBonus,
                CreditsPerWave = _creditsPerWave,
                MaxIntegrityBonus = _maxIntegrityBonus * hp,
                RepairPerWave = _repairPerWave * hp,
            };
        }

        /// <summary>Fills the asset from a definition (used to generate assets from the code defaults).</summary>
        public void CopyFrom(ModuleDefinition d, bool inShop)
        {
            long hp = SimConstants.HpScale;
            _kind = d.Kind;
            _category = d.Category;
            _rarity = d.Rarity;
            _cost = d.Cost;
            _inShop = inShop;
            _damage = (int)(d.Damage / hp);
            _cooldownTicks = d.CooldownTicks;
            _rangeMilli = d.RangeMilli;
            _targetCount = d.TargetCount;
            _damageMultiplierBonusPermille = d.DamageMultiplierBonusPermille;
            _flatDamageBonus = (int)(d.FlatDamageBonus / hp);
            _rangeBonusMilli = d.RangeBonusMilli;
            _cooldownReductionPermille = d.CooldownReductionPermille;
            _interestCapBonus = d.InterestCapBonus;
            _creditsPerWave = d.CreditsPerWave;
            _maxIntegrityBonus = (int)(d.MaxIntegrityBonus / hp);
            _repairPerWave = (int)(d.RepairPerWave / hp);
        }
    }
}
