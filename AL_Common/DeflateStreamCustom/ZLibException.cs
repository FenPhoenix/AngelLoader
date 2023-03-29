using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Runtime.Serialization;
using System.Security.Permissions;
using System.Text;
using System.Threading.Tasks;

namespace AL_Common.DeflateStreamCustom;

[Serializable]
internal class ZLibException : IOException, ISerializable
{
    private string zlibErrorContext;
    private string zlibErrorMessage;
    private ZLibNative.ErrorCode zlibErrorCode;

    public ZLibException(
      string message,
      string zlibErrorContext,
      int zlibErrorCode,
      string zlibErrorMessage)
      : base(message)
    {
        this.Init(zlibErrorContext, (ZLibNative.ErrorCode)zlibErrorCode, zlibErrorMessage);
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
        this.Init(info.GetString(nameof(zlibErrorContext)), (ZLibNative.ErrorCode)info.GetInt32(nameof(zlibErrorCode)), info.GetString(nameof(zlibErrorMessage)));
    }

    [SecurityPermission(SecurityAction.LinkDemand, SerializationFormatter = true)]
    void ISerializable.GetObjectData(SerializationInfo si, StreamingContext context)
    {
        this.GetObjectData(si, context);
        si.AddValue("zlibErrorContext", (object)this.zlibErrorContext);
        si.AddValue("zlibErrorCode", (int)this.zlibErrorCode);
        si.AddValue("zlibErrorMessage", (object)this.zlibErrorMessage);
    }

    private void Init() => this.Init("", ZLibNative.ErrorCode.Ok, "");

    private void Init(
      string zlibErrorContext,
      ZLibNative.ErrorCode zlibErrorCode,
      string zlibErrorMessage)
    {
        this.zlibErrorContext = zlibErrorContext;
        this.zlibErrorCode = zlibErrorCode;
        this.zlibErrorMessage = zlibErrorMessage;
    }

    public string ZLibContext
    {
        [PermissionSet(SecurityAction.LinkDemand, Unrestricted = true)]
        get => this.zlibErrorContext;
    }

    public int ZLibErrorCode
    {
        [PermissionSet(SecurityAction.LinkDemand, Unrestricted = true)]
        get => (int)this.zlibErrorCode;
    }

    public string ZLibErrorMessage
    {
        [PermissionSet(SecurityAction.LinkDemand, Unrestricted = true)]
        get => this.zlibErrorMessage;
    }
}
