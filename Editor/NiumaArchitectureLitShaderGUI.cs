using UnityEditor;
using UnityEngine;

namespace NiumaShader.Editor
{
    /// <summary>
    /// Niuma 古建筑 Shader 的材质面板。
    /// 2.0-C 阶段负责分组展示基础光照、法线、DetailMap、Parallax、各向异性高光、MaskMap 和旧化参数，避免美术成员在默认 Inspector 中误填贴图通道。
    /// </summary>
    public sealed class NiumaArchitectureLitShaderGUI : ShaderGUI
    {
        private static readonly string[] SurfaceTypeNames =
        {
            "Wood 木构",
            "Roof Tile 青瓦",
            "Stone 石材",
            "Wall 灰墙",
            "Painted 彩绘"
        };

        private static readonly string[] DebugViewNames =
        {
            "Final 最终",
            "BaseColor 基础色",
            "Normal 法线",
            "AO 遮蔽",
            "Smoothness 光滑度",
            "EdgeWear 边缘磨损",
            "Dirt 灰尘",
            "Moss 苔痕",
            "PaintFade 彩绘褪色",
            "Rain 雨痕",
            "VertexColor 顶点色",
            "Detail 细节遮罩",
            "ParallaxHeight 视差高度"
        };

        private static readonly GUIContent SurfaceTypeLabel = new GUIContent("材质模板分类", "只用于编辑器分类和模板默认值，不参与 Fragment 动态分支。");
        private static readonly GUIContent BaseMapLabel = new GUIContent("基础颜色贴图", "sRGB 开启。不要把过重 AO 或场景光影烘进 BaseMap。");
        private static readonly GUIContent BaseColorLabel = new GUIContent("基础颜色乘色", "用于整体微调，不建议用它替代贴图本身的颜色设计。");
        private static readonly GUIContent NormalMapLabel = new GUIContent("法线贴图", "仅支持 Tangent Space NormalMap。模型导入时必须包含 Tangent / Binormal。");
        private static readonly GUIContent NormalScaleLabel = new GUIContent("法线强度", "木纹、石材、青瓦可适当增强；彩绘表面不建议过强。");
        private static readonly GUIContent DetailMapLabel = new GUIContent("细节颜色贴图", "2.0-A 功能。RGB=细节颜色乘算，A=混合遮罩，0.5 灰为中性。");
        private static readonly GUIContent DetailStrengthLabel = new GUIContent("细节强度", "控制 DetailMap.A 的混合强度。用于近景瓦粒、木纹、灰墙颗粒，不承担旧化污渍语义。");
        private static readonly GUIContent HeightMapLabel = new GUIContent("高度贴图", "2.0-B 功能。R=高度，黑色低、白色高，用于砖缝、瓦沟、石雕浅槽的视差深度。");
        private static readonly GUIContent ParallaxStrengthLabel = new GUIContent("视差强度", "单步 Offset Mapping 强度。建议 0.01~0.035，过大会出现贴图游泳或边缘拉伸。");
        private static readonly GUIContent ParallaxCenterLabel = new GUIContent("高度中心", "默认 0.5。低于中心向内凹，高于中心向外凸，用于控制 HeightMap 的中性高度。");
        private static readonly GUIContent AnisotropyStrengthLabel = new GUIContent("各向异性高光强度", "2.0-C 功能。用于漆面木梁、湿润青瓦、丝绸装饰附近材质的方向性拉丝高光。");
        private static readonly GUIContent AnisotropyDirectionLabel = new GUIContent("高光方向混合", "0 沿模型 Tangent，1 沿模型 Bitangent。模型 Tangent 不稳定时方向会错误。");
        private static readonly GUIContent AnisotropyColorLabel = new GUIContent("各向异性高光颜色", "方向性高光的颜色。漆面可偏暖，湿瓦可偏冷灰。");
        private static readonly GUIContent MaskMapLabel = new GUIContent("Niuma 遮罩贴图", "通道：R=AO，G=Smoothness，B=EdgeWear/Damage，A=Reserved。不可直接使用 URP Lit MaskMap。");
        private static readonly GUIContent OcclusionStrengthLabel = new GUIContent("AO 强度", "控制 MaskMap.R 对间接光的遮蔽强度。");
        private static readonly GUIContent SmoothnessLabel = new GUIContent("整体光滑度", "与 MaskMap.G 相乘得到最终光滑度；青瓦可略高，灰墙和石材应较低。");
        private static readonly GUIContent WeatherMapLabel = new GUIContent("旧化遮罩贴图", "通道：R=灰尘，G=苔痕，B=彩绘褪色，A=雨痕/保留。");
        private static readonly GUIContent WeatherStrengthLabel = new GUIContent("整体旧化强度", "统一控制 WeatherMap 所有旧化效果的总强度。");
        private static readonly GUIContent DirtColorLabel = new GUIContent("灰尘颜色", "用于墙脚、屋檐下、石缝和斗拱转角积灰。");
        private static readonly GUIContent DirtStrengthLabel = new GUIContent("灰尘强度", "与 WeatherMap.R 相乘。");
        private static readonly GUIContent MossColorLabel = new GUIContent("苔痕颜色", "用于潮湿石材、墙脚和瓦沟的青苔色调。");
        private static readonly GUIContent MossStrengthLabel = new GUIContent("苔痕强度", "与 WeatherMap.G 相乘。");
        private static readonly GUIContent PaintAgeStrengthLabel = new GUIContent("彩绘褪色强度", "与 WeatherMap.B 相乘，用于梁枋彩绘的旧化降饱和。");
        private static readonly GUIContent PaintFadeSaturationLabel = new GUIContent("彩绘褪色保留饱和度", "默认 0.45。0 表示完全灰度化，1 表示保留原始饱和度。");
        private static readonly GUIContent RainStreakStrengthLabel = new GUIContent("雨痕压暗强度", "与 WeatherMap.A 相乘，第一版只做轻微压暗。");
        private static readonly GUIContent EdgeWearColorLabel = new GUIContent("边缘磨损颜色", "用于石阶、木梁、瓦片边缘的磨损露底色。");
        private static readonly GUIContent EdgeWearStrengthLabel = new GUIContent("边缘磨损强度", "与 MaskMap.B 相乘。");
        private static readonly GUIContent VertexWeatherStrengthLabel = new GUIContent("顶点色旧化加成", "0 表示忽略顶点色；R 加强灰尘/褪色，G 加强苔痕，B 加强边缘磨损。");
        private static readonly GUIContent DebugViewLabel = new GUIContent("调试视图", "用于检查基础色、法线、AO、光滑度、旧化遮罩、顶点色、DetailMap 和视差高度。各向异性效果请在 Final 下旋转视角观察。");

