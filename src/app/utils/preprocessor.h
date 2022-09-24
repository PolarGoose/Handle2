// Workaround for a code parsing issue I encountered when I opened the project
// in "CLion 2022.3" IDE. The code parser can't parse the "__FUNCTION__" macro
// and showed an error "Expected ')'"
#ifdef __CLION_IDE__
#define __FUNCTION__ "dummy func name"
#endif

#define PRIV_NUMBER_TO_WSTR_HELPER(x) L#x
#define PRIV_NUMBER_TO_WSTR(x) PRIV_NUMBER_TO_WSTR_HELPER(x)

#define LINE_INFO _T(__FUNCTION__) L":" PRIV_NUMBER_TO_WSTR(__LINE__) L": "