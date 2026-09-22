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
        [SerializeField] private WeaponBehaviour _behaviour = WeaponBehaviour.Nearest;
        [SerializeField, Min(0)] private int _chainCount;
        [SerializeField, Min(0)] private int _chainRangeMilli;
        [SerializeField] private int _chainFalloffPermille = 1000;
        [SerializeField, Min(0)] private int _pierceWidthMilli;
        [SerializeField, Min(0)] private int _splashRadiusMilli;
        [SerializeField, Min(0)] private int _minRangeMilli;

        [Header("Booster (both neighbours)")]
        [SerializeField] private int _damageMultiplierBonusPermille;
        [SerializeField] private int _flatDamageBonus;
        [SerializeField] private int _rangeBonusMilli;
        [SerializeField] private int _cooldownReductionPermille;
        [SerializeField] private int _echoPermille;

        [Header("Economy and utility")]
        [SerializeField] private int _interestCapBonus;
        [SerializeField] private int _creditsPerWave;
        [SerializeField] private int _maxIntegrityBonus;
        [SerializeField] private int _repairPerWave;
        [SerializeField, Min(0)] private int _killsPerCredit;
        [SerializeField] private int _slowPermille;
        [SerializeField, Min(0)] private int _slowRadiusMilli;
        [SerializeField] private int _pulseCooldownReductionPermille;
        [SerializeField] private int _pulseDamageBonusPermille;

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
                Behaviour = _behaviour,
                ChainCount = _chainCount,
                ChainRangeMilli = _chainRangeMilli,
                ChainFalloffPermille = _chainFalloffPermille,
                PierceWidthMilli = _pierceWidthMilli,
                SplashRadiusMilli = _splashRadiusMilli,
                MinRangeMilli = _minRangeMilli,
                DamageMultiplierBonusPermille = _damageMultiplierBonusPermille,
                FlatDamageBonus = _flatDamageBonus * hp,
                RangeBonusMilli = _rangeBonusMilli,
                CooldownReductionPermille = _cooldownReductionPermille,
                EchoPermille = _echoPermille,
                InterestCapBonus = _interestCapBonus,
                CreditsPerWave = _creditsPerWave,
                MaxIntegrityBonus = _maxIntegrityBonus * hp,
                RepairPerWave = _repairPerWave * hp,
                KillsPerCredit = _killsPerCredit,
                SlowPermille = _slowPermille,
                SlowRadiusMilli = _slowRadiusMilli,
                PulseCooldownReductionPermille = _pulseCooldownReductionPermille,
                PulseDamageBonusPermille = _pulseDamageBonusPermille,
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
            _behaviour = d.Behaviour;
            _chainCount = d.ChainCount;
            _chainRangeMilli = d.ChainRangeMilli;
            _chainFalloffPermille = d.ChainFalloffPermille;
            _pierceWidthMilli = d.PierceWidthMilli;
            _splashRadiusMilli = d.SplashRadiusMilli;
            _minRangeMilli = d.MinRangeMilli;
            _damageMultiplierBonusPermille = d.DamageMultiplierBonusPermille;
            _flatDamageBonus = (int)(d.FlatDamageBonus / hp);
            _rangeBonusMilli = d.RangeBonusMilli;
            _cooldownReductionPermille = d.CooldownReductionPermille;
            _echoPermille = d.EchoPermille;
            _interestCapBonus = d.InterestCapBonus;
            _creditsPerWave = d.CreditsPerWave;
            _maxIntegrityBonus = (int)(d.MaxIntegrityBonus / hp);
            _repairPerWave = (int)(d.RepairPerWave / hp);
            _killsPerCredit = d.KillsPerCredit;
            _slowPermille = d.SlowPermille;
            _slowRadiusMilli = d.SlowRadiusMilli;
            _pulseCooldownReductionPermille = d.PulseCooldownReductionPermille;
            _pulseDamageBonusPermille = d.PulseDamageBonusPermille;
        }
    }
}
