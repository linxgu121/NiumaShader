# NiumaShader 材质制作规范

本文档记录 Unity 工程内的实际材质制作约定。完整架构说明见外部技术设计文档 `NiumaShader技术设计.md`。

## 当前阶段

当前已进入第三阶段，验证内容包括：

- `Niuma/Architecture/Lit` 能在 URP 下正常显示
- `BaseMap × BaseColor` 输出正确
- URP 主光、主光阴影、环境光 / Lightmap 基础接入
- Tangent Space NormalMap 接入
- Niuma MaskMap 的 AO / Smoothness 接入
- WeatherMap 的灰尘、苔痕、彩绘褪色、雨痕接入
- MaskMap.B 的边缘磨损接入
- 顶点色旧化加成接入
- ShaderGUI 能显示法线、MaskMap、旧化和调试视图分组
- ShaderGUI 能显示中文分组和 MaskMap 警告
- `.meta` 文件由 Unity 自动生成

第三阶段暂不包含：

- DetailMap
- Additional Lights
- ShadowCaster / DepthOnly / DepthNormals 独立 Pass

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
