#pragma once
#include "app/utils/exception.h"

namespace app {

namespace priv {

bool is_help_arg(std::wstring_view arg) {
  return arg == L"-h" || arg == L"--help" || arg == L"/?" || arg == L"-?" ||
         arg == L"/h";
}

std::wstring join_args(const std::span<std::wstring_view> args) {
  std::wstring res;
  for (const auto& arg : args) {
    res += std::format(L"'{}' ", arg);
  }
  return res;
}

bool check_if_flag_exists_and_remove_it_from_args(
    std::vector<std::wstring_view>& args, std::wstring_view flag) {
  for (size_t i = 0; i < args.size(); i++) {
    if (args[i] == flag) {
      args.erase(args.begin() + i);
      return true;
    }
  }
  return false;
}

}

struct command_line_args {
  bool json_output = false;
  bool print_help = false;
  std::filesystem::path file_or_folder_name;
  std::wstring error_message;
};

inline command_line_args parse_commandline_args(const int argc,
                                                const wchar_t* const* argv) {
  command_line_args res;

  // skip the first argument because it is a name of the executable
  std::vector<std::wstring_view> args(argv + 1, argv + argc);

  if (args.empty()) {
    res.error_message = L"No arguments are provided";
    return res;
  }

  if (args.size() == 1 && priv::is_help_arg(args[0])) {
    res.print_help = true;
    return res;
  }

  res.json_output =
      priv::check_if_flag_exists_and_remove_it_from_args(args, L"--json");

  if (args.size() > 1) {
    res.error_message =
        L"Too many arguments are provided. Provided arguments: " +
        priv::join_args(args);
    return res;
  }

  if (args.size() != 1) {
    res.error_message = L"'file_or_folder_name' is not provided";
    return res;
  }

  res.file_or_folder_name = args[0];
  return res;
}

}
