using ChihuahuaOS.CoreLib;
using ChihuahuaOS.Kernel.MemoryManager.PMM;

namespace ChihuahuaOS.Kernel.MemoryManager.InternalAvlTree;

internal static unsafe class MemAvlChunkManager
{
    /// <summary>
    /// The maximum number of nodes in a chunk (sizeof(AvlTreeNode) = 40, AVL chunk = 32 KiB, from which 30 KiB are
    /// used by nodes, 2 KiB for bookkeeping, so 30 KiB / 40 B = 768).
    /// </summary>
    public const uint MAX_NODES_IN_AVL_CHUNK = 764;

    internal static ulong AllocateNewAvlChunk()
    {
        long address = MainMemManager.Pmm.Allocate(ChunkLevel1.MIN_CHUNK_SIZE);
        if (address <= 0)
        {
            CoreLibManager.Panic((byte*)"Could not allocate new AVL chunk\0"u8);
            return 0;
        }

        return (ulong)address;
    }

    internal static void DeallocateAvlChunk(ulong physicalAddress)
    {
        MainMemManager.Pmm.Deallocate(ChunkLevel1.MIN_CHUNK_SIZE, (long)physicalAddress);
    }

    internal static void InitializeNewAvlChunk(MemAvlTreeNode* avlNode)
    {
        short* array = GetAvlChunkFreeSlotsArray(avlNode);

        //write the stack pointer (only the last position is free, the rest is occupied with offsets
        *(array - 1) = (short)(MAX_NODES_IN_AVL_CHUNK - 2);

        //all nodes are initially free, except the first, as it's occupied by the root node
        array[0] = 1;
        for (int i = 1; i < MAX_NODES_IN_AVL_CHUNK - 1; i++)
        {
            array[i] = (short)(MAX_NODES_IN_AVL_CHUNK - 1 - i);
        }

        array[MAX_NODES_IN_AVL_CHUNK - 1] = -1;
    }

    internal static short* GetAvlChunkFreeSlotsArray(MemAvlTreeNode* avlObject)
    {
        ulong avlChunkAddr = (ulong)avlObject / ChunkLevel1.MIN_CHUNK_SIZE * ChunkLevel1.MIN_CHUNK_SIZE;
        return (short*)(avlChunkAddr + ChunkLevel1.MIN_CHUNK_SIZE - sizeof(short) * MAX_NODES_IN_AVL_CHUNK);
    }

    internal static MemAvlTreeNode* GetAvlChunkArrayStart(MemAvlTreeNode* avlObject)
    {
        return (MemAvlTreeNode*)((ulong)avlObject / ChunkLevel1.MIN_CHUNK_SIZE * ChunkLevel1.MIN_CHUNK_SIZE);
    }

    internal static bool IsAvlChunkFree(MemAvlTreeNode* avlNode)
    {
        short* array = GetAvlChunkFreeSlotsArray(avlNode);
        short stackPointer = *(array - 1);
        return stackPointer == MAX_NODES_IN_AVL_CHUNK - 1;
    }

    internal static bool IsAvlChunkFull(MemAvlTreeNode* avlNode)
    {
        short* array = GetAvlChunkFreeSlotsArray(avlNode);
        short stackPointer = *(array - 1);
        return stackPointer == 0;
    }

    /// <summary>
    /// 
    /// </summary>
    /// <param name="avlNode">Any node that's in the AVL chunk that you are working on.</param>
    /// <param name="offset"></param>
    internal static void PushFreeSlot(MemAvlTreeNode* avlNode, short offset)
    {
        short* array = GetAvlChunkFreeSlotsArray(avlNode);
        short stackPointer = *(array - 1);

        array[stackPointer + 1] = offset;
        if (stackPointer >= MAX_NODES_IN_AVL_CHUNK - 1)
        {
            //should be unreachable
            return;
        }

        *(array - 1) = (short)(stackPointer + 1);
    }

    /// <summary>
    /// 
    /// </summary>
    /// <param name="avlNode">Any node that's in the AVL chunk that you are working on.</param>
    /// <returns>-1 if it fails, otherwise the offset in the AVL chunk of a free slot.</returns>
    internal static short PopFreeSlot(MemAvlTreeNode* avlNode)
    {
        short* array = GetAvlChunkFreeSlotsArray(avlNode);
        short stackPointer = *(array - 1);
        if (stackPointer <= 0)
        {
            //should be unreachable
            return -1;
        }

        short offset = array[stackPointer];
        *(array - 1) = (short)(stackPointer - 1);
        return offset;
    }
}
