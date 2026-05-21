# NiumaShader 材质制作规范

本文档记录 Unity 工程内的实际材质制作约定。完整架构说明见外部技术设计文档 `NiumaShader技术设计.md`。

## 当前阶段

当前已进入 2.0-A 阶段，验证内容包括：

- `Niuma/Architecture/Lit` 能在 URP 下正常显示
- `BaseMap × BaseColor` 输出正确
- URP 主光、主光阴影、环境光 / Lightmap 基础接入
- Tangent Space NormalMap 接入
- Niuma MaskMap 的 AO / Smoothness 接入
- WeatherMap 的灰尘、苔痕、彩绘褪色、雨痕接入
- MaskMap.B 的边缘磨损接入
- 顶点色旧化加成接入
- ShadowCaster Pass 接入
- DepthOnly Pass 接入
- DepthNormals Pass 接入
- 所有 Pass 共用 `NiumaArchitectureMaterial.hlsl` 中的 UnityPerMaterial 布局
- ShaderGUI 能显示法线、MaskMap、旧化和调试视图分组
- ShaderGUI 能显示中文分组和 MaskMap 警告
- Editor 菜单可生成古建 Shader 验证场景
- Editor 菜单可生成 / 刷新木、瓦、石、墙四套模板材质
- Editor 菜单可生成 256×256 线性 MaskMap / WeatherMap 测试贴图
- 模板材质默认开启 GPU Instancing，方便重复瓦片、斗拱、栏杆等构件直接复用
- Editor 菜单可执行性能冻结检查，确认 Pass、CBUFFER、Instancing、DOTS、Variant 预算和 Additional Lights 策略没有被破坏
- Editor 菜单可生成 Instancing 压力测试场景，用于验证重复瓦片等构件的批处理预留
- 2.0 版本开始加入 DetailMap、Parallax、各向异性高光、自发光 / 灯笼四类增强能力
- 2.0-A 阶段先实现 DetailMap，用于解决瓦片、木纹、灰墙、石阶近景细节发糊
- `.meta` 文件由 Unity 自动生成

2.0-A 阶段暂不包含：

- Additional Lights
- 自定义 RenderFeature / RenderGraph Pass
- Parallax 视差映射
- 各向异性高光
- 自发光 / 灯笼

## 美术验证工具

Unity 导入脚本后，顶部菜单会出现：

```text
Niuma/Shader/生成古建 Shader 测试场景
Niuma/Shader/刷新古建 Shader 模板材质
Niuma/Shader/执行性能冻结检查
Niuma/Shader/生成 Instancing 压力测试场景
```

生成内容：

```text
Assets/Game/Moudle/NiumaShader/Runtime/Materials
  M_Wood_Painted_Template.mat
  M_RoofTile_BlueGray_Template.mat
  M_Stone_Step_Template.mat
  M_Wall_Lime_Template.mat

Assets/Game/Moudle/NiumaShader/Runtime/Textures/Test
  T_Niuma_Validation_Mask.png
  T_Niuma_Validation_Detail.png
  T_Niuma_Validation_Weather.png

Assets/Game/Moudle/NiumaShader/Runtime/TestScenes
  NiumaShader_Architecture_Test.unity
  NiumaShader_Instancing_Test.unity
```

这些 `.mat`、`.png`、`.unity` 和 `.meta` 文件都由 Unity 编辑器生成，不手写。

测试贴图尺寸为 256×256，并关闭压缩，避免近距离观察旧化渐变时把贴图像素块或压缩噪点误判为 Shader 问题。

建议验证顺序：

1. 点击 `Niuma/Shader/生成古建 Shader 测试场景`
2. 确认 Console 没有 Shader 或 C# 编译错误
3. 依次切换材质的调试视图：Final、BaseColor、Normal、AO、Smoothness、EdgeWear、Dirt、Moss、PaintFade、Rain、VertexColor
4. 开启场景 Lighting / Shadows，确认 ShadowCaster、DepthOnly、DepthNormals 没有粉材质或异常黑块
5. 点击 `Niuma/Shader/执行性能冻结检查`，确认 Console 中没有 Error
6. 点击 `Niuma/Shader/生成 Instancing 压力测试场景`，打开生成的场景检查 10×10 青瓦重复件是否正常显示

## 2.0 版本增强能力

2.0 版本计划加入：

