using System.Runtime.CompilerServices;
using Internal.Runtime;
using Internal.Runtime.CompilerHelpers;

#pragma warning disable CS8500 // This takes the address of, gets the size of, or declares a pointer to a managed type

namespace System.Runtime;

internal static class RuntimeExports
{
    public static unsafe object RhBox(MethodTable* pEeType, ref byte data)
    {
        //TODO: this is most likely not a complete implementation
        void* rawResultData = StartupCodeHelpers.RhpNewFast(pEeType);
        object result = *(object*)rawResultData;
        byte startByte = *(byte*)rawResultData;
        Unsafe.CopyBlock(ref startByte, ref data, pEeType->UsComponentSize);
        return result;
    }
}