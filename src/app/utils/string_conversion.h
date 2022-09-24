#pragma once
#include <format>

namespace app {

inline std::string to_utf8(std::wstring_view wideStr) {
#pragma warning(push)
#pragma warning(disable : 4996)
  static std::wstring_convert<std::codecvt_utf8_utf16<wchar_t>> utf8_conv;
  return utf8_conv.to_bytes(wideStr.data());
#pragma warning(pop)
}

inline std::wstring to_upper(std::wstring str) {
  CharUpper(str.data());
  return str;
}

}

// Allow std::format(L"..") to format std::filesystem::path
namespace std {

template <>
struct std::formatter<std::filesystem::path, wchar_t>
    : std::formatter<std::wstring, wchar_t> {
  auto format(const std::filesystem::path& path, wformat_context& ctx) {
    return formatter<wstring, wchar_t>::format(path.c_str(), ctx);
  }
};

}
