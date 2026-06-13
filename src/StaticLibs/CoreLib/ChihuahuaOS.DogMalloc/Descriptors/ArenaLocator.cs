using System.Runtime.CompilerServices;
using System.Runtime.InteropServices;

namespace ChihuahuaOS.DogMalloc.Descriptors;

[StructLayout(LayoutKind.Sequential, Pack = 1)]
internal unsafe struct ArenaLocator
{
    public const int NUM_ARENAS_PER_LOCATOR = 32;

    public ArenaDescriptorsArray ArenaDescriptors;

    public ArenaLocator* Previous { get; internal set; }
    public ArenaLocator* Next { get; internal set; }

    public ArenaDescriptor* FindFreeArena()
    {
        for (int i = 0; i < NUM_ARENAS_PER_LOCATOR; i++)
        {
            ulong descriptorAddress = ArenaDescriptors[i];
            if (descriptorAddress == 0)
            {
                continue;
            }

            ArenaDescriptor* descriptorPtr = (ArenaDescriptor*)descriptorAddress;
            if ((descriptorPtr->Flags & ArenaDescriptorFlags.Locked) != ArenaDescriptorFlags.None)
            {
                continue;
            }

            return descriptorPtr;
        }

        return null;
    }

    public ArenaDescriptor* FindArenaContainingAddress(ulong address)
    {
        _ = ArenaDescriptors[0];
        //TODO: use arena's ContainsAddress
        return null;
    }


    [InlineArray(NUM_ARENAS_PER_LOCATOR)]
    internal struct ArenaDescriptorsArray
    {
        private ulong _descriptor;
    }
}
