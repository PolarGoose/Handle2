#include "app/locking_info_provider.h"
#include "test/utils/utils.h"

app::process_info get_process(const std::vector<app::process_info>& all_info,
                              const std::filesystem::path& process_full_name) {
  for (const auto& info : all_info) {
    if (info.process_full_name == process_full_name) {
      return info;
    }
  }
  THROW(L"Couldn't find process with full_name={}", process_full_name);
}

void assert_contains_locked_path(const app::process_info& info,
                                 const std::filesystem::path& locked_path) {
  for (const auto& p : info.locked_paths) {
    if (p == locked_path) {
      return;
    }
  }
  THROW(L"The process '{}' pid='{}' doesn't contain locked path '{}'",
        info.process_full_name, info.pid, locked_path.c_str());
}

TEST_CASE("Receive correct locking info") {
  const auto& current_user_and_domain_name =
      test::get_current_domain_and_user_name();
  app::locking_info_provider l;
  const auto& locking_info = l.get_processes_locking_path(L"C:\\Windows");

  {
    const auto& p = get_process(locking_info, L"System");
    REQUIRE(p.pid == 4);
    REQUIRE(p.domain == L"NT AUTHORITY");
    REQUIRE(p.user == L"SYSTEM");
    assert_contains_locked_path(p, L"C:\\Windows\\bootstat.dat");
    assert_contains_locked_path(p, L"C:\\Windows\\CSC\\");
    assert_contains_locked_path(p, L"C:\\Windows\\System32\\config\\SECURITY");
  }

  {
    const auto& p = get_process(locking_info, L"C:\\Windows\\explorer.exe");
    REQUIRE(p.domain + L"\\" + p.user == current_user_and_domain_name);
    assert_contains_locked_path(p, L"C:\\Windows\\Fonts\\StaticCache.dat");
    assert_contains_locked_path(p, L"C:\\Windows\\bcastdvr\\");
  }
}

TEST_CASE("Throws if folder doesn't exist") {
  app::locking_info_provider l;
  REQUIRE_THROWS_AS(
      l.get_processes_locking_path(L"C:\\nonExistentFolder_123someText"),
      app::app_exception);
}
