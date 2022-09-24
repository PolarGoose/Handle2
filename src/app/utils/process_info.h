#pragma once
#include "app/procexpsys512_driver.h"

namespace app {

namespace priv {

inline auto open_process_token_with_query_access(const HANDLE opened_process) {
  HANDLE token{nullptr};
  if (!OpenProcessToken(opened_process, TOKEN_QUERY, &token)) {
    THROW_WINAPI_FUNC_FAILED(OpenProcessToken);
  }
  return scoped_handle{token};
}

inline auto get_token_user_information(HANDLE token) {
  DWORD tokenSize = 0;
  if (!GetTokenInformation(token, TokenUser, nullptr, 0, &tokenSize) &&
      GetLastError() != ERROR_INSUFFICIENT_BUFFER) {
    THROW_WINAPI_FUNC_FAILED(GetTokenInformation);
  }

  auto buf = std::make_unique<std::byte[]>(tokenSize);
  if (!GetTokenInformation(token, TokenUser, buf.get(), tokenSize,
                           &tokenSize)) {
    THROW_WINAPI_FUNC_FAILED(GetTokenInformation);
  }

  return std::unique_ptr<TOKEN_USER>(
      reinterpret_cast<TOKEN_USER*>(buf.release()));
}

inline auto get_user_and_domain(const PSID sid) {
  DWORD userSize = 0;
  DWORD domainSize = 0;
  SID_NAME_USE sidName;
  if (!LookupAccountSid(nullptr, sid, nullptr, &userSize, nullptr, &domainSize,
                        &sidName) &&
      GetLastError() != ERROR_INSUFFICIENT_BUFFER) {
    THROW_WINAPI_FUNC_FAILED(LookupAccountSid);
  }

  auto user = std::make_unique<wchar_t[]>(userSize);
  auto domain = std::make_unique<wchar_t[]>(domainSize);
  if (!LookupAccountSid(nullptr, sid, user.get(), &userSize, domain.get(),
                        &domainSize, &sidName)) {
    THROW_WINAPI_FUNC_FAILED(LookupAccountSid);
  }

  return std::pair{std::wstring(user.get()), std::wstring(domain.get())};
}

inline auto get_user_and_domain_name(const HANDLE opened_process) {
  const auto& token =
      priv::open_process_token_with_query_access(opened_process);
  const auto& user_info = priv::get_token_user_information(token.get());
  return priv::get_user_and_domain(user_info->User.Sid);
}

inline auto get_process_full_name(HANDLE opened_process) {
  const DWORD buf_size = 16'384;
  auto buf = std::make_unique<wchar_t[]>(buf_size);

  DWORD res_buf_size = buf_size;
  const auto res =
      QueryFullProcessImageName(opened_process, 0, buf.get(), &res_buf_size);
  if (res == 0 || res_buf_size == buf_size) {
    THROW_WINAPI_FUNC_FAILED(QueryFullProcessImageName);
  }
  return std::filesystem::path(buf.get());
}

}

struct process_info {
  ULONG_PTR pid{};
  std::filesystem::path process_full_name;
  std::wstring user;
  std::wstring domain;
  std::vector<std::filesystem::path> locked_paths;
};

inline process_info get_process_info(const HANDLE opened_proc,
                                     const UINT_PTR& pid) {
  if (pid == 0) {
    return process_info{.pid = pid,
                        .process_full_name = L"System Idle Process",
                        .user = L"SYSTEM",
                        .domain = L"NT AUTHORITY"};
  }

  if (pid == 4) {
    return process_info{.pid = pid,
                        .process_full_name = L"System",
                        .user = L"SYSTEM",
                        .domain = L"NT AUTHORITY"};
  }

  const auto& [user, domain] = priv::get_user_and_domain_name(opened_proc);
  const auto& name = priv::get_process_full_name(opened_proc);
  return process_info{
      .pid = pid, .process_full_name = name, .user = user, .domain = domain};
}

}
