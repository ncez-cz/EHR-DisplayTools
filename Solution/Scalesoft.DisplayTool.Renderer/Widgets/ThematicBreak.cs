using System.Reflection;
using Scalesoft.DisplayTool.Renderer.Extensions;
using Scalesoft.DisplayTool.Renderer.Models;
using Scalesoft.DisplayTool.Renderer.Renderers;
using Scalesoft.DisplayTool.Renderer.Utils;
using Scalesoft.DisplayTool.Renderer.Widgets.WidgetUtils;
using Scalesoft.DisplayTool.Shared.DocumentNavigation;

namespace Scalesoft.DisplayTool.Renderer.Widgets;

public class ThematicBreak(string? optionalClass = null) : Widget
{
    public override async Task<RenderResult> Render(
        XmlDocumentNavigator navigator,
        IWidgetRenderer renderer,
        RenderContext context
    )
    {
        ViewModel viewModel = new()
        {
            CustomClass = optionalClass,
        };

        return await renderer.RenderThematicBreak(viewModel);
    }

    /// <summary>
    ///     Adds a thematic break based on the property presence and their hideability in simple mode.
    ///     Assumes resource property definitions include <see cref="HiddenInSimpleModeAttribute" /> and/or
    ///     <see cref="NarrativeDisplayTypeAttribute" /> attributes.
    /// </summary>
    /// <param name="props">Evaluated resource properties</param>
    /// <param name="before">Possible resource properties before the thematic break</param>
    /// <param name="after">Possible resource properties after the thematic break</param>
    public static Widget SurroundedThematicBreak<T>(
        InfrequentPropertiesDataInContext<T> props,
        IList<T> before,
        IList<T> after
    ) where T : Enum
    {
        var typesBefore = new HashSet<ContentHideabilityTypes>();
        var typesAfter = new HashSet<ContentHideabilityTypes>();

        var beforeWithoutSpecialCases = before.Where(x =>
                !HasAttr<NarrativeDisplayTypeAttribute, T>(x) &&
                !HasAttr<HiddenRedundantSubjectDisplayTypeAttribute, T>(x))
            .ToList();
        var afterWithoutSpecialCases = after.Where(x =>
                !HasAttr<NarrativeDisplayTypeAttribute, T>(x) &&
                !HasAttr<HiddenRedundantSubjectDisplayTypeAttribute, T>(x))
            .ToList();
        var beforeNarrative = before.Where(HasAttr<NarrativeDisplayTypeAttribute, T>).ToList();
        var afterNarrative = after.Where(HasAttr<NarrativeDisplayTypeAttribute, T>).ToList();
        var beforeSubject = before.Where(HasAttr<HiddenRedundantSubjectDisplayTypeAttribute, T>).ToList();
        var afterSubject = after.Where(HasAttr<HiddenRedundantSubjectDisplayTypeAttribute, T>).ToList();
        IterateProps<HiddenInSimpleModeAttribute, T>(props, beforeWithoutSpecialCases, typesBefore);
        IterateProps<HiddenInSimpleModeAttribute, T>(props, afterWithoutSpecialCases, typesAfter);
        IterateNarrativeProps(props, beforeNarrative, typesBefore);
        IterateNarrativeProps(props, afterNarrative, typesAfter);
        IterateRedundantProps(props, beforeSubject, typesBefore);
        IterateRedundantProps(props, afterSubject, typesAfter);

        // at least one side is empty, skip thematic break 
        if (typesBefore.Count == 0 || typesAfter.Count == 0)
        {
            return new NullWidget();
        }

        // display regular thematic break only if both sides contain non-hideable display types
        if (typesBefore.Contains(ContentHideabilityTypes.NonHiddeableInSimpleMode) &&
            typesAfter.Contains(ContentHideabilityTypes.NonHiddeableInSimpleMode))
        {
            return new ThematicBreak();
        }

        // display thematic break hideable-by-narrative only if both sides consist of hideable-narrative display type
        if (typesAfter.Only(ContentHideabilityTypes.HideableNarrativeCollapser) ||
            typesBefore.Only(ContentHideabilityTypes.HideableNarrativeCollapser))
        {
            return new ThematicBreak(optionalClass: "narrative-print-collapser");
        }

        // display thematic break hideable-by-simple mode only if both sides consist of hideable-by-simple mode display type
        if (
            typesAfter.Only(ContentHideabilityTypes.HiddeableInSimpleMode) ||
            typesBefore.Only(ContentHideabilityTypes.HiddeableInSimpleMode))
        {
            return new HideableDetails(new ThematicBreak());
        }

        // here the only remaining case is a mix of both hideable types on both sides
        return new HideableDetails(ContainerType.Auto, "narrative-print-collapser", true,
            new ThematicBreak()); // optional-detail display hidden must be overridable also by narrative
    }

