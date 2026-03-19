use serde::{Deserialize, Serialize};
use std::collections::HashMap;
use std::fs;
use std::path::PathBuf;
use std::sync::{Arc, Mutex};

/// 快捷键配置
#[derive(Debug, Clone, Serialize, Deserialize)]
pub struct ShortcutConfig {
    #[serde(default = "default_true")]
    pub enabled: bool,
    pub key_code: i32,
    pub modifier_flags: u32,
}

fn default_true() -> bool {
    true
}

impl Default for ShortcutConfig {
    fn default() -> Self {
        Self {
            enabled: true,
            key_code: 0,
            modifier_flags: 0,
        }
    }
}

/// 吸附区域配置
#[derive(Debug, Clone, Serialize, Deserialize)]
pub struct SnapAreaConfig {
    #[serde(default = "default_true")]
    pub drag_to_snap: bool,
    #[serde(default = "default_true")]
    pub restore_size_on_snap_end: bool,
    #[serde(default)]
    pub haptic_feedback: bool,
    #[serde(default)]
    pub snap_animation: bool,
    #[serde(default)]
    pub area_actions: HashMap<String, String>,
}

impl Default for SnapAreaConfig {
    fn default() -> Self {
        Self {
            drag_to_snap: true,
            restore_size_on_snap_end: true,
            haptic_feedback: false,
            snap_animation: false,
            area_actions: HashMap::new(),
        }
    }
}

/// 动画配置
#[derive(Debug, Clone, Serialize, Deserialize)]
pub struct AnimationConfig {
    #[serde(default = "default_true")]
    pub enabled: bool,
    #[serde(default = "default_duration")]
    pub duration_ms: i32,
    #[serde(default = "default_frame_rate")]
    pub frame_rate: i32,
    #[serde(default = "default_easing")]
    pub easing_type: String,
    #[serde(default = "default_true")]
    pub enable_move_animation: bool,
    #[serde(default = "default_true")]
    pub enable_resize_animation: bool,
    #[serde(default = "default_true")]
    pub enable_hotkey_feedback: bool,
    #[serde(default = "default_feedback_duration")]
    pub hotkey_feedback_duration_ms: i32,
}

fn default_duration() -> i32 {
    200
}

fn default_frame_rate() -> i32 {
    60
}

fn default_easing() -> String {
    "EaseOutCubic".to_string()
}

fn default_feedback_duration() -> i32 {
    800
}

impl Default for AnimationConfig {
    fn default() -> Self {
        Self {
            enabled: true,
            duration_ms: 200,
            frame_rate: 60,
            easing_type: "EaseOutCubic".to_string(),
            enable_move_animation: true,
            enable_resize_animation: true,
            enable_hotkey_feedback: true,
            hotkey_feedback_duration_ms: 800,
        }
    }
}

/// 历史配置
#[derive(Debug, Clone, Serialize, Deserialize)]
pub struct HistoryConfig {
    #[serde(default = "default_true")]
    pub enabled: bool,
    #[serde(default = "default_max_history")]
    pub max_history_count: i32,
    #[serde(default = "default_true")]
    pub enable_undo: bool,
    #[serde(default = "default_undo_shortcut")]
    pub undo_shortcut: String,
    #[serde(default = "default_redo_shortcut")]
    pub redo_shortcut: String,
}

fn default_max_history() -> i32 {
    50
}

fn default_undo_shortcut() -> String {
    "Ctrl+Alt+Z".to_string()
}

fn default_redo_shortcut() -> String {
    "Ctrl+Alt+Shift+Z".to_string()
}

impl Default for HistoryConfig {
    fn default() -> Self {
        Self {
            enabled: true,
            max_history_count: 50,
            enable_undo: true,
            undo_shortcut: "Ctrl+Alt+Z".to_string(),
            redo_shortcut: "Ctrl+Alt+Shift+Z".to_string(),
        }
    }
}

