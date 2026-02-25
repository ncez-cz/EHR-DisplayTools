using System.Diagnostics.CodeAnalysis;

namespace DisplayTool.DocSignAuthority.Service.FhirManipulation;

public class RootResourceResult
{
    public string? Id { get; set; }

    public required string ResourceName { get; set; }

    public required string Content { get; set; }
}

public class RootResourceValidatedResult
{
    public static bool Validate(
        RootResourceResult rootResourceResult,
        [NotNullWhen(true)] out RootResourceValidatedResult? result
    )
    {
        if (rootResourceResult.Id == null)
        {
            result = null;
            return false;
        }

        result = new RootResourceValidatedResult
        {
            Id = rootResourceResult.Id,
            ResourceName = rootResourceResult.ResourceName,
            Content = rootResourceResult.Content,
        };

        return true;
    }

    public required string Id { get; set; }

    public required string ResourceName { get; set; }

    public required string Content { get; set; }
}