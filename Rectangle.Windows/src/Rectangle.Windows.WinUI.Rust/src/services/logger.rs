use log::{Level, LevelFilter, Metadata, Record};
use simplelog::{Config, WriteLogger};
use std::fs::File;
use std::sync::Once;

static INIT: Once = Once::new();

/// 初始化日志
pub fn init() {
    INIT.call_once(|| {
        let config = Config::default();
        let _ = simplelog::TermLogger::init(
            LevelFilter::Info,
            config,
            simplelog::TerminalMode::Mixed,
            simplelog::ColorChoice::Auto,
        );
    });
}

/// 日志记录器
pub struct Logger;

impl Logger {
    /// 记录调试日志
    pub fn debug(category: &str, message: &str) {
        log::debug!("[{}] {}", category, message);
    }

    /// 记录信息日志
    pub fn info(category: &str, message: &str) {
        log::info!("[{}] {}", category, message);
    }

    /// 记录警告日志
    pub fn warning(category: &str, message: &str) {
        log::warn!("[{}] {}", category, message);
    }

    /// 记录错误日志
    pub fn error(category: &str, message: &str) {
        log::error!("[{}] {}", category, message);
    }
}

/// 文件日志初始化
pub fn init_file_logging(log_path: &str) -> Result<(), Box<dyn std::error::Error>> {
    let file = File::create(log_path)?;
    WriteLogger::init(
        LevelFilter::Debug,
        Config::default(),
        file,
    )?;
    Ok(())
}
