using System.Collections.Generic;
using System.IO;
using System.Text;
using UnityEditor;
using UnityEditor.SceneManagement;
using UnityEngine;
using UnityEngine.SceneManagement;

namespace NiumaShader.Editor
{
    /// <summary>
    /// 第六阶段性能冻结检查工具。
    /// 用于在团队修改 Shader 后快速确认变体数量、批处理预留和 Pass 完整性没有被破坏。
    /// </summary>
    public static class NiumaArchitecturePerformanceValidator
    {
        private const string ShaderName = "Niuma/Architecture/Lit";
        private const string ModuleRoot = "Assets/Game/Moudle/NiumaShader";
        private const string ShaderFolder = ModuleRoot + "/Runtime/Shaders";
        private const string ShaderPath = ShaderFolder + "/NiumaArchitectureLit.shader";
        private const string MaterialSharedPath = ShaderFolder + "/NiumaArchitectureMaterial.hlsl";
        private const string MaterialsFolder = ModuleRoot + "/Runtime/Materials";
        private const string TestScenesFolder = ModuleRoot + "/Runtime/TestScenes";
        private const string InstancingScenePath = TestScenesFolder + "/NiumaShader_Instancing_Test.unity";
        private const int MaxLocalShaderFeatureCount = 4;
        private const int ExpectedRenderPassCount = 4;

        [MenuItem("Niuma/Shader/执行性能冻结检查")]
        public static void RunPerformanceFreezeCheck()
        {
            var report = new PerformanceReport();

            CheckShaderAsset(report);
            CheckSharedMaterialBuffer(report);
            CheckRenderPasses(report);
            CheckInstancingAndDots(report);
            CheckAdditionalLightsPolicy(report);
            CheckShaderVariantBudget(report);
            CheckTemplateMaterials(report);

            report.LogToConsole();

            if (report.ErrorCount > 0)
            {
                EditorUtility.DisplayDialog("NiumaShader 性能冻结检查", "检查完成，但存在错误。请查看 Console。", "确定");
            }
            else if (report.WarningCount > 0)
            {
                EditorUtility.DisplayDialog("NiumaShader 性能冻结检查", "检查通过，但有警告。请查看 Console。", "确定");
            }
            else
            {
                EditorUtility.DisplayDialog("NiumaShader 性能冻结检查", "检查通过。", "确定");
            }
        }

        [MenuItem("Niuma/Shader/生成 Instancing 压力测试场景")]
        public static void CreateInstancingStressScene()
        {
            var material = AssetDatabase.LoadAssetAtPath<Material>(MaterialsFolder + "/M_RoofTile_BlueGray_Template.mat");
            if (material == null)
            {
                EditorUtility.DisplayDialog("NiumaShader", "没有找到青瓦模板材质。请先执行：Niuma/Shader/生成古建 Shader 测试场景 或 刷新古建 Shader 模板材质。", "确定");
                return;
            }

            if (!EditorSceneManager.SaveCurrentModifiedScenesIfUserWantsTo())
            {
                return;
            }

            EnsureFolder(TestScenesFolder);
            material.enableInstancing = true;
            EditorUtility.SetDirty(material);

            var scene = EditorSceneManager.NewScene(NewSceneSetup.EmptyScene, NewSceneMode.Single);
            CreateLighting();
            CreateCamera();

            var root = new GameObject("Instancing_重复青瓦_10x10");
            const int grid = 10;
            const float spacing = 1.25f;
            for (var z = 0; z < grid; z++)
            {
                for (var x = 0; x < grid; x++)
                {
                    var tile = GameObject.CreatePrimitive(PrimitiveType.Cube);
                    tile.name = "青瓦重复件";
                    tile.transform.SetParent(root.transform);
                    tile.transform.position = new Vector3((x - grid * 0.5f) * spacing, 0.12f, z * spacing);
                    tile.transform.localScale = new Vector3(1.0f, 0.18f, 0.9f);
                    tile.transform.rotation = Quaternion.Euler(0f, 0f, -12f);

                    var renderer = tile.GetComponent<Renderer>();
                    if (renderer != null)
                    {
                        renderer.sharedMaterial = material;
                    }
                }
            }

            EditorSceneManager.SaveScene(scene, InstancingScenePath);
            AssetDatabase.SaveAssets();
            AssetDatabase.Refresh();
            EditorUtility.DisplayDialog("NiumaShader", "Instancing 压力测试场景已生成：\n" + InstancingScenePath, "确定");
        }

        private static void CheckShaderAsset(PerformanceReport report)
        {
            var shader = Shader.Find(ShaderName);
            if (shader == null)
            {
                report.Error("没有找到 Shader：" + ShaderName);
                return;
            }

            report.Pass("Shader 可通过 Shader.Find 找到：" + ShaderName);

            if (!File.Exists(ToAbsolutePath(ShaderPath)))
            {
                report.Error("找不到 Shader 源文件：" + ShaderPath);
            }
        }

