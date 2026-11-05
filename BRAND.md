# 景·除视觉设计

名称：景·除；英文辅助标识：Jing Delete。

Logo：切角文件轮廓、J 形清扫轨迹、四角闪光。深青底与薄荷绿高光，透明圆角外部。界面使用深青导航、浅灰绿画布、白色卡片、青绿主操作和浅红危险操作。

生成方式：内置 image_gen 工具（imagegen 技能），先生成，再做背景透明化编辑。最终检查四角 alpha=0。程序中的 PNG 为最终原图；ICO 通过 `create-icon.ps1` 进行标准缩放和格式封装，包含七种尺寸。

## 初始生成提示词（原文）

Use case: logo-brand. Asset type: Windows desktop application icon for a Chinese file-cleaning utility named 景·除 (Jing Chu). Create one premium, exceptionally crisp app icon, no text or lettering. A rounded-square deep midnight teal tile, generous transparent margins outside the tile. Inside it, a bold sculpted mint/aqua ribbon forming an elegant sweeping J-shaped cleaning stroke, subtly integrates a folded-corner file silhouette in the negative space. One small four-point sparkle at upper right represents a clean result. Tasteful restrained luminous gradient with precise smooth edges, understated depth, sophisticated modern desktop software identity, not cartoon. Front-facing orthographic, centered, square composition, single icon only. At 32px the silhouette should remain readable. Outside rounded tile must be truly transparent. Do not include any wordmark, Chinese characters, alphabet letters, caption, watermark, mockup, presentation board or extra objects.

## 最终透明化编辑提示词（原文）

Edit target: the attached existing Jing Chu application icon. Background-extraction only. Preserve the exact rounded-square teal app tile, mint sweeping ribbon, folded paper silhouette and sparkle, colors, composition, gradients and clean details. Remove only the black exterior background OUTSIDE the rounded-square tile. The exterior and all four image corners MUST be genuinely transparent (PNG alpha=0), not black, not a checkerboard graphic. Do not regenerate, recolor or redesign the icon; do not add text. Return a single square transparent PNG icon.

PNG 原图 1254×1254，随源码保存为 `Brand.png`，交付根目录保存为 `JingChu-Logo.png`。图标交付为 `JingChu.ico`。此版本没有进行商标检索或注册。
