using System;
using System.IO;
using System.Runtime.InteropServices;
using System.Security;
using System.Threading;

namespace AL_Common.DeflateStreamCustom;

internal sealed class InflaterZlibCustom : IDisposable
{
    private bool _finished;
    private bool _isDisposed;
    private ZLibNativeCustom.ZLibStreamHandle _zlibStream;
    private GCHandle _inputBufferHandle;
    private readonly object _syncLock = new();
    private int _isValid;

    internal InflaterZlibCustom()
    {
        _finished = false;
        _isDisposed = false;
        InflateInit();
    }

    public bool Finished() => _finished;

    public int Inflate(byte[] bytes, int offset, int length)
    {
        if (length == 0)
        {
            return 0;
        }
        try
        {
            if (ReadInflateOutput(bytes, offset, length, ZLibNativeCustom.FlushCode.NoFlush, out int bytesRead) == ZLibNativeCustom.ErrorCode.StreamEnd)
            {
                _finished = true;
            }
            return bytesRead;
        }
        finally
        {
            if (_zlibStream.AvailIn == 0U && _inputBufferHandle.IsAllocated)
            {
                DeallocateInputBufferHandle();
            }
        }
    }

    public void SetInput(byte[] inputBuffer, int startIndex, int count)
    {
        if (count == 0)
        {
            return;
        }
        lock (_syncLock)
        {
            _inputBufferHandle = GCHandle.Alloc(inputBuffer, GCHandleType.Pinned);
            _isValid = 1;
            _zlibStream.NextIn = _inputBufferHandle.AddrOfPinnedObject() + startIndex;
            _zlibStream.AvailIn = (uint)count;
            _finished = false;
        }
    }

    [SecuritySafeCritical]
    private void Dispose(bool disposing)
    {
        if (_isDisposed)
        {
            return;
        }
        if (disposing)
        {
            _zlibStream.Dispose();
        }
        if (_inputBufferHandle.IsAllocated)
        {
            DeallocateInputBufferHandle();
        }
        _isDisposed = true;
    }

    public void Dispose()
    {
        Dispose(true);
        GC.SuppressFinalize(this);
    }

    ~InflaterZlibCustom()
    {
        if (Environment.HasShutdownStarted)
        {
            return;
        }
        Dispose(false);
    }

    [SecuritySafeCritical]
    private void InflateInit()
    {
        ZLibNativeCustom.ErrorCode streamForInflate;
        try
        {
            streamForInflate = ZLibNativeCustom.CreateZLibStreamForInflate(out _zlibStream, -15);
        }
        catch (Exception ex)
        {
            throw new ZLibException("ZLibErrorDLLLoadError", ex);
        }
        switch (streamForInflate)
        {
            case ZLibNativeCustom.ErrorCode.VersionError:
                throw new ZLibException("ZLibErrorVersionMismatch", "inflateInit2_", (int)streamForInflate, _zlibStream.GetErrorMessage());
            case ZLibNativeCustom.ErrorCode.MemError:
                throw new ZLibException("ZLibErrorNotEnoughMemory", "inflateInit2_", (int)streamForInflate, _zlibStream.GetErrorMessage());
            case ZLibNativeCustom.ErrorCode.StreamError:
                throw new ZLibException("ZLibErrorIncorrectInitParameters", "inflateInit2_", (int)streamForInflate, _zlibStream.GetErrorMessage());
            case ZLibNativeCustom.ErrorCode.Ok:
                break;
            default:
                throw new ZLibException("ZLibErrorUnexpected", "inflateInit2_", (int)streamForInflate, _zlibStream.GetErrorMessage());
        }
    }

    private unsafe ZLibNativeCustom.ErrorCode ReadInflateOutput(
      byte[] outputBuffer,
      int offset,
      int length,
      ZLibNativeCustom.FlushCode flushCode,
      out int bytesRead)
    {
        lock (_syncLock)
        {
            fixed (byte* numPtr = outputBuffer)
            {
                _zlibStream.NextOut = (IntPtr)numPtr + offset;
                _zlibStream.AvailOut = (uint)length;
                ZLibNativeCustom.ErrorCode errorCode = Inflate(flushCode);
                bytesRead = length - (int)_zlibStream.AvailOut;
                return errorCode;
            }
        }
    }

    [SecuritySafeCritical]
    private ZLibNativeCustom.ErrorCode Inflate(ZLibNativeCustom.FlushCode flushCode)
    {
        ZLibNativeCustom.ErrorCode zlibErrorCode;
        try
        {
            zlibErrorCode = _zlibStream.Inflate(flushCode);
        }
        catch (Exception ex)
        {
            throw new ZLibException("ZLibErrorDLLLoadError", ex);
        }
        switch (zlibErrorCode)
        {
            case ZLibNativeCustom.ErrorCode.BufError:
                return zlibErrorCode;
            case ZLibNativeCustom.ErrorCode.MemError:
                throw new ZLibException("ZLibErrorNotEnoughMemory", "inflate_", (int)zlibErrorCode, _zlibStream.GetErrorMessage());
            case ZLibNativeCustom.ErrorCode.DataError:
                throw new InvalidDataException("GenericInvalidData");
            case ZLibNativeCustom.ErrorCode.StreamError:
                throw new ZLibException("ZLibErrorInconsistentStream", "inflate_", (int)zlibErrorCode, _zlibStream.GetErrorMessage());
            case ZLibNativeCustom.ErrorCode.Ok:
            case ZLibNativeCustom.ErrorCode.StreamEnd:
                return zlibErrorCode;
            default:
                throw new ZLibException("ZLibErrorUnexpected", "inflate_", (int)zlibErrorCode, _zlibStream.GetErrorMessage());
        }
    }

    private void DeallocateInputBufferHandle()
    {
        lock (_syncLock)
        {
            _zlibStream.AvailIn = 0U;
            _zlibStream.NextIn = ZLibNativeCustom.ZNullPtr;
            if (Interlocked.Exchange(ref _isValid, 0) == 0)
            {
                return;
            }
            _inputBufferHandle.Free();
        }
    }
}