        private static void CheckSharedMaterialBuffer(PerformanceReport report)
        {
            var shaderFolderPath = ToAbsolutePath(ShaderFolder);
            if (!Directory.Exists(shaderFolderPath))
            {
                report.Error("找不到 Shader 文件夹：" + ShaderFolder);
                return;
            }

            var files = Directory.GetFiles(shaderFolderPath, "*.*", SearchOption.TopDirectoryOnly);
            var cbufferOwners = new List<string>();
            foreach (var file in files)
            {
                var extension = Path.GetExtension(file);
                if (extension != ".hlsl" && extension != ".shader")
                {
                    continue;
                }

                var content = File.ReadAllText(file);
                if (content.Contains("CBUFFER_START(UnityPerMaterial)"))
                {
                    cbufferOwners.Add(Path.GetFileName(file));
                }
            }

            if (cbufferOwners.Count != 1 || cbufferOwners[0] != Path.GetFileName(MaterialSharedPath))
            {
                report.Error("UnityPerMaterial 必须只在 NiumaArchitectureMaterial.hlsl 中定义。当前定义位置：" + string.Join(", ", cbufferOwners.ToArray()));
                return;
            }

            report.Pass("UnityPerMaterial 只在共享材质文件中定义。");
        }

        private static void CheckRenderPasses(PerformanceReport report)
        {
            var shaderSource = ReadAssetText(ShaderPath);
            var passNames = new[] { "ForwardLit", "ShadowCaster", "DepthOnly", "DepthNormals" };
            var lightModes = new[] { "UniversalForward", "ShadowCaster", "DepthOnly", "DepthNormals" };

            for (var i = 0; i < passNames.Length; i++)
            {
                CheckContains(report, shaderSource, "Name \"" + passNames[i] + "\"", "Pass 存在：" + passNames[i]);
                CheckContains(report, shaderSource, "\"LightMode\" = \"" + lightModes[i] + "\"", "LightMode 正确：" + lightModes[i]);
            }
        }

        private static void CheckInstancingAndDots(PerformanceReport report)
        {
            var shaderSource = ReadAssetText(ShaderPath);
            var instancingCount = CountOccurrences(shaderSource, "#pragma multi_compile_instancing");
            var dotsCount = CountOccurrences(shaderSource, "include_with_pragmas \"Packages/com.unity.render-pipelines.universal/ShaderLibrary/DOTS.hlsl\"");

            if (instancingCount < ExpectedRenderPassCount)
            {
                report.Error("每个 Pass 都应保留 multi_compile_instancing。当前数量：" + instancingCount);
            }
            else
            {
                report.Pass("Instancing pragma 数量满足 Pass 覆盖：" + instancingCount);
            }

            if (dotsCount < ExpectedRenderPassCount)
            {
                report.Warning("DOTS include 预留数量少于 Pass 数。当前数量：" + dotsCount);
            }
            else
            {
                report.Pass("DOTS include 预留数量满足 Pass 覆盖：" + dotsCount);
            }
        }

        private static void CheckAdditionalLightsPolicy(PerformanceReport report)
        {
            var shaderSource = ReadAssetText(ShaderPath);
            var activeAdditionalLightPragmas = new List<string>();
            var lines = shaderSource.Split('\n');
            for (var i = 0; i < lines.Length; i++)
            {
                var line = lines[i].Trim();
                if (line.StartsWith("//") || !line.Contains("#pragma"))
                {
                    continue;
                }

                if (line.Contains("_ADDITIONAL_LIGHTS") || line.Contains("_ADDITIONAL_LIGHT_SHADOWS"))
                {
                    activeAdditionalLightPragmas.Add("Line " + (i + 1) + ": " + line);
                }
            }

            if (activeAdditionalLightPragmas.Count > 0)
            {
                report.Error("建筑 Shader 第一版默认不允许启用 Additional Lights。发现：" + string.Join(" | ", activeAdditionalLightPragmas.ToArray()));
            }
            else
            {
                report.Pass("未发现启用 Additional Lights 的有效 pragma。");
            }
        }

        private static void CheckShaderVariantBudget(PerformanceReport report)
        {
            var shaderSource = ReadAssetText(ShaderPath);
            var keywords = new HashSet<string>();
            var lines = shaderSource.Split('\n');
            foreach (var lineRaw in lines)
            {
                var line = lineRaw.Trim();
                if (line.StartsWith("//") || !line.Contains("#pragma shader_feature_local"))
                {
                    continue;
                }

                var parts = line.Split(new[] { ' ', '\t' }, System.StringSplitOptions.RemoveEmptyEntries);
                for (var i = 0; i < parts.Length; i++)
                {
                    if (parts[i].StartsWith("_NIUMA_"))
                    {
                        keywords.Add(parts[i]);
                    }
                }
            }

            var variantBudget = 1 << keywords.Count;
            variantBudget *= 2; // Instancing 维度。

            if (keywords.Count > MaxLocalShaderFeatureCount || variantBudget > 32)
            {
                report.Error("Shader Variant 超出预算。Local Keyword 数：" + keywords.Count + "，理论 Variant：" + variantBudget);
                return;
            }

            report.Pass("Shader Variant 预算通过。Local Keyword 数：" + keywords.Count + "，理论 Variant：" + variantBudget + " / 32");
        }

