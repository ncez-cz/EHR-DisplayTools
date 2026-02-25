using System.ComponentModel.DataAnnotations;
using System.Diagnostics.CodeAnalysis;
using Microsoft.Extensions.Logging;

namespace Scalesoft.DisplayTool.Renderer.ConfigurationUtils;

public static class ConfigValidator
{
    public static bool TryGetValidConfig<T>(T? maybeValidConfig, ILogger logger, [NotNullWhen(true)] out T? config)
        where T : class
    {
        if (maybeValidConfig == null)
        {
            config = null;
            return false;
        }

        var validationResult = new List<ValidationResult>();
        var configIsValid = Validator.TryValidateObject(maybeValidConfig, new ValidationContext(maybeValidConfig),
            validationResult, true);
        if (configIsValid)
        {
            config = maybeValidConfig;
            return true;
        }

        if (logger.IsEnabled(LogLevel.Error))
        {
            logger.LogError("Invalid configuration, missing {param}",
                string.Join(", ", validationResult.Select(x => x.ErrorMessage)));
        }

        config = null;
        return false;
    }
}