#if UEFI
using System.Runtime.CompilerServices;
using ChihuahuaOS.EfiApi;
using ChihuahuaOS.EfiApi.BootServices;

namespace System.Runtime.InteropServices;

public static unsafe class NativeMemory
{
    public static void* Alloc(nuint byteCount)
    {
        if (Environment.EfiSysTable == null)
        {
            return null;
        }

        void** result;
        EfiStatus status =
            Environment.EfiSysTable->BootServices->AllocatePool(
                EfiMemoryType.EfiLoaderData,
                byteCount,
                (void**)&result);

        return status != EfiStatus.Success ? null : result;
    }

    public static void* AllocZeroed(nuint byteCount)
    {
        if (Environment.EfiSysTable == null)
        {
            return null;
        }

        void** result;
        EfiStatus status =
            Environment.EfiSysTable->BootServices->AllocatePool(
                EfiMemoryType.EfiLoaderData,
                byteCount,
                (void**)&result);

        Fill(result, byteCount, 0);
        return status != EfiStatus.Success ? null : result;
    }

    public static void Clear(void* ptr, nuint byteCount)
    {
        if (Environment.EfiSysTable == null || ptr == null || byteCount == 0)
        {
            return;
        }

        Fill(ptr, byteCount, 0);
    }

    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    public static void Fill(void* ptr, nuint byteCount, byte value)
    {
        if (ptr == null)
        {
            return;
        }

        byte* traverser = (byte*)ptr;
        for (nuint i = 0; i < byteCount; i++)
        {
            traverser[i] = value;
        }
    }

    public static void Free(void* ptr)
    {
        if (Environment.EfiSysTable == null || ptr == null)
        {
            return;
        }

        Environment.EfiSysTable->BootServices->FreePool(ptr);
    }
}

#endif