using System.Collections.Generic;
using TowerDefense.Simulation;
using UnityEngine;

namespace TowerDefense.Presentation.Content
{
    /// <summary>
    /// The set of modules and enemies of one balance version. The order of the module list is the shop pool
    /// order and therefore affects the seeded offers: never reorder it without bumping the balance version.
    /// </summary>
    [CreateAssetMenu(menuName = "TowerDefense/Content/Catalog", fileName = "Catalog")]
    public sealed class ContentCatalog : ScriptableObject
    {
        /// <summary>Resources path of the catalog the prototype loads (Assets/Content/Resources/PrototypeCatalog.asset).</summary>
        public const string PrototypeResourcePath = "PrototypeCatalog";

        [SerializeField] private List<ModuleAsset> _modules = new List<ModuleAsset>();
        [SerializeField] private List<EnemyAsset> _enemies = new List<EnemyAsset>();

        public IReadOnlyList<ModuleAsset> Modules => _modules;
        public IReadOnlyList<EnemyAsset> Enemies => _enemies;

        public void SetEntries(List<ModuleAsset> modules, List<EnemyAsset> enemies)
        {
            _modules = modules;
            _enemies = enemies;
        }
    }

    /// <summary>Builds the simulation's <see cref="ContentDatabase"/> from authored assets.</summary>
    public static class ContentLoader
    {
        public static ContentDatabase Build(ContentCatalog catalog)
        {
            var db = new ContentDatabase();
            foreach (ModuleAsset module in catalog.Modules)
            {
                if (module != null)
                {
                    db.Add(module.ToDefinition(), module.InShop);
                }
            }

            foreach (EnemyAsset enemy in catalog.Enemies)
            {
                if (enemy != null)
                {
                    db.Add(enemy.ToDefinition());
                }
            }

            return db;
        }

        /// <summary>The prototype catalog from Resources, or the code defaults if it is missing (with a warning).</summary>
        public static ContentDatabase LoadPrototypeOrDefaults()
        {
            var catalog = Resources.Load<ContentCatalog>(ContentCatalog.PrototypeResourcePath);
            if (catalog == null)
            {
                Debug.LogWarning($"Content catalog '{ContentCatalog.PrototypeResourcePath}' not found in Resources: using code defaults.");
                return ContentDatabase.CreatePrototypeDefaults();
            }

            return Build(catalog);
        }

        /// <summary>
        /// Creates in-memory assets mirroring a database (the code defaults today). The editor menu saves them;
        /// the tests use them directly to prove the round trip is lossless.
        /// </summary>
        public static ContentCatalog CreateCatalogFrom(ContentDatabase source)
        {
            var modules = new List<ModuleAsset>();
            foreach (ModuleKind kind in source.ModuleKinds)
            {
                var asset = ScriptableObject.CreateInstance<ModuleAsset>();
                asset.name = kind.ToString();
                asset.CopyFrom(source.Module(kind), inShop: source.IsInShop(kind));
                modules.Add(asset);
            }

            var enemies = new List<EnemyAsset>();
            foreach (EnemyKind kind in source.EnemyKinds)
            {
                var asset = ScriptableObject.CreateInstance<EnemyAsset>();
                asset.name = kind.ToString();
                asset.CopyFrom(source.Enemy(kind));
                enemies.Add(asset);
            }

            var catalog = ScriptableObject.CreateInstance<ContentCatalog>();
            catalog.name = "PrototypeCatalog";
            catalog.SetEntries(modules, enemies);
            return catalog;
        }
    }
}
