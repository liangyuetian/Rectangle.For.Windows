use crate::services::config::ConfigService;
use std::sync::Arc;
use windows::core::Result;

/// 快捷键编辑器控件
pub struct ShortcutEditor {
    action_name: String,
    label: String,
    key_code: i32,
    modifier_flags: u32,
    enabled: bool,
}

impl ShortcutEditor {
    /// 创建新的快捷键编辑器
    pub fn new(action_name: &str, label: &str) -> Result<Self> {
        Ok(Self {
            action_name: action_name.to_string(),
            label: label.to_string(),
            key_code: 0,
            modifier_flags: 0,
            enabled: true,
        })
    }

    /// 获取动作名称
    pub fn action_name(&self) -> &str {
        &self.action_name
    }

    /// 获取标签
    pub fn label(&self) -> &str {
        &self.label
    }

    /// 设置快捷键
    pub fn set_shortcut(&mut self, key_code: i32, modifier_flags: u32) {
        self.key_code = key_code;
        self.modifier_flags = modifier_flags;
    }

    /// 获取快捷键字符串
    pub fn get_shortcut_text(&self) -> String {
        if !self.enabled || self.key_code == 0 {
            return "无快捷键".to_string();
        }

        let mut parts = Vec::new();
        if (self.modifier_flags & 0x0002) != 0 {
            parts.push("Ctrl");
        }
        if (self.modifier_flags & 0x0001) != 0 {
            parts.push("Alt");
        }
        if (self.modifier_flags & 0x0004) != 0 {
            parts.push("Shift");
        }
        if (self.modifier_flags & 0x0008) != 0 {
            parts.push("Win");
        }
        parts.push(&self.vk_to_string(self.key_code));
        parts.join(" + ")
    }

    /// 虚拟键码转字符串
    fn vk_to_string(&self, vk: i32) -> String {
        match vk {
            0x25 => "←".to_string(),
            0x26 => "↑".to_string(),
            0x27 => "→".to_string(),
            0x28 => "↓".to_string(),
            0x0D => "Enter".to_string(),
            0x08 => "Backspace".to_string(),
            0x2E => "Delete".to_string(),
            0x20 => "Space".to_string(),
            0xBB => "=".to_string(),
            0xBD => "-".to_string(),
            0x70..=0x7B => format!("F{}", vk - 0x6F),
            0x41..=0x5A => ((vk as u8) as char).to_string(),
            0x30..=0x39 => ((vk as u8) as char).to_string(),
            _ => format!("0x{:X}", vk),
        }
    }

    /// 设置启用状态
    pub fn set_enabled(&mut self, enabled: bool) {
        self.enabled = enabled;
    }

    /// 是否启用
    pub fn is_enabled(&self) -> bool {
        self.enabled
    }
}
