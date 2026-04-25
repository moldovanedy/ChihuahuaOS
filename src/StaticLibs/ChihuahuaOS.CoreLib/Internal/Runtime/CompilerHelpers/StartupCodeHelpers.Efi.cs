#if UEFI || DEBUG

using System;
using System.Runtime;
using ChihuahuaOS.EfiApi;
using ChihuahuaOS.EfiApi.BootServices;

namespace Internal.Runtime.CompilerHelpers;

internal unsafe partial class StartupCodeHelpers
{
    private static MethodTable** AllocObject(uint size)
    {
        MethodTable** result;

        if (Environment.EfiSysTable == null)
        {
            ThrowHelpers.ThrowNullReferenceException();
            return null;
        }

        EfiStatus status =
            Environment.EfiSysTable->BootServices->AllocatePool(EfiMemoryType.EfiLoaderData, size, (void**)&result);
        if (status != EfiStatus.Success)
        {
            result = null;
        }

        if (result == null)
        {
            Environment.FailFast("Allocation failed");
        }

        //zero out memory
        SpanHelpers.Fill(ref *(byte*)result, 0, size);
        return result;
    }
}

#endif