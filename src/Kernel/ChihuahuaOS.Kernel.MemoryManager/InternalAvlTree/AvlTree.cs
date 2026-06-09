using System;
using System.Runtime.InteropServices;
using ChihuahuaOS.CoreLib;
using ChihuahuaOS.Kernel.MemoryManager.PMM;

namespace ChihuahuaOS.Kernel.MemoryManager.InternalAvlTree;

[StructLayout(LayoutKind.Sequential, Pack = 1)]
public unsafe struct AvlTree
{
    public AvlTreeNode* Root { get; private set; }

    public ulong HeapEndPointer { get; set; }

    public ulong StackBottomPointer { get; set; }

    private ulong LastFreeAvlChunkPhysAddress;

    public AvlTree(AvlTreeNode* root)
    {
        Root = root;
        LastFreeAvlChunkPhysAddress = (ulong)root / ChunkLevel1.MIN_CHUNK_SIZE * ChunkLevel1.MIN_CHUNK_SIZE;
        AvlChunkManager.InitializeNewAvlChunk(root);
    }

    public AvlTreeNode* CreateNode(AvlTreeNode* parentHint, bool isFreeable = true)
    {
        short offset;
        if (parentHint != null && !AvlChunkManager.IsAvlChunkFull(parentHint))
        {
            offset = AvlChunkManager.PopFreeSlot(parentHint);
            if (offset < 0)
            {
                CoreLibManager.Panic((byte*)"AVL: chunk not full, but offset negative\0"u8);
                return null;
            }

            AvlTreeNode* nodeArray = AvlChunkManager.GetAvlChunkArrayStart(parentHint);
            return Reinitialize(nodeArray + offset);
        }

        AvlTreeNode* freeChunkArray;
        if (LastFreeAvlChunkPhysAddress > 0 &&
            !AvlChunkManager.IsAvlChunkFull((AvlTreeNode*)LastFreeAvlChunkPhysAddress))
        {
            freeChunkArray = (AvlTreeNode*)LastFreeAvlChunkPhysAddress;
            offset = AvlChunkManager.PopFreeSlot(freeChunkArray);
            if (offset < 0)
            {
                CoreLibManager.Panic((byte*)"AVL: chunk not full, but offset negative\0"u8);
                return null;
            }

            return Reinitialize(freeChunkArray + offset);
        }

        LastFreeAvlChunkPhysAddress = AvlChunkManager.AllocateNewAvlChunk();
        freeChunkArray = (AvlTreeNode*)LastFreeAvlChunkPhysAddress;
        AvlChunkManager.InitializeNewAvlChunk(freeChunkArray);

        offset = AvlChunkManager.PopFreeSlot(freeChunkArray);
        if (offset < 0)
        {
            CoreLibManager.Panic((byte*)"AVL: chunk not full, but offset negative\0"u8);
            return null;
        }

        return Reinitialize(freeChunkArray + offset, isFreeable);


        static AvlTreeNode* Reinitialize(AvlTreeNode* newNode, bool isFreeable = true)
        {
            newNode->VirtualStart = 0;
            newNode->PhysicalStart = 0;
            newNode->Size = 0;
            newNode->Info = isFreeable ? 0U : 0x100;
            newNode->Left = null;
            newNode->Right = null;
            return newNode;
        }
    }


    #region Operations

    // public void Traverse()
    // {
    //     TraverseInorder(Root);
    // }
    //
    // public static void TraverseInorder(AvlTreeNode* currentNode)
    // {
    //     if (currentNode->Left != null)
    //     {
    //         TraverseInorder(currentNode->Left);
    //     }
    //
    //     if (currentNode->Right != null)
    //     {
    //         TraverseInorder(currentNode->Right);
    //     }
    // }

    public bool TryInsert(ulong virtualAddress, ulong physicalAddress, uint size, bool isFreeable = true)
    {
        AvlTreeNode* newNode = CreateNode(null, isFreeable);
        newNode->VirtualStart = virtualAddress;
        newNode->PhysicalStart = physicalAddress;
        newNode->Size = size;

        if (Root == null)
        {
            Root = newNode;
            return true;
        }

        Span<ulong> ancestorPointers = stackalloc ulong[255];
        ancestorPointers[0] = (ulong)Root;
        int index = 1;

        bool success = Root->TryInsert(newNode, ancestorPointers, ref index);
        if (!success)
        {
            return false;
        }

        RetraceInsertion(ancestorPointers, index);
        return true;
    }

