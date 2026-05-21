using System.IO;
using UnityEditor;
using UnityEditor.SceneManagement;
using UnityEngine;
using UnityEngine.SceneManagement;

namespace NiumaShader.Editor
{
    /// <summary>
    /// 2.0-A 美术验证工具。
    /// 通过菜单生成测试材质、测试贴图和测试场景，避免手写 .mat / .unity 文件造成序列化差异。
    /// </summary>
    public static class NiumaArchitectureValidationSceneBuilder
    {
        private const string ShaderName = "Niuma/Architecture/Lit";
        private const string ModuleRoot = "Assets/Game/Moudle/NiumaShader";
        private const string MaterialsFolder = ModuleRoot + "/Runtime/Materials";
        private const string TestTexturesFolder = ModuleRoot + "/Runtime/Textures/Test";
        private const string TestScenesFolder = ModuleRoot + "/Runtime/TestScenes";
        private const string TestScenePath = TestScenesFolder + "/NiumaShader_Architecture_Test.unity";
        private const string MaskTexturePath = TestTexturesFolder + "/T_Niuma_Validation_Mask.png";
        private const string DetailTexturePath = TestTexturesFolder + "/T_Niuma_Validation_Detail.png";
        private const string WeatherTexturePath = TestTexturesFolder + "/T_Niuma_Validation_Weather.png";
        private const int ValidationTextureSize = 256;

        [MenuItem("Niuma/Shader/生成古建 Shader 测试场景")]
        public static void CreateValidationScene()
        {
            var shader = Shader.Find(ShaderName);
            if (shader == null)
            {
                EditorUtility.DisplayDialog("NiumaShader", "没有找到 Shader：Niuma/Architecture/Lit。请先确认 Shader 已成功导入。", "确定");
                return;
            }

            if (!EditorSceneManager.SaveCurrentModifiedScenesIfUserWantsTo())
            {
                return;
            }

            EnsureFolder(MaterialsFolder);
            EnsureFolder(TestTexturesFolder);
            EnsureFolder(TestScenesFolder);

            var maskTexture = CreateOrLoadMaskTexture();
            var detailTexture = CreateOrLoadDetailTexture();
            var weatherTexture = CreateOrLoadWeatherTexture();

            var wood = CreateOrUpdateTemplateMaterial(
                shader,
                "M_Wood_Painted_Template",
                0,
                new Color(0.56f, 0.22f, 0.14f, 1f),
                0.32f,
                0.55f,
                new Color(0.66f, 0.51f, 0.36f, 1f),
                0.35f,
                maskTexture,
                detailTexture,
                weatherTexture);

            var roofTile = CreateOrUpdateTemplateMaterial(
                shader,
                "M_RoofTile_BlueGray_Template",
                1,
                new Color(0.28f, 0.36f, 0.40f, 1f),
                0.48f,
                0.70f,
                new Color(0.55f, 0.60f, 0.58f, 1f),
                0.28f,
                maskTexture,
                detailTexture,
                weatherTexture);

            var stone = CreateOrUpdateTemplateMaterial(
                shader,
                "M_Stone_Step_Template",
                2,
                new Color(0.48f, 0.46f, 0.41f, 1f),
                0.22f,
                0.80f,
                new Color(0.72f, 0.68f, 0.60f, 1f),
                0.55f,
                maskTexture,
                detailTexture,
                weatherTexture);

            var wall = CreateOrUpdateTemplateMaterial(
                shader,
                "M_Wall_Lime_Template",
                3,
                new Color(0.78f, 0.74f, 0.66f, 1f),
                0.14f,
                0.60f,
                new Color(0.86f, 0.82f, 0.72f, 1f),
                0.12f,
                maskTexture,
                detailTexture,
                weatherTexture);

            var scene = EditorSceneManager.NewScene(NewSceneSetup.EmptyScene, NewSceneMode.Single);

            CreateLighting();
            CreateCamera();
            CreateGround();

            CreateSample("木构 / 彩绘", PrimitiveType.Cube, wood, new Vector3(-4.5f, 1.35f, 0f), new Vector3(1.1f, 2.7f, 1.1f), Vector3.zero);
            CreateSample("青瓦屋面", PrimitiveType.Cube, roofTile, new Vector3(-1.5f, 1.25f, 0f), new Vector3(2.5f, 0.28f, 1.7f), new Vector3(0f, 0f, -18f));
            CreateSample("石阶", PrimitiveType.Cube, stone, new Vector3(1.5f, 0.28f, 0f), new Vector3(2.2f, 0.55f, 1.8f), Vector3.zero);
            CreateSample("灰墙", PrimitiveType.Cube, wall, new Vector3(4.5f, 1.0f, 0f), new Vector3(2.1f, 2.0f, 0.35f), Vector3.zero);

            EditorSceneManager.SaveScene(scene, TestScenePath);
            AssetDatabase.SaveAssets();
            AssetDatabase.Refresh();

            EditorUtility.DisplayDialog("NiumaShader", "古建 Shader 测试场景已生成：\n" + TestScenePath, "确定");
        }