        private static void CheckTemplateMaterials(PerformanceReport report)
        {
            var expectedMaterials = new[]
            {
                "M_Wood_Painted_Template",
                "M_RoofTile_BlueGray_Template",
                "M_Stone_Step_Template",
                "M_Wall_Lime_Template"
            };

            for (var i = 0; i < expectedMaterials.Length; i++)
            {
                var materialPath = MaterialsFolder + "/" + expectedMaterials[i] + ".mat";
                var material = AssetDatabase.LoadAssetAtPath<Material>(materialPath);
                if (material == null)
                {
                    report.Warning("模板材质尚未生成：" + materialPath);
                    continue;
                }

                if (material.shader == null || material.shader.name != ShaderName)
                {
                    report.Error("模板材质 Shader 不正确：" + materialPath);
                }
                else if (!material.enableInstancing)
                {
                    report.Warning("模板材质未开启 GPU Instancing：" + materialPath);
                }
                else
                {
                    report.Pass("模板材质通过：" + materialPath);
                }
            }
        }

        private static void CheckContains(PerformanceReport report, string content, string token, string successMessage)
        {
            if (content.Contains(token))
            {
                report.Pass(successMessage);
            }
            else
            {
                report.Error("缺少：" + token);
            }
        }

        private static string ReadAssetText(string assetPath)
        {
            var path = ToAbsolutePath(assetPath);
            return File.Exists(path) ? File.ReadAllText(path) : string.Empty;
        }

        private static int CountOccurrences(string text, string token)
        {
            if (string.IsNullOrEmpty(text) || string.IsNullOrEmpty(token))
            {
                return 0;
            }

            var count = 0;
            var index = 0;
            while ((index = text.IndexOf(token, index, System.StringComparison.Ordinal)) >= 0)
            {
                count++;
                index += token.Length;
            }

            return count;
        }

        private static string ToAbsolutePath(string assetPath)
        {
            var projectRoot = Directory.GetParent(Application.dataPath).FullName;
            return Path.Combine(projectRoot, assetPath);
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

        private static void CreateLighting()
        {
            var lightObject = new GameObject("主方向光");
            var light = lightObject.AddComponent<Light>();
            light.type = LightType.Directional;
            light.intensity = 2.0f;
            light.color = new Color(1f, 0.92f, 0.82f, 1f);
            light.shadows = LightShadows.Soft;
            lightObject.transform.rotation = Quaternion.Euler(50f, -30f, 0f);
        }

        private static void CreateCamera()
        {
            var cameraObject = new GameObject("Instancing 测试相机");
            var camera = cameraObject.AddComponent<Camera>();
            camera.clearFlags = CameraClearFlags.Skybox;
            camera.fieldOfView = 45f;
            cameraObject.transform.position = new Vector3(0f, 8.0f, -11.5f);
            cameraObject.transform.rotation = Quaternion.Euler(36f, 0f, 0f);
            cameraObject.tag = "MainCamera";
        }

        private sealed class PerformanceReport
        {
            private readonly List<string> _passes = new List<string>();
            private readonly List<string> _warnings = new List<string>();
            private readonly List<string> _errors = new List<string>();

            public int WarningCount
            {
                get { return _warnings.Count; }
            }

            public int ErrorCount
            {
                get { return _errors.Count; }
            }

            public void Pass(string message)
            {
                _passes.Add(message);
            }

            public void Warning(string message)
            {
                _warnings.Add(message);
            }

            public void Error(string message)
            {
                _errors.Add(message);
            }

            public void LogToConsole()
            {
                var builder = new StringBuilder();
                builder.AppendLine("NiumaShader 性能冻结检查报告");
                builder.AppendLine("通过：" + _passes.Count);
                builder.AppendLine("警告：" + _warnings.Count);
                builder.AppendLine("错误：" + _errors.Count);

                AppendSection(builder, "通过项", _passes);
                AppendSection(builder, "警告项", _warnings);
                AppendSection(builder, "错误项", _errors);

                if (_errors.Count > 0)
                {
                    Debug.LogError(builder.ToString());
                }
                else if (_warnings.Count > 0)
                {
                    Debug.LogWarning(builder.ToString());
                }
                else
                {
                    Debug.Log(builder.ToString());
                }
            }

            private static void AppendSection(StringBuilder builder, string title, List<string> items)
            {
                if (items.Count == 0)
                {
                    return;
                }

                builder.AppendLine();
                builder.AppendLine(title + "：");
                for (var i = 0; i < items.Count; i++)
                {
                    builder.AppendLine("- " + items[i]);
                }
            }
        }
    }
}
