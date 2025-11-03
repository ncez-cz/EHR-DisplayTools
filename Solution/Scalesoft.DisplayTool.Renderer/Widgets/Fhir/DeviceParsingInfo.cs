namespace Scalesoft.DisplayTool.Renderer.Widgets.Fhir;

public static class DeviceParsingInfo
{
    public static Widget[] CompactRenderingWidgets =
    [
        new Optional("f:manufacturer", new NameValuePair([new ConstantText("Výrobce")], [new Text("@value")])),
        new Condition("f:deviceName", new NameValuePair([new ConstantText("Název")],
            [new CommaSeparatedBuilder("f:deviceName", _ => [new Optional("f:name/@value", new Text())]),])),
        new Optional("f:modelNumber",
            new NameValuePair([new ConstantText("Číslo modelu")], [new Text("@value")])),
        new Optional("f:serialNumber",
            new NameValuePair([new ConstantText("Sériové číslo")], [new Text("@value"),])),
        new Condition("f:specialization",
            new NameValuePair([new ConstantText("Specializace")], [
                new CommaSeparatedBuilder("f:specialization",
                    _ => [new Optional("f:systemType", new CodeableConcept())]),
            ])
        ),
        new Optional("f:expirationDate",
            new NameValuePair([new ConstantText("Datum expirace")], [new ShowDateTime(),])),
    ];
}