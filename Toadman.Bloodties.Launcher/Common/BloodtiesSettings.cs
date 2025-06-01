using System;
using System.Collections.Generic;
using System.Linq;
using System.IO;
using System.Windows;
using Toadman.GameLauncher.Core;
using Json;

namespace Toadman.Bloodties.Launcher
{
    public enum BloodtiesScreenMode
    {
        Windowed,
        FullScreen,
        Borderless
    }

    public enum BloodtiesGameQuality
    {
        Lowest,
        Low,
        Medium,
        High,
        Extreme
    }

    public class BloodtiesSettings
    {
        private ImmutableJson userConfig;
        private VideoMode currentVideoMode;

        private string lowestSettings = @"
            char_texture_quality = ""low"",
            env_texture_quality = ""low"",
            particles_quality = ""low"",
            use_physic_debris = false,
            sun_shadow_quality = ""low"",
            local_light_shadow_quality = ""low"",
            num_blood_decals = 0.0,
            animation_lod_distance_multiplier = 0.0,
            use_high_quality_fur = false,
            render_settings = {
                sun_shadows = false,
                max_shadow_casting_lights = 1,
                deferred_local_lights_cast_shadows = false,
                forward_local_lights_cast_shadows = false,
                lod_scatter_density = 0,
                fxaa_enabled = false,
                taa_enabled = false,
                ao_enabled = true,
                dof_enabled = false,
                bloom_enabled = false,
                light_shaft_enabled = false,
                skin_material_enabled = false,
                ssr_enabled = false,
                motion_blur_enabled = false,
                low_res_transparency = true,
            }";

        private string lowSettings = @"
            char_texture_quality = ""low"",
            env_texture_quality = ""low"",
            particles_quality = ""low"",
            use_physic_debris = false,
            sun_shadow_quality = ""low"",
            local_light_shadow_quality = ""low"",
            num_blood_decals = 10,
            animation_lod_distance_multiplier = 0.0,
            use_high_quality_fur = false,
            render_settings = {
                sun_shadows = true,
                max_shadow_casting_lights = 1,
                deferred_local_lights_cast_shadows = false,
                forward_local_lights_cast_shadows = false,
                lod_scatter_density = 0.25,
                fxaa_enabled = false,
                taa_enabled = false,
                ao_enabled = true,
                dof_enabled = false,
                bloom_enabled = true,
                light_shaft_enabled = true,
                skin_material_enabled = false,
                ssr_enabled = false,
                motion_blur_enabled = false,
                low_res_transparency = true,
            }";

        private string mediumSettings = @"
            char_texture_quality = ""medium"",
            env_texture_quality = ""medium"",
            particles_quality = ""low"",
            use_physic_debris = true,
            sun_shadow_quality = ""medium"",
            local_light_shadow_quality = ""medium"",
            num_blood_decals = 25,
            animation_lod_distance_multiplier = 0.5,
            use_high_quality_fur = false,
            render_settings = {
                sun_shadows = true,
                max_shadow_casting_lights = 1,
                deferred_local_lights_cast_shadows = false,
                forward_local_lights_cast_shadows = false,
                lod_scatter_density = 0.5,
                fxaa_enabled = true,
                taa_enabled = false,
                ao_enabled = true,
                dof_enabled = false,
                bloom_enabled = true,
                light_shaft_enabled = true,
                skin_material_enabled = false,
                ssr_enabled = false,
                motion_blur_enabled = true,
                low_res_transparency = true,
            }";

        private string highSettings = @"
            char_texture_quality = ""high"",
            env_texture_quality = ""high"",
            particles_quality = ""medium"",
            use_physic_debris = true,
            sun_shadow_quality = ""high"",
            local_light_shadow_quality = ""high"",
            num_blood_decals = 50,
            animation_lod_distance_multiplier = 1,
            use_high_quality_fur = true,
            render_settings = {
                sun_shadows = true,
                max_shadow_casting_lights = 2,
                deferred_local_lights_cast_shadows = true,
                forward_local_lights_cast_shadows = true,
                lod_scatter_density = 1,
                fxaa_enabled = false,
                taa_enabled = true,
                ao_enabled = true,
                dof_enabled = true,
                bloom_enabled = true,
                light_shaft_enabled = true,
                skin_material_enabled = true,
                ssr_enabled = true,
                motion_blur_enabled = true,
                low_res_transparency = true,
            }";

