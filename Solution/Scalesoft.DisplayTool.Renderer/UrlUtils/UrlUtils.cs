namespace Scalesoft.DisplayTool.Renderer.UrlUtils;

public static class UrlUtil
{
    public static string PreprocessBaseUrl(string baseUrl)
    {
        if (!string.IsNullOrEmpty(baseUrl) && !baseUrl.EndsWith('/'))
        {
            baseUrl += '/';
        }

        return baseUrl;
    }
}