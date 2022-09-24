#include "app/utils/exception.h"
#include "app/utils/noncopyable.h"
#include "app/utils/string_conversion.h"
#include "app/winternl_extensions.h"

namespace app {

namespace priv {

template <typename T>
T* get_function(HMODULE dll, std::wstring_view function_name) {
  const auto function =
      reinterpret_cast<T*>(GetProcAddress(dll, to_utf8(function_name).c_str()));
  if (!function) {
    THROW_WINAPI_FUNC_FAILED_MSG(GetProcAddress, L"{}", function_name);
  }
  return function;
}

}

#define GET_FUNCTION(dll, function)                                            \
  app::priv::get_function<decltype(function)>(dll, L#function)

class ntdll_functions final : noncopyable {
private:
  decltype(NtQuerySystemInformation)* ntQuerySystemInformation_func;

public:
  ntdll_functions() {
    const auto ntdll_dll = LoadLibrary(L"ntdll.dll");
    if (!ntdll_dll) {
      THROW_WINAPI_FUNC_FAILED(LoadLibrary);
    }

    ntQuerySystemInformation_func =
        GET_FUNCTION(ntdll_dll, NtQuerySystemInformation);
  }

  [[nodiscard]] auto QuerySystemHandleInformation() const {
    const auto mb = 1024 * 1024;
    const auto gb = mb * 1024;
    for (auto buf_size = 32 * mb; buf_size <= gb; buf_size *= 2) {
      auto buf = std::make_unique<std::byte[]>(buf_size);

      ULONG returned_length;
      const auto& status = ntQuerySystemInformation_func(
          SystemExtendedHandleInformation, buf.get(),
          static_cast<ULONG>(buf_size), &returned_length);

      if (NT_SUCCESS(status)) {
        return std::unique_ptr<SYSTEM_HANDLE_INFORMATION_EX>(
            reinterpret_cast<SYSTEM_HANDLE_INFORMATION_EX*>(buf.release()));
      }

      if (!NT_SUCCESS(status) && status != STATUS_INFO_LENGTH_MISMATCH) {
        THROW_WINAPI_FUNC_FAILED(NtQuerySystemInformation);
      }
    }

    THROW_WINAPI_FUNC_FAILED_MSG(
        NtQuerySystemInformation,
        L"NtQuerySystemInformation buffer size is not enough");
  }
};

}
