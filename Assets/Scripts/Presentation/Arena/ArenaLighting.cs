using UnityEngine;
using UnityEngine.Rendering;
using UnityEngine.Rendering.Universal;
using TowerDefense.Presentation.Settings;

namespace TowerDefense.Presentation.Arena
{
    /// <summary>
    /// Light and post-processing of the arena (docs/03 A6): one warm key light from the upper left with soft shadows,
    /// a cool hemisphere ambient (sky above, night below), light bloom only on emissive values above 1.1, a soft
    /// vignette and no tone mapping (AgX/ACES grey the ivory Core). The act theme changes key and ambient colours.
    /// </summary>
    internal sealed class ArenaLighting
    {
        private static readonly int AmbientSkyId = Shader.PropertyToID("_TD_AmbientSky");
        private static readonly int AmbientGroundId = Shader.PropertyToID("_TD_AmbientGround");

        private readonly Light _key;
        private readonly VolumeProfile _profile;
        private readonly Vignette _vignette;
        private readonly UniversalAdditionalCameraData _cameraData;

        public ArenaLighting(Transform parent, Camera camera)
        {
            var keyObject = new GameObject("Key light");
            keyObject.transform.SetParent(parent, false);
            // From the upper left of the portrait screen (far-left of the arena), about 45° above the ground.
            keyObject.transform.rotation = Quaternion.LookRotation(new Vector3(0.55f, -0.85f, -0.45f));
            _key = keyObject.AddComponent<Light>();
            _key.type = LightType.Directional;
            _key.shadows = LightShadows.Soft;
            _key.shadowStrength = 0.75f;

            var volumeObject = new GameObject("Post volume");
            volumeObject.transform.SetParent(parent, false);
            Volume volume = volumeObject.AddComponent<Volume>();
            volume.isGlobal = true;
            _profile = ScriptableObject.CreateInstance<VolumeProfile>();
            Bloom bloom = _profile.Add<Bloom>(true);
            bloom.threshold.Override(1.1f);
            bloom.intensity.Override(0.3f);
            bloom.scatter.Override(0.6f);
            _vignette = _profile.Add<Vignette>(true);
            _vignette.intensity.Override(0.25f);
            _vignette.smoothness.Override(0.5f);
            Tonemapping tonemapping = _profile.Add<Tonemapping>(true);
            tonemapping.mode.Override(TonemappingMode.None);
            volume.sharedProfile = _profile;

            _cameraData = camera.GetUniversalAdditionalCameraData();
            ApplyQuality();
        }

        /// <summary>
        /// The Low level (<see cref="GraphicsQuality"/>) drops the post pass and the shadow pass entirely: its URP asset
        /// has no shadows and no HDR, and skipping the passes here saves the full-screen blits as well.
        /// </summary>
        public void ApplyQuality()
        {
            _cameraData.renderPostProcessing = GraphicsQuality.PostProcessing;
            _cameraData.renderShadows = GraphicsQuality.Shadows;
            _key.shadows = GraphicsQuality.Shadows ? LightShadows.Soft : LightShadows.None;
        }

        public void ApplyTheme(in SkyTheme theme)
        {
            _key.color = theme.Key;
            _key.intensity = theme.KeyIntensity;
            Shader.SetGlobalColor(AmbientSkyId, theme.AmbientSky);
            Shader.SetGlobalColor(AmbientGroundId, theme.AmbientGround);
            _vignette.color.Override(theme.Near);
        }

        public void Dispose()
        {
            Object.Destroy(_profile);
        }
    }
}
