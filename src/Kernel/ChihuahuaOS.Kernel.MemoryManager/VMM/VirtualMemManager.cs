using ChihuahuaOS.CoreLib;
using ChihuahuaOS.CoreLib.Extra;
using ChihuahuaOS.EfiApi;
using ChihuahuaOS.Kernel.MemoryManager.InternalAvlTree;
using ChihuahuaOS.Kernel.MemoryManager.PMM;
using ChihuahuaOS.MemPaginator;

namespace ChihuahuaOS.Kernel.MemoryManager.VMM;

public struct VirtualMemManager
{
    private AvlTree _kernelTree;

    public VirtualMemManager(PagingManager kernelPagingManager)
    {
        bool success = SetupKernelAvlTree(kernelPagingManager);
        if (!success)
        {
            unsafe
            {
                CoreLibManager.Panic((byte*)"VMM: Could not set up the kernel's AVL tree!\0"u8);
                return;
            }
        }
    }

    private bool SetupKernelAvlTree(PagingManager kernelPagingManager)
    {
        long avlPhysAddress = MainMemManager.Pmm.Allocate(ChunkLevel1.MIN_CHUNK_SIZE);
        if (avlPhysAddress <= 0)
        {
            return false;
        }

        PageError error;
        for (int i = 0; i < ChunkLevel1.MIN_CHUNK_SIZE / EfiConstants.EFI_PAGE_SIZE; i++)
        {
            error = kernelPagingManager.IdentityMapPage(
                (ulong)(avlPhysAddress + i * EfiConstants.EFI_PAGE_SIZE),
                PageFlags.Present | PageFlags.ReadPermission | PageFlags.WritePermission);
            if (error != PageError.Success)
            {
                return false;
            }
        }

        error = kernelPagingManager.SubmitChanges();
        if (error != PageError.Success)
        {
            return false;
        }

        unsafe
        {
            RawMemory.MemSet((void*)avlPhysAddress, 0, ChunkLevel1.MIN_CHUNK_SIZE);
            _kernelTree = new AvlTree((AvlTreeNode*)avlPhysAddress);
        }

        return true;
    }
}
