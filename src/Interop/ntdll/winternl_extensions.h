// This file contains definitions which are missing from the "winternl.h" Windows header file
#pragma once

// Copied from ntstatus.h because "um/winnt.h" conflicts with general inclusion of "ntstatus.h"
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

struct OBJECT_BASIC_INFORMATION {
  ULONG Attributes;
  ACCESS_MASK GrantedAccess;
  ULONG HandleCount;
  ULONG PointerCount;
  ULONG PagedPoolCharge;
  ULONG NonPagedPoolCharge;
  ULONG Reserved[3];
  ULONG NameInfoSize;
  ULONG TypeInfoSize;
  ULONG SecurityDescriptorSize;
  LARGE_INTEGER CreationTime;
};

struct OBJECT_TYPE_INFORMATION {
  UNICODE_STRING TypeName;
  ULONG TotalNumberOfObjects;
  ULONG TotalNumberOfHandles;
  ULONG TotalPagedPoolUsage;
  ULONG TotalNonPagedPoolUsage;
  ULONG TotalNamePoolUsage;
  ULONG TotalHandleTableUsage;
  ULONG HighWaterNumberOfObjects;
  ULONG HighWaterNumberOfHandles;
  ULONG HighWaterPagedPoolUsage;
  ULONG HighWaterNonPagedPoolUsage;
  ULONG HighWaterNamePoolUsage;
  ULONG HighWaterHandleTableUsage;
  ULONG InvalidAttributes;
  GENERIC_MAPPING GenericMapping;
  ULONG ValidAccessMask;
  BOOLEAN SecurityRequired;
  BOOLEAN MaintainHandleCount;
  UCHAR TypeIndex; // since WINBLUE
  CHAR ReservedByte;
  ULONG PoolType;
  ULONG DefaultPagedPoolCharge;
  ULONG DefaultNonPagedPoolCharge;
};


const auto SystemExtendedHandleInformation = static_cast<SYSTEM_INFORMATION_CLASS>(64);
const auto ObjectNameInformation = static_cast<SYSTEM_INFORMATION_CLASS>(1);
