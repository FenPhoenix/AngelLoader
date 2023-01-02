using System;
using System.IO;
using System.Runtime.CompilerServices;
using System.Runtime.ConstrainedExecution;
using System.Runtime.InteropServices;
using System.Security;
using System.Security.Permissions;
using JetBrains.Annotations;
using Microsoft.Win32.SafeHandles;

namespace FMScanner.FastZipReader;

internal static class ZLibNative
{
    internal static readonly IntPtr ZNullPtr = (IntPtr)0;

    [SecurityCritical]
    public static ErrorCode CreateZLibStreamForInflate(
      out ZLibStreamHandle zLibStreamHandle,
      int windowBits)
    {
        zLibStreamHandle = new ZLibStreamHandle();
        return zLibStreamHandle.InflateInit2_(windowBits);
    }

    [UsedImplicitly(ImplicitUseTargetFlags.WithMembers)]
    public enum FlushCode
    {
        NoFlush,
        PartialFlush,
        SyncFlush,
        FullFlush,
        Finish,
        Block
    }

    [UsedImplicitly(ImplicitUseTargetFlags.WithMembers)]
    public enum ErrorCode
    {
        VersionError = -6, // 0xFFFFFFFA
        BufError = -5, // 0xFFFFFFFB
        MemError = -4, // 0xFFFFFFFC
        DataError = -3, // 0xFFFFFFFD
        StreamError = -2, // 0xFFFFFFFE
        ErrorNo = -1, // 0xFFFFFFFF
        Ok = 0,
        StreamEnd = 1,
        NeedDictionary = 2
    }

    [UsedImplicitly(ImplicitUseTargetFlags.WithMembers)]
    private enum CompressionLevel
    {
        DefaultCompression = -1, // 0xFFFFFFFF
        NoCompression = 0,
        BestSpeed = 1,
        BestCompression = 9
    }

    [UsedImplicitly(ImplicitUseTargetFlags.WithMembers)]
    private enum CompressionStrategy
    {
        DefaultStrategy,
        Filtered,
        HuffmanOnly,
        Rle,
        Fixed
    }

    [UsedImplicitly(ImplicitUseTargetFlags.WithMembers)]
    private enum CompressionMethod
    {
        Deflated = 8
    }

#pragma warning disable CS0649
    private struct ZStream
    {
        internal IntPtr nextIn;
        internal uint availIn;
        internal uint totalIn;
        internal IntPtr nextOut;
        internal uint availOut;
        internal uint totalOut;
        internal IntPtr msg;
        internal IntPtr state;
        internal IntPtr zalloc;
        internal IntPtr zfree;
        internal IntPtr opaque;
        internal int dataType;
        internal uint adler;
        internal uint reserved;
    }
#pragma warning restore CS0649

    [UnmanagedFunctionPointer(CallingConvention.StdCall)]
    [SuppressUnmanagedCodeSecurity]
    [SecurityCritical]
    private unsafe delegate ErrorCode DeflateInit2_Delegate(
      ZStream* stream,
      CompressionLevel level,
      CompressionMethod method,
      int windowBits,
      int memLevel,
      CompressionStrategy strategy,
      [MarshalAs(UnmanagedType.LPStr)] string version,
      int streamSize);

    [UnmanagedFunctionPointer(CallingConvention.StdCall)]
    [SuppressUnmanagedCodeSecurity]
    [SecurityCritical]
    private unsafe delegate ErrorCode DeflateDelegate(
      ZStream* stream,
      FlushCode flush);

    [UnmanagedFunctionPointer(CallingConvention.StdCall)]
    [SuppressUnmanagedCodeSecurity]
    [SecurityCritical]
    private unsafe delegate ErrorCode DeflateEndDelegate(ZStream* stream);

    [UnmanagedFunctionPointer(CallingConvention.StdCall)]
    [SuppressUnmanagedCodeSecurity]
    [SecurityCritical]
    private unsafe delegate ErrorCode InflateInit2_Delegate(
      ZStream* stream,
      int windowBits,
      [MarshalAs(UnmanagedType.LPStr)] string version,
      int streamSize);

    [UnmanagedFunctionPointer(CallingConvention.StdCall)]
    [SuppressUnmanagedCodeSecurity]
    [SecurityCritical]
    private unsafe delegate ErrorCode InflateDelegate(
      ZStream* stream,
      FlushCode flush);

    [UnmanagedFunctionPointer(CallingConvention.StdCall)]
    [SuppressUnmanagedCodeSecurity]
    [SecurityCritical]
    private unsafe delegate ErrorCode InflateEndDelegate(ZStream* stream);

    [UnmanagedFunctionPointer(CallingConvention.StdCall)]
    [SuppressUnmanagedCodeSecurity]
    [SecurityCritical]
    private delegate int ZlibCompileFlagsDelegate();

    private static class NativeMethods
    {
        [SuppressUnmanagedCodeSecurity]
        [SecurityCritical]
        [DllImport("kernel32.dll", CharSet = CharSet.Ansi, BestFitMapping = false)]
        internal static extern IntPtr GetProcAddress(
          SafeLibraryHandle moduleHandle,
          string procName);