        public override void OnGUI(MaterialEditor materialEditor, MaterialProperty[] properties)
        {
            if (materialEditor == null)
            {
                return;
            }

            var surfaceType = FindProperty("_SurfaceType", properties, false);
            var baseMap = FindProperty("_BaseMap", properties, false);
            var baseColor = FindProperty("_BaseColor", properties, false);
            var normalMap = FindProperty("_NormalMap", properties, false);
            var normalScale = FindProperty("_NormalScale", properties, false);
            var detailMap = FindProperty("_DetailMap", properties, false);
            var detailStrength = FindProperty("_DetailStrength", properties, false);
            var heightMap = FindProperty("_HeightMap", properties, false);
            var parallaxStrength = FindProperty("_ParallaxStrength", properties, false);
            var parallaxCenter = FindProperty("_ParallaxCenter", properties, false);
            var anisotropyStrength = FindProperty("_AnisotropyStrength", properties, false);
            var anisotropyDirection = FindProperty("_AnisotropyDirection", properties, false);
            var anisotropyColor = FindProperty("_AnisotropyColor", properties, false);
            var maskMap = FindProperty("_MaskMap", properties, false);
            var occlusionStrength = FindProperty("_OcclusionStrength", properties, false);
            var smoothness = FindProperty("_Smoothness", properties, false);
            var weatherMap = FindProperty("_WeatherMap", properties, false);
            var weatherStrength = FindProperty("_WeatherStrength", properties, false);
            var dirtColor = FindProperty("_DirtColor", properties, false);
            var dirtStrength = FindProperty("_DirtStrength", properties, false);
            var mossColor = FindProperty("_MossColor", properties, false);
            var mossStrength = FindProperty("_MossStrength", properties, false);
            var paintAgeStrength = FindProperty("_PaintAgeStrength", properties, false);
            var paintFadeSaturation = FindProperty("_PaintFadeSaturation", properties, false);
            var rainStreakStrength = FindProperty("_RainStreakStrength", properties, false);
            var edgeWearColor = FindProperty("_EdgeWearColor", properties, false);
            var edgeWearStrength = FindProperty("_EdgeWearStrength", properties, false);
            var vertexWeatherStrength = FindProperty("_VertexWeatherStrength", properties, false);
            var debugView = FindProperty("_DebugView", properties, false);

            EditorGUILayout.Space(4f);
            EditorGUILayout.HelpBox("Niuma 建筑 Shader 2.0-C：已接入各向异性高光，用于漆面、湿瓦和丝绸相关材质的方向性反光。", MessageType.Info);
            EditorGUILayout.HelpBox("警告：Niuma MaskMap 与 URP Lit MaskMap 通道不同，不可直接混用。", MessageType.Warning);
            EditorGUILayout.HelpBox("_SurfaceType 只用于材质模板分类，不允许在 Fragment Shader 中做材质类型动态分支。", MessageType.None);

            DrawTemplateSection(materialEditor, surfaceType);
            DrawBaseSection(materialEditor, baseMap, baseColor);
            DrawNormalSection(materialEditor, normalMap, normalScale);
            DrawDetailSection(materialEditor, detailMap, detailStrength);
            DrawParallaxSection(materialEditor, heightMap, parallaxStrength, parallaxCenter);
            DrawAnisotropySection(materialEditor, anisotropyStrength, anisotropyDirection, anisotropyColor);
            DrawMaskSection(materialEditor, maskMap, occlusionStrength, smoothness);
            DrawWeatheringSection(
                materialEditor,
                weatherMap,
                weatherStrength,
                dirtColor,
                dirtStrength,
                mossColor,
                mossStrength,
                paintAgeStrength,
                paintFadeSaturation,
                rainStreakStrength,
                edgeWearColor,
                edgeWearStrength,
                vertexWeatherStrength);
            DrawBatchingAndDebugSection(materialEditor, debugView);
            ApplyKeywords(materialEditor.targets);
        }

