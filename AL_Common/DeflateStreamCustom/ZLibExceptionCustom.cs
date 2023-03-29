#if NETFRAMEWORK

using System;
using System.IO;
using System.Runtime.Serialization;
using System.Security.Permissions;

namespace AL_Common.DeflateStreamCustom;

[Serializable]
internal sealed class ZLibException : IOException, ISerializable
{
    private string _zlibErrorContext;
    private string _zlibErrorMessage;
    private ZLibNativeCustom.ErrorCode _zlibErrorCode;

    public ZLibException(
      string message,
      string zlibErrorContext,
      int zlibErrorCode,
      string zlibErrorMessage)
      : base(message)
    {
        this.Init(zlibErrorContext, (ZLibNativeCustom.ErrorCode)zlibErrorCode, zlibErrorMessage);
    }

    public ZLibException() => this.Init();

    public ZLibException(string message)
      : base(message)
    {
        this.Init();
    }

    public ZLibException(string message, Exception inner)
      : base(message, inner)
    {
        this.Init();
    }

    [SecurityPermission(SecurityAction.LinkDemand, SerializationFormatter = true)]
    protected ZLibException(SerializationInfo info, StreamingContext context)
      : base(info, context)
    {
        this.Init(info.GetString(nameof(_zlibErrorContext)), (ZLibNativeCustom.ErrorCode)info.GetInt32(nameof(_zlibErrorCode)), info.GetString(nameof(_zlibErrorMessage)));
    }

    [SecurityPermission(SecurityAction.LinkDemand, SerializationFormatter = true)]
    void ISerializable.GetObjectData(SerializationInfo si, StreamingContext context)
    {
        this.GetObjectData(si, context);
        si.AddValue("zlibErrorContext", (object)this._zlibErrorContext);
        si.AddValue("zlibErrorCode", (int)this._zlibErrorCode);
        si.AddValue("zlibErrorMessage", (object)this._zlibErrorMessage);
    }

    private void Init() => this.Init("", ZLibNativeCustom.ErrorCode.Ok, "");

    private void Init(
      string zlibErrorContext,
      ZLibNativeCustom.ErrorCode zlibErrorCode,
      string zlibErrorMessage)
    {
        this._zlibErrorContext = zlibErrorContext;
        this._zlibErrorCode = zlibErrorCode;
        this._zlibErrorMessage = zlibErrorMessage;
    }

    public string ZLibContext
    {
        [PermissionSet(SecurityAction.LinkDemand, Unrestricted = true)]
        get => this._zlibErrorContext;
    }

    public int ZLibErrorCode
    {
        [PermissionSet(SecurityAction.LinkDemand, Unrestricted = true)]
        get => (int)this._zlibErrorCode;
    }

    public string ZLibErrorMessage
    {
        [PermissionSet(SecurityAction.LinkDemand, Unrestricted = true)]
        get => this._zlibErrorMessage;
    }
}
#endif