        [SuppressUnmanagedCodeSecurity]
        [SecurityCritical]
        [DllImport("kernel32.dll", CharSet = CharSet.Unicode, SetLastError = true)]
        internal static extern SafeLibraryHandle LoadLibrary(string libPath);

        [ReliabilityContract(Consistency.WillNotCorruptState, Cer.Success)]
        [SuppressUnmanagedCodeSecurity]
        [SecurityCritical]
        [DllImport("kernel32.dll")]
        internal static extern bool FreeLibrary(IntPtr moduleHandle);
    }

    [SecurityCritical]
    [UsedImplicitly]
    private class SafeLibraryHandle : SafeHandleZeroOrMinusOneIsInvalid
    {
        [SecurityCritical]
        internal SafeLibraryHandle()
          : base(true)
        {
        }

        [ReliabilityContract(Consistency.WillNotCorruptState, Cer.MayFail)]
        [SecurityCritical]
        protected override bool ReleaseHandle()
        {
            bool flag = NativeMethods.FreeLibrary(handle);
            handle = IntPtr.Zero;
            return flag;
        }
    }

    [SecurityCritical]
    public sealed class ZLibStreamHandle : SafeHandleMinusOneIsInvalid
    {
        [SecurityCritical]
        private static SafeLibraryHandle? zlibLibraryHandle;
        [SecurityCritical]
        private unsafe ZStream* zStreamPtr;
        [SecurityCritical]
        private volatile State initializationState;

        public unsafe ZLibStreamHandle()
          : base(true)
        {
            zStreamPtr = (ZStream*)(void*)AllocWithZeroOut(sizeof(ZStream));
            initializationState = State.NotInitialized;
            handle = IntPtr.Zero;
        }

        private State InitializationState
        {
            [SecurityCritical]
            get => initializationState;
        }

        [ReliabilityContract(Consistency.WillNotCorruptState, Cer.MayFail)]
        [SecurityCritical]
        protected override unsafe bool ReleaseHandle()
        {
            try
            {
                if (zlibLibraryHandle?.IsInvalid != false)
                {
                    return false;
                }
                switch (InitializationState)
                {
                    case State.NotInitialized:
                        return true;
                    case State.InitializedForInflate:
                        return InflateEnd() == ErrorCode.Ok;
                    case State.Disposed:
                        return true;
                    default:
                        return false;
                }
            }
            finally
            {
                if ((IntPtr)zStreamPtr != IntPtr.Zero)
                {
                    Marshal.FreeHGlobal((IntPtr)(void*)zStreamPtr);
                    zStreamPtr = (ZStream*)null;
                }
            }
        }

        public unsafe IntPtr NextIn
        {
            [SecurityCritical]
            get => zStreamPtr->nextIn;
            [SecurityCritical]
            set
            {
                if ((IntPtr)zStreamPtr == IntPtr.Zero) return;
                zStreamPtr->nextIn = value;
            }
        }

        public unsafe uint AvailIn
        {
            [SecurityCritical]
            get => zStreamPtr->availIn;
            [SecurityCritical]
            set
            {
                if ((IntPtr)zStreamPtr == IntPtr.Zero) return;
                zStreamPtr->availIn = value;
            }
        }

        public unsafe IntPtr NextOut
        {
            [SecurityCritical]
            get => zStreamPtr->nextOut;
            [SecurityCritical]
            set
            {
                if ((IntPtr)zStreamPtr == IntPtr.Zero) return;
                zStreamPtr->nextOut = value;
            }
        }

        public unsafe uint AvailOut
        {
            [SecurityCritical]
            get => zStreamPtr->availOut;
            [SecurityCritical]
            set
            {
                if ((IntPtr)zStreamPtr == IntPtr.Zero) return;
                zStreamPtr->availOut = value;
            }
        }

        [SecurityCritical]
        private void EnsureNotDisposed()
        {
            if (InitializationState == State.Disposed)
            {
                throw new ObjectDisposedException(GetType().Name);
            }
        }

        [SecurityCritical]
        private void EnsureState(State requiredState)
        {
            if (InitializationState != requiredState)
            {
                throw new InvalidOperationException("InitializationState != " + requiredState);
            }
        }

        [SecurityCritical]
        public unsafe ErrorCode InflateInit2_(int windowBits)
        {
            EnsureNotDisposed();
            EnsureState(State.NotInitialized);
            bool success = false;
            RuntimeHelpers.PrepareConstrainedRegions();
            ErrorCode errorCode;
            try
            {
            }
            finally
            {
                errorCode = NativeZLibDLLStub.inflateInit2_Delegate(zStreamPtr, windowBits, "1.2.11", sizeof(ZStream));
                initializationState = State.InitializedForInflate;
                zlibLibraryHandle.DangerousAddRef(ref success);
            }
            return errorCode;
        }

