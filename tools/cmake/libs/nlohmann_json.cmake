file(DOWNLOAD
  https://github.com/nlohmann/json/releases/download/v3.11.2/json.hpp
  ${CMAKE_BINARY_DIR}/nlohmann_json/json.hpp
  EXPECTED_MD5 b1c1ce77d46b72b72f38051d8384c7b8)

function(add_nlohmann_json target)
  target_include_directories(${target} PRIVATE ${CMAKE_BINARY_DIR}/nlohmann_json)
endfunction()
