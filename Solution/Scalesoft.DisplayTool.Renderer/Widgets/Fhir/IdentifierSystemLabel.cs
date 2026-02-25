using Scalesoft.DisplayTool.Renderer.Models;
using Scalesoft.DisplayTool.Renderer.Renderers;
using Scalesoft.DisplayTool.Shared.DocumentNavigation;

namespace Scalesoft.DisplayTool.Renderer.Widgets.Fhir;

public class IdentifierSystemLabel : Widget
{
    public override Task<RenderResult> Render(
        XmlDocumentNavigator navigator,
        IWidgetRenderer renderer,
        RenderContext context
    )
    {
        //TODO Add all the identifier systems that are explicitly specified
        var result = new ShowEnum(new Dictionary<string, string>
            {
                { "https://ncez.mzcr.cz/fhir/sid/rcis", "Rodné číslo" },
                { "https://ncez.mzcr.cz/fhir/sid/cpoj", "Číslo pojištěnce" },
                { "https://ncez.mzcr.cz/fhir/sid/op", "Číslo občanského průkazu" },
                { "https://ncez.mzcr.cz/fhir/sid/krpzs", "Kmenový registr zdravotnických pracovníků - KRZP" },
                { "https://ncez.mzcr.cz/fhir/sid/icp", "IČP" },
                { "https://ncez.mzcr.cz/fhir/sid/ico", "IČO" },
                { "https://ncez.mzcr.cz/fhir/sid/icz", "IČZ" },
                { "https://ncez.mzcr.cz/fhir/sid/kp", "Kód pojišťovny" },
                { "https://ncez.mzcr.cz/fhir/sid/clk", "Česká lékařská komora - ČLK" },
                { "https://ncez.mzcr.cz/fhir/sid/clek", "Česká lékárnická komora - ČLéK" },
            }, "f:system/@value",
            fallbackDisplay: new Condition(
                "f:type/f:coding[f:system/@value='http://terminology.hl7.org/CodeSystem/v2-0203' and f:code/@value='PPN']",
                new LocalizedLabel("general.passport-number")
            )
        );
        return result.Render(navigator, renderer, context);
    }
}