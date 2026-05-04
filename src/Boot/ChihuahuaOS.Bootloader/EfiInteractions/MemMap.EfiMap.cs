using System;
using System.Runtime.CompilerServices;
using ChihuahuaOS.EfiApi;
using ChihuahuaOS.EfiApi.BootServices;
using Internal.Runtime.CompilerHelpers;

namespace ChihuahuaOS.Bootloader.EfiInteractions;

public static partial class MemMap
{
    public unsafe struct EfiMap : IDisposable
    {
        public int ArrayLength { get; private set; }

        private EfiMemoryDescriptor* _descriptors;
        private ulong _elementSize;

        public EfiMap(EfiMemoryDescriptor* descriptors, int arrayLength, ulong elementSize)
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
        }

        public void Dispose()
        {
            if (Environment.EfiSysTable == null)
            {
                return;
            }

            ulong numPages =
                (_elementSize * (ulong)ArrayLength + (EfiConstants.EFI_PAGE_SIZE - 1))
                / EfiConstants.EFI_PAGE_SIZE;
            Environment.EfiSysTable->BootServices->FreePages((ulong)_descriptors, numPages);

            _descriptors = null;
            _elementSize = 0;
            ArrayLength = 0;
        }
    }
}