```text
DetailMap              瓦片、木纹、灰墙、石阶的微观颜色细节
Parallax               砖缝、瓦沟、雕刻浅槽的视差深度感
Anisotropic Highlight  漆面、丝绸、湿润瓦片的方向性高光
Emission / Lantern     灯笼、窗纸、夜景建筑暖光
```

2.0-A 当前只实现 DetailMap。其余三项进入后续阶段，避免一次增加过多采样和 Keyword。

DetailMap 通道约定：

```text
RGB = 细节颜色乘算，0.5 为中性
A   = 细节混合遮罩
```

混合顺序：

```text
BaseColor -> Detail -> EdgeWear -> Dirt -> Moss -> PaintFade -> RainStreak -> Lighting
```

材质制作建议：

- 青瓦 DetailMap 可以使用细碎颗粒和轻微色差
- 木梁 DetailMap 可以使用细木纹和漆面微裂
- 灰墙 DetailMap 可以使用石灰颗粒，但不要画成强污渍，污渍仍交给 WeatherMap
- DetailMap 默认应接近 0.5 灰，避免整体颜色被过度压暗或提亮

## 性能冻结检查

第六阶段新增 `NiumaArchitecturePerformanceValidator`，用于把容易被后续修改破坏的性能约束固定下来。

检查内容：

```text
Shader.Find("Niuma/Architecture/Lit") 是否可用
UnityPerMaterial 是否只在 NiumaArchitectureMaterial.hlsl 中定义
ForwardLit / ShadowCaster / DepthOnly / DepthNormals 是否完整
每个 Pass 是否保留 multi_compile_instancing
每个 Pass 是否保留 DOTS include 预留
是否误开启 _ADDITIONAL_LIGHTS / _ADDITIONAL_LIGHT_SHADOWS pragma
shader_feature_local 理论 Variant 是否控制在 64 个以内
四套模板材质是否存在、Shader 是否正确、是否开启 GPU Instancing
```

使用规则：

- 修改 `.shader` 或 `.hlsl` 后，必须执行一次 `Niuma/Shader/执行性能冻结检查`
- 如果检查出现 Error，优先修复 Shader 结构，不要先调材质参数
- Warning 可以进入下一步验证，但需要在提交说明中写明原因
- `NiumaShader_Instancing_Test.unity` 只用于性能与批处理预留验证，不作为正式场景资产
- 2.0 版本 Variant 预算上限提升到 64；新增 Keyword 必须通过性能冻结检查

## 渲染 Pass

当前 Shader 包含：

```text
UniversalForward  正式渲染，负责基础光照、法线、MaskMap、WeatherMap
ShadowCaster      阴影贴图写入，使用 URP Shadow Bias
DepthOnly         深度预写入，服务深度纹理、遮挡和后处理
DepthNormals      法线写入，服务 SSAO 和法线相关效果
```

`DepthNormals` 会在 `_NIUMA_NORMALMAP` 开启时采样 Tangent Space NormalMap。这样 SSAO 等依赖法线的效果与 ForwardLit 中的法线方向保持一致。

## 旧化贴图规则

`_WeatherMap` 通道定义：

```text
R = 灰尘 / 积灰
G = 苔痕 / 潮湿
B = 彩绘褪色
A = 雨痕 / 预留
```

旧化混合顺序固定为：

```text
BaseColor -> EdgeWear -> Dirt -> Moss -> PaintFade -> RainStreak -> Lighting
```

顶点色只用于局部旧化加成：

```text
VertexColor.R = 灰尘 / 彩绘褪色加成
VertexColor.G = 苔痕加成
VertexColor.B = 边缘磨损加成
VertexColor.A = 预留
```

`顶点色旧化加成` 默认为 0，表示即使模型带顶点色，也不会意外影响材质。需要美术主动开启后才参与旧化。

`边缘磨损强度` 也默认为 0。原因是 `_MaskMap` 的默认贴图是白图，B 通道会是 1；如果默认开启边缘磨损，新建材质会整块变成磨损色。

## 材质创建位置

材质模板统一放在：

```text
D:\zhizuo\sava\NiumaM\Assets\Game\Moudle\NiumaShader\Runtime\Materials
```

第一阶段暂不手写 `.mat` 文件，等 Unity 导入 Shader 后再在编辑器中创建模板材质。
