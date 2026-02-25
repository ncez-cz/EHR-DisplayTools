using System.Text;

namespace Scalesoft.DisplayTool.Renderer.Utils;

public static class VersionProvider
{
    public static string GetVersion()
    {
        // If we're on the release tag, don't show the commit hash
        if (GitVersionInformation.CommitsSinceVersionSource == "0")
    #pragma warning disable CS0162 // Unreachable code detected
        {
            return GitVersionInformation.SemVer;
        }
#pragma warning restore CS0162 // Unreachable code detected

        var builder = new StringBuilder();
        builder.Append(GitVersionInformation.MajorMinorPatch);
        builder.Append(GitVersionInformation.PreReleaseLabelWithDash);
        builder.Append('+');
        builder.Append(GitVersionInformation.ShortSha);

        return builder.ToString();
    }
}