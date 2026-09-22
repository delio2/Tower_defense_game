using System.Collections.Generic;
using System.IO;
using TowerDefense.Presentation.Content;
using TowerDefense.Simulation;
using UnityEditor;
using UnityEngine;

namespace TowerDefense.Editor
{
    /// <summary>
    /// Generates the ScriptableObject catalog from the code defaults, so the assets start equal to GDD v0.2 and
    /// stay editable in the inspector. Re-running overwrites values in place (asset GUIDs are preserved).
    /// </summary>
    public static class ContentMenu
    {
        private const string ContentRoot = "Assets/Content/Resources";
        private const string ModulesFolder = ContentRoot + "/Modules";
        private const string EnemiesFolder = ContentRoot + "/Enemies";

        [MenuItem("TowerDefense/Content/Generate prototype catalog from code defaults")]
        public static void GenerateFromDefaults()
        {
            Directory.CreateDirectory(ModulesFolder);
            Directory.CreateDirectory(EnemiesFolder);
            AssetDatabase.Refresh();

            ContentCatalog source = ContentLoader.CreateCatalogFrom(ContentDatabase.CreatePrototypeDefaults());
            var modules = new List<ModuleAsset>();
            foreach (ModuleAsset module in source.Modules)
            {
                modules.Add(SaveOrUpdate(module, $"{ModulesFolder}/{module.name}.asset"));
            }

            var enemies = new List<EnemyAsset>();
            foreach (EnemyAsset enemy in source.Enemies)
            {
                enemies.Add(SaveOrUpdate(enemy, $"{EnemiesFolder}/{enemy.name}.asset"));
            }

            string catalogPath = $"{ContentRoot}/{ContentCatalog.PrototypeResourcePath}.asset";
            var catalog = AssetDatabase.LoadAssetAtPath<ContentCatalog>(catalogPath);
            if (catalog == null)
            {
                catalog = ScriptableObject.CreateInstance<ContentCatalog>();
                AssetDatabase.CreateAsset(catalog, catalogPath);
            }

            catalog.SetEntries(modules, enemies);
            EditorUtility.SetDirty(catalog);
            AssetDatabase.SaveAssets();
            Debug.Log($"Content catalog written: {modules.Count} modules, {enemies.Count} enemies at {catalogPath}");
        }

        private static T SaveOrUpdate<T>(T generated, string path) where T : ScriptableObject
        {
            var existing = AssetDatabase.LoadAssetAtPath<T>(path);
            if (existing == null)
            {
                AssetDatabase.CreateAsset(generated, path);
                return generated;
            }

            EditorUtility.CopySerialized(generated, existing);
            EditorUtility.SetDirty(existing);
            Object.DestroyImmediate(generated);
            return existing;
        }
    }
}
