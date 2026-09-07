using System;
using System.Globalization;
using System.Text;

namespace Byd.CoreExtensions.Extensions;

/// <summary>
/// Kararli metinleri PostgreSQL snake_case adlandirma bicimine donusturur.
/// </summary>
// islevi: Unicode karakter sinirlarini EFCore.NamingConventions ile ayni kurallarla snake_case'e cevirir.
// sistemdeki gorevi: Model seviyesindeki fiziksel veritabani adlandirmasinin paylasilan donusum sahibidir.
public static class StringExtensions
{
    // Metni culture-invariant snake_case bicimine donusturur; bos degerleri degistirmeden dondurur.
    public static string ConvertToSnakeCase(this string str)
    {
        if (string.IsNullOrWhiteSpace(str))
        {
            return str;
        }

        var builder = new StringBuilder(str.Length + Math.Min(2, str.Length / 5));
        UnicodeCategory? previousCategory = null;

        for (var i = 0; i < str.Length; i++)
        {
            var current = str[i];
            if (current == '_')
            {
                builder.Append('_');
                previousCategory = null;
                continue;
            }

            var currentCategory = char.GetUnicodeCategory(current);
            switch (currentCategory)
            {
                case UnicodeCategory.UppercaseLetter:
                case UnicodeCategory.TitlecaseLetter:
                    if (previousCategory == UnicodeCategory.SpaceSeparator ||
                        previousCategory == UnicodeCategory.LowercaseLetter ||
                        (previousCategory != UnicodeCategory.DecimalDigitNumber &&
                         previousCategory.HasValue &&
                         i > 0 &&
                         i + 1 < str.Length &&
                         char.IsLower(str[i + 1])))
                    {
                        builder.Append('_');
                    }

                    current = char.ToLower(current, CultureInfo.InvariantCulture);
                    break;

                case UnicodeCategory.LowercaseLetter:
                case UnicodeCategory.DecimalDigitNumber:
                    if (previousCategory == UnicodeCategory.SpaceSeparator)
                    {
                        builder.Append('_');
                    }

                    break;

                default:
                    if (previousCategory.HasValue)
                    {
                        previousCategory = UnicodeCategory.SpaceSeparator;
                    }

                    continue;
            }

            builder.Append(current);
            previousCategory = currentCategory;
        }

        return builder.ToString();
    }
}
