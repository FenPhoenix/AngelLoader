using System;
using System.IO;
using System.Runtime.InteropServices;
using System.Security;
using System.Threading;
using AL_Common.FastZipReader;

namespace AL_Common.DeflateStreamCustom;

internal class InflaterZlib : IDisposable
{
    private bool _finished;
    private bool _isDisposed;
    private ZLibNative.ZLibStreamHandle _zlibStream;
    private GCHandle _inputBufferHandle;
    private readonly object _syncLock = new object();
    private int _isValid;

    internal InflaterZlib()
    {
        _finished = false;
        _isDisposed = false;
        InflateInit();
    }

    public int AvailableOutput => (int)_zlibStream.AvailOut;

    public bool Finished() => _finished;

    public int Inflate(byte[] bytes, int offset, int length)
    {
        if (length == 0)
            return 0;
        try
        {
            int bytesRead;
            if (ReadInflateOutput(bytes, offset, length, ZLibNative.FlushCode.NoFlush, out bytesRead) == ZLibNative.ErrorCode.StreamEnd)
                _finished = true;
            return bytesRead;
        }
        finally
        {
            if (_zlibStream.AvailIn == 0U && _inputBufferHandle.IsAllocated)
                DeallocateInputBufferHandle();
        }
    }

    public bool NeedsInput() => _zlibStream.AvailIn == 0U;

    public void SetInput(byte[] inputBuffer, int startIndex, int count)
    {
        if (count == 0)
            return;
        lock (_syncLock)
        {
            _inputBufferHandle = GCHandle.Alloc((object)inputBuffer, GCHandleType.Pinned);
            _isValid = 1;
            _zlibStream.NextIn = _inputBufferHandle.AddrOfPinnedObject() + startIndex;
            _zlibStream.AvailIn = (uint)count;
            _finished = false;
        }
    }

    [SecuritySafeCritical]
    protected virtual void Dispose(bool disposing)
    {
        if (_isDisposed)
            return;
        if (disposing)
            _zlibStream.Dispose();
        if (_inputBufferHandle.IsAllocated)
            DeallocateInputBufferHandle();
        _isDisposed = true;
    }

    public void Dispose()
    {
        Dispose(true);
        GC.SuppressFinalize((object)this);
    }

    ~InflaterZlib()
    {
        if (Environment.HasShutdownStarted)
            return;
        Dispose(false);
    }

    [SecuritySafeCritical]
    private void InflateInit()
    {
        ZLibNative.ErrorCode streamForInflate;
        try
        {
            streamForInflate = ZLibNative.CreateZLibStreamForInflate(out _zlibStream, -15);
        }
        catch (Exception ex)
        {
            throw new ZLibException(("ZLibErrorDLLLoadError"), ex);
        }
        switch (streamForInflate)
        {
            case ZLibNative.ErrorCode.VersionError:
                throw new ZLibException(("ZLibErrorVersionMismatch"), "inflateInit2_", (int)streamForInflate, _zlibStream.GetErrorMessage());
            case ZLibNative.ErrorCode.MemError:
                throw new ZLibException(("ZLibErrorNotEnoughMemory"), "inflateInit2_", (int)streamForInflate, _zlibStream.GetErrorMessage());
            case ZLibNative.ErrorCode.StreamError:
                throw new ZLibException(("ZLibErrorIncorrectInitParameters"), "inflateInit2_", (int)streamForInflate, _zlibStream.GetErrorMessage());
            case ZLibNative.ErrorCode.Ok:
                break;
            default:
                throw new ZLibException(("ZLibErrorUnexpected"), "inflateInit2_", (int)streamForInflate, _zlibStream.GetErrorMessage());
        }
    }

    private unsafe ZLibNative.ErrorCode ReadInflateOutput(
      byte[] outputBuffer,
      int offset,
      int length,
      ZLibNative.FlushCode flushCode,
      out int bytesRead)
    {
        lock (_syncLock)
        {
            fixed (byte* numPtr = outputBuffer)
            {
                _zlibStream.NextOut = (IntPtr)(void*)numPtr + offset;
                _zlibStream.AvailOut = (uint)length;
                ZLibNative.ErrorCode errorCode = Inflate(flushCode);
                bytesRead = length - (int)_zlibStream.AvailOut;
                return errorCode;
            }
        }
    }

    [SecuritySafeCritical]
    private ZLibNative.ErrorCode Inflate(ZLibNative.FlushCode flushCode)
    {
        ZLibNative.ErrorCode zlibErrorCode;
        try
        {
            zlibErrorCode = _zlibStream.Inflate(flushCode);
        }
        catch (Exception ex)
        {
            throw new ZLibException(("ZLibErrorDLLLoadError"), ex);
        }
        switch (zlibErrorCode)
        {
            case ZLibNative.ErrorCode.BufError:
                return zlibErrorCode;
            case ZLibNative.ErrorCode.MemError:
                throw new ZLibException(("ZLibErrorNotEnoughMemory"), "inflate_", (int)zlibErrorCode, _zlibStream.GetErrorMessage());
            case ZLibNative.ErrorCode.DataError:
                throw new InvalidDataException(("GenericInvalidData"));
            case ZLibNative.ErrorCode.StreamError:
                throw new ZLibException(("ZLibErrorInconsistentStream"), "inflate_", (int)zlibErrorCode, _zlibStream.GetErrorMessage());
            case ZLibNative.ErrorCode.Ok:
            case ZLibNative.ErrorCode.StreamEnd:
                return zlibErrorCode;
            default:
                throw new ZLibException(("ZLibErrorUnexpected"), "inflate_", (int)zlibErrorCode, _zlibStream.GetErrorMessage());
        }
    }

    private void DeallocateInputBufferHandle()
    {
        lock (_syncLock)
        {
            _zlibStream.AvailIn = 0U;
            _zlibStream.NextIn = ZLibNative.ZNullPtr;
            if (Interlocked.Exchange(ref _isValid, 0) == 0)
                return;
            _inputBufferHandle.Free();
        }
    }
}