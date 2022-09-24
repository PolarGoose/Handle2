#include "app/utils/serialization.h"

const std::vector<app::process_info> info = {
    {.pid = 123,
     .process_full_name = L"C:\\process_name.exe",
     .user = L"user_name1",
     .domain = L"domain_name1",
     .locked_paths = {L"C:\\path1", L"C:\\path2"}},
    {.pid = 456,
     .process_full_name = L"C:\\process_name2.exe",
     .user = L"user_name2",
     .domain = L"domain_name2",
     .locked_paths = {L"C:\\path3", L"C:\\path4"}}};

TEST_CASE("Serialization to text") {
  std::stringstream s;
  app::print_as_text(s, info);
  std::cout << std::format("Process info to text:\n{}\n", s.str());
}

TEST_CASE("Serialization to json") {
  std::stringstream s;
  app::print_as_json(s, info);
  std::cout << std::format("Process info to json:\n{}\n", s.str());
}
