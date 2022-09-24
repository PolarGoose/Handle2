// This file contains definitions which are missing from the "winternl.h"
// Windows header file
#pragma once

// Copied from ntstatus.h because "um/winnt.h" conflicts with general inclusion
// of "ntstatus.h"
#define STATUS_INFO_LENGTH_MISMATCH ((NTSTATUS)0xC0000004L)

struct SYSTEM_HANDLE_TABLE_ENTRY_INFO_EX {
  void* Object;
  ULONG_PTR UniqueProcessId;
  HANDLE HandleValue;
  ULONG GrantedAccess;
  USHORT CreatorBackTraceIndex;
  USHORT ObjectTypeIndex;
  ULONG HandleAttributes;
  ULONG Reserved;
};

struct SYSTEM_HANDLE_INFORMATION_EX {
  ULONG_PTR NumberOfHandles;
  ULONG_PTR Reserved;
  SYSTEM_HANDLE_TABLE_ENTRY_INFO_EX Handles[1];
};

const auto SystemExtendedHandleInformation =
    static_cast<SYSTEM_INFORMATION_CLASS>(64);