        [MenuItem("Niuma/Shader/刷新古建 Shader 模板材质")]
        public static void RefreshTemplateMaterials()
        {
            var shader = Shader.Find(ShaderName);
            if (shader == null)
            {
                EditorUtility.DisplayDialog("NiumaShader", "没有找到 Shader：Niuma/Architecture/Lit。请先确认 Shader 已成功导入。", "确定");
                return;
            }

            EnsureFolder(MaterialsFolder);
            EnsureFolder(TestTexturesFolder);

            var maskTexture = CreateOrLoadMaskTexture();
            var detailTexture = CreateOrLoadDetailTexture();
            var weatherTexture = CreateOrLoadWeatherTexture();

            CreateOrUpdateTemplateMaterial(shader, "M_Wood_Painted_Template", 0, new Color(0.56f, 0.22f, 0.14f, 1f), 0.32f, 0.55f, new Color(0.66f, 0.51f, 0.36f, 1f), 0.35f, maskTexture, detailTexture, weatherTexture);
            CreateOrUpdateTemplateMaterial(shader, "M_RoofTile_BlueGray_Template", 1, new Color(0.28f, 0.36f, 0.40f, 1f), 0.48f, 0.70f, new Color(0.55f, 0.60f, 0.58f, 1f), 0.28f, maskTexture, detailTexture, weatherTexture);
            CreateOrUpdateTemplateMaterial(shader, "M_Stone_Step_Template", 2, new Color(0.48f, 0.46f, 0.41f, 1f), 0.22f, 0.80f, new Color(0.72f, 0.68f, 0.60f, 1f), 0.55f, maskTexture, detailTexture, weatherTexture);
            CreateOrUpdateTemplateMaterial(shader, "M_Wall_Lime_Template", 3, new Color(0.78f, 0.74f, 0.66f, 1f), 0.14f, 0.60f, new Color(0.86f, 0.82f, 0.72f, 1f), 0.12f, maskTexture, detailTexture, weatherTexture);

            AssetDatabase.SaveAssets();
            AssetDatabase.Refresh();
            EditorUtility.DisplayDialog("NiumaShader", "模板材质已刷新。", "确定");
        }

        private static Material CreateOrUpdateTemplateMaterial(
            Shader shader,
            string materialName,
            float surfaceType,
            Color baseColor,
            float smoothness,
            float weatherStrength,
            Color edgeWearColor,
            float edgeWearStrength,
            Texture2D maskTexture,
            Texture2D detailTexture,
            Texture2D weatherTexture)
        {
            var materialPath = MaterialsFolder + "/" + materialName + ".mat";
            var material = AssetDatabase.LoadAssetAtPath<Material>(materialPath);
            if (material == null)
            {
                material = new Material(shader);
                AssetDatabase.CreateAsset(material, materialPath);
            }

            material.shader = shader;
            SetFloat(material, "_SurfaceType", surfaceType);
            SetColor(material, "_BaseColor", baseColor);
            SetTexture(material, "_MaskMap", maskTexture);
            SetTexture(material, "_DetailMap", detailTexture);
            SetTexture(material, "_WeatherMap", weatherTexture);
            SetFloat(material, "_DetailStrength", 0.42f);
            SetTextureScale(material, "_DetailMap", new Vector2(4f, 4f));
            SetFloat(material, "_Smoothness", smoothness);
            SetFloat(material, "_WeatherStrength", weatherStrength);
            SetFloat(material, "_DirtStrength", 0.45f);
            SetFloat(material, "_MossStrength", 0.35f);
            SetFloat(material, "_PaintAgeStrength", 0.28f);
            SetFloat(material, "_PaintFadeSaturation", 0.45f);
            SetFloat(material, "_RainStreakStrength", 0.22f);
            SetColor(material, "_EdgeWearColor", edgeWearColor);
            SetFloat(material, "_EdgeWearStrength", edgeWearStrength);
            SetFloat(material, "_VertexWeatherStrength", 0.35f);
            material.enableInstancing = true;
            material.EnableKeyword("_NIUMA_DETAILMAP");
            material.EnableKeyword("_NIUMA_WEATHERING");
            EditorUtility.SetDirty(material);
            return material;
        }

