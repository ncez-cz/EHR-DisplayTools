using System.Runtime.Serialization;
using Scalesoft.DisplayTool.Renderer.Extensions;
using Scalesoft.DisplayTool.Shared.DocumentNavigation;

namespace Scalesoft.DisplayTool.Renderer.Utils;

public static class NarrativeUtils
{
    private static readonly NarrativeStatus[] m_showByDefault =
        [NarrativeStatus.Additional, NarrativeStatus.Extensions];

    private static NarrativeStatus? GetNarrativeStatus(XmlDocumentNavigator narrativeNode)
    {
        if (narrativeNode.Node == null)
        {
            return null;
        }

        var status = narrativeNode.SelectSingleNode("f:status/@value");
        var statusEnum = status.Node?.Value.ToEnum<NarrativeStatus>();

        return statusEnum;
    }

    public static bool ShowNarrativeByDefault(XmlDocumentNavigator narrativeNode)
    {
        var narrativeStatus = GetNarrativeStatus(narrativeNode);

        return narrativeStatus != null && m_showByDefault.Contains(narrativeStatus.Value);
    }

    private enum NarrativeStatus
    {
        [EnumMember(Value = "generated")] Generated,
        [EnumMember(Value = "extensions")] Extensions,
        [EnumMember(Value = "additional")] Additional,
        [EnumMember(Value = "empty")] Empty
    }
}