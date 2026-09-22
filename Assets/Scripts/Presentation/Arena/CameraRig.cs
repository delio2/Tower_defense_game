using TowerDefense.Simulation;
using UnityEngine;

namespace TowerDefense.Presentation.Arena
{
    /// <summary>
    /// The arena camera: wave view shows the whole arena, shop view a close-up on the ring so slots are comfortable to
    /// tap (48 dp+). The zoom between them is eased, never a cut (docs/03 A6).
    /// </summary>
    internal sealed class CameraRig
    {
        private const float TopBarFraction = 0.1f;
        private const float BottomBarFraction = 0.26f;
        private const float ArenaViewRadius = 9.4f;
        private const float ShopViewRadius = 3.4f;

        private float _viewRadius = ShopViewRadius;

        public CameraRig()
        {
            Camera = Camera.main;
            if (Camera == null)
            {
                var cameraObject = new GameObject("Main Camera") { tag = "MainCamera" };
                Camera = cameraObject.AddComponent<Camera>();
            }

            Camera.orthographic = true;
            Camera.clearFlags = CameraClearFlags.SolidColor;
            Camera.backgroundColor = Palette.Background;
            Camera.transform.rotation = Quaternion.Euler(90f, 0f, 0f);
            Camera.nearClipPlane = 0.1f;
            Camera.farClipPlane = 100f;
        }

        public Camera Camera { get; }

        public void ResetView() => _viewRadius = ShopViewRadius;

        public void UpdateLayout(GamePhase phase, float deltaTime)
        {
            float target = phase == GamePhase.Shop ? ShopViewRadius : ArenaViewRadius;
            _viewRadius = Mathf.Lerp(_viewRadius, target, 1f - Mathf.Exp(-deltaTime * 3f));

            float aspect = Mathf.Max(0.1f, Camera.aspect);
            float usable = 1f - TopBarFraction - BottomBarFraction;
            float size = Mathf.Max(_viewRadius / usable, _viewRadius / aspect);
            Camera.orthographicSize = size;
            float bandCentre = BottomBarFraction + usable * 0.5f;
            float offset = (bandCentre - 0.5f) * size * 2f;
            Camera.transform.position = new Vector3(0f, 20f, -offset);
        }

        /// <summary>Screen point to the ground plane (y = 0).</summary>
        public Vector3 ScreenToWorld(Vector2 screen)
        {
            Ray ray = Camera.ScreenPointToRay(screen);
            float distance = Mathf.Abs(ray.direction.y) < 1e-5f ? 0f : -ray.origin.y / ray.direction.y;
            return ray.origin + ray.direction * distance;
        }
    }
}
