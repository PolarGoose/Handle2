#include "app/utils/extract_procexp_resource_to_file.h"
#include "test/utils/utils.h"

using namespace Catch::Matchers;

class extract_procexp_resource_to_file_test {
protected:
  const size_t expected_binary_res_size{37280};
  std::filesystem::path extraction_file_full_name;

  extract_procexp_resource_to_file_test()
      : extraction_file_full_name{get_temp_file_full_name()} {}

  ~extract_procexp_resource_to_file_test() {
    std::filesystem::remove(extraction_file_full_name);
  }

  static std::filesystem::path get_temp_file_full_name() {
    std::array<wchar_t, L_tmpnam_s> s_tempName;
    _wtmpnam_s(s_tempName.data(), L_tmpnam_s);
    return s_tempName.data();
  }
};

TEST_CASE_METHOD(extract_procexp_resource_to_file_test,
                 "PROCEXP152_can_be_extracted") {
  app::extract_procexp_resource_to_file(extraction_file_full_name);
  std::fstream file(extraction_file_full_name, std::ios::in | std::ios::binary);
  REQUIRE(file);
  REQUIRE(expected_binary_res_size ==
          std::filesystem::file_size(extraction_file_full_name));
}

TEST_CASE_METHOD(extract_procexp_resource_to_file_test,
                 "Throws_if_cant_create_file") {
  try {
    app::extract_procexp_resource_to_file("wrong file path ><;");
    FAIL();
  } catch (const std::exception& ex) {
    test::print_exception(ex);
    REQUIRE_THAT(ex.what(), ContainsSubstring("'wrong file path ><;'"));
  }
}

TEST_CASE_METHOD(extract_procexp_resource_to_file_test,
                 "Does_not_override_file_if_it_already_exists") {
  {
    std::fstream file(std::filesystem::path(extraction_file_full_name),
                      std::ios::out | std::ios::binary);
    file << "123";
  }
  app::extract_procexp_resource_to_file(extraction_file_full_name);
  REQUIRE(std::filesystem::file_size(extraction_file_full_name) == 3);
}