/// 应用配置
#[derive(Debug, Clone, Serialize, Deserialize)]
pub struct AppConfig {
    #[serde(default)]
    pub gap_size: i32,
    #[serde(default = "default_split_ratio")]
    pub horizontal_split_ratio: Option<i32>,
    #[serde(default = "default_split_ratio")]
    pub vertical_split_ratio: Option<i32>,
    #[serde(default)]
    pub launch_on_login: bool,
    #[serde(default = "default_ignored_apps")]
    pub ignored_apps: Vec<String>,
    #[serde(default)]
    pub shortcuts: HashMap<String, ShortcutConfig>,
    #[serde(default)]
    pub snap_areas: SnapAreaConfig,
    #[serde(default = "default_subsequent_mode")]
    pub subsequent_execution_mode: i32,
    #[serde(default = "default_almost_maximize")]
    pub almost_maximize_height: f32,
    #[serde(default = "default_almost_maximize")]
    pub almost_maximize_width: f32,
    #[serde(default)]
    pub minimum_window_width: f32,
    #[serde(default)]
    pub minimum_window_height: f32,
    #[serde(default = "default_size_offset")]
    pub size_offset: f32,
    #[serde(default)]
    pub centered_directional_move: bool,
    #[serde(default)]
    pub resize_on_directional_move: bool,
    #[serde(default)]
    pub use_cursor_screen_detection: bool,
    #[serde(default)]
    pub move_cursor: bool,
    #[serde(default)]
    pub move_cursor_across_displays: bool,
    #[serde(default = "default_footprint_alpha")]
    pub footprint_alpha: f32,
    #[serde(default = "default_footprint_border")]
    pub footprint_border_width: i32,
    #[serde(default = "default_footprint_color")]
    pub footprint_color: i32,
    #[serde(default = "default_footprint_border_color")]
    pub footprint_border_color: i32,
    #[serde(default = "default_true")]
    pub footprint_fade: bool,
    #[serde(default = "default_animation_duration")]
    pub footprint_animation_duration: i32,
    #[serde(default = "default_true")]
    pub unsnap_restore: bool,
    #[serde(default = "default_true")]
    pub drag_to_snap: bool,
    #[serde(default = "default_snap_margin")]
    pub snap_edge_margin_top: i32,
    #[serde(default = "default_snap_margin")]
    pub snap_edge_margin_bottom: i32,
    #[serde(default = "default_snap_margin")]
    pub snap_edge_margin_left: i32,
    #[serde(default = "default_snap_margin")]
    pub snap_edge_margin_right: i32,
    #[serde(default = "default_corner_snap")]
    pub corner_snap_area_size: i32,
    #[serde(default)]
    pub snap_modifiers: i32,
    #[serde(default)]
    pub haptic_feedback_on_snap: bool,
    #[serde(default)]
    pub todo_mode: bool,
    #[serde(default)]
    pub todo_application: String,
    #[serde(default = "default_todo_width")]
    pub todo_sidebar_width: i32,
    #[serde(default = "default_todo_side")]
    pub todo_sidebar_side: i32,
    #[serde(default = "default_cascade_delta")]
    pub cascade_all_delta_size: i32,
    #[serde(default = "default_log_level")]
    pub log_level: i32,
    #[serde(default)]
    pub log_to_file: bool,
    #[serde(default)]
    pub log_file_path: String,
    #[serde(default = "default_max_log_size")]
    pub max_log_file_size: i32,
    #[serde(default = "default_max_history_count")]
    pub max_window_history_count: i32,
    #[serde(default = "default_history_expiration")]
    pub window_history_expiration_minutes: i32,
    #[serde(default = "default_specified_width")]
    pub specified_width: i32,
    #[serde(default = "default_specified_height")]
    pub specified_height: i32,
    #[serde(default = "default_theme")]
    pub theme: String,
    #[serde(default = "default_language")]
    pub language: String,
    #[serde(default = "default_true")]
    pub check_for_updates: bool,
    #[serde(default)]
    pub animation: AnimationConfig,
    #[serde(default)]
    pub history: HistoryConfig,
}

fn default_split_ratio() -> Option<i32> {
    Some(50)
}

fn default_ignored_apps() -> Vec<String> {
    vec![
        "Rectangle.Windows.exe".to_string(),
        "Rectangle.Windows.WinUI.exe".to_string(),
    ]
}

fn default_subsequent_mode() -> i32 {
    1
}

fn default_almost_maximize() -> f32 {
    0.9
}

fn default_size_offset() -> f32 {
    30.0
}

fn default_footprint_alpha() -> f32 {
    0.3
}

fn default_footprint_border() -> i32 {
    2
}

fn default_footprint_color() -> i32 {
    -16711614
}

fn default_footprint_border_color() -> i32 {
    -16711614
}

fn default_animation_duration() -> i32 {
    150
}

fn default_snap_margin() -> i32 {
    5
}

fn default_corner_snap() -> i32 {
    20
}

fn default_todo_width() -> i32 {
    400
}

fn default_todo_side() -> i32 {
    1
}

fn default_cascade_delta() -> i32 {
    30
}