        private static void DrawTemplateSection(MaterialEditor materialEditor, MaterialProperty surfaceType)
        {
            EditorGUILayout.Space(6f);
            EditorGUILayout.LabelField("材质模板", EditorStyles.boldLabel);
            if (surfaceType != null)
            {
                // 自定义 ShaderGUI 中 ShaderProperty 偶尔会忽略 [Enum] 特性并退化为 float。
                // 这里显式绘制 Popup，确保团队成员看到稳定的材质分类下拉框。
                EditorGUI.BeginChangeCheck();
                var current = Mathf.Clamp(Mathf.RoundToInt(surfaceType.floatValue), 0, SurfaceTypeNames.Length - 1);
                var selected = EditorGUILayout.Popup(SurfaceTypeLabel, current, SurfaceTypeNames);
                if (EditorGUI.EndChangeCheck())
                {
                    surfaceType.floatValue = selected;
                }
            }
        }

        private static void DrawBaseSection(MaterialEditor materialEditor, MaterialProperty baseMap, MaterialProperty baseColor)
        {
            EditorGUILayout.Space(6f);
            EditorGUILayout.LabelField("基础贴图", EditorStyles.boldLabel);
            if (baseMap != null && baseColor != null)
            {
                materialEditor.TexturePropertySingleLine(BaseMapLabel, baseMap, baseColor);
                materialEditor.TextureScaleOffsetProperty(baseMap);
            }
            else if (baseColor != null)
            {
                materialEditor.ShaderProperty(baseColor, BaseColorLabel);
            }
        }

        private static void DrawNormalSection(MaterialEditor materialEditor, MaterialProperty normalMap, MaterialProperty normalScale)
        {
            EditorGUILayout.Space(6f);
            EditorGUILayout.LabelField("法线", EditorStyles.boldLabel);
            if (normalMap != null)
            {
                if (normalScale != null)
                {
                    materialEditor.TexturePropertySingleLine(NormalMapLabel, normalMap, normalScale);
                }
                else
                {
                    materialEditor.TexturePropertySingleLine(NormalMapLabel, normalMap);
                }
            }
        }

