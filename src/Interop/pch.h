#pragma once

#define WIN32_LEAN_AND_MEAN
#define NOCOMM
#define NOMINMAX
#include <array>
#include <codecvt>
#include <filesystem>
#include <format>
#include <memory>
#include <string>
#include <tchar.h>
#include <windows.h>
#include <oaidl.h>
#include <winternl.h>
#include <atlsafe.h>

#ifdef INTEROP_EXPORTS
#define INTEROP_API __declspec(dllexport)
#else
#define INTEROP_API __declspec(dllimport)
#endif
