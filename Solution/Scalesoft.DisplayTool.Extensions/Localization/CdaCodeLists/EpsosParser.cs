using System.Xml;
using Scalesoft.DisplayTool.Shared.Configuration;
using Scalesoft.DisplayTool.Shared.Translation;

namespace Scalesoft.DisplayTool.Extensions.Localization.CdaCodeLists;

public class EpsosParser(KnownOidsConfiguration? knownOids)
{
    public void LoadIntoStorage(ITranslationsStorage storage)
    {
        // Get XML files from Resources directory in output
        var baseDirectory = AppContext.BaseDirectory;
        var resourcesDirectory = Path.Combine(baseDirectory, "Resources");

        if (!Directory.Exists(resourcesDirectory))
        {
            throw new DirectoryNotFoundException($"Resources directory not found at: {resourcesDirectory}");
        }

        var xmlFiles = Directory.GetFiles(resourcesDirectory, "*.xml", SearchOption.TopDirectoryOnly);
        if (xmlFiles.Length == 0)
        {
            return;
        }

        // Check if any file was modified after the last initialization
        var lastInitTime = storage.GetLastInitializationTime()?.ToUniversalTime();
        var mostRecentFileTime = xmlFiles.Max(File.GetLastWriteTimeUtc);

        if (lastInitTime.HasValue && mostRecentFileTime <= lastInitTime.Value)
        {
            // All files are older than last initialization, no need to reload
            return;
        }

        // At least one file has changed, clear storage and reload everything
        storage.Clear();

        foreach (var filePath in xmlFiles)
        {
            using var batchWriter = storage.BeginBatch();

            var doc = new XmlDocument();
            doc.Load(filePath);
            var root = doc.DocumentElement;
            if (root == null)
            {
                continue;
            }

            var valueSet = root.SelectSingleNode("/ValueSet/@oid")?.Value;

            foreach (XmlNode node in root.SelectNodes("/ValueSet/concept")!)
            {
                var systemUrl = node.SelectSingleNode("@codeSystemUrl")?.Value;
                var oid = node.SelectSingleNode("@codeSystem")?.Value;

                var systemId = GetSystemId(systemUrl, oid);
                if (systemId == null)
                {
                    continue;
                }

                var code = node.SelectSingleNode("@code")?.Value;
                if (code == null)
                {
                    continue;
                }

                var translations = new LocalizedValue();
                var properties = new Dictionary<string, LocalizedValue>();

                foreach (XmlNode propertyNode in node.ChildNodes)
                {
                    var lang = propertyNode.Attributes?["lang"]?.Value ?? "en-GB";
                    var value = propertyNode.InnerText;
                    var propertyType = propertyNode.Name;

                    if (propertyType.Equals("designation", StringComparison.InvariantCultureIgnoreCase))
                    {
                        translations[lang] = propertyNode.InnerText;
                    }
                    else
                    {
                        if (!properties.TryGetValue(propertyNode.Name, out var property))
                        {
                            property = new LocalizedValue();
                            properties[propertyNode.Name] = property;
                        }

                        property[lang] = value;
                    }
                }

                var concept = new Concept
                {
                    Code = code,
                    System = systemId,
                    ValueSets = valueSet != null ? [valueSet] : [],
                    Translations = translations,
                    Properties = properties,
                };

                batchWriter.AddConcept(concept);
            }

            // Flush remaining entries from this file
            batchWriter.Flush();
        }

        // Update the last initialization time to current time
        storage.SetLastInitializationTime(DateTime.UtcNow);
    }

    private string? GetSystemId(string? systemUrl, string? oid)
    {
        if (systemUrl != null)
        {
            return systemUrl;
        }

        if (knownOids == null)
        {
            return oid;
        }

        var url = knownOids!.GetValueOrDefault(oid);
        return url ?? oid;
    }
}