        private static void DrawDetailSection(MaterialEditor materialEditor, MaterialProperty detailMap, MaterialProperty detailStrength)
        {
            EditorGUILayout.Space(6f);
            EditorGUILayout.LabelField("细节贴图 2.0", EditorStyles.boldLabel);
            EditorGUILayout.HelpBox("DetailMap：RGB=细节颜色乘算，A=细节遮罩。0.5 灰为中性，适合近景瓦粒、木纹和墙面颗粒。", MessageType.Info);
            if (detailMap != null)
            {
                if (detailStrength != null)
                {
                    materialEditor.TexturePropertySingleLine(DetailMapLabel, detailMap, detailStrength);
                }
                else
                {
                    materialEditor.TexturePropertySingleLine(DetailMapLabel, detailMap);
                }

                materialEditor.TextureScaleOffsetProperty(detailMap);
            }
        }

        private static void DrawParallaxSection(
            MaterialEditor materialEditor,
            MaterialProperty heightMap,
            MaterialProperty parallaxStrength,
            MaterialProperty parallaxCenter)
        {
            EditorGUILayout.Space(6f);
            EditorGUILayout.LabelField("视差映射 2.0-B", EditorStyles.boldLabel);
            EditorGUILayout.HelpBox("Parallax 只移动采样 UV，不改变模型轮廓、阴影轮廓和真实碰撞。需要模型有稳定 Tangent。", MessageType.Info);
            if (heightMap != null)
            {
                if (parallaxStrength != null)
                {
                    materialEditor.TexturePropertySingleLine(HeightMapLabel, heightMap, parallaxStrength);
                }
                else
                {
                    materialEditor.TexturePropertySingleLine(HeightMapLabel, heightMap);
                }

                materialEditor.TextureScaleOffsetProperty(heightMap);
            }

            DrawProperty(materialEditor, parallaxCenter, ParallaxCenterLabel);
        }

        private static void DrawAnisotropySection(
            MaterialEditor materialEditor,
            MaterialProperty anisotropyStrength,
            MaterialProperty anisotropyDirection,
            MaterialProperty anisotropyColor)
        {
            EditorGUILayout.Space(6f);
            EditorGUILayout.LabelField("各向异性高光 2.0-C", EditorStyles.boldLabel);
            EditorGUILayout.HelpBox("各向异性高光依赖模型 Tangent。适合漆面、湿瓦、丝绸装饰等少量材质，不建议给灰墙和普通石材默认开启。", MessageType.Info);
            DrawProperty(materialEditor, anisotropyStrength, AnisotropyStrengthLabel);
            DrawProperty(materialEditor, anisotropyDirection, AnisotropyDirectionLabel);
            DrawProperty(materialEditor, anisotropyColor, AnisotropyColorLabel);
        }

        private static void DrawMaskSection(MaterialEditor materialEditor, MaterialProperty maskMap, MaterialProperty occlusionStrength, MaterialProperty smoothness)
        {
            EditorGUILayout.Space(6f);
            EditorGUILayout.LabelField("Niuma MaskMap", EditorStyles.boldLabel);
            EditorGUILayout.HelpBox("通道定义：R=AO，G=Smoothness，B=EdgeWear/Damage，A=Reserved。它不是 URP Lit 标准 MaskMap。", MessageType.Warning);
            if (maskMap != null)
            {
                materialEditor.TexturePropertySingleLine(MaskMapLabel, maskMap);
            }

            if (occlusionStrength != null)
            {
                materialEditor.ShaderProperty(occlusionStrength, OcclusionStrengthLabel);
            }

            if (smoothness != null)
            {
                materialEditor.ShaderProperty(smoothness, SmoothnessLabel);
            }
        }

