# NiumaShader 贴图打包规范

## 重要警告

`Niuma/Architecture/Lit` 的 `_MaskMap` 通道定义与 URP Lit 标准 MaskMap 不同，不可直接混用。

URP Lit 常见通道：

```text
R = Metallic
G = Occlusion
B = Detail Mask
A = Smoothness
```

Niuma 建筑 Shader 通道：

```text
R = Ambient Occlusion
G = Smoothness
B = Edge Wear / Damage
A = Material Blend / Reserved
```

## BaseMap

- sRGB 开启
- 不要把强 AO 或固定光影烘进 BaseMap
- 彩绘纹样应保留清晰边界

## NormalMap

- 第一版仅支持 Tangent Space NormalMap
- Texture Type 必须设置为 Normal Map
- 模型导入时必须包含 Tangent / Binormal
- 不支持 Object Space NormalMap

## WeatherMap

```text
R = Dirt / Dust
G = Moss / Humidity
B = Paint Fade
A = Rain Streak / Reserved
```

中文约定：

```text
R = 灰尘 / 积灰
G = 苔痕 / 潮湿
B = 彩绘褪色
A = 雨痕 / 预留
```

旧化混合顺序：

```text
BaseColor -> Detail -> EdgeWear -> Dirt -> Moss -> PaintFade -> RainStreak -> Lighting
```

注意：边缘磨损来自 MaskMap.B，不来自 WeatherMap。

## VertexColor

顶点色只用于旧化局部加成，不用于材质类型分支。

```text
R = 灰尘 / 彩绘褪色加成
G = 苔痕加成
B = 边缘磨损加成
A = 预留
```

材质上的 `顶点色旧化加成` 默认为 0，需要主动调高才会生效。

## DetailMap

2.0-A 阶段 DetailMap 是细节颜色 / 细节噪声，不是 Detail Normal。

```text
RGB = 细节颜色乘算。0.5 为中性，低于 0.5 压暗，高于 0.5 提亮
A   = 细节混合遮罩
```

使用建议：

- 青瓦、木纹、灰墙、石阶近景材质优先使用
- 不要把灰尘、苔痕、雨痕画进 DetailMap，这些仍交给 WeatherMap
- DetailMap 可以提高 Tiling，但不要用它替代 BaseMap 的主体纹样
