namespace System.Runtime.CompilerServices;

public static class RuntimeFeature
{
    // ReSharper disable InconsistentNaming

    /// <summary>
    /// Name of the Portable PDB feature.
    /// </summary>
    public const string PortablePdb = nameof(PortablePdb);

    /// <summary>
    /// Indicates that this version of runtime supports default interface method implementations.
    /// </summary>
    public const string DefaultImplementationsOfInterfaces = nameof(DefaultImplementationsOfInterfaces);

    /// <summary>
    /// Indicates that this version of runtime supports the Unmanaged calling convention value.
    /// </summary>
    public const string UnmanagedSignatureCallingConvention = nameof(UnmanagedSignatureCallingConvention);

    /// <summary>
    /// Indicates that this version of runtime supports covariant returns in overrides of methods declared in classes.
    /// </summary>
    public const string CovariantReturnsOfClasses = nameof(CovariantReturnsOfClasses);

    /// <summary>
    /// Represents a runtime feature where types can define ref fields.
    /// </summary>
    public const string ByRefFields = nameof(ByRefFields);

    /// <summary>
    /// Represents a runtime feature where byref-like types can be used in Generic parameters.
    /// </summary>
    public const string ByRefLikeGenerics = nameof(ByRefLikeGenerics);

    /// <summary>
    /// Indicates that this version of runtime supports virtual static members of interfaces.
    /// </summary>
    public const string VirtualStaticsInInterfaces = nameof(VirtualStaticsInInterfaces);

    /// <summary>
    /// Indicates that this version of runtime supports <see cref="System.IntPtr" /> and <see cref="System.UIntPtr" /> as numeric types.
    /// </summary>
    public const string NumericIntPtr = nameof(NumericIntPtr);

    // ReSharper restore InconsistentNaming

    /// <summary>
    /// Checks whether a certain feature is supported by the Runtime.
    /// </summary>
    public static bool IsSupported(string feature)
    {
        return true;

        //TODO: enable when we have string comparison

        // return feature switch
        // {
        //     PortablePdb or
        //         CovariantReturnsOfClasses or
        //         ByRefFields or
        //         ByRefLikeGenerics or
        //         UnmanagedSignatureCallingConvention or
        //         DefaultImplementationsOfInterfaces or
        //         VirtualStaticsInInterfaces or
        //         NumericIntPtr => true,
        //
        //     nameof(IsDynamicCodeSupported) => IsDynamicCodeSupported,
        //     nameof(IsDynamicCodeCompiled) => IsDynamicCodeCompiled,
        //     nameof(IsMultithreadingSupported) => IsMultithreadingSupported,
        //     _ => false
        // };
    }

    /// <summary>
    /// Gets a value that indicates whether the runtime supports multithreading, including
    /// creating threads and using blocking synchronization primitives. This property
    /// returns <see langword="false"/> on platforms or configurations where multithreading
    /// is not supported or is disabled, such as single-threaded browser environments and WASI.
    /// </summary>
    // [FeatureSwitchDefinition("System.Runtime.CompilerServices.RuntimeFeature.IsMultithreadingSupported")]
    public static bool IsMultithreadingSupported
#if FEATURE_SINGLE_THREADED
            => false;
#else
        => true;
#endif

    // [FeatureSwitchDefinition("System.Runtime.CompilerServices.RuntimeFeature.IsDynamicCodeSupported")]
    public static bool IsDynamicCodeSupported => false;
    // = AppContext.TryGetSwitch("System.Runtime.CompilerServices.RuntimeFeature.IsDynamicCodeSupported", out bool isDynamicCodeSupported) ? isDynamicCodeSupported : true;

    // [FeatureGuard(typeof(RequiresDynamicCodeAttribute))]
    public static bool IsDynamicCodeCompiled
    {
#if CORECLR && !FEATURE_DYNAMIC_CODE_COMPILED
        get => false;
#else
        get => IsDynamicCodeSupported;
#endif
    }
}