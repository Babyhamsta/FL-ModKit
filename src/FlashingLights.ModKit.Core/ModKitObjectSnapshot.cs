using System.Globalization;
using System.Reflection;
using System.Runtime.CompilerServices;
using System.Text;
using UnityEngine;

namespace FlashingLights.ModKit.Core;

public static class ModKitObjectSnapshot
{
    private const BindingFlags MemberFlags = BindingFlags.Instance | BindingFlags.Public | BindingFlags.NonPublic;

    public static string Capture(object? target, params string[] memberNames)
    {
        var builder = new StringBuilder();
        if (target == null)
        {
            ModKitLogFormat.AppendKeyValue(builder, "target", "null");
            return builder.ToString();
        }

        ModKitLogFormat.AppendKeyValue(builder, "type", target.GetType().FullName);
        ModKitLogFormat.AppendKeyValue(builder, "id", RuntimeHelpers.GetHashCode(target));
        if (ShouldReadUnityState(target))
        {
            AppendGameObjectState(builder, target);
            AppendComponentState(builder, target);
            AppendRigidbodyState(builder, target);
        }

        foreach (var memberName in memberNames)
        {
            if (TryReadMember(target, memberName, out var value))
            {
                ModKitLogFormat.AppendKeyValue(builder, memberName, SafeFormatValue(memberName, value));
            }
        }

        return builder.ToString();
    }

    private static bool ShouldReadUnityState(object target)
    {
        var fullName = target.GetType().FullName ?? string.Empty;
        return fullName.StartsWith("UnityEngine.", StringComparison.Ordinal)
            || fullName.StartsWith("Il2Cpp", StringComparison.Ordinal);
    }

    private static void AppendComponentState(StringBuilder builder, object target)
    {
        if (target is not Component component)
        {
            return;
        }

        try
        {
            var gameObject = component.gameObject;
            ModKitLogFormat.AppendKeyValue(builder, "object", gameObject.name);
            ModKitLogFormat.AppendKeyValue(builder, "active", gameObject.activeInHierarchy);
            ModKitLogFormat.AppendKeyValue(builder, "pos", ModKitLogFormat.Vector3(component.transform.position));
        }
        catch (Exception ex)
        {
            ModKitLogFormat.AppendKeyValue(builder, "componentReadError", $"{ex.GetType().Name}: {ex.Message}");
        }
    }

    private static void AppendGameObjectState(StringBuilder builder, object target)
    {
        if (target is not GameObject gameObject)
        {
            return;
        }

        try
        {
            ModKitLogFormat.AppendKeyValue(builder, "object", gameObject.name);
            ModKitLogFormat.AppendKeyValue(builder, "active", gameObject.activeInHierarchy);
            ModKitLogFormat.AppendKeyValue(builder, "pos", ModKitLogFormat.Vector3(gameObject.transform.position));
            ModKitLogFormat.AppendKeyValue(builder, "rotation", ModKitLogFormat.Vector3(gameObject.transform.rotation.eulerAngles));
        }
        catch (Exception ex)
        {
            ModKitLogFormat.AppendKeyValue(builder, "gameObjectReadError", $"{ex.GetType().Name}: {ex.Message}");
        }
    }

    private static void AppendRigidbodyState(StringBuilder builder, object target)
    {
        var rigidbody = FindRigidbody(target);
        if (rigidbody == null)
        {
            return;
        }

        try
        {
            ModKitLogFormat.AppendKeyValue(builder, "velocity", ModKitLogFormat.Vector3(rigidbody.velocity));
            ModKitLogFormat.AppendKeyValue(builder, "rbSpeed", rigidbody.velocity.magnitude.ToString("0.###", CultureInfo.InvariantCulture));
            ModKitLogFormat.AppendKeyValue(builder, "rbSleeping", rigidbody.IsSleeping());
        }
        catch (Exception ex)
        {
            ModKitLogFormat.AppendKeyValue(builder, "rigidbodyReadError", $"{ex.GetType().Name}: {ex.Message}");
        }
    }

    private static Rigidbody? FindRigidbody(object target)
    {
        if (target is Rigidbody rigidbody)
        {
            return rigidbody;
        }

        foreach (var memberName in new[] { "vehicleRigidbody", "rb", "m_rigidbody", "cachedRigidbody" })
        {
            if (TryReadMember(target, memberName, out var value) && value is Rigidbody memberRigidbody)
            {
                return memberRigidbody;
            }
        }

        try
        {
            return target switch
            {
                Component component => component.GetComponent<Rigidbody>(),
                GameObject gameObject => gameObject.GetComponent<Rigidbody>(),
                _ => null
            };
        }
        catch
        {
            return null;
        }
    }

