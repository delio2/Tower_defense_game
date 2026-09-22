using TowerDefense.Simulation;
using UnityEngine;

namespace TowerDefense.Presentation.Arena
{
    /// <summary>
    /// The arena camera: the shop is a close-up on the ring so slots are comfortable to tap (48 dp+), the wave
    /// breathes between a near view and the whole arena depending on how far out the enemies are. The zoom is always
    /// eased, never a cut (docs/03 A6).
    /// </summary>
    internal sealed class CameraRig
    {
        private const float TopBarFraction = 0.1f;
        private const float BottomBarFraction = 0.26f;
        /// <summary>
        /// Wave framing (D34b): the mood shot frames the flower large, the arena rings reach 9.0. Rather than choose,
        /// the view follows the fight — <see cref="WaveNearRadius"/> while everything is close, out to
        /// <see cref="ArenaViewRadius"/> as soon as something is further away, so a distant enemy is never a surprise.
        /// </summary>
        private const float ArenaViewRadius = 9.4f;
        private const float WaveNearRadius = 5.6f;
        private const float ShopViewRadius = 3.4f;

        /// <summary>Margin kept beyond the furthest enemy, in world units.</summary>
        private const float WaveViewMargin = 1.2f;

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
            Camera.transform.rotation = Quaternion.Euler(90f - TiltDegrees, 0f, 0f);
            Camera.nearClipPlane = 0.1f;
            Camera.farClipPlane = 100f;
        }

        /// <summary>Tilt from straight down (docs/03 A6): volume and soft shadows, still as readable as top-down.</summary>
        private const float TiltDegrees = 35f;
        private const float CameraDistance = 20f;

        public Camera Camera { get; }

        public void ResetView() => _viewRadius = ShopViewRadius;

        public void UpdateLayout(GamePhase phase, float deltaTime, float furthestEnemy = 0f)
        {
            float target = phase == GamePhase.Shop
                ? ShopViewRadius
                : Mathf.Clamp(furthestEnemy + WaveViewMargin, WaveNearRadius, ArenaViewRadius);

            // Zooming out is quicker than zooming in: the view opens as soon as something appears out there, and
            // closes again slowly, so it never pumps while enemies come and go.
            float speed = target > _viewRadius ? 4.5f : 1.5f;
            _viewRadius = Mathf.Lerp(_viewRadius, target, 1f - Mathf.Exp(-deltaTime * speed));

            // On a tilted orthographic camera the ground's depth is foreshortened by sin(elevation).
            float foreshortening = Mathf.Sin((90f - TiltDegrees) * Mathf.Deg2Rad);
            float aspect = Mathf.Max(0.1f, Camera.aspect);
            float usable = 1f - TopBarFraction - BottomBarFraction;
            float size = Mathf.Max(_viewRadius * foreshortening / usable, _viewRadius / aspect);
            Camera.orthographicSize = size;
            float bandCentre = BottomBarFraction + usable * 0.5f;
            float screenOffset = (bandCentre - 0.5f) * size * 2f;
            Vector3 lookAt = new Vector3(0f, 0f, -screenOffset / foreshortening);
            Camera.transform.position = lookAt - Camera.transform.forward * CameraDistance;
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
