using Markdig;
using Microsoft.AspNetCore.Components;

namespace DaggerheartHelper.Shared.Helpers;

public static class MarkdownHelper
{
    private static readonly MarkdownPipeline Pipeline = new MarkdownPipelineBuilder()
        .UseAdvancedExtensions()
        .Build();

    public static MarkupString ToMarkdownHtml(this string? text)
    {
        if (string.IsNullOrWhiteSpace(text))
            return new MarkupString(string.Empty);

        var html = Markdown.ToHtml(text, Pipeline);
        return new MarkupString(html);
    }
}