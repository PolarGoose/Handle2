#include "Interop/ntdll/winternl_extensions.h"
#include "Interop/utils/exception.h"
#include "Interop/utils/noncopyable.h"
#include "Interop/utils/string_conversion.h"

#define GET_FUNCTION(dll, function) interop::ntdll::priv::get_function<decltype(function)>(dll, #function)

namespace interop::ntdll {

namespace priv {

template <typename T> T* get_function(HMODULE dll, std::string_view function_name) {
  const auto function = reinterpret_cast<T*>(GetProcAddress(dll, function_name));
  if (!function) {
    THROW_WINAPI_FUNC_FAILED_MSG(GetProcAddress, L"{}", to_utf8(function_name));
  }
  return function;
}

}

class ntdll_functions final : utils::noncopyable {
private:
  decltype(NtQuerySystemInformation)* ntQuerySystemInformation_func;
  decltype(NtQueryObject)* ntQueryObject_func;

public:
  ntdll_functions() {
    const auto ntdll_dll = LoadLibrary(L"ntdll.dll");
    if (!ntdll_dll) {
      THROW_WINAPI_FUNC_FAILED(LoadLibrary);
    }

    ntQuerySystemInformation_func = GET_FUNCTION(ntdll_dll, NtQuerySystemInformation);
    ntQueryObject_func = GET_FUNCTION(ntdll_dll, NtQueryObject);
  }

  [[nodiscard]] auto query_system_handle_information() const {
    const auto mb = 1024 * 1024;
    const auto gb = mb * 1024;
    for (auto buf_size = 32 * mb; buf_size <= gb; buf_size *= 2) {
      auto buf = std::make_unique<std::byte[]>(buf_size);

      ULONG returned_length;
      const auto& status = ntQuerySystemInformation_func(SystemExtendedHandleInformation, buf.get(), static_cast<ULONG>(buf_size), &returned_length);

      if (NT_SUCCESS(status)) {
        return std::unique_ptr<SYSTEM_HANDLE_INFORMATION_EX>(reinterpret_cast<SYSTEM_HANDLE_INFORMATION_EX*>(buf.release()));
      }

      if (!NT_SUCCESS(status) && status != STATUS_INFO_LENGTH_MISMATCH) {
        THROW_WINAPI_FUNC_FAILED(NtQuerySystemInformation);
      }
    }

    THROW_WINAPI_FUNC_FAILED_MSG(NtQuerySystemInformation, L"NtQuerySystemInformation buffer size is not enough");
  }

  std::optional<std::wstring> get_handle_name(HANDLE handle) {
    ULONG returnLength;
    UNICODE_STRING objectName;
    OBJECT_BASIC_INFORMATION objectInfo;

    auto res = ntQueryObject_func(handle, ObjectBasicInformation, &objectInfo, sizeof(objectInfo), &returnLength);
    if (!NT_SUCCESS(res)) {
      return {};
    }

    if (objectInfo.NameInfoSize == 0) {
      return std::wstring();
    }

    auto nameBuffer = std::make_unique<std::byte[]>(objectInfo.NameInfoSize);
    UNICODE_STRING name {
        .MaximumLength = static_cast<USHORT>(objectInfo.NameInfoSize),
        .Buffer = reinterpret_cast<PWSTR>(nameBuffer.get()),
    };

    res = ntQueryObject_func(handle, ObjectNameInformation, &name, objectInfo.NameInfoSize, &returnLength);
    if (!NT_SUCCESS(res)) {
      return {};
    }

    return std::wstring(name.Buffer, name.Length / sizeof(wchar_t));
  }

  std::optional<std::wstring> get_handle_type(HANDLE handle) {
    ULONG objectTypeInfoSize;
    auto res = ntQueryObject_func(handle, ObjectTypeInformation, nullptr, 0, &objectTypeInfoSize);
    if (!NT_SUCCESS(res) && res != STATUS_INFO_LENGTH_MISMATCH) {
      return {};
    }

    auto typeInfoBuffer = std::make_unique<std::byte[]>(objectTypeInfoSize);
    OBJECT_TYPE_INFORMATION* typeInfo = reinterpret_cast<OBJECT_TYPE_INFORMATION*>(typeInfoBuffer.get());

    res = ntQueryObject_func(handle, ObjectTypeInformation, typeInfo, objectTypeInfoSize, &objectTypeInfoSize);
    if (!NT_SUCCESS(res)) {
      return {};
    }

    return std::wstring(typeInfo->TypeName.Buffer, typeInfo->TypeName.Length / sizeof(wchar_t));
  }
};

}
