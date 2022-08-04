using JetBrains.Annotations;

namespace Shared.Utility;

public static class Ensure
{
    [ContractAnnotation("isSatisfied:false => halt")]
    public static void That(bool isSatisfied, string? errorMessage = default)
    {
        if (isSatisfied)
            return;
        Unreachable(errorMessage);
    }    
    public static void Unreachable(string? errorMessage = default) 
        => throw new(errorMessage ?? "the execution flow isn't supposed to ever reach this point in code");
}