        private string extremeSettings = @"
            char_texture_quality = ""extreme"",
            env_texture_quality = ""high"",
            particles_quality = ""high"",
            use_physic_debris = true,
            sun_shadow_quality = ""extreme"",
            local_light_shadow_quality = ""extreme"",
            num_blood_decals = 100,
            animation_lod_distance_multiplier = 1,
            use_high_quality_fur = true,
            render_settings = {
                sun_shadows = true,
                max_shadow_casting_lights = 6,
                deferred_local_lights_cast_shadows = true,
                forward_local_lights_cast_shadows = true,
                lod_scatter_density = 1,
                fxaa_enabled = false,
                taa_enabled = true,
                ao_enabled = true,
                dof_enabled = true,
                bloom_enabled = true,
                light_shaft_enabled = true,
                skin_material_enabled = true,
                ssr_enabled = true,
                motion_blur_enabled = true,
                low_res_transparency = true,
            }";

        public List<VideoMode> AvailableVideoModes = new List<VideoMode>();
        public VideoMode GameVideoMode;
        public BloodtiesScreenMode ScreenMode;
        public BloodtiesGameQuality GameQuality;

        public string ConfigFilePath
        {
            get
            {
                var appDataPath = Environment.ExpandEnvironmentVariables("%AppData%");
                return Path.Combine(appDataPath, "Toadman Interactive", "Bloodties", "user_settings.config");
            }
        }

        public BloodtiesSettings()
        {
            // Get applicable video modes
            AvailableVideoModes = ScreenHelper.GetVideoModes()
                .Where(vm => vm.Width >= 1024 && vm.DisplayFrequency == 60 && IsValidAspect(vm.Width, vm.Height))
                .OrderBy(vm => vm.Width)
                .Distinct(new VideoModeComparer())
                .ToList();

            // Get primary display video mode
            var primaryScreenWidth = SystemParameters.PrimaryScreenWidth;
            currentVideoMode = AvailableVideoModes
                .Where(vm => vm.Width == SystemParameters.PrimaryScreenWidth && vm.Height == SystemParameters.PrimaryScreenHeight)
                .FirstOrDefault();

            if (currentVideoMode == null)
                currentVideoMode = AvailableVideoModes.FirstOrDefault();

            if (File.Exists(ConfigFilePath))
            {
                // Load configuration file
                userConfig = SJson.SJson.Parse(File.ReadAllText(ConfigFilePath));

                // Guess configuration
                GameVideoMode = GuessVideoMode();
                ScreenMode = GuessScreenMode();
                GameQuality = GuessGameQuality();
            }
            else
            {
                // No game configuration file - assume defaults
                Directory.CreateDirectory(new FileInfo(ConfigFilePath).Directory.FullName);
                userConfig = ImmutableJson.EmptyObject;
                GameVideoMode = currentVideoMode;
                ScreenMode = BloodtiesScreenMode.Borderless;
                GameQuality = BloodtiesGameQuality.High;
            }
        }

        private bool IsValidAspect(int width, int height)
        {
            
            if ((width * 3 / 4) == height)          // 4:3
                return true;
            else if ((width * 9 / 16) == height)    // 16:9
                return true;
            else if ((width * 10 / 16) == height)   // 16:10
                return true;

            return false;
        }

        private VideoMode GuessVideoMode()
        {
            var screenWidth = 1024;
            var screenHeight = 768;

            try
            {
                screenWidth = userConfig.AsObject["screen_resolution"].AsArray.ElementAt(0).AsInt;
                screenHeight = userConfig.AsObject["screen_resolution"].AsArray.ElementAt(1).AsInt;
            }
            catch (Exception) {
                screenWidth = 1024;
                screenHeight = 768;
            }

            return AvailableVideoModes
                .Where(vm => vm.Width == screenWidth && vm.Height == screenHeight)
                .FirstOrDefault();
        }

