using NUnit.Framework;
using TowerDefense.Editor;
using TowerDefense.Presentation.Content;
using UnityEngine;

namespace TowerDefense.Simulation.Tests
{
    /// <summary>
    /// The ScriptableObject content must be a lossless mirror of the code defaults: same numbers, same shop pool
    /// order, same simulation hash on the same seed (GDD v0.2 §16).
    /// </summary>
    public class ContentTests
    {
        private const ulong Seed = 7;

        private static ulong HashAfterBotRun(ContentDatabase content)
        {
            return BalanceBot.Play(new RunConfig { Seed = Seed }, content, BotStrategy.MaxDps).FinalHash;
        }

        [Test]
        public void Catalog_RoundTripIsLossless()
        {
            ContentDatabase defaults = ContentDatabase.CreatePrototypeDefaults();
            ContentCatalog catalog = ContentLoader.CreateCatalogFrom(defaults);
            ContentDatabase loaded = ContentLoader.Build(catalog);

            CollectionAssert.AreEqual(defaults.ModuleKinds, loaded.ModuleKinds, "module order");
            CollectionAssert.AreEqual(defaults.ShopPool, loaded.ShopPool, "shop pool order");
            CollectionAssert.AreEqual(defaults.EnemyKinds, loaded.EnemyKinds, "enemy order");

            foreach (ModuleKind kind in defaults.ModuleKinds)
            {
                AssertSameModule(defaults.Module(kind), loaded.Module(kind));
            }

            foreach (EnemyKind kind in defaults.EnemyKinds)
            {
                AssertSameEnemy(defaults.Enemy(kind), loaded.Enemy(kind));
            }

            Assert.AreEqual(HashAfterBotRun(defaults), HashAfterBotRun(loaded), "same run on both databases");
        }

        [Test]
        public void PrototypeCatalogAsset_MatchesDefaults()
        {
            var catalog = Resources.Load<ContentCatalog>(ContentCatalog.PrototypeResourcePath);
            if (catalog == null)
            {
                // First run on a fresh project: generate the assets, then require them.
                ContentMenu.GenerateFromDefaults();
                catalog = Resources.Load<ContentCatalog>(ContentCatalog.PrototypeResourcePath);
            }

            Assert.IsNotNull(catalog, "Assets/Content/Resources/PrototypeCatalog.asset is missing: run TowerDefense/Content/Generate prototype catalog");
            Assert.AreEqual(7, catalog.Modules.Count);
            Assert.AreEqual(4, catalog.Enemies.Count);
            Assert.AreEqual(HashAfterBotRun(ContentDatabase.CreatePrototypeDefaults()), HashAfterBotRun(ContentLoader.Build(catalog)),
                "the asset catalog diverged from the code defaults: regenerate it or bump the balance version");
        }

        private static void AssertSameModule(ModuleDefinition a, ModuleDefinition b)
        {
            string k = a.Kind.ToString();
            Assert.AreEqual(a.Category, b.Category, k);
            Assert.AreEqual(a.Rarity, b.Rarity, k);
            Assert.AreEqual(a.Cost, b.Cost, k);
            Assert.AreEqual(a.Damage, b.Damage, k);
            Assert.AreEqual(a.CooldownTicks, b.CooldownTicks, k);
            Assert.AreEqual(a.RangeMilli, b.RangeMilli, k);
            Assert.AreEqual(a.TargetCount, b.TargetCount, k);
            Assert.AreEqual(a.DamageMultiplierBonusPermille, b.DamageMultiplierBonusPermille, k);
            Assert.AreEqual(a.FlatDamageBonus, b.FlatDamageBonus, k);
            Assert.AreEqual(a.RangeBonusMilli, b.RangeBonusMilli, k);
            Assert.AreEqual(a.CooldownReductionPermille, b.CooldownReductionPermille, k);
            Assert.AreEqual(a.InterestCapBonus, b.InterestCapBonus, k);
            Assert.AreEqual(a.CreditsPerWave, b.CreditsPerWave, k);
            Assert.AreEqual(a.MaxIntegrityBonus, b.MaxIntegrityBonus, k);
            Assert.AreEqual(a.RepairPerWave, b.RepairPerWave, k);
        }

        private static void AssertSameEnemy(EnemyDefinition a, EnemyDefinition b)
        {
            string k = a.Kind.ToString();
            Assert.AreEqual(a.Hp, b.Hp, k);
            Assert.AreEqual(a.Armor, b.Armor, k);
            Assert.AreEqual(a.SpeedMilli, b.SpeedMilli, k);
            Assert.AreEqual(a.ContactDamage, b.ContactDamage, k);
            Assert.AreEqual(a.BudgetCostMilli, b.BudgetCostMilli, k);
            Assert.AreEqual(a.GroupSize, b.GroupSize, k);
        }
    }
}
