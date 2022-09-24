#include "app/parse_commandline_args.h"

app::command_line_args call_parse(std::vector<std::wstring> args) {
  args.insert(args.begin(), L"path_to_executable");

  std::vector<wchar_t*> raw_args;
  for (auto& str : args) {
    raw_args.emplace_back(str.data());
  }

  return app::parse_commandline_args(static_cast<int>(raw_args.size()),
                                     raw_args.data());
}

TEST_CASE("Path is provided") {
  const auto res = call_parse({L"C:\\Program Files"});
  REQUIRE(res.file_or_folder_name == L"C:\\Program Files");
}

TEST_CASE("No arguments are provided") {
  const auto res = call_parse({});
  REQUIRE(res.error_message == L"No arguments are provided");
}

TEST_CASE("Too many parameters") {
  {
    const auto res = call_parse({L"par 1", L"--help", L"--json"});
    REQUIRE(res.error_message == L"Too many arguments are provided. Provided "
                                 L"arguments: 'par 1' '--help' ");
  }
  {
    const auto res = call_parse({L"par 1", L"par 2", L"par 3"});
    REQUIRE(res.error_message == L"Too many arguments are provided. Provided "
                                 L"arguments: 'par 1' 'par 2' 'par 3' ");
  }
}

TEST_CASE("Help parameter") {
  auto help_arg = GENERATE(L"-h", L"--help", L"/?", L"-?", L"/h");

  const auto res = call_parse({help_arg});
  INFO(std::format("Used help parameter: {}", app::to_utf8(help_arg)));
  REQUIRE(res.print_help == true);
}

TEST_CASE("Path with json flag") {
  {
    const auto res = call_parse({L"C:\\Program Files", L"--json"});
    REQUIRE(res.file_or_folder_name == L"C:\\Program Files");
    REQUIRE(res.json_output == true);
  }

  {
    const auto res = call_parse({L"--json", L"C:\\Program Files"});
    REQUIRE(res.file_or_folder_name == L"C:\\Program Files");
    REQUIRE(res.json_output == true);
  }
}
