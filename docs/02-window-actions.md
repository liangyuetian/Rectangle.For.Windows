# 窗口操作完整列表

Rectangle 支持 90+ 种窗口操作，以下为完整列表及说明。

## 半屏操作

| 操作名称 | 说明 |
|----------|------|
| `left-half` | 左半屏 |
| `right-half` | 右半屏 |
| `center-half` | 中间半屏（居中半宽） |
| `top-half` | 上半屏 |
| `bottom-half` | 下半屏 |

## 四分之一操作

| 操作名称 | 说明 |
|----------|------|
| `top-left` | 左上角 |
| `top-right` | 右上角 |
| `bottom-left` | 左下角 |
| `bottom-right` | 右下角 |

## 三分之一操作

| 操作名称 | 说明 |
|----------|------|
| `first-third` | 第一三分之一（左/上，取决于屏幕方向） |
| `first-two-thirds` | 前三分之二 |
| `center-third` | 中间三分之一 |
| `center-two-thirds` | 中间三分之二 |
| `last-two-thirds` | 后三分之二 |
| `last-third` | 最后三分之一 |
| `top-left-third` | 左上角三分之二（需 CLI 配置） |
| `top-right-third` | 右上角三分之二 |
| `bottom-left-third` | 左下角三分之二 |
| `bottom-right-third` | 右下角三分之二 |
| `top-vertical-third` | 顶部垂直三分之一 |
| `middle-vertical-third` | 中间垂直三分之一 |
| `bottom-vertical-third` | 底部垂直三分之一 |
| `top-vertical-two-thirds` | 顶部垂直三分之二 |
| `bottom-vertical-two-thirds` | 底部垂直三分之二 |

## 四分之一细分

| 操作名称 | 说明 |
|----------|------|
| `first-fourth` | 第一四分之一 |
| `second-fourth` | 第二四分之一 |
| `third-fourth` | 第三四分之一 |
| `last-fourth` | 最后四分之一 |
| `first-three-fourths` | 前四分之三 |
| `center-three-fourths` | 中间四分之三 |
| `last-three-fourths` | 后四分之三 |

## 六分之一操作

| 操作名称 | 说明 |
|----------|------|
| `top-left-sixth` | 左上六分之一 |
| `top-center-sixth` | 顶部中间六分之一 |
| `top-right-sixth` | 右上六分之一 |
| `bottom-left-sixth` | 左下六分之一 |
| `bottom-center-sixth` | 底部中间六分之一 |
| `bottom-right-sixth` | 右下六分之一 |

## 九分之一操作（需 CLI 配置）

| 操作名称 | 说明 |
|----------|------|
| `top-left-ninth` | 左上九分之一 |
| `top-center-ninth` | 顶部中间九分之一 |
| `top-right-ninth` | 右上九分之一 |
| `middle-left-ninth` | 中间左侧九分之一 |
| `middle-center-ninth` | 正中央九分之一 |
| `middle-right-ninth` | 中间右侧九分之一 |
| `bottom-left-ninth` | 左下九分之一 |
| `bottom-center-ninth` | 底部中间九分之一 |
| `bottom-right-ninth` | 右下九分之一 |

## 八分之一操作（需 CLI 配置）

| 操作名称 | 说明 |
|----------|------|
| `top-left-eighth` | 左上八分之一 |
| `top-center-left-eighth` | 顶部左中八分之一 |
| `top-center-right-eighth` | 顶部右中八分之一 |
| `top-right-eighth` | 右上八分之一 |
| `bottom-left-eighth` | 左下八分之一 |
| `bottom-center-left-eighth` | 底部左中八分之一 |
| `bottom-center-right-eighth` | 底部右中八分之一 |
| `bottom-right-eighth` | 右下八分之一 |

## 最大化与居中

| 操作名称 | 说明 |
|----------|------|
| `maximize` | 最大化 |
| `almost-maximize` | 几乎最大化（默认 90%） |
| `maximize-height` | 仅最大化高度 |
| `center` | 居中 |
| `center-prominently` | 突出居中（略高于中心） |
| `specified` | 自定义尺寸居中（需 CLI 配置） |

## 尺寸调整

| 操作名称 | 说明 |
|----------|------|
| `larger` | 放大 |
| `smaller` | 缩小 |
| `larger-width` | 仅加宽（需 CLI 配置） |
| `smaller-width` | 仅变窄 |
| `larger-height` | 仅增高（需 CLI 配置） |
| `smaller-height` | 仅变矮 |

## 尺寸倍增/减半（需 CLI 配置）

| 操作名称 | 说明 |
|----------|------|
| `double-height-up` | 高度加倍（向上扩展） |
| `double-height-down` | 高度加倍（向下扩展） |
| `double-width-left` | 宽度加倍（向左扩展） |
| `double-width-right` | 宽度加倍（向右扩展） |
| `halve-height-up` | 高度减半（向上） |
| `halve-height-down` | 高度减半（向下） |
| `halve-width-left` | 宽度减半（向左） |
| `halve-width-right` | 宽度减半（向右） |

## 方向移动

| 操作名称 | 说明 |
|----------|------|
| `move-left` | 向左移动（不调整大小） |
| `move-right` | 向右移动 |
| `move-up` | 向上移动 |
| `move-down` | 向下移动 |

## 显示器操作

| 操作名称 | 说明 |
|----------|------|
| `next-display` | 移至下一显示器 |
| `previous-display` | 移至上一显示器 |

## 平铺与层叠（需 CLI 配置）

| 操作名称 | 说明 |
|----------|------|
| `tile-all` | 平铺所有可见窗口 |
| `cascade-all` | 层叠所有可见窗口 |
| `cascade-active-app` | 层叠当前应用窗口 |
| `tile-active-app` | 平铺当前应用窗口 |

## Todo 模式

| 操作名称 | 说明 |
|----------|------|
| `left-todo` | 左侧 Todo 布局 |
| `right-todo` | 右侧 Todo 布局 |

## 其他

| 操作名称 | 说明 |
|----------|------|
| `restore` | 恢复上次布局 |
| `reverse-all` | 反转所有窗口 |

## URL 中的操作名称格式

URL 方案使用连字符格式，如 `left-half`、`top-left-sixth`。驼峰命名会自动转换（如 `leftHalf` → `left-half`）。
