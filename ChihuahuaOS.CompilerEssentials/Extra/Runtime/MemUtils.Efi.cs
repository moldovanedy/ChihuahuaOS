#if UEFI || DEBUG

using System;
using ChihuahuaOS.EfiApi.EfiSysTable;

namespace Extra.Runtime;

public static class MemUtils
{
    /// <summary>
    /// This is an extension only found in this library. It deallocates the memory of this object, as we don't have
    /// a GC, so it is mandatory to call this to avoid leaks. Use <seIDisposable.Disposeble.Free"/> as much as
    /// possible instead of this (that method itself will use this method).
    /// </summary>
    public static unsafe void FreeMemory(object? obj)
    {
        if (obj == null)
        {
            return;
        }

        EfiSystemTable* st = Environment.EfiSysTable;
        if (st == null)
        {
            return;
        }

        //TODO: fix this if possible; for now it works, but it's probably bad
#pragma warning disable CS8500 // This declares a pointer to a managed type
        void* objectAddress = *(void**)&obj;
        st->BootServices->FreePool(objectAddress);
#pragma warning restore CS8500 // This declares a pointer to a managed type
    }
}

#endif