file(DOWNLOAD
  https://github.com/catchorg/Catch2/releases/download/v3.1.1/catch_amalgamated.hpp
  ${CMAKE_BINARY_DIR}/catch2/catch_amalgamated.hpp
  EXPECTED_MD5 bed56d7db3f0cfb6a29e10c9de29af39)
file(DOWNLOAD
  https://github.com/catchorg/Catch2/releases/download/v3.1.1/catch_amalgamated.cpp
  ${CMAKE_BINARY_DIR}/catch2/catch_amalgamated.cpp
  EXPECTED_MD5 fc3626c4a36675e4d59864eec8d29218)

add_library(catch2
  ${CMAKE_BINARY_DIR}/catch2/catch_amalgamated.hpp
  ${CMAKE_BINARY_DIR}/catch2/catch_amalgamated.cpp)
add_compiler_options_with_warnings_disabled(catch2)
target_include_directories(catch2 INTERFACE ${CMAKE_BINARY_DIR}/catch2)

function(add_catch2 target)
  target_link_libraries(${target} catch2)
endfunction()
