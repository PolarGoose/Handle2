#pragma once
#include "app/utils/exception.h"

namespace app {

using device_name_to_drive_letter_map = std::map<std::wstring, wchar_t>;

// Creates a conversion map {device_name, drive_letter}, consisting of all
// available logical drives on the machine. Example:
//   [ {"\Device\HardDiskVolume2\"                                 , 'C'},
//     {"\Device\HardDiskVolume15\"                                , 'D'},
//     {"\Device\VBoxMiniRdr\;H:\VBoxSvr\My-H\"                    , 'H'},
//     {"\Device\LanmanRedirector\;I:000215d7\10.22.3.84\i\"       , 'I'},
//     {"\Device\LanmanRedirector\;S:000215d7\10.22.3.84\devshare\", 'S'},
//     {"\Device\LanmanRedirector\;U:000215d7\10.22.3.190\d$\"     , 'U'},
//     {"\Device\LanmanRedirector\;V:000215d7\10.22.3.153\d$\"     , 'V'},
//     {"\Device\CdRom0\"                                          , 'X'} ]
inline device_name_to_drive_letter_map
create_device_name_to_drive_letter_convertion_map() {
  device_name_to_drive_letter_map conversion_map;

  for (auto drive_letter = L'A'; drive_letter <= 'Z'; drive_letter++) {
    std::array<wchar_t, 1024> device_name_buf;
    const wchar_t drive[]{drive_letter, L':', 0};
    const auto length =
        QueryDosDevice(drive, device_name_buf.data(),
                       static_cast<DWORD>(device_name_buf.size()));
    if (length == 0) {
      // Failed to find a device name for the corresponding drive_letter
      continue;
    }

    // The returned from QueryDosDevice device name doesn't have an '\' at the
    // end. We add it to be able to distinguish "\Device\HardDiskVolume1\path"
    // from "\Device\HardDiskVolume10\path" when converting device names to
    // drive letters.
    conversion_map.emplace(std::wstring{device_name_buf.data()} + L'\\',
                           drive_letter);
  }

  return conversion_map;
}

// Converts path
// from "\Device\HardDiskVolume3\Windows\System32\en-US\KernelBase.dll.mui"
// to   "C:\Windows\System32\en-US\KernelBase.dll.mui".
inline std::wstring get_drive_letter_based_full_name(
    const device_name_to_drive_letter_map& conversion_map,
    std::wstring_view device_based_file_full_name) {
  for (const auto& [device_name, drive_letter] : conversion_map) {
    if (device_based_file_full_name.starts_with(device_name)) {
      return std::format(
          L"{}:\\{}", drive_letter,
          device_based_file_full_name.substr(device_name.length()));
    }
  }
  THROW(L"Couldn't convert '{}' to the drive letter based path",
        device_based_file_full_name);
}

}
