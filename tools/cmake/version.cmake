if(NOT VERSION_STRING MATCHES "^([0-9]+)\.([0-9]+).*$")
    message(FATAL_ERROR "The specified value VERSION _STRING='${VERSION_STRING}' has an invalid format. The format should be 'MajorVersion.MinorVersion-CommitHash'")
endif()

set(VERSION_MAJOR ${CMAKE_MATCH_1})
set(VERSION_MINOR ${CMAKE_MATCH_2})

function(configure_version target version_resource_file)
  configure_file(${version_resource_file} ${CMAKE_BINARY_DIR}/version.rc @ONLY)
  target_sources(${target} PRIVATE ${CMAKE_BINARY_DIR}/version.rc)
endfunction()
