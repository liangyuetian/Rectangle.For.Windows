#pragma once
#include "pch.h"
#include <fstream>
#include <filesystem>

namespace winrt::Rectangle::Services
{
    enum class LogLevel
    {
        Debug = 0,
        Info = 1,
        Warning = 2,
        Error = 3
    };

    class Logger
    {
    public:
        static Logger& Instance();

        void SetLogLevel(LogLevel level);
        void SetLogToFile(bool enable, const std::wstring& filePath = L"");
        void SetConfigService(void* configService);

        void Debug(const std::wstring& tag, const std::wstring& message);
        void Info(const std::wstring& tag, const std::wstring& message);
        void Warning(const std::wstring& tag, const std::wstring& message);
        void Error(const std::wstring& tag, const std::wstring& message);

        void Log(LogLevel level, const std::wstring& tag, const std::wstring& message);

    private:
        Logger();
        ~Logger();
        Logger(const Logger&) = delete;
        Logger& operator=(const Logger&) = delete;

        std::wstring FormatMessage(LogLevel level, const std::wstring& tag, const std::wstring& message);
        void WriteToFile(const std::wstring& message);

        LogLevel m_logLevel{ LogLevel::Info };
        bool m_logToFile{ false };
        std::wstring m_logFilePath;
        std::mutex m_mutex;
        std::ofstream m_fileStream;
        void* m_configService{ nullptr };
    };
}
