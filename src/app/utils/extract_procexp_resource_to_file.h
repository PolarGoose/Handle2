#pragma once
#include "app/utils/exception.h"
#include "app/utils/string_conversion.h"

namespace app {

namespace priv {

inline auto get_procexp152_resource() {
  const auto res_handle =
      FindResource(nullptr, MAKEINTRESOURCE(101), RT_RCDATA);
  if (res_handle == nullptr) {
    THROW_WINAPI_FUNC_FAILED(FindResource);
  }

  const auto loaded_res = LoadResource(nullptr, res_handle);
  if (loaded_res == nullptr) {
    THROW_WINAPI_FUNC_FAILED(LoadResource);
  }

  const auto res_data = LockResource(loaded_res);
  if (res_data == nullptr) {
    THROW_WINAPI_FUNC_FAILED(LockResource);
  }

  const auto res_size = SizeofResource(nullptr, res_handle);
  if (res_size == 0) {
    THROW_WINAPI_FUNC_FAILED(SizeofResource);
  }

  return std::span(static_cast<const char*>(res_data), res_size);
}

}

inline void extract_procexp_resource_to_file(
    const std::filesystem::path& dst_file_full_name) {
  if (std::filesystem::exists(dst_file_full_name)) {
    return;
  }
  const auto& res = priv::get_procexp152_resource();
  std::fstream file(dst_file_full_name, std::ios::out | std::ios::binary);
  if (!file) {
    THROW(L"Failed to create a destination file '{}'",
          dst_file_full_name.c_str());
  }
  file.write(res.data(), res.size());
}

}
