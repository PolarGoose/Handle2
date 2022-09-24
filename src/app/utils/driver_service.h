#pragma once

#include "app/utils/exception.h"
#include "app/utils/scoped_handle.h"

namespace app {

class driver_service {
private:
  std::filesystem::path sys_file_full_name;
  std::wstring service_name;
  std::wstring display_name;

public:
  driver_service(std::filesystem::path sys_file_full_name,
                 std::wstring_view service_name, std::wstring_view display_name)
      : sys_file_full_name(std::move(sys_file_full_name)),
        service_name(service_name), display_name(display_name) {}

  void start() const {
    scoped_sc_handle service_manager{
        OpenSCManager(nullptr, nullptr, SC_MANAGER_CREATE_SERVICE)};
    if (!service_manager) {
      THROW_WINAPI_FUNC_FAILED(OpenSCManager);
    }

    scoped_sc_handle driver_service{
        try_create_driver_service(service_manager.get())};
    if (!driver_service) {
      driver_service = open_driver_service(service_manager.get());
    }

    StartService(driver_service.get(), 0, nullptr);
  }

  [[nodiscard]] scoped_handle connect_to_driver() const {
    const auto& file_full_name = std::format(L"\\\\.\\{}", service_name);

    scoped_handle driver_file{CreateFile(file_full_name.c_str(), GENERIC_ALL, 0,
                                         nullptr, OPEN_EXISTING,
                                         FILE_ATTRIBUTE_NORMAL, nullptr)};

    if (driver_file.get() == INVALID_HANDLE_VALUE) {
      THROW_WINAPI_FUNC_FAILED(CreateFile);
    }

    return driver_file;
  }

private:
  scoped_sc_handle
  try_create_driver_service(const SC_HANDLE service_manager) const {
    scoped_sc_handle service{CreateService(
        service_manager, service_name.data(), display_name.data(),
        SERVICE_ALL_ACCESS, SERVICE_KERNEL_DRIVER, SERVICE_DEMAND_START,
        SERVICE_ERROR_NORMAL, sys_file_full_name.c_str(), nullptr, nullptr,
        nullptr, nullptr, nullptr)};
    if (service) {
      return service;
    }
    if (GetLastError() != ERROR_SERVICE_EXISTS) {
      THROW_WINAPI_FUNC_FAILED(CreateService);
    }
    return nullptr;
  }

  scoped_sc_handle open_driver_service(const SC_HANDLE service_manager) const {
    scoped_sc_handle service{
        OpenService(service_manager, service_name.data(), SERVICE_ALL_ACCESS)};
    if (!service) {
      THROW_WINAPI_FUNC_FAILED(OpenService);
    }

    return service;
  }
};

}
