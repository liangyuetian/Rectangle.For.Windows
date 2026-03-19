pub mod rect;
pub mod action;
pub mod history;
pub mod calculator;
pub mod calculator_factory;

pub use rect::{WorkArea, WindowRect};
pub use action::WindowAction;
pub use history::{WindowHistory, RectangleAction};
pub use calculator::RectCalculator;
pub use calculator_factory::CalculatorFactory;
