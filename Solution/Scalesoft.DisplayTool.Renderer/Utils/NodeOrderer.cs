using Scalesoft.DisplayTool.Shared.DocumentNavigation;

namespace Scalesoft.DisplayTool.Renderer.Utils;

public delegate List<XmlDocumentNavigator> NodeOrderer(IEnumerable<XmlDocumentNavigator> nodes);
