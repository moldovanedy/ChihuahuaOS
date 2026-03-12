using System;

namespace ChihuahuaOS.EfiApi.RuntimeServices;

[Flags]
public enum EfiVariableAttributes
{
    NonVolatile = 0x01,
    BootServiceAccess = 0x02,
    RuntimeAccess = 0x04,
    HardwareErrorRecord = 0x08,
    AuthenticatedWriteAccess = 0x10, //deprecated
    TimeBasedAuthenticatedWriteAccess = 0x20,
    AppendWrite = 0x40,
    EnhancedAuthenticatedAccess = 0x80
}