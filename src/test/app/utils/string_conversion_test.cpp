#include "app/utils/string_conversion.h"

using namespace app;

TEST_CASE("to_utf8") {
  std::wstring ws = L"車B1234 こんにちは";
  std::string s = to_utf8(ws);
  REQUIRE(s == "車B1234 こんにちは");
}

TEST_CASE("to_upper") {
  std::wstring upper_letters = to_upper(L"абґїαβß");
  REQUIRE(upper_letters == L"АБҐЇΑΒ");
}
