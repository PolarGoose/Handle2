#pragma once
#include "app/ntdll_functions.h"
#include "app/procexpsys512_driver.h"
#include "app/utils/device_path_to_drive_path_converter.h"
#include "app/utils/noncopyable.h"
#include "app/utils/normalize_path.h"
#include "app/utils/process_info.h"

namespace app {

class locking_info_provider final : app::noncopyable {
  procexpsys512_driver drv;
  ntdll_functions ntdll;
  const device_name_to_drive_letter_map& path_conversion_map =
      create_device_name_to_drive_letter_convertion_map();

public:
  [[nodiscard]] std::vector<process_info>
  get_processes_locking_path(std::filesystem::path p) const {
    if (!std::filesystem::exists(p)) {
      THROW(L"Path '{}' doesn't exist", p);
    }

    p = normalize_path(p);
    auto path_in_capital_letters = to_upper(p);

    std::map<ULONG_PTR, process_info> processes;
    const auto all_handles = ntdll.QuerySystemHandleInformation();
    for (ULONG_PTR i = 0; i < all_handles->NumberOfHandles; i++) {
      try {
        process_handle_if_it_is_a_file(processes, all_handles->Handles[i],
                                       path_in_capital_letters);
      } catch (const app_exception&) {
        /* ignore the exception */
      }
    }

    return to_vector(processes);
  }

private:
  void process_handle_if_it_is_a_file(
      std::map<ULONG_PTR, process_info>& processes,
      const SYSTEM_HANDLE_TABLE_ENTRY_INFO_EX& handle,
      const std::wstring_view& path_in_all_capital_letters) const {
    auto handle_type = drv.get_handle_type(handle);
    if (handle_type != L"File") {
      return;
    }
    auto handle_path = get_handle_path(handle);
    auto handle_path_in_all_capital_letters = to_upper(handle_path);

    if (!handle_path_in_all_capital_letters.starts_with(
            path_in_all_capital_letters)) {
      return;
    }

    const auto& pid = handle.UniqueProcessId;
    if (!processes.contains(pid)) {
      const auto& proc =
          drv.open_process_handle_with_duplicate_handle_access_right(pid);
      processes.emplace(pid, get_process_info(proc.get(), pid));
    }

    processes[pid].locked_paths.emplace_back(handle_path);
  }

  [[nodiscard]] std::wstring
  get_handle_path(const SYSTEM_HANDLE_TABLE_ENTRY_INFO_EX& handle_info) const {
    const auto& handle_name = drv.get_handle_name(handle_info);
    const auto& handle_path =
        get_drive_letter_based_full_name(path_conversion_map, handle_name);
    return normalize_path(handle_path);
  }

  std::vector<process_info> static to_vector(
      const std::map<ULONG_PTR, process_info>& processes) {
    std::vector<process_info> res;
    for (const auto& [_, process_info] : processes) {
      res.emplace_back(process_info);
    }
    return res;
  }
};

}