fn default_log_level() -> i32 {
    1
}

fn default_max_log_size() -> i32 {
    10
}

fn default_max_history_count() -> i32 {
    100
}

fn default_history_expiration() -> i32 {
    60
}

fn default_specified_width() -> i32 {
    1680
}

fn default_specified_height() -> i32 {
    1050
}

fn default_theme() -> String {
    "Default".to_string()
}

fn default_language() -> String {
    "zh-CN".to_string()
}

impl Default for AppConfig {
    fn default() -> Self {
        Self {
            gap_size: 0,
            horizontal_split_ratio: Some(50),
            vertical_split_ratio: Some(50),
            launch_on_login: false,
            ignored_apps: default_ignored_apps(),
            shortcuts: Self::default_shortcuts(),
            snap_areas: SnapAreaConfig::default(),
            subsequent_execution_mode: 1,
            almost_maximize_height: 0.9,
            almost_maximize_width: 0.9,
            minimum_window_width: 0.0,
            minimum_window_height: 0.0,
            size_offset: 30.0,
            centered_directional_move: false,
            resize_on_directional_move: false,
            use_cursor_screen_detection: false,
            move_cursor: false,
            move_cursor_across_displays: false,
            footprint_alpha: 0.3,
            footprint_border_width: 2,
            footprint_color: -16711614,
            footprint_border_color: -16711614,
            footprint_fade: true,
            footprint_animation_duration: 150,
            unsnap_restore: true,
            drag_to_snap: true,
            snap_edge_margin_top: 5,
            snap_edge_margin_bottom: 5,
            snap_edge_margin_left: 5,
            snap_edge_margin_right: 5,
            corner_snap_area_size: 20,
            snap_modifiers: 0,
            haptic_feedback_on_snap: false,
            todo_mode: false,
            todo_application: String::new(),
            todo_sidebar_width: 400,
            todo_sidebar_side: 1,
            cascade_all_delta_size: 30,
            log_level: 1,
            log_to_file: false,
            log_file_path: String::new(),
            max_log_file_size: 10,
            max_window_history_count: 100,
            window_history_expiration_minutes: 60,
            specified_width: 1680,
            specified_height: 1050,
            theme: "Default".to_string(),
            language: "zh-CN".to_string(),
            check_for_updates: true,
            animation: AnimationConfig::default(),
            history: HistoryConfig::default(),
        }
    }
}

