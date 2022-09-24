#include "app/utils/normalize_path.h"

using namespace app;

TEST_CASE("normalize_path works") {
  REQUIRE(normalize_path(L"C:/Windows/../Windows/System32") ==
          L"C:\\Windows\\System32\\");
  REQUIRE(normalize_path(L"C:/Windows/../Windows/System32/mfsvr.dll") ==
          L"C:/Windows/System32/mfsvr.dll");
}
