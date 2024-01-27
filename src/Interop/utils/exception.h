#pragma once
#include "Interop/pch.h"
#include "Interop/utils/preprocessor.h"
#include "Interop/utils/string_conversion.h"

#define THROW(fmt_str, ...)                                                    \
  throw interop::utils::app_exception(std::format(LINE_INFO fmt_str, __VA_ARGS__))

#define THROW_WINAPI_FUNC_FAILED(func_name)                                    \
  throw interop::utils::app_exception(                                                    \
      std::format(LINE_INFO L#func_name L" function call failed. {}. ",        \
                  interop::utils::priv::get_last_error()))

#define THROW_WINAPI_FUNC_FAILED_MSG(func_name, fmt_str, ...)                  \
  throw interop::utils::app_exception(                                                                                                                     \
      std::format(                                        \
      LINE_INFO L#func_name L" function call failed. {}. " fmt_str,            \
      interop::utils::priv::get_last_error(), __VA_ARGS__))

namespace interop::utils {

namespace priv {

inline std::wstring get_last_error() {
  if (const auto& last_error = GetLastError(); last_error != 0) {
    LPTSTR err_msg = nullptr;
    FormatMessage(FORMAT_MESSAGE_ALLOCATE_BUFFER | FORMAT_MESSAGE_FROM_SYSTEM |
                      FORMAT_MESSAGE_IGNORE_INSERTS |
                      FORMAT_MESSAGE_MAX_WIDTH_MASK,
                  nullptr, last_error, 0, (LPTSTR)&err_msg, 0, nullptr);
    const auto& msg = std::format(L"ErrorCode={}(0x{:x}). "
                                  L"ErrorMessage='{}'",
                                  last_error, last_error, err_msg);
    LocalFree(err_msg);
    return msg;
  }
  return L"No last error information";
}

}

class app_exception final : public std::exception {
public:
  explicit app_exception(std::wstring_view msg)
      : std::exception{to_utf8(msg).c_str()} {}
};

}
