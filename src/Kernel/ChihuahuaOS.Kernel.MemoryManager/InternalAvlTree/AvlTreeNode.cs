using System;
using System.Runtime.CompilerServices;
using System.Runtime.InteropServices;
using ChihuahuaOS.CoreLib;
using ChihuahuaOS.Kernel.MemoryManager.PMM;

namespace ChihuahuaOS.Kernel.MemoryManager.InternalAvlTree;

[StructLayout(LayoutKind.Sequential, Pack = 1)]
public unsafe struct AvlTreeNode
{
    public ulong VirtualStart { get; set; }

    /// <summary>
    /// The physical start of the region. If the region cannot be freed, this should be ignored as it might be 0.
    /// </summary>
    public ulong PhysicalStart { get; set; }

    /// <summary>
    /// The lowest byte stores the node height and balance factor as such: the lowest 6 bits hold the height (&lt; 64),
    /// the 6th bit is the balance value (0 ro 1), the 7th bit holds the balance sign (0 for +, 1 for -).
    /// The second byte has a single relevant bit: if bit 0 is set, it means the region cannot be freed (it is used
    /// by UEFI or by the kernel).
    /// </summary>
    /// <remarks>
    /// According to Wikipedia (https://en.wikipedia.org/wiki/AVL_tree#Properties, the formula for the max height
    /// of an AVL tree given n nodes), the tree can only exceed the height of 63 if it has over ~17 trillion nodes,
    /// so no risk for that ever.
    /// </remarks>
    public uint Info { get; set; }

    public uint Size { get; set; }

    public AvlTreeNode* Left { get; internal set; }
    public AvlTreeNode* Right { get; internal set; }


    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    public int GetBalanceFactor()
    {
        byte data = (byte)(Info & 0xFF);
        //positive
        if ((data & 0b1000_0000) == 0)
        {
            return (data & 0b0100_0000) == 0 ? 0 : 1;
        }

        //negative
        return (data & 0b0100_0000) == 0 ? 0 : -1;
    }

    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    internal void SetBalanceFactor(int factor)
    {
        uint data;
        switch (factor)
        {
            case 0:
                data = 0;
                break;
            case < 0:
                data = 0b11;
                break;
            default:
                data = 0b01;
                break;
        }

        data <<= 6;

        //reset the bits, then set them again
        Info &= ~0b1100_0000u;
        Info |= data;
    }

    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    public int GetSubtreeHeight()
    {
        return (int)(Info & 0b0011_1111);
    }

    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    internal void SetSubtreeHeight(int height)
    {
        //reset the bits, then set them again
        Info &= ~0b0011_1111u;
        Info |= (uint)(height & 0b0011_1111);
    }

    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    public bool IsFreeable()
    {
        return (Info & 0x100) == 0;
    }


    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    internal static int ComputeBalanceFactor(AvlTreeNode* node)
    {
        int rightSubHeight = node->Right == null ? 0 : node->Right->GetSubtreeHeight();
        int leftSubHeight = node->Left == null ? 0 : node->Left->GetSubtreeHeight();
        return rightSubHeight - leftSubHeight;
    }

    /// <returns>
    /// Returns the new descendant of the parent node.
    /// </returns>
    internal static AvlTreeNode* Delete(
        AvlTreeNode* parentNode,
        ulong virtualAddress,
        Span<ulong> ancestors,
        ref int index)
    {
        if (virtualAddress == parentNode->VirtualStart)
        {
            //leaf node
            if (parentNode->Left == null && parentNode->Right == null)
            {
                FreeNode(parentNode);
                return null;
            }

            //one child
            if (parentNode->Left == null || parentNode->Right == null)
            {
                AvlTreeNode* replacingChild = parentNode->Left == null ? parentNode->Right : parentNode->Left;
                FreeNode(parentNode);
                return replacingChild;
            }

            //two children
            AvlTreeNode* replacingNode = parentNode->Left;
            if (replacingNode == null)
            {
                CoreLibManager.Panic((byte*)"AVL: assertion failed: replacingNode (Left subtree) was null\0"u8);
                return null;
            }

            while (replacingNode->Right != null)
            {
                replacingNode = replacingNode->Right;
            }

            AvlTreeNode* minLeftSubtree = replacingNode;

            //move data
            //NOTE: Info does not need moving (at least not the part with the balance factors and subtree height)
            parentNode->VirtualStart = minLeftSubtree->VirtualStart;
            parentNode->Size = minLeftSubtree->Size;
            parentNode->PhysicalStart = minLeftSubtree->PhysicalStart;


            ancestors[index] = (ulong)parentNode->Left;
            index++;
            parentNode->Left = Delete(parentNode->Left, minLeftSubtree->VirtualStart, ancestors, ref index);
        }
        //keep searching
        else
        {
            if (virtualAddress < parentNode->VirtualStart)
            {
                ancestors[index] = (ulong)parentNode->Left;
                index++;
                parentNode->Left = Delete(parentNode->Left, virtualAddress, ancestors, ref index);
            }
            else
            {
                ancestors[index] = (ulong)parentNode->Right;
                index++;
                parentNode->Right = Delete(parentNode->Right, virtualAddress, ancestors, ref index);
            }
        }

        //if we reach here, we don't free this node, we only move data, so we can safely return the same pointer
        return parentNode;
    }


    internal bool TryInsert(AvlTreeNode* newNode, Span<ulong> ancestors, ref int index)
    {
        if (newNode->VirtualStart == VirtualStart)
        {
            return false;
        }

        // ReSharper disable RedundantIfElseBlock
        if (newNode->VirtualStart < VirtualStart)
        {
            if (Left == null)
            {
                if (newNode->VirtualStart + newNode->Size > VirtualStart)
                {
                    return false;
                }

                Left = newNode;
                return true;
            }
            else
            {
                ancestors[index] = (ulong)Left;
                index++;
                return Left->TryInsert(newNode, ancestors, ref index);
            }
        }
        else
        {
            if (Right == null)
            {
                if (VirtualStart + Size > newNode->VirtualStart)
                {
                    return false;
                }

                Right = newNode;
                return true;
            }
            else
            {
                ancestors[index] = (ulong)Right;
                index++;
                return Right->TryInsert(newNode, ancestors, ref index);
            }
        }
        // ReSharper restore RedundantIfElseBlock
    }

    internal AvlTreeNode* Search(ulong virtualAddress, Span<ulong> ancestors, ref int index)
    {
        fixed (AvlTreeNode* thisPtr = &this)
        {
            if (virtualAddress == VirtualStart)
            {
                return thisPtr;
            }

            if (virtualAddress < VirtualStart)
            {
                if (Left == null)
                {
                    return null;
                }

                ancestors[index] = (ulong)Left;
                index++;
                return Left->Search(virtualAddress, ancestors, ref index);
            }

            if (Right == null)
            {
                return null;
            }

            ancestors[index] = (ulong)Right;
            index++;
            return Right->Search(virtualAddress, ancestors, ref index);
        }
    }

    private static void FreeNode(AvlTreeNode* node)
    {
        AvlTreeNode* avlChunkStart =
            (AvlTreeNode*)((ulong)node / ChunkLevel1.MIN_CHUNK_SIZE * ChunkLevel1.MIN_CHUNK_SIZE);
        int offset = (int)(node - avlChunkStart) / sizeof(AvlTreeNode);
        AvlChunkManager.PushFreeSlot(avlChunkStart, (short)offset);
    }
}
