namespace Shop.Catalog.Api.Common;
using System.Text;

using Microsoft.AspNetCore.Http.Connections;

public static class SlugGenerator
{
    private static readonly Dictionary<char, string> Translit = new()
    {
        ['а'] = "a", ['б'] = "b", ['в'] = "v", ['г'] = "g", ['д'] = "d", ['е'] = "e",
        ['ё'] = "e", ['ж'] = "zh", ['з'] = "z", ['и'] = "i", ['й'] = "y", ['к'] = "k",
        ['л'] = "l", ['м'] = "m", ['н'] = "n", ['о'] = "o", ['п'] = "p", ['р'] = "r",
        ['с'] = "s", ['т'] = "t", ['у'] = "u", ['ф'] = "f", ['х'] = "h", ['ц'] = "ts",
        ['ч'] = "ch", ['ш'] = "sh", ['щ'] = "sch", ['ъ'] = "", ['ы'] = "y", ['ь'] = "",
        ['э'] = "e", ['ю'] = "yu", ['я'] = "ya",
    };

    public static string Generate(string text)
    {
        var result = new StringBuilder(text.Length);
        var lastWasDash = true;
        foreach (var ch in text.ToLowerInvariant())
        {
            if(Translit.TryGetValue(ch, out var latin))
            {
                if(latin.Length > 0)
                {
                    result.Append(latin);
                    lastWasDash = false;
                }
            }
            else if (ch is >= 'a' and <= 'z' or >= '0' and <= '9')
            {
                result.Append(ch);
                lastWasDash = false;
            }
            else if (!lastWasDash)
            {
                result.Append('-');
                lastWasDash = true;
            }
        }
        return result.ToString().TrimEnd('-');
    }

    public static async Task<string> MakeUniqueAsync(
        string baseSlug, int maxLength, Func<string, Task<bool>> exists)
    {
        var trimmed = baseSlug.Length > maxLength - 6
            ? baseSlug[..(maxLength - 6)].TrimEnd('-')
            : baseSlug;
        
        var candidate = trimmed;
        var suffix = 2;
        while(await exists(candidate))
        {
            candidate = $"{trimmed} - {suffix++}";
        }
        return candidate;
    }

}