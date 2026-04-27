using System;

namespace ChihuahuaOS.CoreLib.Extra.Runtime;

public static class MemUtils
{
    /// <summary>
    /// This is an extension only found in this library. It deallocates the memory of this object, as we don't have
    /// a GC, so it is mandatory to call this to avoid leaks. Use <see cref="IDisposable.Dispose"/> as much as
    /// possible instead of this (that method itself will use this method).
    /// </summary>
    public static unsafe void FreeMemory(object? obj)
    {
        if (obj == null)
        {
            return;
        }

        //this is OK, since we don't have a managed runtime and GC, so the "managed" pointer is simply the raw address
#pragma warning disable CS8500 // This declares a pointer to a managed type
        CoreLibManager.Free(*(void**)&obj);
#pragma warning restore CS8500 // This declares a pointer to a managed type
    }
}