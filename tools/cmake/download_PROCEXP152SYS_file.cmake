# There is no official link to download the "PROCEXP152.SYS" driver.
# However, this driver can be extracted from "Sysinternals Handle" utility using "Nirsoft ResourcesExtract" program.

include(FetchContent)

FetchContent_Declare(
  nirsoft_resources_extract
  URL https://www.nirsoft.net/utils/resourcesextract-x64.zip
  URL_HASH MD5=b63142248234cd687f43bdc20621992b
)
FetchContent_MakeAvailable(nirsoft_resources_extract)

FetchContent_Declare(
  sysinternals_handle
  URL https://download.sysinternals.com/files/Handle.zip
  URL_HASH MD5=16f9948cb1ddf8ca3c958ad2f1407d09
)
FetchContent_MakeAvailable(sysinternals_handle)

# ResourcesExtract doesn't work if paths are specified using "/" as a separator
file(TO_NATIVE_PATH ${sysinternals_handle_SOURCE_DIR}/handle64.exe sysinternals_handle_exe)
file(TO_NATIVE_PATH ${CMAKE_BINARY_DIR} cmake_bin_dir)

if(NOT EXISTS ${CMAKE_BINARY_DIR}/PROCEXP152.SYS)
  execute_process(
    COMMAND ${nirsoft_resources_extract_SOURCE_DIR}/ResourcesExtract.exe /Source ${sysinternals_handle_exe} /DestFolder ${cmake_bin_dir} /ExtractBinary 1 /FileExistMode 1 /OpenDestFolder 0
    COMMAND_ECHO STDOUT
    COMMAND_ERROR_IS_FATAL ANY)

  # After the extraction we get a file with a name "handle64_103_BINRES.bin". It is a "PROCEXP152.SYS" driver.
  file(RENAME ${CMAKE_BINARY_DIR}/handle64_103_BINRES.bin ${CMAKE_BINARY_DIR}/PROCEXP152.SYS)
endif()

if(NOT EXISTS ${CMAKE_BINARY_DIR}/PROCEXP152_SYS.rc)
  # Generate a rc file
  file(WRITE ${CMAKE_BINARY_DIR}/PROCEXP152_SYS.rc "101 RCDATA \"PROCEXP152.SYS\"")
endif()

function(add_procexp_resource target)
  target_sources(${target} PRIVATE ${CMAKE_BINARY_DIR}/PROCEXP152_SYS.rc)
endfunction()
