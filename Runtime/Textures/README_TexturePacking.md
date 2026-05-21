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

## HeightMap / Parallax

2.0-B 阶段 HeightMap 只用于 Parallax 视差映射。

```text
R = 高度。黑色低、白色高
G = 暂不使用
B = 暂不使用
A = 暂不使用
```

制作建议：

- 砖缝、瓦沟、石雕浅槽画低值
- 瓦脊、砖面、石雕凸起画高值
- 中性高度建议接近 0.5，对应材质参数 `高度中心`
- 不要用 HeightMap 表达真实几何轮廓，Shader 不会改变阴影轮廓或碰撞
- 视差强度过大会出现边缘拉伸和贴图游泳，正式材质建议从 0.01 到 0.035 微调

## EmissionMap / Lantern（2.0-D 待做）

2.0-D 计划加入自发光 / 灯笼贴图，用于灯笼纸面、夜景窗纸、室内暖光溢出和牌匾金字轻微发光。

```text
RGB = 自发光颜色
A   = 自发光遮罩
```

制作建议：

- 自发光贴图只表达材质表面发亮，不负责照亮周围环境
- Bloom 由后处理承担，只扩散已经发亮的表面
- 2.0-D 不新增 Shader Keyword，不使用 `_NIUMA_EMISSION`
- 材质通过 `_EmissionStrength` 这个 uniform 参数控制是否采样和叠加自发光
