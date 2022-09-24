set(CLANGFORMAT_EXECUTABLE "C:/Program Files/Microsoft Visual Studio/2022/Community/VC/Tools/Llvm/x64/bin/clang-format.exe")

function(add_clang_format target)
  if(NOT EXISTS ${CLANGFORMAT_EXECUTABLE})
    message(WARNING "clang-format is not found. Skip code formatting step")
    return()
  endif()

  get_target_property(target_src_files ${target} SOURCES)

  foreach(src_file_name ${target_src_files})
    get_filename_component(src_file_full_name ${src_file_name} ABSOLUTE)
    list(APPEND src_files_full_names ${src_file_full_name})
  endforeach()

  add_custom_target(${target}_clangformat
    COMMAND
      ${CLANGFORMAT_EXECUTABLE}
      -style=file
      -i
      ${src_files_full_names}
    WORKING_DIRECTORY
      ${CMAKE_SOURCE_DIR}
    COMMENT
      "Formatting '${target}' project's source code with ${CLANGFORMAT_EXECUTABLE} ..."
  )

  add_dependencies(${target} ${target}_clangformat)
endfunction()
