#pragma once

#ifdef INTEROP_EXPORTS
#define INTEROP_API __declspec(dllexport)
#else
#define INTEROP_API __declspec(dllimport)
#endif

extern "C" {

struct raw_handle_info {
  HANDLE handle;
  int pid;
};

// https://www.codeproject.com/Articles/1189085/Passing-Strings-Between-Managed-and-Unmanaged-Code

INTEROP_API bool get_handles(BSTR* error_message, SAFEARRAY*& raw_handle_infos);

}
