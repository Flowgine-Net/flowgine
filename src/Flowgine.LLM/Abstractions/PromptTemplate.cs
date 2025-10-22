using System.Text.RegularExpressions;

namespace Flowgine.LLM.Abstractions;

/// <summary>
/// Template for simple string-based prompts with variable substitution.
/// Uses {variable} syntax for placeholders.
/// </summary>
/// <example>
/// <code>
/// var template = PromptTemplate.FromTemplate("Tell me about {topic} in {language}");
/// var prompt = template.Format(new { topic = "AI", language = "Czech" });
/// // Result: "Tell me about AI in Czech"
/// </code>
/// </example>
public sealed class PromptTemplate
{
    private readonly string _template;
    private readonly string[] _variables;

    private PromptTemplate(string template, string[] variables)
    {
        _template = template;
        _variables = variables;
    }

    /// <summary>
    /// Creates a prompt template from a string with {variable} placeholders.
    /// </summary>
    /// <param name="template">Template string with {variable} syntax</param>
    /// <returns>A new PromptTemplate instance</returns>
    /// <exception cref="ArgumentNullException">Thrown when template is null</exception>
    /// <example>
    /// <code>
    /// var template = PromptTemplate.FromTemplate("Tell me about {topic}");
    /// </code>
    /// </example>
    public static PromptTemplate FromTemplate(string template)
    {
        ArgumentNullException.ThrowIfNull(template);
        
        var variables = ExtractVariables(template);
        return new PromptTemplate(template, variables);
    }

    /// <summary>
    /// Formats the template with the provided values from a dictionary.
    /// </summary>
    /// <param name="values">Dictionary of variable names to values</param>
    /// <returns>Formatted string with all variables replaced</returns>
    /// <exception cref="ArgumentNullException">Thrown when values is null</exception>
    /// <exception cref="ArgumentException">Thrown when a required variable is missing</exception>
    /// <example>
    /// <code>
    /// var formatted = template.Format(new Dictionary&lt;string, object&gt; 
    /// { 
    ///     ["topic"] = "AI",
    ///     ["language"] = "Czech"
    /// });
    /// </code>
    /// </example>
    public string Format(IDictionary<string, object> values)
    {
        ArgumentNullException.ThrowIfNull(values);
        
        var result = _template;
        foreach (var variable in _variables)
        {
            if (!values.TryGetValue(variable, out var value))
            {
                throw new ArgumentException($"Missing value for variable '{variable}'", nameof(values));
            }
            result = result.Replace($"{{{variable}}}", value?.ToString() ?? "");
        }
        return result;
    }

    /// <summary>
    /// Formats the template with an anonymous object or any object with matching properties.
    /// Property names must match the variable names in the template (case-sensitive).
    /// </summary>
    /// <param name="values">Object with properties matching variable names</param>
    /// <returns>Formatted string with all variables replaced</returns>
    /// <exception cref="ArgumentNullException">Thrown when values is null</exception>
    /// <exception cref="ArgumentException">Thrown when a required variable is missing</exception>
    /// <example>
    /// <code>
    /// var formatted = template.Format(new { topic = "AI", language = "Czech" });
    /// </code>
    /// </example>
    public string Format(object values)
    {
        ArgumentNullException.ThrowIfNull(values);
        
        var dict = values.GetType()
            .GetProperties()
            .ToDictionary(p => p.Name, p => p.GetValue(values)!);
        
        return Format(dict);
    }

    /// <summary>
    /// Gets the list of variables found in the template.
    /// </summary>
    public IReadOnlyList<string> Variables => _variables;

    /// <summary>
    /// Gets the original template string.
    /// </summary>
    public string Template => _template;

    private static string[] ExtractVariables(string template)
    {
        var matches = Regex.Matches(template, @"\{(\w+)\}");
        return matches.Select(m => m.Groups[1].Value).Distinct().ToArray();
    }

    /// <summary>
    /// Returns the template string.
    /// </summary>
    public override string ToString() => _template;
}