        private BloodtiesScreenMode GuessScreenMode()
        {
            var fullscreen = userConfig.AsObject.ContainsKey("fullscreen")
                ? userConfig.AsObject["fullscreen"].AsBool : false;

            var borderless = userConfig.AsObject.ContainsKey("borderless_fullscreen")
                ? userConfig.AsObject["borderless_fullscreen"].AsBool : false;

            if (fullscreen && borderless)
                return BloodtiesScreenMode.Borderless;
            else if (fullscreen)
                return BloodtiesScreenMode.FullScreen;
            else
                return BloodtiesScreenMode.Windowed;
        }

        private BloodtiesGameQuality GuessGameQuality()
        {
            var charTextureQuality = userConfig.AsObject.ContainsKey("char_texture_quality")
                ? userConfig.AsObject["char_texture_quality"].AsString : "medium";

            var numBloodDecals = 25;

            if (userConfig.AsObject.ContainsKey("num_blood_decals"))
            {
                if (userConfig.AsObject["num_blood_decals"].IsInt)
                    numBloodDecals = userConfig.AsObject["num_blood_decals"].AsInt;
                else if (userConfig.AsObject["num_blood_decals"].IsLong)
                    numBloodDecals = (int) userConfig.AsObject["num_blood_decals"].AsLong;
                else if (userConfig.AsObject["num_blood_decals"].IsNumber)
                    numBloodDecals = (int) Math.Floor(userConfig.AsObject["num_blood_decals"].AsNumber);
            }

            switch (charTextureQuality)
            {
                case "extreme": return BloodtiesGameQuality.Extreme;
                case "high": return BloodtiesGameQuality.High;
                case "medium": return BloodtiesGameQuality.Medium;
                case "low": return numBloodDecals > 0 ? BloodtiesGameQuality.Low : BloodtiesGameQuality.Lowest;
                default: return BloodtiesGameQuality.Medium;
            }
        }

        public void Save()
        {
            // Get an editable copy of userConfig
            var editableConfig = new JsonObject(userConfig.AsObject);
            var renderSettings = editableConfig.ContainsKey("render_settings")
                ? new JsonObject(editableConfig["render_settings"].AsObject)
                : new JsonObject();

            // Game quality patch
            var gameQualityPatch = ImmutableJson.EmptyObject;

            switch (GameQuality)
            {
                case BloodtiesGameQuality.Lowest: gameQualityPatch = SJson.SJson.Parse(lowestSettings); break;
                case BloodtiesGameQuality.Low: gameQualityPatch = SJson.SJson.Parse(lowSettings); break;
                case BloodtiesGameQuality.Medium: gameQualityPatch = SJson.SJson.Parse(mediumSettings); break;
                case BloodtiesGameQuality.High: gameQualityPatch = SJson.SJson.Parse(highSettings); break;
                case BloodtiesGameQuality.Extreme: gameQualityPatch = SJson.SJson.Parse(extremeSettings); break;
            }

            // Screen resolution
            var screenResolution = new JsonArray();
            screenResolution.Add(GameVideoMode.Width);
            screenResolution.Add(GameVideoMode.Height);

            // Patch screen resolution and mode
            editableConfig["screen_resolution"] = screenResolution;
            editableConfig["fullscreen"] = ScreenMode != BloodtiesScreenMode.Windowed;
            editableConfig["borderless_fullscreen"] = ScreenMode == BloodtiesScreenMode.Borderless;

            // Patch game quality
            foreach (var kv in gameQualityPatch.AsObject)
            {
                if (kv.Key != "render_settings")
                {
                    editableConfig[kv.Key] = kv.Value;
                    continue;
                }

                foreach (var kvr in kv.Value.AsObject)
                    renderSettings[kvr.Key] = kvr.Value;

                editableConfig[kv.Key] = renderSettings;
            }

            var sjson = SJson.SJson.Stringify(editableConfig.ToImmutable());
            File.WriteAllText(ConfigFilePath, sjson);
        }
    }
}