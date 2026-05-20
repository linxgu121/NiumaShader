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

旧化混合顺序：

```text
BaseColor -> Detail -> Dirt -> Moss -> PaintFade -> RainStreak -> Lighting
```

## DetailMap

第一版 DetailMap 是细节颜色 / 细节噪声，不是 Detail Normal。

```text
RGB = 细节颜色乘算或细节噪声
A   = 细节混合遮罩
```
