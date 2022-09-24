#include "app/utils/expand_path_with_environment_variables.h"

TEST_CASE("Expands_env_variables") {
  const auto& res = app::expand_path_with_environment_variables(
      L"%WINDIR%\\System32\\drivers");
  REQUIRE(res == L"C:\\Windows\\System32\\drivers");
}
