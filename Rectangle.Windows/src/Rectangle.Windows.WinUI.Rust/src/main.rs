mod calculators;
mod core;
mod services;
mod ui;

use ui::WinUIApp;
use windows::core::Result;

fn main() -> Result<()> {
    // 创建并运行 WinUI 3 应用
    let mut app = WinUIApp::new()?;
    app.run()?;
    Ok(())
}
