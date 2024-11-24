﻿//#define ENABLE_UNUSED

// Licensed to the .NET Foundation under one or more agreements.
// The .NET Foundation licenses this file to you under the MIT license.

namespace AL_Common;

internal static partial class Interop
{
    // As defined in winerror.h and https://learn.microsoft.com/windows/win32/debug/system-error-codes
    internal static class Errors
    {
        internal const int ERROR_SUCCESS = 0x0;
#if ENABLE_UNUSED
        internal const int ERROR_INVALID_FUNCTION = 0x1;
#endif
        internal const int ERROR_FILE_NOT_FOUND = 0x2;
        internal const int ERROR_PATH_NOT_FOUND = 0x3;
        internal const int ERROR_ACCESS_DENIED = 0x5;
#if ENABLE_UNUSED
        internal const int ERROR_INVALID_HANDLE = 0x6;
        internal const int ERROR_NOT_ENOUGH_MEMORY = 0x8;
        internal const int ERROR_INVALID_ACCESS = 0xC;
        internal const int ERROR_INVALID_DATA = 0xD;
        internal const int ERROR_OUTOFMEMORY = 0xE;
        internal const int ERROR_INVALID_DRIVE = 0xF;
#endif
        internal const int ERROR_NO_MORE_FILES = 0x12;
#if ENABLE_UNUSED
        internal const int ERROR_NOT_READY = 0x15;
        internal const int ERROR_BAD_COMMAND = 0x16;
        internal const int ERROR_BAD_LENGTH = 0x18;
#endif
        internal const int ERROR_SHARING_VIOLATION = 0x20;
#if ENABLE_UNUSED
        internal const int ERROR_LOCK_VIOLATION = 0x21;
        internal const int ERROR_HANDLE_EOF = 0x26;
        internal const int ERROR_NOT_SUPPORTED = 0x32;
        internal const int ERROR_BAD_NETPATH = 0x35;
        internal const int ERROR_NETWORK_ACCESS_DENIED = 0x41;
        internal const int ERROR_BAD_NET_NAME = 0x43;
#endif
        internal const int ERROR_FILE_EXISTS = 0x50;
        internal const int ERROR_INVALID_PARAMETER = 0x57;
#if ENABLE_UNUSED
        internal const int ERROR_BROKEN_PIPE = 0x6D;
        internal const int ERROR_DISK_FULL = 0x70;
        internal const int ERROR_SEM_TIMEOUT = 0x79;
        internal const int ERROR_CALL_NOT_IMPLEMENTED = 0x78;
        internal const int ERROR_INSUFFICIENT_BUFFER = 0x7A;
        internal const int ERROR_INVALID_NAME = 0x7B;
        internal const int ERROR_INVALID_LEVEL = 0x7C;
        internal const int ERROR_MOD_NOT_FOUND = 0x7E;
        internal const int ERROR_NEGATIVE_SEEK = 0x83;
#endif
        internal const int ERROR_DIR_NOT_EMPTY = 0x91;
#if ENABLE_UNUSED
        internal const int ERROR_BAD_PATHNAME = 0xA1;
        internal const int ERROR_LOCK_FAILED = 0xA7;
        internal const int ERROR_BUSY = 0xAA;
#endif
        internal const int ERROR_ALREADY_EXISTS = 0xB7;
#if ENABLE_UNUSED
        internal const int ERROR_BAD_EXE_FORMAT = 0xC1;
        internal const int ERROR_ENVVAR_NOT_FOUND = 0xCB;
#endif
        internal const int ERROR_FILENAME_EXCED_RANGE = 0xCE;
#if ENABLE_UNUSED
        internal const int ERROR_EXE_MACHINE_TYPE_MISMATCH = 0xD8;
        internal const int ERROR_FILE_TOO_LARGE = 0xDF;
        internal const int ERROR_PIPE_BUSY = 0xE7;
        internal const int ERROR_NO_DATA = 0xE8;
        internal const int ERROR_PIPE_NOT_CONNECTED = 0xE9;
        internal const int ERROR_MORE_DATA = 0xEA;
        internal const int ERROR_NO_MORE_ITEMS = 0x103;
        internal const int ERROR_DIRECTORY = 0x10B;
        internal const int ERROR_NOT_OWNER = 0x120;
        internal const int ERROR_TOO_MANY_POSTS = 0x12A;
        internal const int ERROR_PARTIAL_COPY = 0x12B;
        internal const int ERROR_ARITHMETIC_OVERFLOW = 0x216;
        internal const int ERROR_PIPE_CONNECTED = 0x217;
        internal const int ERROR_PIPE_LISTENING = 0x218;
        internal const int ERROR_MUTANT_LIMIT_EXCEEDED = 0x24B;
#endif
        internal const int ERROR_OPERATION_ABORTED = 0x3E3;
#if ENABLE_UNUSED
        internal const int ERROR_IO_INCOMPLETE = 0x3E4;
        internal const int ERROR_IO_PENDING = 0x3E5;
        internal const int ERROR_INVALID_FLAGS = 0x3EC;
        internal const int ERROR_NO_TOKEN = 0x3f0;
        internal const int ERROR_SERVICE_DOES_NOT_EXIST = 0x424;
        internal const int ERROR_EXCEPTION_IN_SERVICE = 0x428;
        internal const int ERROR_PROCESS_ABORTED = 0x42B;
        internal const int ERROR_FILEMARK_DETECTED = 0x44D;
        internal const int ERROR_NO_UNICODE_TRANSLATION = 0x459;
        internal const int ERROR_DLL_INIT_FAILED = 0x45A;
        internal const int ERROR_COUNTER_TIMEOUT = 0x461;
        internal const int ERROR_NO_ASSOCIATION = 0x483;
        internal const int ERROR_DDE_FAIL = 0x484;
        internal const int ERROR_DLL_NOT_FOUND = 0x485;
        internal const int ERROR_NOT_FOUND = 0x490;
        internal const int ERROR_INVALID_DOMAINNAME = 0x4BC;
        internal const int ERROR_CANCELLED = 0x4C7;
        internal const int ERROR_NETWORK_UNREACHABLE = 0x4CF;
        internal const int ERROR_NON_ACCOUNT_SID = 0x4E9;
        internal const int ERROR_NOT_ALL_ASSIGNED = 0x514;
        internal const int ERROR_UNKNOWN_REVISION = 0x519;
        internal const int ERROR_INVALID_OWNER = 0x51B;
        internal const int ERROR_INVALID_PRIMARY_GROUP = 0x51C;
        internal const int ERROR_NO_LOGON_SERVERS = 0x51F;
        internal const int ERROR_NO_SUCH_LOGON_SESSION = 0x520;
        internal const int ERROR_NO_SUCH_PRIVILEGE = 0x521;
        internal const int ERROR_PRIVILEGE_NOT_HELD = 0x522;
        internal const int ERROR_INVALID_ACL = 0x538;
        internal const int ERROR_INVALID_SECURITY_DESCR = 0x53A;
        internal const int ERROR_INVALID_SID = 0x539;
        internal const int ERROR_BAD_IMPERSONATION_LEVEL = 0x542;
        internal const int ERROR_CANT_OPEN_ANONYMOUS = 0x543;
        internal const int ERROR_NO_SECURITY_ON_OBJECT = 0x546;
        internal const int ERROR_NO_SUCH_DOMAIN = 0x54B;
        internal const int ERROR_CANNOT_IMPERSONATE = 0x558;
        internal const int ERROR_CLASS_ALREADY_EXISTS = 0x582;
        internal const int ERROR_NO_SYSTEM_RESOURCES = 0x5AA;
        internal const int ERROR_TIMEOUT = 0x5B4;
        internal const int ERROR_EVENTLOG_FILE_CHANGED = 0x5DF;
        internal const int RPC_S_OUT_OF_RESOURCES = 0x6B9;
        internal const int RPC_S_SERVER_UNAVAILABLE = 0x6BA;
        internal const int RPC_S_CALL_FAILED = 0x6BE;
        internal const int ERROR_TRUSTED_RELATIONSHIP_FAILURE = 0x6FD;
        internal const int ERROR_RESOURCE_TYPE_NOT_FOUND = 0x715;
        internal const int ERROR_RESOURCE_LANG_NOT_FOUND = 0x717;
        internal const int RPC_S_CALL_CANCELED = 0x71A;
        internal const int ERROR_NO_SITENAME = 0x77F;
        internal const int ERROR_NOT_A_REPARSE_POINT = 0x1126;
        internal const int ERROR_DS_NAME_UNPARSEABLE = 0x209E;
        internal const int ERROR_DS_UNKNOWN_ERROR = 0x20EF;
        internal const int ERROR_DS_DRA_BAD_DN = 0x20F7;
        internal const int ERROR_DS_DRA_OUT_OF_MEM = 0x20FE;
        internal const int ERROR_DS_DRA_ACCESS_DENIED = 0x2105;
        internal const int DNS_ERROR_RCODE_NAME_ERROR = 0x232B;
        internal const int ERROR_EVT_QUERY_RESULT_STALE = 0x3AA3;
        internal const int ERROR_EVT_QUERY_RESULT_INVALID_POSITION = 0x3AA4;
        internal const int ERROR_EVT_INVALID_EVENT_DATA = 0x3A9D;
        internal const int ERROR_EVT_PUBLISHER_METADATA_NOT_FOUND = 0x3A9A;
        internal const int ERROR_EVT_CHANNEL_NOT_FOUND = 0x3A9F;
        internal const int ERROR_EVT_MESSAGE_NOT_FOUND = 0x3AB3;
        internal const int ERROR_EVT_MESSAGE_ID_NOT_FOUND = 0x3AB4;
        internal const int ERROR_EVT_PUBLISHER_DISABLED = 0x3ABD;
#endif
    }
}
