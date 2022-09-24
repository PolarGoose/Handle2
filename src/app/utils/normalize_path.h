#pragma once

namespace app {

// Converts a path to an absolute and replaces "/" separator to "\".
// Examples:
// "C:/folder1/../folder2/file" -> "C:\folder2\file"
// "C:/folder1/../folder2" -> "C:\folder2\"
inline auto normalize_path(std::filesystem::path p) {
  p = std::filesystem::absolute(p);
  p = p.make_preferred();

  // there are path for which std::filesystem::is_directory fails with "Access
  // denied" exception. we want to silently ignore such errors.
  std::error_code ec;
  if (std::filesystem::is_directory(p, ec) && !p.wstring().ends_with(L'\\')) {
    p += L"\\";
  }
  return p;
}

}
