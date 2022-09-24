#include "app/utils/enable_privilege.h"

TEST_CASE("It_is_possible_to_enable_a_privilege") {
  app::enable_privilege(L"SeLoadDriverPrivilege");
}

TEST_CASE("Throws_if_a_user_tries_to_enable_non_existent_privilege") {
  REQUIRE_THROWS_AS(app::enable_privilege(L"NonExistentPrivilege"),
                    app::app_exception);
}
