#include "Interop/interface/api.h"
#include "Interop/ntdll/ntdll_functions.h"
#include "pch.h"

extern "C" {

// INTEROP_API void get_handles(BSTR* error_message, SAFEARRAY*& handle_info) {
//   std::vector<std::string> names = {"123", "321", "name"};
//   CComSafeArray<BSTR> safe_array;
//
//   try {
//     for (const auto& name : names) {
//       CComBSTR bstr_name(name.c_str());
//       safe_array.Add(bstr_name);
//     }
//     handle_info = safe_array.Detach();
//   } catch (const std::exception& e) {
//     *error_message = CComBSTR(CA2W(e.what())).Detach();
//   }
// }
}
