#pragma once

#include "app/utils/exception.h"
#include "app/utils/scoped_handle.h"

namespace app {

namespace priv {

inline scoped_handle get_current_process_access_token_to_adjust_privileges() {
  HANDLE access_token;
  if (!OpenProcessToken(GetCurrentProcess(), TOKEN_ADJUST_PRIVILEGES,
                        &access_token)) {
    THROW_WINAPI_FUNC_FAILED(OpenProcessToken);
  }
  return scoped_handle{access_token};
}

inline LUID lookup_privilege(std::wstring_view privilege_name) {
  LUID luid;

  if (!LookupPrivilegeValue(nullptr, privilege_name.data(), &luid)) {
    THROW_WINAPI_FUNC_FAILED(LookupPrivilegeValue);
  }

  return luid;
}

inline void enable_privilege(const HANDLE access_token,
                             const LUID privilege_id) {
  const LUID_AND_ATTRIBUTES attributes{.Luid = privilege_id,
                                       .Attributes = SE_PRIVILEGE_ENABLED};
  TOKEN_PRIVILEGES token{.PrivilegeCount = 1};
  token.Privileges[0] = attributes;

  if (!AdjustTokenPrivileges(access_token, FALSE, &token,
                             sizeof(TOKEN_PRIVILEGES), nullptr, nullptr)) {
    THROW_WINAPI_FUNC_FAILED(AdjustTokenPrivileges);
  }

  if (GetLastError() == ERROR_NOT_ALL_ASSIGNED) {
    THROW_WINAPI_FUNC_FAILED(AdjustTokenPrivileges);
  }
}

}

inline void enable_privilege(std::wstring_view privilege_name) {
  const auto& access_token =
      priv::get_current_process_access_token_to_adjust_privileges();
  const auto& privilege_id = priv::lookup_privilege(privilege_name);
  priv::enable_privilege(access_token.get(), privilege_id);
}

}
