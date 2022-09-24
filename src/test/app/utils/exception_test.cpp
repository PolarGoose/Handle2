#include "app/utils/exception.h"
#include "test/utils/utils.h"

using namespace Catch::Matchers;

TEST_CASE("THROW_throws_runtime_error_with_line_info") {
  try {
    THROW(L"some message {}", 1);
  } catch (const app::app_exception& ex) {
    test::print_exception(ex);
    REQUIRE_THAT(ex.what(),
                 ContainsSubstring("CATCH2_INTERNAL_TEST_0:8: some message 1"));
  }
}

TEST_CASE(
    "WinApi_exception_message_contains_name_of_a_function_and_a_line_number") {
  try {
    THROW_WINAPI_FUNC_FAILED(some_winapi_function);
  } catch (const app::app_exception& ex) {
    test::print_exception(ex);
    REQUIRE_THAT(ex.what(), ContainsSubstring("CATCH2_INTERNAL_TEST_2:19: "));
  }
}

TEST_CASE("WinApi_exception_message_contains_no_last_error_information_when_"
          "last_error_is_0") {
  try {
    SetLastError(NOERROR);
    THROW_WINAPI_FUNC_FAILED(some_winapi_function);
  } catch (const app::app_exception& ex) {
    test::print_exception(ex);
    REQUIRE_THAT(ex.what(), ContainsSubstring("No last error information"));
  }
}

TEST_CASE("WinApi_exception_message_contains_last_error_information_when_last_"
          "error_is_not_0") {
  try {
    SetLastError(ERROR_FILE_NOT_FOUND);
    THROW_WINAPI_FUNC_FAILED(some_winapi_function);
  } catch (const app::app_exception& ex) {
    test::print_exception(ex);
    REQUIRE_THAT(
        ex.what(),
        ContainsSubstring("ErrorCode=2(0x2). ErrorMessage='The system cannot "
                          "find the file specified. '"));
  }
}

TEST_CASE("WinApi_exception_with_custom_message") {
  try {
    THROW_WINAPI_FUNC_FAILED_MSG(some_winapi_function, L"some custom message");
  } catch (const app::app_exception& ex) {
    test::print_exception(ex);
    REQUIRE_THAT(ex.what(), ContainsSubstring(
                                "some_winapi_function function call failed."));
    REQUIRE_THAT(ex.what(), ContainsSubstring("some custom message"));
  }
}
