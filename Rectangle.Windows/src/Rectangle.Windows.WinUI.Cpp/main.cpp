#include "pch.h"
#include "App.h"
#include <Windows.h>
#include <exception>

using namespace winrt::Rectangle;

static BOOL WINAPI ConsoleCtrlHandler(DWORD ctrlType)
{
    switch (ctrlType)
    {
    case CTRL_C_EVENT:
    case CTRL_BREAK_EVENT:
    case CTRL_CLOSE_EVENT:
    case CTRL_SHUTDOWN_EVENT:
        App::Current().Exit();
        return TRUE;
    default:
        return FALSE;
    }
}

int WINAPI wWinMain(HINSTANCE hInstance, HINSTANCE hPrevInstance, PWSTR pCmdLine, int nCmdShow)
{
    SetUnhandledExceptionFilter([](EXCEPTION_POINTERS*) -> LONG {
        App::Current().Exit();
        return EXCEPTION_EXECUTE_HANDLER;
    });
    std::set_terminate([]() {
        App::Current().Exit();
        abort();
    });
    SetConsoleCtrlHandler(ConsoleCtrlHandler, TRUE);
    App::Current().Initialize();
    App::Current().Run();
    return 0;
}
