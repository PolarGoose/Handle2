#pragma once
#include "app/utils/exception.h"
#include "app/utils/scoped_handle.h"

namespace test {

inline void print_exception(const std::exception& ex) {
  std::cout << std::format("Exception type '{}' message:\n{}\n",
                           typeid(ex).name(), ex.what());
}

inline auto get_list_of_processes() {
  PROCESSENTRY32 entry{};
  entry.dwSize = sizeof(PROCESSENTRY32);
  std::vector<DWORD> proc_ids;
  const app::scoped_handle snapshot{
      CreateToolhelp32Snapshot(TH32CS_SNAPPROCESS, NULL)};

  if (!Process32First(snapshot.get(), &entry)) {
    THROW_WINAPI_FUNC_FAILED(Process32First);
  }

  do {
    proc_ids.emplace_back(entry.th32ProcessID);
  } while (Process32Next(snapshot.get(), &entry));

  return proc_ids;
}

// Returns a string "domain\username"
inline std::wstring get_current_domain_and_user_name() {
  const auto nameAndDomainLen = 1024;
  wchar_t domain_username[nameAndDomainLen];
  DWORD domain_username_len = nameAndDomainLen;
  const auto& res =
      GetUserNameEx(NameSamCompatible, domain_username, &domain_username_len);
  if (res == 0) {
    THROW_WINAPI_FUNC_FAILED(GetUserNameEx);
  }
  return domain_username;
}

}
