#include "app/utils/process_info.h"
#include "test/utils/utils.h"

using namespace app;

TEST_CASE("Get process info") {
  const auto& current_user_and_domain_name =
      test::get_current_domain_and_user_name();

  std::map<std::wstring, process_info> infos;
  for (const auto& proc_id : test::get_list_of_processes()) {
    app::scoped_handle opened_process{
        OpenProcess(PROCESS_ALL_ACCESS, FALSE, proc_id)};
    if (!opened_process) {
      continue;
    }
    try {
      const auto& info = get_process_info(opened_process.get(), proc_id);
      infos.emplace(info.process_full_name, info);
    } catch (const app_exception&) {
    }
  }

  {
    std::filesystem::path proc_full_name =
        L"C:\\Windows\\System32\\dllhost.exe";
    REQUIRE(infos.contains(proc_full_name));
    const auto& info = infos[proc_full_name];
    REQUIRE(info.domain + L"\\" + info.user == current_user_and_domain_name);
  }

  {
    std::filesystem::path proc_full_name =
        L"C:\\Windows\\System32\\AggregatorHost.exe";
    REQUIRE(infos.contains(proc_full_name));
    const auto& info = infos[proc_full_name];
    REQUIRE(info.user == L"SYSTEM");
    REQUIRE(info.domain == L"NT AUTHORITY");
  }
}
