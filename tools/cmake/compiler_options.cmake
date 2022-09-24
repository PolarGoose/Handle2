if (NOT CMAKE_CXX_COMPILER_ID STREQUAL "MSVC")
  message(FATAL_ERROR "Unsupported compiler: ${CMAKE_CXX_COMPILER_ID}")
endif()

# Statically link against a runtime library.
set(CMAKE_MSVC_RUNTIME_LIBRARY "MultiThreaded$<$<CONFIG:Debug>:Debug>")

function(_add_compiler_options target)
  target_compile_features(${target} PRIVATE cxx_std_20)
  target_compile_options(${target} PRIVATE /utf-8 /external:anglebrackets /external:W0)
  target_compile_definitions(${target} PRIVATE
    SECURITY_WIN32
    UNICODE
    _UNICODE
    WIN32_LEAN_AND_MEAN
    NOCOMM
    NOMINMAX
    _WIN32_WINNT=_WIN32_WINNT_WIN10
    APP_VERSION_STRING="${VERSION_STRING}")
  target_link_libraries(${target} Advapi32.lib Secur32.lib)
  target_link_options(${target} PRIVATE
    /MANIFESTUAC:level='requireAdministrator'
    /ENTRY:wmainCRTStartup)
endfunction()

function(add_compiler_options_with_warnings target)
  _add_compiler_options(${target})
  target_compile_options(${target} PRIVATE /WX /W4)
endfunction()

function(add_compiler_options_with_warnings_disabled target)
  _add_compiler_options(${target})
  target_compile_options(${target} PRIVATE /W0)
endfunction()
