namespace Scalesoft.DisplayTool.Renderer.Utils;

/// <summary>
///     Used to mark resource properties hidden dynamically based on their redundancy (mainly 'subject')
/// </summary>
[AttributeUsage(AttributeTargets.Field)]
public class HiddenRedundantSubjectDisplayTypeAttribute : Attribute
{
}