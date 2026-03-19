#include "pch.h"
#include "Services/Logger.h"

namespace winrt::Rectangle::Services
{
    Logger::Logger() = default;

    Logger::~Logger()
    {
        if (m_fileStream.is_open())
        {
            m_fileStream.close();
        }
    }

    Logger& Logger::Instance()
    {
        static Logger instance;
        return instance;
    }

    void Logger::SetLogLevel(LogLevel level)
    {
        std::lock_guard lock(m_mutex);
        m_logLevel = level;
    }

    void Logger::SetLogToFile(bool enable, const std::wstring& filePath)
    {
        std::lock_guard lock(m_mutex);
        m_logToFile = enable;
        if (m_fileStream.is_open())
        {
            m_fileStream.close();
        }
        if (enable && !filePath.empty())
        {
            m_logFilePath = filePath;
            m_fileStream.open(filePath, std::ios::app);
        }
    }

    void Logger::SetConfigService(void* configService)
    {
        std::lock_guard lock(m_mutex);
        m_configService = configService;
    }

    std::wstring Logger::FormatMessage(LogLevel level, const std::wstring& tag, const std::wstring& message)
    {
        auto now = std::chrono::system_clock::now();
        auto time = std::chrono::system_clock::to_time_t(now);
        std::tm tm;
        localtime_s(&tm, &time);

        std::wostringstream oss;
        oss << std::put_time(&tm, L"%Y-%m-%d %H:%M:%S");

        std::wstring levelStr;
        switch (level)
        {
        case LogLevel::Debug: levelStr = L"DEBUG"; break;
        case LogLevel::Info: levelStr = L"INFO"; break;
        case LogLevel::Warning: levelStr = L"WARN"; break;
        case LogLevel::Error: levelStr = L"ERROR"; break;
        }

        oss << L" [" << levelStr << L"] [" << tag << L"] " << message;
        return oss.str();
    }

    void Logger::WriteToFile(const std::wstring& message)
    {
        if (m_fileStream.is_open())
        {
            m_fileStream << std::wstring_view(message) << std::endl;
            m_fileStream.flush();
        }
    }

    void Logger::Log(LogLevel level, const std::wstring& tag, const std::wstring& message)
    {
        if (level < m_logLevel)
        {
            return;
        }

        std::lock_guard lock(m_mutex);
        auto formatted = FormatMessage(level, tag, message);

        OutputDebugString(formatted.c_str());
        OutputDebugString(L"\n");

        if (m_logToFile)
        {
            WriteToFile(formatted);
        }
    }

    void Logger::Debug(const std::wstring& tag, const std::wstring& message)
    {
        Log(LogLevel::Debug, tag, message);
    }

    void Logger::Info(const std::wstring& tag, const std::wstring& message)
    {
        Log(LogLevel::Info, tag, message);
    }

    void Logger::Warning(const std::wstring& tag, const std::wstring& message)
    {
        Log(LogLevel::Warning, tag, message);
    }

    void Logger::Error(const std::wstring& tag, const std::wstring& message)
    {
        Log(LogLevel::Error, tag, message);
    }
}
