#pragma once
#include "app/utils/exception.h"
#include "app/utils/string_conversion.h"

namespace app {

inline std::filesystem::path
expand_path_with_environment_variables(const std::filesystem::path& path) {
  const auto len = ExpandEnvironmentStrings(path.c_str(), nullptr, 0);
  if (!len) {
    THROW_WINAPI_FUNC_FAILED_MSG(
        ExpandEnvironmentStrings,
        L"Failed to calculate length for expanding the path '{}'", path);
  }

  const auto buffer = std::make_unique<wchar_t[]>(len);

  if (!ExpandEnvironmentStrings(path.c_str(), buffer.get(), len)) {
    THROW_WINAPI_FUNC_FAILED_MSG(ExpandEnvironmentStrings,
                                 L"Failed to expand '{}'", path);
  }
  return std::filesystem::path{buffer.get()};
}

}
