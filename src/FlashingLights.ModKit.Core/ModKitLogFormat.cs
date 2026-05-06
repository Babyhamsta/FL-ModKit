using System.Globalization;
using System.Text;
using UnityEngine;

namespace FlashingLights.ModKit.Core;

public static class ModKitLogFormat
{
    public static string Quote(string? value)
    {
        if (value == null)
        {
            return "\"\"";
        }

        var builder = new StringBuilder(value.Length + 2);
        builder.Append('"');
        foreach (var c in value)
        {
            switch (c)
            {
                case '\\': builder.Append("\\\\"); break;
                case '"': builder.Append("\\\""); break;
                case '\n': builder.Append("\\n"); break;
                case '\r': builder.Append("\\r"); break;
                case '\t': builder.Append("\\t"); break;
                default:
                    if (c < 0x20 || c == 0x7F)
                    {
                        builder.Append("\\u").Append(((int)c).ToString("X4", CultureInfo.InvariantCulture));
                    }
                    else
                    {
                        builder.Append(c);
                    }
                    break;
            }
        }
        builder.Append('"');
        return builder.ToString();
    }

    public static string KeyValue(string key, object? value)
    {
        if (string.IsNullOrEmpty(key))
        {
            throw new ArgumentException("Key cannot be empty.", nameof(key));
        }

        var rendered = value == null ? null : Convert.ToString(value, CultureInfo.InvariantCulture);
        return $"{key}={Quote(rendered)}";
    }

    public static void AppendKeyValue(StringBuilder builder, string key, object? value)
    {
        ArgumentNullException.ThrowIfNull(builder);

        if (builder.Length > 0)
        {
            builder.Append(' ');
        }
        builder.Append(KeyValue(key, value));
    }

    public static string Vector3(UnityEngine.Vector3 value) => Vector3(value.x, value.y, value.z);

    public static string Vector3(float x, float y, float z) => string.Format(
        CultureInfo.InvariantCulture,
        "({0:0.###},{1:0.###},{2:0.###})",
        x,
        y,
        z);
}
