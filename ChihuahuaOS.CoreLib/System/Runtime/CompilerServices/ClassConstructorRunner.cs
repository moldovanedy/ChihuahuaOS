//we don't know whether this is required or not


// using System.Runtime.InteropServices;
//
// namespace System.Runtime.CompilerServices;
//
// // A class responsible for running static constructors. The compiler will call into this
// // code to ensure static constructors run and that they only run once.
// // internal static partial class ClassConstructorRunner
// // {
// //     private static IntPtr CheckStaticClassConstructionReturnNonGCStaticBase(ref StaticClassConstructionContext context,
// //         IntPtr nonGcStaticBase)
// //     {
// //         CheckStaticClassConstruction(ref context);
// //         return nonGcStaticBase;
// //     }
// //
// //     private static unsafe void CheckStaticClassConstruction(ref StaticClassConstructionContext context)
// //     {
// //         // Not dealing with multithreading issues.
// //         if (context.cctorMethodAddress != 0)
// //         {
// //             IntPtr address = context.cctorMethodAddress;
// //             context.cctorMethodAddress = 0;
// //             ((delegate*<void>)address)();
// //         }
// //     }
// // }
//
// // This data structure is a contract with the compiler. It holds the address of a static
// // constructor and a flag that specifies whether the constructor already executed.
// // [StructLayout(LayoutKind.Sequential)]
// // public struct StaticClassConstructionContext
// // {
// //     public IntPtr cctorMethodAddress;
// // }