    public bool IsAddressFree(ulong address)
    {
        return TryGetFreeAddress(address, address, 0) == address;
    }

    public ulong TryGetFreeAddress(ulong start, ulong end, ulong size)
    {
        AvlTreeNode* current = Root;
        while (start <= end)
        {
            ulong leftLimit = 0;
            ulong rightLimit = ulong.MaxValue;

            bool needsTryingFurther = false;
            while (current != null)
            {
                if (current->VirtualStart + current->Size <= start)
                {
                    leftLimit = current->VirtualStart + current->Size;
                    current = current->Right;
                }
                else if (current->VirtualStart > start + size)
                {
                    rightLimit = current->VirtualStart;
                    current = current->Left;
                }
                else
                {
                    //this means that the start address is occupied, go further
                    start = current->VirtualStart + current->Size;
                    current = Root;
                    needsTryingFurther = true;
                    break;
                }
            }

            if (needsTryingFurther)
            {
                continue;
            }

            if (rightLimit - leftLimit < size)
            {
                //this means that the start address was free, but it wasn't as big as needed, so go further 
                start = rightLimit;
                current = Root;
            }
            else
            {
                //it means there was no current node on the start address, so it is free
                return start;
            }
        }

        return 0;
    }

    public void Delete(ulong virtualAddress)
    {
        Span<ulong> ancestorPointers = stackalloc ulong[255];
        ancestorPointers[0] = (ulong)Root;
        int index = 1;

        Root = AvlTreeNode.Delete(Root, virtualAddress, ancestorPointers, ref index);
        RetraceDeletion(ancestorPointers, index);
    }

    #endregion

    private static AvlTreeNode* RotateLeft(AvlTreeNode* x)
    {
        AvlTreeNode* y = x->Right;
        x->Right = y->Left;
        y->Left = x;

        UpdateSubtreeHeight(x);
        UpdateSubtreeHeight(y);

        return y;
    }

    private static AvlTreeNode* RotateRight(AvlTreeNode* y)
    {
        AvlTreeNode* x = y->Left;
        y->Left = x->Right;
        x->Right = y;

        UpdateSubtreeHeight(y);
        UpdateSubtreeHeight(x);

        return x;
    }

    private void RetraceInsertion(Span<ulong> ancestors, int index)
    {
        index--;

        for (; index >= 0; index--)
        {
            AvlTreeNode* parent = (AvlTreeNode*)ancestors[index];
            int oldHeight = parent->GetSubtreeHeight();

            //recalculate balance factor based on actual heights
            int leftHeight = parent->Left == null ? -1 : parent->Left->GetSubtreeHeight();
            int rightHeight = parent->Right == null ? -1 : parent->Right->GetSubtreeHeight();
            int bf = rightHeight - leftHeight;

            if (bf == 2)
            {
                //right-heavy
                AvlTreeNode* rightChild = parent->Right;
                int rightLeftHeight = rightChild->Left == null ? -1 : rightChild->Left->GetSubtreeHeight();
                int rightRightHeight = rightChild->Right == null ? -1 : rightChild->Right->GetSubtreeHeight();

                if (rightLeftHeight > rightRightHeight)
                {
                    //right-left case
                    parent->Right = RotateRight(rightChild);
                }

                //right-left case or left case
                parent = RotateLeft(parent);
            }
            else if (bf == -2)
            {
                //left-heavy
                AvlTreeNode* leftChild = parent->Left;
                int leftLeftHeight = leftChild->Left == null ? -1 : leftChild->Left->GetSubtreeHeight();
                int leftRightHeight = leftChild->Right == null ? -1 : leftChild->Right->GetSubtreeHeight();

                if (leftRightHeight > leftLeftHeight)
                {
                    // left-right case
                    parent->Left = RotateLeft(leftChild);
                }

                //left-right case or right case
                parent = RotateRight(parent);
            }
            else
            {
                //update height and BF
                UpdateSubtreeHeight(parent);
                parent->SetBalanceFactor(bf);

                //if the height didn't change, we can stop
                if (parent->GetSubtreeHeight() == oldHeight)
                {
                    break;
                }

                continue;
            }

            // After rotation, 'parent' is the new root of this subtree
            // Update the reference in the grandparent or Root
            if (index == 0)
            {
                Root = parent;
            }
            else
            {
                AvlTreeNode* grandParent = (AvlTreeNode*)ancestors[index - 1];
                if (grandParent->Left == (AvlTreeNode*)ancestors[index])
                {
                    grandParent->Left = parent;
                }
                else
                {
                    grandParent->Right = parent;
                }
            }

            // Recalculate balance factors for the new rotated subroots
            UpdateBalanceFactor(parent);
            if (parent->Left != null)
            {
                UpdateBalanceFactor(parent->Left);
            }

            if (parent->Right != null)
            {
                UpdateBalanceFactor(parent->Right);
            }

            // After rebalancing, height of this subtree typically remains the same as before insertion,
            // but let's check if we need to continue
            if (parent->GetSubtreeHeight() == oldHeight)
            {
                break;
            }
        }
    }

