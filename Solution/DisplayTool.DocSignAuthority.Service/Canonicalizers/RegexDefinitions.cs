using System.Text.RegularExpressions;

namespace DisplayTool.DocSignAuthority.Service.Canonicalizers;

public partial class RegexDefinitions
{
    [GeneratedRegex(@"\s+")]
    public static partial Regex MultipleWhitespaceRegex();
}