    private static bool TryReadMember(object target, string memberName, out object? value)
    {
        value = null;
        var type = target.GetType();

        var property = type.GetProperty(memberName, MemberFlags);
        if (property is { CanRead: true } && property.GetIndexParameters().Length == 0)
        {
            try
            {
                value = property.GetValue(target);
                return true;
            }
            catch
            {
            }
        }

        var field = type.GetField(memberName, MemberFlags);
        if (field != null)
        {
            try
            {
                value = field.GetValue(target);
                return true;
            }
            catch
            {
            }
        }

        return false;
    }

    private static string SafeFormatValue(string memberName, object? value)
    {
        try
        {
            return FormatValue(value);
        }
        catch (Exception ex)
        {
            return $"{memberName}:format-error:{ex.GetType().Name}";
        }
    }

    private static string FormatValue(object? value, int depth = 0)
    {
        if (value == null)
        {
            return "null";
        }

        if (depth > 2)
        {
            return value.GetType().Name;
        }

        if (TryFormatVectorLike(value, out var vectorText))
        {
            return vectorText;
        }

        return value switch
        {
            string text => text,
            bool or byte or sbyte or short or ushort or int or uint or long or ulong => Convert.ToString(value, CultureInfo.InvariantCulture) ?? string.Empty,
            float number => number.ToString("0.###", CultureInfo.InvariantCulture),
            double number => number.ToString("0.###", CultureInfo.InvariantCulture),
            decimal number => number.ToString("0.###", CultureInfo.InvariantCulture),
            Enum enumValue => enumValue.ToString(),
            UnityEngine.Vector2 vector => string.Format(CultureInfo.InvariantCulture, "({0:0.###},{1:0.###})", vector.x, vector.y),
            UnityEngine.Vector3 vector => ModKitLogFormat.Vector3(vector),
            GameObject gameObject => FormatGameObject(gameObject),
            Transform transform => FormatTransform(transform),
            Rigidbody rigidbody => FormatRigidbody(rigidbody),
            Component component => FormatComponent(component),
            _ => FormatObject(value, depth)
        };
    }

    private static bool TryFormatVectorLike(object value, out string text)
    {
        text = string.Empty;
        var type = value.GetType();
        if (!TryReadNumericMember(type, value, "x", out var x)
            || !TryReadNumericMember(type, value, "y", out var y)
            || !TryReadNumericMember(type, value, "z", out var z))
        {
            return false;
        }

        text = ModKitLogFormat.Vector3((float)x, (float)y, (float)z);
        return true;
    }

    private static bool TryReadNumericMember(Type type, object target, string memberName, out double value)
    {
        value = 0;
        var property = type.GetProperty(memberName, MemberFlags);
        if (property is { CanRead: true } && TryConvertNumber(property.GetValue(target), out value))
        {
            return true;
        }

        var field = type.GetField(memberName, MemberFlags);
        return field != null && TryConvertNumber(field.GetValue(target), out value);
    }

    private static bool TryConvertNumber(object? value, out double result)
    {
        result = 0;
        if (value == null)
        {
            return false;
        }

        try
        {
            result = Convert.ToDouble(value, CultureInfo.InvariantCulture);
            return true;
        }
        catch
        {
            return false;
        }
    }

    private static string FormatObject(object value, int depth)
    {
        if (TryReadMember(value, "Value", out var fsmValue) && !ReferenceEquals(value, fsmValue))
        {
            return FormatValue(fsmValue, depth + 1);
        }

        var text = value.ToString() ?? value.GetType().Name;
        return text.Length <= 160 ? text : text[..160];
    }

    private static string FormatGameObject(GameObject gameObject)
    {
        try
        {
            return $"GameObject({gameObject.name},active={gameObject.activeInHierarchy},pos={ModKitLogFormat.Vector3(gameObject.transform.position)})";
        }
        catch
        {
            return "GameObject(unreadable)";
        }
    }

    private static string FormatTransform(Transform transform)
    {
        try
        {
            return $"Transform({transform.name},pos={ModKitLogFormat.Vector3(transform.position)})";
        }
        catch
        {
            return "Transform(unreadable)";
        }
    }

    private static string FormatRigidbody(Rigidbody rigidbody)
    {
        try
        {
            return $"Rigidbody(speed={rigidbody.velocity.magnitude.ToString("0.###", CultureInfo.InvariantCulture)},velocity={ModKitLogFormat.Vector3(rigidbody.velocity)},sleeping={rigidbody.IsSleeping()})";
        }
        catch
        {
            return "Rigidbody(unreadable)";
        }
    }

    private static string FormatComponent(Component component)
    {
        try
        {
            return $"{component.GetType().Name}({component.name},pos={ModKitLogFormat.Vector3(component.transform.position)})";
        }
        catch
        {
            return $"{component.GetType().Name}(unreadable)";
        }
    }
}
