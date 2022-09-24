#pragma once
#include "app/procexpsys512_driver_service.h"
#include "app/winternl_extensions.h"

namespace app {

class procexpsys512_driver final : noncopyable {
private:
  enum class ioctl_command : DWORD {
    open_protected_process_handle = 2201288764, // 0x8335003C
    get_handle_name = 2201288776,               // 0x83350048
    get_handle_type = 2201288780                // 0x8335004C
  };
  const procexpsys512_driver_service driver;

public:
  [[nodiscard]] scoped_handle
  open_process_handle_with_duplicate_handle_access_right(ULONGLONG pid) const {
    HANDLE opened_process_handle{};
    const auto res =
        DeviceIoControl(driver.get_driver_file_handle(),
                        (DWORD)ioctl_command::open_protected_process_handle,
                        (LPVOID)&pid, sizeof(pid), &opened_process_handle,
                        sizeof(opened_process_handle), nullptr, nullptr);
    if (!res) {
      THROW_WINAPI_FUNC_FAILED_MSG(
          DeviceIoControl,
          L"open_process_handle_with_duplicate_handle_access_right");
    }

    return scoped_handle{opened_process_handle};
  }

  // returns "<No name>" in case a handle is unnamed
  [[nodiscard]] std::wstring
  get_handle_name(const SYSTEM_HANDLE_TABLE_ENTRY_INFO_EX& handle_info) const {
    return get_handle_name_or_type(ioctl_command::get_handle_name, handle_info);
  }

  // handle types:
  // * ALPC Port
  // * Composition
  // * CoreMessaging
  // * DebugObject
  // * Desktop
  // * Directory
  // * DxgkCompositionObject
  // * DxgkDisplayManagerObject
  // * DxgkSharedResource
  // * DxgkSharedSyncObject
  // * EnergyTracker
  // * EtwConsumer
  // * EtwRegistration
  // * Event
  // * File
  // * FilterCommunicationPort
  // * FilterConnectionPort
  // * IRTimer
  // * IoCompletion
  // * IoCompletionReserve
  // * Job
  // * Key
  // * Mutant
  // * Partition
  // * PcwObject
  // * PowerRequest
  // * Process
  // * RawInputManager
  // * Section
  // * Semaphore
  // * Session
  // * SymbolicLink
  // * Thread
  // * Timer
  // * TmEn
  // * TmRm
  // * TmTm
  // * Token
  // * TpWorkerFactory
  // * UserApcReserve
  // * WaitCompletionPacket
  // * WindowStation
  // * WmiGuid
  [[nodiscard]] std::wstring
  get_handle_type(const SYSTEM_HANDLE_TABLE_ENTRY_INFO_EX& handle_info) const {
    return get_handle_name_or_type(ioctl_command::get_handle_type, handle_info);
  }

private:
  [[nodiscard]] std::wstring get_handle_name_or_type(
      const ioctl_command get_name_or_type_io_control_code,
      const SYSTEM_HANDLE_TABLE_ENTRY_INFO_EX& handle_info) const {
    struct PROCEXP_DATA_EXCHANGE {
      ULONGLONG Pid;
      void* ObjectAddress;
      ULONGLONG Size;
      HANDLE Handle;
    };

    PROCEXP_DATA_EXCHANGE data{
        .Pid = handle_info.UniqueProcessId,
        .ObjectAddress = handle_info.Object,
        .Size = 0,
        .Handle = handle_info.HandleValue,
    };

    // based on ProcExp max object name size
    const DWORD max_buf_size = 2056 + 1;

    wchar_t handle_name_or_type[max_buf_size]{};
    DWORD bytesReturned{};

    const auto res = DeviceIoControl(
        driver.get_driver_file_handle(),
        (DWORD)get_name_or_type_io_control_code, (LPVOID)&data, sizeof(data),
        handle_name_or_type, max_buf_size, &bytesReturned, nullptr);
    if (!res) {
      THROW_WINAPI_FUNC_FAILED_MSG(DeviceIoControl, L"io_control_code = {}",
                                   to_string(get_name_or_type_io_control_code));
    }

    if (bytesReturned == 8) {
      return L"<No name>";
    }

    return std::wstring{handle_name_or_type + 2};
  }

  static std::wstring to_string(const ioctl_command& command) {
    switch (command) {
    case ioctl_command::open_protected_process_handle:
      return L"ioctl_command::open_protected_process_handle";
    case ioctl_command::get_handle_name:
      return L"ioctl_command::get_handle_name";
    case ioctl_command::get_handle_type:
      return L"ioctl_command::get_handle_type";
    }
    THROW(L"Unknown ioctl_command={}", (DWORD)command);
  }
};

}
