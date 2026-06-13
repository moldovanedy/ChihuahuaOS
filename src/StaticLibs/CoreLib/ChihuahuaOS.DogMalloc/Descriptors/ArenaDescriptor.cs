using System.Runtime.InteropServices;
using ChihuahuaOS.CoreLib;

namespace ChihuahuaOS.DogMalloc.Descriptors;

/// <summary>
/// It is MANDATORY for the arena descriptor to be located at the start of the arena virtual space.
/// </summary>
[StructLayout(LayoutKind.Sequential, Pack = 1)]
internal struct ArenaDescriptor
{
    public const ulong DEFAULT_STARTING_SIZE = 256 * 1024;
    public const ulong MIN_STARTING_SIZE = 128 * 1024;

    public ulong TotalSize { get; private set; }
    public ulong RemainingSize { get; private set; }
    public ArenaDescriptorFlags Flags { get; internal set; }

    public BucketDescriptor List16Bytes { get; private set; }
    public BucketDescriptor List24Bytes { get; private set; }
    public BucketDescriptor List32Bytes { get; private set; }
    public BucketDescriptor List40Bytes { get; private set; }
    public BucketDescriptor List48Bytes { get; private set; }
    public BucketDescriptor List56Bytes { get; private set; }
    public BucketDescriptor List64Bytes { get; private set; }

    public BucketDescriptor List80Bytes { get; private set; }
    public BucketDescriptor List96Bytes { get; private set; }
    public BucketDescriptor List112Bytes { get; private set; }
    public BucketDescriptor List128Bytes { get; private set; }

    public BucketDescriptor List160Bytes { get; private set; }
    public BucketDescriptor List192Bytes { get; private set; }
    public BucketDescriptor List224Bytes { get; private set; }
    public BucketDescriptor List256Bytes { get; private set; }

    public BucketDescriptor List320Bytes { get; private set; }
    public BucketDescriptor List384Bytes { get; private set; }
    public BucketDescriptor List448Bytes { get; private set; }
    public BucketDescriptor List512Bytes { get; private set; }

    public ArenaDescriptor()
    {
    }

    public unsafe ArenaDescriptor(ulong initialSize)
    {
        if (initialSize < MIN_STARTING_SIZE)
        {
            CoreLibManager.PrimitiveDebug(
                (byte*)
                "Malloc: Arena starting size was smaller than the minimum required size. Arena not initialized."u8);
            return;
        }

        TotalSize = initialSize;
    }

    internal void Initialize()
    {
        InitializeBuckets();
    }

    public ulong Allocate(ulong size)
    {
        switch (size)
        {
            case > 512:
                //TODO: allocate from large object heap
                return 0;
            case <= 16:
                return List16Bytes.Allocate();
            case <= 24:
                return List24Bytes.Allocate();
            case <= 32:
                return List32Bytes.Allocate();
            case <= 40:
                return List40Bytes.Allocate();
            case <= 48:
                return List48Bytes.Allocate();
            case <= 56:
                return List56Bytes.Allocate();
            case <= 64:
                return List64Bytes.Allocate();
            case <= 80:
                return List80Bytes.Allocate();
            case <= 96:
                return List96Bytes.Allocate();
            case <= 112:
                return List112Bytes.Allocate();
            case <= 128:
                return List128Bytes.Allocate();
            case <= 160:
                return List160Bytes.Allocate();
            case <= 192:
                return List192Bytes.Allocate();
            case <= 224:
                return List224Bytes.Allocate();
            case <= 256:
                return List256Bytes.Allocate();
            case <= 320:
                return List320Bytes.Allocate();
            case <= 384:
                return List384Bytes.Allocate();
            case <= 448:
                return List448Bytes.Allocate();
            default:
                return List512Bytes.Allocate();
        }
    }

    public void Free(ulong address)
    {
        //TODO: track all slabs in the large object heap so we can get the corresponding slab from the address
        // in log time

        //SlabDescriptor* slabPtr;
        //slabPtr->Free(address);
    }


    internal bool ContainsAddress(ulong address)
    {
        return false;
    }

    internal ulong AllocateSlab(int slabSize)
    {
        //NOTE: zeroing out memory is NOT necessary
        if (RemainingSize < (ulong)slabSize)
        {
            //TODO: request more memory
            return 0;
        }

        //TODO: allocate this from the large object heap
        return 0;
    }


    private unsafe void InitializeBuckets()
    {
        ulong address;
        ulong startAddress;

        fixed (ArenaDescriptor* descriptorPtr = &this)
        {
            startAddress = (ulong)descriptorPtr;
            address = (ulong)(descriptorPtr + 1);

            //16*128+128 + 24*192+192 + 32*256+256 + 40*192+192 + 48*128+128 + 56*64+64 + 64*64+64 + 80*32+32 + 96*32+32 + 112*16+16 + 128*16+16 + 160*16+16 + 192*16+16 + 224*8+8 + 256*8+8 + 320*8+8 + 384*8+8 + 448*4+4 + 512*2+2
            // + 8-byte alignment and a sizeof(SlabDescriptor) for each
            // = ~65-66 KiB

            List16Bytes = new BucketDescriptor(ref address, 128, 16, descriptorPtr);
            List24Bytes = new BucketDescriptor(ref address, 192, 24, descriptorPtr);
            List32Bytes = new BucketDescriptor(ref address, 256, 32, descriptorPtr);
            List40Bytes = new BucketDescriptor(ref address, 192, 40, descriptorPtr);
            List48Bytes = new BucketDescriptor(ref address, 128, 48, descriptorPtr);
            List56Bytes = new BucketDescriptor(ref address, 64, 56, descriptorPtr);
            List64Bytes = new BucketDescriptor(ref address, 64, 64, descriptorPtr);

            List80Bytes = new BucketDescriptor(ref address, 32, 80, descriptorPtr);
            List96Bytes = new BucketDescriptor(ref address, 32, 96, descriptorPtr);
            List112Bytes = new BucketDescriptor(ref address, 16, 112, descriptorPtr);
            List128Bytes = new BucketDescriptor(ref address, 16, 128, descriptorPtr);

            List160Bytes = new BucketDescriptor(ref address, 16, 160, descriptorPtr);
            List192Bytes = new BucketDescriptor(ref address, 16, 192, descriptorPtr);
            List224Bytes = new BucketDescriptor(ref address, 8, 224, descriptorPtr);
            List256Bytes = new BucketDescriptor(ref address, 8, 256, descriptorPtr);

            List320Bytes = new BucketDescriptor(ref address, 8, 320, descriptorPtr);
            List384Bytes = new BucketDescriptor(ref address, 8, 384, descriptorPtr);
            List448Bytes = new BucketDescriptor(ref address, 4, 448, descriptorPtr);
            List512Bytes = new BucketDescriptor(ref address, 2, 512, descriptorPtr);
        }

        RemainingSize = TotalSize - (address - startAddress);
    }
}
