#include "app/locking_info_provider.h"
#include "app/parse_commandline_args.h"
#include "app/utils/serialization.h"

const std::string_view help_text =
    "Handle2 v" APP_VERSION_STRING
    " - a tool to show what processes lock a file or folder.\n"
    R"(The program outputs the following information:
    process_name|pid|domain/user_name|locked_file_path

Usage: handle2.exe [--help] [--json] file_or_folder_name

--help                 display this help and exit
--json                 print output as a json
file_or_folder_name    file or folder name. '\' and '/' separators can be used. Can be a relative path.

Examples:
    handle2.exe "C:\my repository\project\out\file.exe"
    handle2.exe "..\out\file.exe"
    handle2.exe --json "C:/my repository/project/out"
)";

int wmain(int argc, wchar_t* argv[]) {
  try {
    const auto& res = app::parse_commandline_args(argc, argv);
    if (res.print_help) {
      std::cout << help_text;
      return 0;
    }

    if (!res.error_message.empty()) {
      std::cerr << app::to_utf8(
          std::format(L"Failed to parse command line arguments: "
                      L"{}\n\n",
                      res.error_message));
      std::cerr << std::format("Usage information:\n{}", help_text);
      return -1;
    }

    const app::locking_info_provider info_provider;
    const auto& info =
        info_provider.get_processes_locking_path(res.file_or_folder_name);

    if (res.json_output) {
      app::print_as_json(std::cout, info);
    } else {
      app::print_as_text(std::cout, info);
    }
  } catch (const std::exception& ex) {
    std::cerr << std::format("Failed: {}", ex.what());
    return -1;
  }

  return 0;
}
