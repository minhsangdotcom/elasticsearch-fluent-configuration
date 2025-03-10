using System.Collections;
using System.Linq.Expressions;
using System.Reflection;
using System.Text;
using CaseConverter;

namespace FluentConfiguration.Configurations;

public static class ElsIndexExtension
{
    /// <summary>
    /// get Index name
    /// </summary>
    /// <typeparam name="T"></typeparam>
    /// <returns></returns>
    public static string GetName<T>(string? prefix = null) =>
        $"{prefix}{typeof(T).Name}".Underscored();

    /// <summary>
    /// Get sub field keyword
    /// </summary>
    /// <typeparam name="T"></typeparam>
    /// <param name="expression"></param>
    /// <returns></returns>
    public static string GetKeywordName<T>(Expression<Func<T, object>> expression)
    {
        PropertyInfo propertyInfo = ToPropertyInfo(expression);
        return $"{propertyInfo.Name.FirstCharToLowerCase()}{ElsPrefix.KeywordPrefixName}";
    }

    public static string GetKeywordName<T>(string propertyName)
    {
        PropertyInfo propertyInfo = propertyName.Contains('.')
            ? typeof(T).GetNestedPropertyInfo(propertyName)
            : typeof(T).GetProperty(
                propertyName,
                BindingFlags.IgnoreCase | BindingFlags.Public | BindingFlags.Instance
            ) ?? throw new ArgumentException($"{propertyName} is not found.");

        return $"{propertyInfo.Name.FirstCharToLowerCase()}{ElsPrefix.KeywordPrefixName}";
    }

    private static string Underscored(this string s)
    {
        var builder = new StringBuilder();

        for (var i = 0; i < s.Length; ++i)
        {
            if (ShouldUnderscore(i, s))
            {
                builder.Append('_');
            }

            builder.Append(char.ToLowerInvariant(s[i]));
        }

        return builder.ToString();
    }

    private static bool ShouldUnderscore(int i, string s)
    {
        if (i == 0 || i >= s.Length || s[i] == '_')
            return false;

        var curr = s[i];
        var prev = s[i - 1];
        var next = i < s.Length - 2 ? s[i + 1] : '_';

        return prev != '_'
            && (
                (char.IsUpper(curr) && (char.IsLower(prev) || char.IsLower(next)))
                || (char.IsNumber(curr) && (!char.IsNumber(prev)))
            );
    }

    private static PropertyInfo ToPropertyInfo(Expression expression)
    {
        if (expression is not LambdaExpression lamda)
        {
            throw new ArgumentException($"Can not parse {expression} to LambdaExpression");
        }

        LambdaExpression lambda = lamda;

        ExpressionType expressionType = lambda.Body.NodeType;

        MemberExpression? memberExpr = expressionType switch
        {
            ExpressionType.Convert => ((UnaryExpression)lambda.Body).Operand as MemberExpression,
            ExpressionType.MemberAccess => lambda.Body as MemberExpression,
            _ => throw new Exception("Expression Type is not support"),
        };

        return (PropertyInfo)memberExpr!.Member;
    }

    private static PropertyInfo GetNestedPropertyInfo(this Type type, string propertyName)
    {
        // Split the propertyName by '.' to handle nested properties
        var propertyParts = propertyName.Trim().Split('.');

        PropertyInfo? propertyInfo = null;

        // Iterate through each part of the property chain
        foreach (var part in propertyParts)
        {
            // Attempt to find the property information for the current part
            propertyInfo =
                type.GetProperty(
                    part.Trim(),
                    BindingFlags.IgnoreCase | BindingFlags.Public | BindingFlags.Instance
                ) ?? throw new Exception($"property {part} is not found.");

            // Move to the next type in the chain (the type of the current property)
            type = IsArrayGenericType(propertyInfo)
                ? propertyInfo.PropertyType.GetGenericArguments()[0]
                : propertyInfo.PropertyType;
        }

        // Return the last found PropertyInfo (non-null due to Guard.Against)
        return propertyInfo!;
    }

    private static bool IsArrayGenericType(this PropertyInfo propertyInfo)
    {
        if (
            propertyInfo.PropertyType.IsGenericType
            && typeof(IEnumerable).IsAssignableFrom(propertyInfo.PropertyType)
            && propertyInfo.PropertyType.GetGenericArguments()[0].IsUserDefineType()
        )
        {
            return true;
        }
        return false;
    }

    private static bool IsUserDefineType(this Type? type)
    {
        if (type == null)
        {
            return false;
        }

        return type?.IsClass == true && type?.FullName?.StartsWith("System.") == false;
    }
}
