namespace System.Runtime.CompilerServices;

[ComponentModel.EditorBrowsable(ComponentModel.EditorBrowsableState.Never)]
[Diagnostics.CodeAnalysis.ExcludeFromCodeCoverage]
internal static class IsExternalInit { }

[AttributeUsage(AttributeTargets.Class | AttributeTargets.Struct | AttributeTargets.Field | AttributeTargets.Property, AllowMultiple = false, Inherited = false)]
[Diagnostics.CodeAnalysis.ExcludeFromCodeCoverage]
internal sealed class RequiredMemberAttribute : Attribute { }

[AttributeUsage(AttributeTargets.All, AllowMultiple = true, Inherited = false)]
[Diagnostics.CodeAnalysis.ExcludeFromCodeCoverage]
internal sealed class CompilerFeatureRequiredAttribute : Attribute
{
    public string FeatureName { get; }
    public bool IsOptional { get; init; }
    public const string RefStructs = nameof(RefStructs);
    public const string RequiredMembers = nameof(RequiredMembers);
    public CompilerFeatureRequiredAttribute(string featureName) => FeatureName = featureName;
}
