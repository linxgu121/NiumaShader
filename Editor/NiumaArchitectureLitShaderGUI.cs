using UnityEditor;
using UnityEngine;

namespace NiumaShader.Editor
{
    /// <summary>
    /// Niuma 古建筑 Shader 的材质面板。
    /// 第一阶段只做分组、警告和基础参数展示，避免美术成员在默认 Inspector 中误填贴图通道。
    /// </summary>
    public sealed class NiumaArchitectureLitShaderGUI : ShaderGUI
    {
        private static readonly GUIContent SurfaceTypeLabel = new GUIContent("材质模板分类", "只用于编辑器分类和模板默认值，不参与 Fragment 动态分支。");
        private static readonly GUIContent BaseMapLabel = new GUIContent("基础颜色贴图", "sRGB 开启。不要把过重 AO 或场景光影烘进 BaseMap。");
        private static readonly GUIContent BaseColorLabel = new GUIContent("基础颜色乘色", "用于整体微调，不建议用它替代贴图本身的颜色设计。");
        private static readonly GUIContent PaintFadeSaturationLabel = new GUIContent("彩绘褪色保留饱和度", "默认 0.45。0 表示完全灰度化，1 表示保留原始饱和度。");
        private static readonly GUIContent DebugViewLabel = new GUIContent("调试视图", "第一阶段只预留 Final/BaseColor/VertexColor，后续阶段会扩展 MaskMap 与 WeatherMap 调试。");

        public override void OnGUI(MaterialEditor materialEditor, MaterialProperty[] properties)
        {
            if (materialEditor == null)
            {
                return;
            }

            var surfaceType = FindProperty("_SurfaceType", properties, false);
            var baseMap = FindProperty("_BaseMap", properties, false);
            var baseColor = FindProperty("_BaseColor", properties, false);
            var paintFadeSaturation = FindProperty("_PaintFadeSaturation", properties, false);
            var debugView = FindProperty("_DebugView", properties, false);

            EditorGUILayout.Space(4f);
            EditorGUILayout.HelpBox("Niuma 建筑 Shader 第一阶段：只输出 BaseMap × BaseColor，并保持 URP / SRP Batcher / Instancing 骨架稳定。", MessageType.Info);
            EditorGUILayout.HelpBox("警告：后续 Niuma MaskMap 与 URP Lit MaskMap 通道不同，不可直接混用。", MessageType.Warning);
            EditorGUILayout.HelpBox("_SurfaceType 只用于材质模板分类，不允许在 Fragment Shader 中做材质类型动态分支。", MessageType.None);

            DrawTemplateSection(materialEditor, surfaceType);
            DrawBaseSection(materialEditor, baseMap, baseColor);
            DrawWeatheringPlaceholderSection(materialEditor, paintFadeSaturation);
            DrawBatchingAndDebugSection(materialEditor, debugView);
        }

        private static void DrawTemplateSection(MaterialEditor materialEditor, MaterialProperty surfaceType)
        {
            EditorGUILayout.Space(6f);
            EditorGUILayout.LabelField("材质模板", EditorStyles.boldLabel);
            if (surfaceType != null)
            {
                materialEditor.ShaderProperty(surfaceType, SurfaceTypeLabel);
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

        private static void DrawWeatheringPlaceholderSection(MaterialEditor materialEditor, MaterialProperty paintFadeSaturation)
        {
            EditorGUILayout.Space(6f);
            EditorGUILayout.LabelField("旧化预留", EditorStyles.boldLabel);
            if (paintFadeSaturation != null)
            {
                materialEditor.ShaderProperty(paintFadeSaturation, PaintFadeSaturationLabel);
            }
        }

        private static void DrawBatchingAndDebugSection(MaterialEditor materialEditor, MaterialProperty debugView)
        {
            EditorGUILayout.Space(6f);
            EditorGUILayout.LabelField("批处理与调试", EditorStyles.boldLabel);
            materialEditor.EnableInstancingField();
            if (debugView != null)
            {
                materialEditor.ShaderProperty(debugView, DebugViewLabel);
            }
        }
    }
}