    private enum ContentHideabilityTypes
    {
        HiddeableInSimpleMode,
        NonHiddeableInSimpleMode,
        HideableNarrativeCollapser,
    }

    private static bool HasAttr<TA, TEnum>(TEnum value) where TA : Attribute where TEnum : Enum
    {
        return typeof(TEnum).GetField(value.ToString())?.GetCustomAttribute<TA>() != null;
    }

    private static void IterateProps<TA, TEnum>(
        InfrequentPropertiesDataInContext<TEnum> presentProps,
        IList<TEnum> testedProps,
        HashSet<ContentHideabilityTypes> presentTypes
    ) where TA : Attribute where TEnum : Enum
    {
        foreach (var testedProp in testedProps)
        {
            if (!presentProps.Contains(testedProp))
            {
                continue;
            }

            var hiddenInSimpleModeAttribute = HasAttr<TA, TEnum>(testedProp);
            if (hiddenInSimpleModeAttribute)
            {
                presentTypes.Add(ContentHideabilityTypes.HiddeableInSimpleMode);
            }
            else
            {
                presentTypes.Add(ContentHideabilityTypes.NonHiddeableInSimpleMode);
            }
        }
    }

    private static void IterateNarrativeProps<TEnum>(
        InfrequentPropertiesDataInContext<TEnum> presentProps,
        IList<TEnum> testedProps,
        HashSet<ContentHideabilityTypes> presentTypes
    ) where TEnum : Enum
    {
        foreach (var testedProp in testedProps)
        {
            if (!presentProps.Contains(testedProp))
            {
                continue;
            }

            var path = presentProps.InfrequentProperties[testedProp];
            var narrativeNav = presentProps.Navigator.SelectSingleNode(path);
            var narrativeVisible = NarrativeUtils.ShowNarrativeByDefault(narrativeNav);
            if (narrativeVisible)
            {
                presentTypes.Add(ContentHideabilityTypes.NonHiddeableInSimpleMode);
            }
            else
            {
                // narrative collapser content is both wrapped in optional-detail and marked by narrative-print class
                presentTypes.Add(ContentHideabilityTypes.HiddeableInSimpleMode);
                presentTypes.Add(ContentHideabilityTypes.HideableNarrativeCollapser);
            }
        }
    }

    private static void IterateRedundantProps<TEnum>(
        InfrequentPropertiesDataInContext<TEnum> presentProps,
        IList<TEnum> testedProps,
        HashSet<ContentHideabilityTypes> presentTypes
    ) where TEnum : Enum
    {
        foreach (var testedProp in testedProps)
        {
            if (!presentProps.Contains(testedProp))
            {
                continue;
            }

            var path = presentProps.InfrequentProperties[testedProp];
            var narrativeNav = presentProps.Navigator.SelectSingleNode(path);
            var isSubjectFromComposition = narrativeNav.IsSubjectFromComposition();
            if (isSubjectFromComposition)
            {
                presentTypes.Add(ContentHideabilityTypes.HiddeableInSimpleMode);
            }
            else
            {
                presentTypes.Add(ContentHideabilityTypes.NonHiddeableInSimpleMode);
            }
        }
    }

    public class ViewModel : ViewModelBase;
}