#include "app/utils/device_path_to_drive_path_converter.h"

bool contains_value(const app::device_name_to_drive_letter_map& map,
                    const wchar_t value) {
  return std::ranges::any_of(
      map, [&](const auto& key_value) { return key_value.second == value; });
}

TEST_CASE("Path conversion map is created correctly") {
  const auto& conversion_map =
      app::create_device_name_to_drive_letter_convertion_map();

  std::cout << "Content of the conversion map:\n";
  for (const auto& [device_name, device_letter] : conversion_map) {
    std::wcout << std::format(L"device_name='{}' drive_letter='{}'\n",
                              device_name, device_letter);
  }

  REQUIRE(conversion_map.empty() == false);
  REQUIRE(contains_value(conversion_map, 'C') == true);
  REQUIRE(conversion_map.begin()->first.ends_with(L"\\") == true);
}

void assert_that_paths_are_converted_properly(
    const app::device_name_to_drive_letter_map& conversion_map,
    const std::map<std::wstring, std::wstring>&
        device_based_path_to_expected_path_map) {
  for (const auto& [device_based_path, expected_drive_based_path] :
       device_based_path_to_expected_path_map) {
    const auto& res = app::get_drive_letter_based_full_name(conversion_map,
                                                            device_based_path);
    REQUIRE(expected_drive_based_path == res);
  }
}

void assert_that_wrong_path_fails_to_convert(
    const app::device_name_to_drive_letter_map& conversion_map,
    const std::vector<std::wstring>& wrong_device_based_paths) {
  for (const auto& path : wrong_device_based_paths) {
    REQUIRE_THROWS_AS(
        app::get_drive_letter_based_full_name(conversion_map, path),
        app::app_exception);
  }
}

TEST_CASE("Path_conversion_works_properly") {
  const app::device_name_to_drive_letter_map conversion_map = {
      {L"\\Device\\HardDiskVolume2\\", L'C'},
      {L"\\Device\\HardDiskVolume15\\", L'D'},
      {L"\\Device\\HardDiskVolume25\\", L'E'},
      {L"\\Device\\Lr\\;I:0d7\\10.22.3.84\\i\\", L'I'},
      {L"\\Device\\CdRom0\\", L'X'}};

  assert_that_paths_are_converted_properly(
      conversion_map,
      {{L"\\Device\\HardDiskVolume2\\Windows\\System32\\en-US\\K.dll",
        L"C:\\Windows\\System32\\en-US\\K.dll"},
       {L"\\Device\\HardDiskVolume2\\", L"C:\\"},
       {L"\\Device\\HardDiskVolume25\\", L"E:\\"},
       {L"\\Device\\HardDiskVolume25\\folder\\1.txt", L"E:\\folder\\1.txt"},
       {L"\\Device\\Lr\\;I:0d7\\10.22.3.84\\i\\folder\\1.txt",
        L"I:\\folder\\1.txt"},
       {L"\\Device\\HardDiskVolume2\\folder\\", L"C:\\folder\\"}});

  assert_that_wrong_path_fails_to_convert(
      conversion_map, {
                          L"",
                          L"\\Device\\HardDiskVolume22\\1.txt",
                          L"\\Device\\HardDiskVolume3\\1.txt",
                      });
}
