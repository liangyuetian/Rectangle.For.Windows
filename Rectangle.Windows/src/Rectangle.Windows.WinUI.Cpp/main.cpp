#include "pch.h"
#include "App.h"

using namespace winrt::Rectangle;

int WINAPI wWinMain(HINSTANCE hInstance, HINSTANCE hPrevInstance, PWSTR pCmdLine, int nCmdShow)
{
    App::Current().Initialize();
    App::Current().Run();
    return 0;
}
