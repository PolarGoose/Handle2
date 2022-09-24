cmake_minimum_required (VERSION 3.22)
project(HandleExe LANGUAGES C CXX)
cmake_policy(SET CMP0135 NEW) # avoid warning: "The DOWNLOAD_EXTRACT_TIMESTAMP option was not given"
set(CMAKE_DISABLE_FIND_PACKAGE_Python ON) # finding Python takes a lot of time. This project doesn't use Python
set(VERSION_STRING "0.0-dev-CommitHash" CACHE STRING "")
# set(CMAKE_VERBOSE_MAKEFILE ON CACHE BOOL "ON" FORCE) # uncomment this line to get verbose build output

include(tools/cmake/compiler_options.cmake)
include(tools/cmake/libs/catch2.cmake)
include(tools/cmake/libs/nlohmann_json.cmake)
include(tools/cmake/clang_format.cmake)
include(tools/cmake/download_PROCEXP152SYS_file.cmake)
include(tools/cmake/version.cmake)

file(GLOB_RECURSE app_srcs_shared_with_test CONFIGURE_DEPENDS "src/app/*.h")
file(GLOB_RECURSE test_srcs CONFIGURE_DEPENDS "src/test/*.h" "src/test/*.cpp")

add_executable(app
  src/app/main.cpp
  ${CMAKE_BINARY_DIR}/version.rc
  ${app_srcs_shared_with_test})
configure_version(app src/app/version.rc.in)
target_include_directories(app PRIVATE "src")
target_precompile_headers(app PRIVATE src/app/precompiled_header.h)
add_procexp_resource(app)
add_clang_format(app)
add_nlohmann_json(app)
add_compiler_options_with_warnings(app)

add_executable(test
  ${test_srcs}
  ${app_srcs_shared_with_test})
target_include_directories(test PRIVATE "src")
target_precompile_headers(test PRIVATE src/test/precompiled_header.h)
add_clang_format(test)
add_nlohmann_json(test)
add_procexp_resource(test)
add_compiler_options_with_warnings(test)
add_catch2(test)