    private void RetraceDeletion(Span<ulong> ancestors, int index)
    {
        index--;

        for (; index >= 0; index--)
        {
            AvlTreeNode* parent = (AvlTreeNode*)ancestors[index];
            int oldHeight = parent->GetSubtreeHeight();

            //recalculate balance factor based on actual heights
            int leftHeight = parent->Left == null ? -1 : parent->Left->GetSubtreeHeight();
            int rightHeight = parent->Right == null ? -1 : parent->Right->GetSubtreeHeight();
            int bf = rightHeight - leftHeight;

            if (bf == 2)
            {
                //right-heavy
                AvlTreeNode* rightChild = parent->Right;
                int rightLeftHeight = rightChild->Left == null ? -1 : rightChild->Left->GetSubtreeHeight();
                int rightRightHeight = rightChild->Right == null ? -1 : rightChild->Right->GetSubtreeHeight();

                if (rightLeftHeight > rightRightHeight)
                {
                    //right-left Case
                    parent->Right = RotateRight(rightChild);
                }

                //right-left case or right-balanced
                parent = RotateLeft(parent);
            }
            else if (bf == -2)
            {
                //left-heavy
                AvlTreeNode* leftChild = parent->Left;
                int leftLeftHeight = leftChild->Left == null ? -1 : leftChild->Left->GetSubtreeHeight();
                int leftRightHeight = leftChild->Right == null ? -1 : leftChild->Right->GetSubtreeHeight();

                if (leftRightHeight > leftLeftHeight)
                {
                    //left-right case
                    parent->Left = RotateLeft(leftChild);
                }

                //left-right case or left-balanced
                parent = RotateRight(parent);
            }
            else
            {
                //update height and BF
                UpdateSubtreeHeight(parent);
                parent->SetBalanceFactor(bf);

                // If height didn't change, we can stop
                if (parent->GetSubtreeHeight() == oldHeight)
                {
                    break;
                }

                continue;
            }

            // After rotation, 'parent' is the new root of this subtree
            // Update the reference in the grandparent or Root
            if (index == 0)
            {
                Root = parent;
            }
            else
            {
                AvlTreeNode* grandParent = (AvlTreeNode*)ancestors[index - 1];
                if (grandParent->Left == (AvlTreeNode*)ancestors[index])
                {
                    grandParent->Left = parent;
                }
                else
                {
                    grandParent->Right = parent;
                }

                // Important: ancestors[index] should be updated so grandparent checks work if we continue
                ancestors[index] = (ulong)parent;
            }

            UpdateBalanceFactor(parent);
            if (parent->Left != null)
            {
                UpdateBalanceFactor(parent->Left);
            }

            if (parent->Right != null)
            {
                UpdateBalanceFactor(parent->Right);
            }

            //standard AVL deletion: if the height of the subtree decreases, we must continue up.
            if (parent->GetSubtreeHeight() == oldHeight)
            {
                break;
            }
        }
    }

    private static void UpdateBalanceFactor(AvlTreeNode* node)
    {
        int leftHeight = node->Left == null ? -1 : node->Left->GetSubtreeHeight();
        int rightHeight = node->Right == null ? -1 : node->Right->GetSubtreeHeight();
        node->SetBalanceFactor(rightHeight - leftHeight);
    }

    private static void UpdateSubtreeHeight(AvlTreeNode* node)
    {
        int leftSubHeight = 0;
        if (node->Left != null)
        {
            leftSubHeight = node->Left->GetSubtreeHeight() + 1;
        }

        int rightSubHeight = 0;
        if (node->Right != null)
        {
            rightSubHeight = node->Right->GetSubtreeHeight() + 1;
        }

        int newSubHeight = Math.Max(leftSubHeight, rightSubHeight);
        if (node->GetSubtreeHeight() != newSubHeight)
        {
            node->SetSubtreeHeight(newSubHeight);
        }
    }
}
