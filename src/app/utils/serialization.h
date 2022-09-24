#pragma once
#include "app/utils/process_info.h"

namespace app {

inline void print_as_text(std::ostream& out,
                          const std::vector<app::process_info>& info) {
  for (const auto& i : info) {
    for (const auto& path : i.locked_paths) {
      out << to_utf8(std::format(L"{:<20}\tpid: {:<7} \t{}\\{:<10}\t{}\n",
                                 i.process_full_name.filename(), i.pid,
                                 i.domain, i.user, path));
    }
  }
}

inline nlohmann::json
locked_paths_to_json(const std::vector<std::filesystem::path>& paths) {
  nlohmann::json json = nlohmann::json::array();
  for (const auto& path : paths) {
    json.push_back(app::to_utf8(path.c_str()));
  }
  return json;
}

inline void print_as_json(std::ostream& out,
                          const std::vector<app::process_info>& info) {
  nlohmann::json json = nlohmann::json::array();
  for (const auto& i : info) {
    json.push_back(
        {{"pid", i.pid},
         {"domain", app::to_utf8(i.domain)},
         {"user", app::to_utf8(i.user)},
         {"process_full_name", app::to_utf8(i.process_full_name.c_str())},
         {"locked_paths", locked_paths_to_json(i.locked_paths)}});
  }
  out << json.dump(2);
}

}
