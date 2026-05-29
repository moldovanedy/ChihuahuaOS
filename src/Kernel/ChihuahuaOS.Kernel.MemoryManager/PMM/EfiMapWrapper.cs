using System.Runtime.CompilerServices;
using ChihuahuaOS.EfiApi.BootServices;
using Internal.Runtime.CompilerHelpers;

namespace ChihuahuaOS.Kernel.MemoryManager.PMM;

public unsafe struct EfiMapWrapper
{
    public int ArrayLength { get; private set; }

    private readonly EfiMemoryDescriptor* _descriptors;
    private readonly ulong _elementSize;

    public EfiMapWrapper(EfiMemoryDescriptor* descriptors, int arrayLength, ulong elementSize)
    {
        ArrayLength = arrayLength;
        _descriptors = descriptors;
        _elementSize = elementSize;
    }

    public EfiMemoryDescriptor this[int index]
    {
        get
        {
            if (index < 0 || index >= ArrayLength)
            {
                ThrowHelpers.ThrowIndexOutOfRangeException();
                return default;
            }

            return Unsafe.AddByteOffset(ref _descriptors[0], (nuint)(_elementSize * (ulong)index));
        }
        set
        {
            if (index < 0 || index >= ArrayLength)
            {
                ThrowHelpers.ThrowIndexOutOfRangeException();
            }

            EfiMemoryDescriptor* ptr = (EfiMemoryDescriptor*)((byte*)_descriptors + _elementSize * (ulong)index);
            *ptr = value;
        }
    }


    public void Sort()
    {
        Sort(0, ArrayLength - 1);
    }


    private void Sort(int leftIndex, int rightIndex)
    {
        int i = leftIndex;
        int j = rightIndex;

        EfiMemoryDescriptor pivot = this[leftIndex];
        while (i <= j)
        {
            while (this[i].PhysicalStart < pivot.PhysicalStart)
            {
                i++;
            }

            while (this[j].PhysicalStart > pivot.PhysicalStart)
            {
                j--;
            }

            if (i <= j)
            {
                (this[i], this[j]) = (this[j], this[i]);
                i++;
                j--;
            }
        }

        if (leftIndex < j)
        {
            Sort(leftIndex, j);
        }

        if (i < rightIndex)
        {
            Sort(i, rightIndex);
        }
    }
}