        private static Texture2D CreateOrLoadMaskTexture()
        {
            var existing = AssetDatabase.LoadAssetAtPath<Texture2D>(MaskTexturePath);
            if (existing != null && existing.width == ValidationTextureSize && existing.height == ValidationTextureSize)
            {
                return existing;
            }

            var texture = new Texture2D(ValidationTextureSize, ValidationTextureSize, TextureFormat.RGBA32, false, true);
            var maxIndex = ValidationTextureSize - 1f;
            for (var y = 0; y < texture.height; y++)
            {
                for (var x = 0; x < texture.width; x++)
                {
                    var u = x / maxIndex;
                    var v = y / maxIndex;
                    var border = Mathf.Max(Mathf.Abs(u - 0.5f), Mathf.Abs(v - 0.5f));
                    var ao = Mathf.Lerp(0.65f, 1f, v);
                    var smoothness = Mathf.Lerp(0.18f, 0.72f, u);
                    var edgeWear = Mathf.SmoothStep(0.30f, 0.50f, border);
                    texture.SetPixel(x, y, new Color(ao, smoothness, edgeWear, 0f));
                }
            }

            texture.Apply();
            SaveTextureAsset(texture, MaskTexturePath, false);
            return AssetDatabase.LoadAssetAtPath<Texture2D>(MaskTexturePath);
        }

        private static Texture2D CreateOrLoadWeatherTexture()
        {
            var existing = AssetDatabase.LoadAssetAtPath<Texture2D>(WeatherTexturePath);
            if (existing != null && existing.width == ValidationTextureSize && existing.height == ValidationTextureSize)
            {
                return existing;
            }

            var texture = new Texture2D(ValidationTextureSize, ValidationTextureSize, TextureFormat.RGBA32, false, true);
            var maxIndex = ValidationTextureSize - 1f;
            for (var y = 0; y < texture.height; y++)
            {
                for (var x = 0; x < texture.width; x++)
                {
                    var u = x / maxIndex;
                    var v = y / maxIndex;
                    var dirt = Mathf.SmoothStep(0.35f, 0f, v);
                    var moss = Mathf.SmoothStep(0.55f, 0f, v) * Mathf.SmoothStep(1f, 0.35f, u);
                    var paintFade = Mathf.Clamp01(Mathf.Sin((u + v) * 12f) * 0.25f + 0.45f);
                    var rain = Mathf.Clamp01(Mathf.Sin(u * 40f) * 0.35f + 0.35f) * Mathf.SmoothStep(1f, 0.2f, v);
                    texture.SetPixel(x, y, new Color(dirt, moss, paintFade, rain));
                }
            }

            texture.Apply();
            SaveTextureAsset(texture, WeatherTexturePath, false);
            return AssetDatabase.LoadAssetAtPath<Texture2D>(WeatherTexturePath);
        }

        private static Texture2D CreateOrLoadDetailTexture()
        {
            var existing = AssetDatabase.LoadAssetAtPath<Texture2D>(DetailTexturePath);
            if (existing != null && existing.width == ValidationTextureSize && existing.height == ValidationTextureSize)
            {
                return existing;
            }

            var texture = new Texture2D(ValidationTextureSize, ValidationTextureSize, TextureFormat.RGBA32, false, true);
            var maxIndex = ValidationTextureSize - 1f;
            for (var y = 0; y < texture.height; y++)
            {
                for (var x = 0; x < texture.width; x++)
                {
                    var u = x / maxIndex;
                    var v = y / maxIndex;
                    var fineNoise = Mathf.Sin((u * 97.0f + v * 53.0f) * Mathf.PI) * 0.5f + 0.5f;
                    var grain = Mathf.Sin((u * 31.0f - v * 43.0f) * Mathf.PI) * 0.5f + 0.5f;
                    var detail = Mathf.Lerp(0.43f, 0.58f, fineNoise * 0.65f + grain * 0.35f);
                    var mask = Mathf.SmoothStep(0.15f, 0.95f, Mathf.Abs(fineNoise - 0.5f) * 2.0f);
                    texture.SetPixel(x, y, new Color(detail, detail, detail, mask));
                }
            }

            texture.Apply();
            SaveTextureAsset(texture, DetailTexturePath, false);
            return AssetDatabase.LoadAssetAtPath<Texture2D>(DetailTexturePath);
        }