        [SecurityCritical]
        public unsafe ErrorCode Inflate(FlushCode flush)
        {
            EnsureNotDisposed();
            EnsureState(State.InitializedForInflate);
            return NativeZLibDLLStub.inflateDelegate(zStreamPtr, flush);
        }

        [SecurityCritical]
        private unsafe ErrorCode InflateEnd()
        {
            EnsureNotDisposed();
            EnsureState(State.InitializedForInflate);
            RuntimeHelpers.PrepareConstrainedRegions();
            ErrorCode errorCode;
            try
            {
            }
            finally
            {
                errorCode = NativeZLibDLLStub.inflateEndDelegate(zStreamPtr);
                initializationState = State.Disposed;
                zlibLibraryHandle.DangerousRelease();
            }
            return errorCode;
        }

        [SecurityCritical]
        private static unsafe IntPtr AllocWithZeroOut(int byteCount)
        {
            IntPtr num1 = Marshal.AllocHGlobal(byteCount);
            byte* numPtr1 = (byte*)(void*)num1;
            int num2 = byteCount;
            int num3 = num2 / 4;
            int* numPtr2 = (int*)numPtr1;
            for (int index = 0; index < num3; ++index)
                numPtr2[index] = 0;
            int num4 = num3 * 4;
            byte* numPtr3 = numPtr1 + num4;
            int num5 = num2 - num4;
            for (int index = 0; index < num5; ++index)
                numPtr3[index] = (byte)0;
            return num1;
        }

        [SecurityCritical]
        private static class NativeZLibDLLStub
        {
            [SecurityCritical]
            internal static DeflateInit2_Delegate deflateInit2_Delegate;
            [SecurityCritical]
            internal static DeflateDelegate deflateDelegate;
            [SecurityCritical]
            internal static DeflateEndDelegate deflateEndDelegate;
            [SecurityCritical]
            internal static InflateInit2_Delegate inflateInit2_Delegate;
            [SecurityCritical]
            internal static InflateDelegate inflateDelegate;
            [SecurityCritical]
            internal static InflateEndDelegate inflateEndDelegate;
            [SecurityCritical]
            internal static ZlibCompileFlagsDelegate zlibCompileFlagsDelegate;

            [SecuritySafeCritical]
            private static void LoadZLibDLL()
            {
                new FileIOPermission(PermissionState.Unrestricted).Assert();
                string str = Path.Combine(RuntimeEnvironment.GetRuntimeDirectory(), "clrcompression.dll");
                SafeLibraryHandle safeLibraryHandle = File.Exists(str) ? NativeMethods.LoadLibrary(str) : throw new DllNotFoundException("clrcompression.dll");
                if (safeLibraryHandle.IsInvalid)
                {
                    Marshal.ThrowExceptionForHR(Marshal.GetHRForLastWin32Error(), new IntPtr(-1));
                    throw new InvalidOperationException();
                }
                zlibLibraryHandle = safeLibraryHandle;
            }

            [SecurityCritical]
            private static DT CreateDelegate<DT>(string entryPointName) where DT : Delegate
            {
                IntPtr procAddress = NativeMethods.GetProcAddress(zlibLibraryHandle, entryPointName);
                return IntPtr.Zero != procAddress ? (DT)Marshal.GetDelegateForFunctionPointer(procAddress, typeof(DT)) : throw new EntryPointNotFoundException("clrcompression.dll!" + entryPointName);
            }

            [SecuritySafeCritical]
            private static void InitDelegates()
            {
                deflateInit2_Delegate = CreateDelegate<DeflateInit2_Delegate>("deflateInit2_");
                deflateDelegate = CreateDelegate<DeflateDelegate>("deflate");
                deflateEndDelegate = CreateDelegate<DeflateEndDelegate>("deflateEnd");
                inflateInit2_Delegate = CreateDelegate<InflateInit2_Delegate>("inflateInit2_");
                inflateDelegate = CreateDelegate<InflateDelegate>("inflate");
                inflateEndDelegate = CreateDelegate<InflateEndDelegate>("inflateEnd");
                zlibCompileFlagsDelegate = CreateDelegate<ZlibCompileFlagsDelegate>("zlibCompileFlags");
                RuntimeHelpers.PrepareDelegate(deflateInit2_Delegate);
                RuntimeHelpers.PrepareDelegate(deflateDelegate);
                RuntimeHelpers.PrepareDelegate(deflateEndDelegate);
                RuntimeHelpers.PrepareDelegate(inflateInit2_Delegate);
                RuntimeHelpers.PrepareDelegate(inflateDelegate);
                RuntimeHelpers.PrepareDelegate(inflateEndDelegate);
                RuntimeHelpers.PrepareDelegate(zlibCompileFlagsDelegate);
            }

            [SecuritySafeCritical]
            static NativeZLibDLLStub()
            {
                LoadZLibDLL();
                InitDelegates();
            }
        }

        private enum State
        {
            NotInitialized,
            InitializedForInflate,
            Disposed
        }
    }
}
