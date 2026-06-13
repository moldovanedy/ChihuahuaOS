using System;

namespace ChihuahuaOS.DogMalloc.Descriptors;

[Flags]
public enum ArenaDescriptorFlags : ulong
{
    None = 0,
    Locked = 1
}