        private static void SaveTextureAsset(Texture2D texture, string assetPath, bool sRgb)
        {
            File.WriteAllBytes(Path.GetFullPath(assetPath), texture.EncodeToPNG());
            AssetDatabase.ImportAsset(assetPath);

            var importer = AssetImporter.GetAtPath(assetPath) as TextureImporter;
            if (importer != null)
            {
                importer.textureType = TextureImporterType.Default;
                importer.sRGBTexture = sRgb;
                importer.mipmapEnabled = true;
                importer.textureCompression = TextureImporterCompression.Uncompressed;
                importer.SaveAndReimport();
            }
        }

        private static void CreateLighting()
        {
            var lightObject = new GameObject("主方向光");
            var light = lightObject.AddComponent<Light>();
            light.type = LightType.Directional;
            light.intensity = 2.2f;
            light.color = new Color(1f, 0.91f, 0.78f, 1f);
            light.shadows = LightShadows.Soft;
            lightObject.transform.rotation = Quaternion.Euler(48f, -35f, 0f);
        }

        private static void CreateCamera()
        {
            var cameraObject = new GameObject("测试相机");
            var camera = cameraObject.AddComponent<Camera>();
            camera.clearFlags = CameraClearFlags.Skybox;
            camera.fieldOfView = 38f;
            cameraObject.transform.position = new Vector3(0f, 3.2f, -8.5f);
            cameraObject.transform.rotation = Quaternion.Euler(18f, 0f, 0f);
            cameraObject.tag = "MainCamera";
        }

        private static void CreateGround()
        {
            var ground = GameObject.CreatePrimitive(PrimitiveType.Plane);
            ground.name = "验证地面";
            ground.transform.position = Vector3.zero;
            ground.transform.localScale = new Vector3(1.25f, 1f, 0.75f);
        }

        private static void CreateSample(string label, PrimitiveType primitiveType, Material material, Vector3 position, Vector3 scale, Vector3 rotation)
        {
            var sample = GameObject.CreatePrimitive(primitiveType);
            sample.name = label;
            sample.transform.position = position;
            sample.transform.localScale = scale;
            sample.transform.rotation = Quaternion.Euler(rotation);

            var renderer = sample.GetComponent<Renderer>();
            if (renderer != null)
            {
                renderer.sharedMaterial = material;
            }

            var labelObject = new GameObject(label + "_标签");
            labelObject.transform.position = position + Vector3.up * (scale.y * 0.65f + 0.45f);
            var text = labelObject.AddComponent<TextMesh>();
            text.text = label;
            text.characterSize = 0.18f;
            text.anchor = TextAnchor.MiddleCenter;
            text.alignment = TextAlignment.Center;
            text.color = Color.black;
        }

        private static void EnsureFolder(string folderPath)
        {
            var parts = folderPath.Split('/');
            var current = parts[0];
            for (var i = 1; i < parts.Length; i++)
            {
                var next = current + "/" + parts[i];
                if (!AssetDatabase.IsValidFolder(next))
                {
                    AssetDatabase.CreateFolder(current, parts[i]);
                }

                current = next;
            }
        }

        private static void SetFloat(Material material, string propertyName, float value)
        {
            if (material.HasProperty(propertyName))
            {
                material.SetFloat(propertyName, value);
            }
        }

        private static void SetColor(Material material, string propertyName, Color value)
        {
            if (material.HasProperty(propertyName))
            {
                material.SetColor(propertyName, value);
            }
        }

        private static void SetTexture(Material material, string propertyName, Texture texture)
        {
            if (material.HasProperty(propertyName))
            {
                material.SetTexture(propertyName, texture);
            }
        }

        private static void SetTextureScale(Material material, string propertyName, Vector2 scale)
        {
            if (material.HasProperty(propertyName))
            {
                material.SetTextureScale(propertyName, scale);
            }
        }
    }
}