impl AppConfig {
    /// 获取默认快捷键
    pub fn default_shortcuts() -> HashMap<String, ShortcutConfig> {
        const MOD_CONTROL: u32 = 0x0002;
        const MOD_ALT: u32 = 0x0001;
        const MOD_SHIFT: u32 = 0x0004;
        const MOD_WIN: u32 = 0x0008;
        let ctrl_alt = MOD_CONTROL | MOD_ALT;
        let ctrl_alt_shift = ctrl_alt | MOD_SHIFT;
        let ctrl_alt_win = ctrl_alt | MOD_WIN;

        let mut shortcuts = HashMap::new();

        // 半屏
        shortcuts.insert("LeftHalf".to_string(), ShortcutConfig { enabled: true, key_code: 0x25, modifier_flags: ctrl_alt });
        shortcuts.insert("RightHalf".to_string(), ShortcutConfig { enabled: true, key_code: 0x27, modifier_flags: ctrl_alt });
        shortcuts.insert("TopHalf".to_string(), ShortcutConfig { enabled: true, key_code: 0x26, modifier_flags: ctrl_alt });
        shortcuts.insert("BottomHalf".to_string(), ShortcutConfig { enabled: true, key_code: 0x28, modifier_flags: ctrl_alt });
        shortcuts.insert("CenterHalf".to_string(), ShortcutConfig { enabled: false, key_code: 0, modifier_flags: 0 });

        // 四角
        shortcuts.insert("TopLeft".to_string(), ShortcutConfig { enabled: true, key_code: 0x55, modifier_flags: ctrl_alt }); // U
        shortcuts.insert("TopRight".to_string(), ShortcutConfig { enabled: true, key_code: 0x49, modifier_flags: ctrl_alt }); // I
        shortcuts.insert("BottomLeft".to_string(), ShortcutConfig { enabled: true, key_code: 0x4A, modifier_flags: ctrl_alt }); // J
        shortcuts.insert("BottomRight".to_string(), ShortcutConfig { enabled: true, key_code: 0x4B, modifier_flags: ctrl_alt }); // K

        // 三分屏
        shortcuts.insert("FirstThird".to_string(), ShortcutConfig { enabled: true, key_code: 0x44, modifier_flags: ctrl_alt }); // D
        shortcuts.insert("CenterThird".to_string(), ShortcutConfig { enabled: true, key_code: 0x46, modifier_flags: ctrl_alt }); // F
        shortcuts.insert("LastThird".to_string(), ShortcutConfig { enabled: true, key_code: 0x47, modifier_flags: ctrl_alt }); // G
        shortcuts.insert("FirstTwoThirds".to_string(), ShortcutConfig { enabled: true, key_code: 0x45, modifier_flags: ctrl_alt }); // E
        shortcuts.insert("CenterTwoThirds".to_string(), ShortcutConfig { enabled: true, key_code: 0x52, modifier_flags: ctrl_alt }); // R
        shortcuts.insert("LastTwoThirds".to_string(), ShortcutConfig { enabled: true, key_code: 0x54, modifier_flags: ctrl_alt }); // T

        // 最大化与缩放
        shortcuts.insert("Maximize".to_string(), ShortcutConfig { enabled: true, key_code: 0x0D, modifier_flags: ctrl_alt }); // Enter
        shortcuts.insert("AlmostMaximize".to_string(), ShortcutConfig { enabled: false, key_code: 0, modifier_flags: 0 });
        shortcuts.insert("MaximizeHeight".to_string(), ShortcutConfig { enabled: true, key_code: 0x26, modifier_flags: ctrl_alt_shift }); // Shift+Up
        shortcuts.insert("Larger".to_string(), ShortcutConfig { enabled: true, key_code: 0xBB, modifier_flags: ctrl_alt }); // =
        shortcuts.insert("Smaller".to_string(), ShortcutConfig { enabled: true, key_code: 0xBD, modifier_flags: ctrl_alt }); // -
        shortcuts.insert("Center".to_string(), ShortcutConfig { enabled: true, key_code: 0x43, modifier_flags: ctrl_alt }); // C
        shortcuts.insert("Restore".to_string(), ShortcutConfig { enabled: true, key_code: 0x08, modifier_flags: ctrl_alt }); // Backspace

        // 四等分
        shortcuts.insert("FirstFourth".to_string(), ShortcutConfig { enabled: false, key_code: 0, modifier_flags: 0 });
        shortcuts.insert("SecondFourth".to_string(), ShortcutConfig { enabled: false, key_code: 0, modifier_flags: 0 });
        shortcuts.insert("ThirdFourth".to_string(), ShortcutConfig { enabled: false, key_code: 0, modifier_flags: 0 });
        shortcuts.insert("LastFourth".to_string(), ShortcutConfig { enabled: false, key_code: 0, modifier_flags: 0 });

        // 六等分
        shortcuts.insert("TopLeftSixth".to_string(), ShortcutConfig { enabled: false, key_code: 0, modifier_flags: 0 });
        shortcuts.insert("TopCenterSixth".to_string(), ShortcutConfig { enabled: false, key_code: 0, modifier_flags: 0 });
        shortcuts.insert("TopRightSixth".to_string(), ShortcutConfig { enabled: false, key_code: 0, modifier_flags: 0 });
        shortcuts.insert("BottomLeftSixth".to_string(), ShortcutConfig { enabled: false, key_code: 0, modifier_flags: 0 });
        shortcuts.insert("BottomCenterSixth".to_string(), ShortcutConfig { enabled: false, key_code: 0, modifier_flags: 0 });
        shortcuts.insert("BottomRightSixth".to_string(), ShortcutConfig { enabled: false, key_code: 0, modifier_flags: 0 });

        // 移动到边缘
        shortcuts.insert("MoveLeft".to_string(), ShortcutConfig { enabled: false, key_code: 0, modifier_flags: 0 });
        shortcuts.insert("MoveRight".to_string(), ShortcutConfig { enabled: false, key_code: 0, modifier_flags: 0 });
        shortcuts.insert("MoveUp".to_string(), ShortcutConfig { enabled: false, key_code: 0, modifier_flags: 0 });
        shortcuts.insert("MoveDown".to_string(), ShortcutConfig { enabled: false, key_code: 0, modifier_flags: 0 });

        // 显示器
        shortcuts.insert("PreviousDisplay".to_string(), ShortcutConfig { enabled: true, key_code: 0x25, modifier_flags: ctrl_alt_win }); // Win+Left
        shortcuts.insert("NextDisplay".to_string(), ShortcutConfig { enabled: true, key_code: 0x27, modifier_flags: ctrl_alt_win }); // Win+Right

        // 撤销/重做
        shortcuts.insert("Undo".to_string(), ShortcutConfig { enabled: true, key_code: 0x5A, modifier_flags: ctrl_alt }); // Z
        shortcuts.insert("Redo".to_string(), ShortcutConfig { enabled: true, key_code: 0x5A, modifier_flags: ctrl_alt_shift }); // Shift+Z

        // 九等分（默认禁用）
        for action in ["TopLeftNinth", "TopCenterNinth", "TopRightNinth",
                       "MiddleLeftNinth", "MiddleCenterNinth", "MiddleRightNinth",
                       "BottomLeftNinth", "BottomCenterNinth", "BottomRightNinth"] {
            shortcuts.insert(action.to_string(), ShortcutConfig { enabled: false, key_code: 0, modifier_flags: 0 });
        }

        // 八等分（默认禁用）
        for action in ["TopLeftEighth", "TopCenterLeftEighth", "TopCenterRightEighth", "TopRightEighth",
                       "BottomLeftEighth", "BottomCenterLeftEighth", "BottomCenterRightEighth", "BottomRightEighth"] {
            shortcuts.insert(action.to_string(), ShortcutConfig { enabled: false, key_code: 0, modifier_flags: 0 });
        }

        // 角落三分之一（默认禁用）
        for action in ["TopLeftThird", "TopRightThird", "BottomLeftThird", "BottomRightThird"] {
            shortcuts.insert(action.to_string(), ShortcutConfig { enabled: false, key_code: 0, modifier_flags: 0 });
        }

        // 垂直三分之一（默认禁用）
        for action in ["TopVerticalThird", "MiddleVerticalThird", "BottomVerticalThird",
                       "TopVerticalTwoThirds", "BottomVerticalTwoThirds"] {
            shortcuts.insert(action.to_string(), ShortcutConfig { enabled: false, key_code: 0, modifier_flags: 0 });
        }

        // Todo 模式（默认禁用）
        for action in ["LeftTodo", "RightTodo"] {
            shortcuts.insert(action.to_string(), ShortcutConfig { enabled: false, key_code: 0, modifier_flags: 0 });
        }

        // 其他调整尺寸动作（默认禁用）
        for action in ["DoubleHeightUp", "DoubleHeightDown", "DoubleWidthLeft", "DoubleWidthRight",
                       "HalveHeightUp", "HalveHeightDown", "HalveWidthLeft", "HalveWidthRight",
                       "LargerWidth", "SmallerWidth", "LargerHeight", "SmallerHeight",
                       "Specified", "CenterProminently"] {
            shortcuts.insert(action.to_string(), ShortcutConfig { enabled: false, key_code: 0, modifier_flags: 0 });
        }

        shortcuts
    }
}

