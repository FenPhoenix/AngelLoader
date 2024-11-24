using System;
using JetBrains.Annotations;

namespace AL_Common.DeviceIoControlLib.Objects.Enums;

[PublicAPI]
[Flags]
public enum IOMethod : uint
{
    Buffered = 0,
    InDirect = 1,
    OutDirect = 2,
    Neither = 3,
}
