using System.Collections.Generic;
using TowerDefense.Simulation;
using UnityEngine;
using UnityEngine.Rendering;

namespace TowerDefense.Presentation.Arena
{
    /// <summary>
    /// Shared services for the arena views: the scene root, materials, mesh and line factories, and the mapping from
    /// simulation coordinates (micro-units) to world space. Views read the simulation through <see cref="Sim"/>;
    /// they never change it (presentation rule: commands only).
    /// </summary>
    internal sealed class ArenaKit
    {
        private static readonly int BaseColorId = Shader.PropertyToID("_BaseColor");
        private static readonly Dictionary<PrimitiveType, Mesh> PrimitiveMeshes = new Dictionary<PrimitiveType, Mesh>();

        private static readonly int RimColorId = Shader.PropertyToID("_RimColor");
        private static readonly int RimStrengthId = Shader.PropertyToID("_RimStrength");
        private static readonly int EmissionColorId = Shader.PropertyToID("_EmissionColor");
        private static readonly int EmissionStrengthId = Shader.PropertyToID("_EmissionStrength");

        private readonly Material _unlit;
        private readonly Material _lineMaterial;
        private readonly Material _surfaceTemplate;
        private readonly MaterialPropertyBlock _block = new MaterialPropertyBlock();
        private readonly Dictionary<SurfaceStyle, Material> _surfaces = new Dictionary<SurfaceStyle, Material>();
        private readonly List<Material> _owned = new List<Material>();

        public ArenaKit(Material unlit, Material lineMaterial, Material surfaceTemplate)
        {
            _unlit = unlit;
            _lineMaterial = lineMaterial;
            _surfaceTemplate = surfaceTemplate;
        }

        /// <summary>
        /// Shared three-surface material for a style (one material per style keeps the SRP Batcher effective:
        /// no property blocks on lit objects).
        /// </summary>
        public Material Surface(in SurfaceStyle style)
        {
            if (!_surfaces.TryGetValue(style, out Material material))
            {
                material = NewSurface(style);
                _surfaces.Add(style, material);
            }

            return material;
        }

        /// <summary>A material of its own, for objects whose colour animates (the Core's hit tint).</summary>
        public Material NewSurface(in SurfaceStyle style)
        {
            var material = new Material(_surfaceTemplate);
            ApplyStyle(material, style);
            _owned.Add(material);
            return material;
        }

        public static void ApplyStyle(Material material, in SurfaceStyle style)
        {
            material.SetColor(BaseColorId, style.Body);
            material.SetColor(RimColorId, style.Rim);
            material.SetFloat(RimStrengthId, style.RimStrength);
            material.SetColor(EmissionColorId, style.Inside);
            material.SetFloat(EmissionStrengthId, style.InsideStrength);
        }

        /// <summary>A lit primitive (no collider) with a three-surface material; casts and receives soft shadows.</summary>
        public GameObject CreateSurface(PrimitiveType type, string objectName, Vector3 position, Vector3 scale, Material material, bool castShadows = true)
        {
            var go = new GameObject(objectName);
            go.AddComponent<MeshFilter>().sharedMesh = PrimitiveMesh(type);
            MeshRenderer renderer = go.AddComponent<MeshRenderer>();
            go.transform.SetParent(Root, false);
            go.transform.localPosition = position;
            go.transform.localScale = scale;
            renderer.sharedMaterial = material;
            renderer.shadowCastingMode = castShadows ? ShadowCastingMode.On : ShadowCastingMode.Off;
            renderer.receiveShadows = true;
            return go;
        }

        private static readonly Dictionary<string, Mesh> Models = new Dictionary<string, Mesh>();

        /// <summary>
        /// A Blender model from <c>Resources/Models</c> (Tools/blender/build_models_v1.py) with the body material on
        /// sub-mesh 0 and the glowing accent on sub-mesh 1. Model "forward" is local −Z. Null when the model is missing,
        /// so callers can fall back to a primitive.
        /// </summary>
        public GameObject CreateModel(string resourcePath, string objectName, Vector3 position, float scale, Material body)
        {
            if (!Models.TryGetValue(resourcePath, out Mesh mesh))
            {
                mesh = Resources.Load<Mesh>(resourcePath);
                Models[resourcePath] = mesh;
            }

            if (mesh == null)
            {
                return null;
            }

            var go = new GameObject(objectName);
            go.AddComponent<MeshFilter>().sharedMesh = mesh;
            MeshRenderer renderer = go.AddComponent<MeshRenderer>();
            go.transform.SetParent(Root, false);
            go.transform.localPosition = position;
            go.transform.localScale = Vector3.one * scale;
            renderer.sharedMaterials = mesh.subMeshCount > 1 ? new[] { body, Surface(Palette.AccentSurface) } : new[] { body };
            renderer.shadowCastingMode = ShadowCastingMode.On;
            renderer.receiveShadows = true;
            return go;
        }