/// 配置服务
#[derive(Debug, Clone)]
pub struct ConfigService {
    config_path: PathBuf,
    config: Arc<Mutex<AppConfig>>,
}

impl ConfigService {
    /// 创建新的配置服务
    pub fn new() -> Self {
        let app_data = dirs::data_dir().unwrap_or_else(|| PathBuf::from("."));
        let app_folder = app_data.join("Rectangle");
        let config_path = app_folder.join("config.json");

        // 确保目录存在
        if let Err(_) = fs::create_dir_all(&app_folder) {
            log::warn!("无法创建配置目录: {:?}", app_folder);
        }

        let config = Self::load_from_file(&config_path);

        Self {
            config_path,
            config: Arc::new(Mutex::new(config)),
        }
    }

    /// 从文件加载配置
    fn load_from_file(path: &PathBuf) -> AppConfig {
        if let Ok(content) = fs::read_to_string(path) {
            if let Ok(config) = serde_json::from_str(&content) {
                return config;
            }
        }
        AppConfig::default()
    }

    /// 加载配置
    pub fn load(&self) -> AppConfig {
        self.config.lock().unwrap().clone()
    }

    /// 保存配置
    pub fn save(&self, config: AppConfig) {
        if let Ok(json) = serde_json::to_string_pretty(&config) {
            if let Err(e) = fs::write(&self.config_path, json) {
                log::error!("保存配置失败: {}", e);
            } else {
                if let Ok(mut guard) = self.config.lock() {
                    *guard = config;
                }
            }
        }
    }

    /// 获取配置路径
    pub fn config_path(&self) -> &PathBuf {
        &self.config_path
    }
}

impl Default for ConfigService {
    fn default() -> Self {
        Self::new()
    }
}
