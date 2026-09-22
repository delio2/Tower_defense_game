using TowerDefense.Simulation;
using UnityEngine;

namespace TowerDefense.Presentation.Content
{
    /// <summary>Authoring data for one enemy type (GDD v0.2 §8). Integers only, see <see cref="ModuleAsset"/>.</summary>
    [CreateAssetMenu(menuName = "TowerDefense/Content/Enemy", fileName = "Enemy")]
    public sealed class EnemyAsset : ScriptableObject
    {
        [SerializeField] private EnemyKind _kind;
        [SerializeField, Min(1)] private int _hp = 20;
        [SerializeField, Min(0)] private int _armor;
        [SerializeField, Min(1)] private int _speedMilli = 900;
        [SerializeField, Min(0)] private int _contactDamage = 5;
        [SerializeField, Min(0)] private int _budgetCostMilli = 1000;
        [SerializeField, Min(1)] private int _groupSize = 1;

        public EnemyKind Kind => _kind;

        public EnemyDefinition ToDefinition()
        {
            long hp = SimConstants.HpScale;
            return new EnemyDefinition(_kind, _hp * hp, _armor * hp, _speedMilli, _contactDamage * hp, _budgetCostMilli, _groupSize);
        }

        public void CopyFrom(EnemyDefinition d)
        {
            long hp = SimConstants.HpScale;
            _kind = d.Kind;
            _hp = (int)(d.Hp / hp);
            _armor = (int)(d.Armor / hp);
            _speedMilli = d.SpeedMilli;
            _contactDamage = (int)(d.ContactDamage / hp);
            _budgetCostMilli = d.BudgetCostMilli;
            _groupSize = d.GroupSize;
        }
    }
}
