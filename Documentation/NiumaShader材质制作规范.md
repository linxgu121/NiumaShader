# NiumaShader 材质制作规范

本文档记录 Unity 工程内的实际材质制作约定。完整架构说明见外部技术设计文档 `NiumaShader技术设计.md`。

## 当前阶段

第一阶段只验证：

- `Niuma/Architecture/Lit` 能在 URP 下正常显示
- `BaseMap × BaseColor` 输出正确
- ShaderGUI 能显示中文分组和 MaskMap 警告
- `.meta` 文件由 Unity 自动生成

## 材质创建位置

材质模板统一放在：

```text
D:\zhizuo\sava\NiumaM\Assets\Game\Moudle\NiumaShader\Runtime\Materials
```

第一阶段暂不手写 `.mat` 文件，等 Unity 导入 Shader 后再在编辑器中创建模板材质。
