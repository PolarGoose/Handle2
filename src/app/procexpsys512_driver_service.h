#pragma once
#include "app/utils/driver_service.h"
#include "app/utils/enable_privilege.h"
#include "app/utils/exception.h"
#include "app/utils/expand_path_with_environment_variables.h"
#include "app/utils/extract_procexp_resource_to_file.h"
#include "app/utils/noncopyable.h"
#include "app/utils/scoped_handle.h"

namespace app {

class procexpsys512_driver_service final : noncopyable {
private:
  const std::filesystem::path sys_file_full_name =
      expand_path_with_environment_variables(
          L"%WINDIR%\\System32\\drivers\\PROCEXP152.SYS");
  const driver_service service{sys_file_full_name, L"PROCEXP152",
                               L"Process Explorer"};
  const scoped_handle driver_file;

public:
  procexpsys512_driver_service()
      : driver_file{start_driver_service_and_connect()} {}

  [[nodiscard]] HANDLE get_driver_file_handle() const {
    return driver_file.get();
  }

private:
  scoped_handle start_driver_service_and_connect() {
    extract_procexp_resource_to_file(sys_file_full_name);
    enable_privilege(L"SeLoadDriverPrivilege");
    enable_privilege(L"SeDebugPrivilege");
    service.start();
    return service.connect_to_driver();
  }
};

}