        private static void DrawWeatheringSection(
            MaterialEditor materialEditor,
            MaterialProperty weatherMap,
            MaterialProperty weatherStrength,
            MaterialProperty dirtColor,
            MaterialProperty dirtStrength,
            MaterialProperty mossColor,
            MaterialProperty mossStrength,
            MaterialProperty paintAgeStrength,
            MaterialProperty paintFadeSaturation,
            MaterialProperty rainStreakStrength,
            MaterialProperty edgeWearColor,
            MaterialProperty edgeWearStrength,
            MaterialProperty vertexWeatherStrength)
        {
            EditorGUILayout.Space(6f);
            EditorGUILayout.LabelField("旧化", EditorStyles.boldLabel);
            EditorGUILayout.HelpBox("WeatherMap 通道：R=灰尘，G=苔痕，B=彩绘褪色，A=雨痕。顶点色只做局部旧化加成，不参与材质类型分支。", MessageType.Info);
            if (weatherMap != null)
            {
                materialEditor.TexturePropertySingleLine(WeatherMapLabel, weatherMap);
                materialEditor.TextureScaleOffsetProperty(weatherMap);
            }

            DrawProperty(materialEditor, weatherStrength, WeatherStrengthLabel);
            DrawProperty(materialEditor, dirtColor, DirtColorLabel);
            DrawProperty(materialEditor, dirtStrength, DirtStrengthLabel);
            DrawProperty(materialEditor, mossColor, MossColorLabel);
            DrawProperty(materialEditor, mossStrength, MossStrengthLabel);
            DrawProperty(materialEditor, paintAgeStrength, PaintAgeStrengthLabel);
            DrawProperty(materialEditor, paintFadeSaturation, PaintFadeSaturationLabel);
            DrawProperty(materialEditor, rainStreakStrength, RainStreakStrengthLabel);
            DrawProperty(materialEditor, edgeWearColor, EdgeWearColorLabel);
            DrawProperty(materialEditor, edgeWearStrength, EdgeWearStrengthLabel);
            DrawProperty(materialEditor, vertexWeatherStrength, VertexWeatherStrengthLabel);
        }

        private static void DrawProperty(MaterialEditor materialEditor, MaterialProperty property, GUIContent label)
        {
            if (property != null)
            {
                materialEditor.ShaderProperty(property, label);
            }
        }

        private static void DrawBatchingAndDebugSection(MaterialEditor materialEditor, MaterialProperty debugView)
        {
            EditorGUILayout.Space(6f);
            EditorGUILayout.LabelField("批处理与调试", EditorStyles.boldLabel);
            materialEditor.EnableInstancingField();
            if (debugView != null)
            {
                EditorGUI.BeginChangeCheck();
                var current = Mathf.Clamp(Mathf.RoundToInt(debugView.floatValue), 0, DebugViewNames.Length - 1);
                var selected = EditorGUILayout.Popup(DebugViewLabel, current, DebugViewNames);
                if (EditorGUI.EndChangeCheck())
                {
                    debugView.floatValue = selected;
                }
            }
        }

        private static void ApplyKeywords(UnityEngine.Object[] targets)
        {
            if (targets == null)
            {
                return;
            }

            foreach (var target in targets)
            {
                var material = target as Material;
                if (material == null)
                {
                    continue;
                }

                SetKeyword(material, "_NIUMA_NORMALMAP", material.GetTexture("_NormalMap") != null);
                SetKeyword(material, "_NIUMA_WEATHERING", material.GetTexture("_WeatherMap") != null);
                SetKeyword(material, "_NIUMA_DETAILMAP", material.GetTexture("_DetailMap") != null);
                var parallaxEnabled = material.HasProperty("_HeightMap")
                                      && material.GetTexture("_HeightMap") != null
                                      && material.HasProperty("_ParallaxStrength")
                                      && material.GetFloat("_ParallaxStrength") > 0f;
                SetKeyword(material, "_NIUMA_PARALLAX", parallaxEnabled);

                var anisotropyEnabled = material.HasProperty("_AnisotropyStrength")
                                        && material.GetFloat("_AnisotropyStrength") > 0f;
                SetKeyword(material, "_NIUMA_ANISOTROPY", anisotropyEnabled);
            }
        }

        private static void SetKeyword(Material material, string keyword, bool enabled)
        {
            if (enabled)
            {
                material.EnableKeyword(keyword);
            }
            else
            {
                material.DisableKeyword(keyword);
            }
        }
    }
}