        public void DestroyOwnedMaterials()
        {
            foreach (Material material in _owned)
            {
                Object.Destroy(material);
            }

            _owned.Clear();
            _surfaces.Clear();
        }

        public GameSimulation Sim { get; set; }

        public Transform Root { get; private set; }

        /// <summary>Destroys the previous arena (if any) and creates an empty root under <paramref name="parent"/>.</summary>
        public void ResetRoot(Transform parent)
        {
            if (Root != null)
            {
                Object.Destroy(Root.gameObject);
            }

            Root = new GameObject("Arena").transform;
            Root.SetParent(parent, false);
        }

        /// <summary>World position of a ring slot on the ground plane.</summary>
        public Vector3 SlotWorld(int slot)
        {
            Directions.PointAt(Directions.OfSlot(slot, Sim.Ring.SlotCount), SimConstants.RingRadius, out long x, out long y);
            return new Vector3(x / (float)SimConstants.Micro, 0f, y / (float)SimConstants.Micro);
        }

        /// <summary>
        /// Primitive meshes without colliders: GameObject.CreatePrimitive would add one, and the Physics module is
        /// stripped from player builds (no physics in gameplay, D11).
        /// </summary>
        public GameObject CreatePrimitive(PrimitiveType type, string objectName, Vector3 position, Vector3 scale, Color color)
        {
            var go = new GameObject(objectName);
            go.AddComponent<MeshFilter>().sharedMesh = PrimitiveMesh(type);
            go.AddComponent<MeshRenderer>();
            go.transform.SetParent(Root, false);
            go.transform.localPosition = position;
            go.transform.localScale = scale;
            Renderer renderer = go.GetComponent<Renderer>();
            renderer.sharedMaterial = _unlit;
            renderer.shadowCastingMode = ShadowCastingMode.Off;
            renderer.receiveShadows = false;
            SetColor(renderer, color);
            return go;
        }

        public LineRenderer CreateLine(string objectName, float width, Color color, bool loop)
        {
            var go = new GameObject(objectName);
            go.transform.SetParent(Root, false);
            LineRenderer line = go.AddComponent<LineRenderer>();
            line.sharedMaterial = _lineMaterial;
            line.useWorldSpace = true;
            line.loop = loop;
            line.widthMultiplier = width;
            line.startColor = color;
            line.endColor = color;
            line.shadowCastingMode = ShadowCastingMode.Off;
            line.receiveShadows = false;
            line.positionCount = 0;
            return line;
        }

        public void SetColor(Renderer renderer, Color color)
        {
            _block.Clear();
            _block.SetColor(BaseColorId, color);
            renderer.SetPropertyBlock(_block);
        }

        public static void SetCircle(LineRenderer line, Vector3 centre, float radius)
        {
            const int segments = 64;
            line.positionCount = segments;
            for (int i = 0; i < segments; i++)
            {
                float angle = i * Mathf.PI * 2f / segments;
                line.SetPosition(i, centre + new Vector3(Mathf.Cos(angle) * radius, 0f, Mathf.Sin(angle) * radius));
            }
        }

        public ModuleInstance FindModule(int id)
        {
            for (int slot = 0; slot < Sim.Ring.SlotCount; slot++)
            {
                ModuleInstance module = Sim.Ring.At(slot);
                if (module != null && module.Id == id)
                {
                    return module;
                }
            }

            return null;
        }

        public Enemy FindEnemy(int id)
        {
            foreach (Enemy enemy in Sim.Enemies)
            {
                if (enemy.Id == id)
                {
                    return enemy;
                }
            }

            return null;
        }

        private static Mesh PrimitiveMesh(PrimitiveType type)
        {
            if (!PrimitiveMeshes.TryGetValue(type, out Mesh mesh))
            {
                string file = type switch
                {
                    PrimitiveType.Sphere => "Sphere.fbx",
                    PrimitiveType.Capsule => "Capsule.fbx",
                    PrimitiveType.Cylinder => "Cylinder.fbx",
                    PrimitiveType.Plane => "Plane.fbx",
                    PrimitiveType.Quad => "Quad.fbx",
                    _ => "Cube.fbx",
                };
                mesh = Resources.GetBuiltinResource<Mesh>(file);
                PrimitiveMeshes[type] = mesh;
            }

            return mesh;
        }
    }
}
