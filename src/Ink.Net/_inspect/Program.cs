using System;
using System.Linq;
using System.Reflection;

var asm = typeof(Facebook.Yoga.YGNodeAPI).Assembly;
var types = asm.GetExportedTypes().OrderBy(t => t.FullName);

foreach (var t in types)
{
    var kind = t.IsClass ? "class" : t.IsEnum ? "enum" : t.IsValueType ? "struct" : t.IsInterface ? "interface" : "other";
    Console.WriteLine($"\n=== {t.FullName} ({kind}) ===");
    
    if (t.IsEnum)
    {
        foreach (var v in Enum.GetNames(t))
            Console.WriteLine($"  {v}");
    }
    else
    {
        foreach (var m in t.GetMethods(BindingFlags.Public | BindingFlags.Static | BindingFlags.Instance | BindingFlags.DeclaredOnly))
        {
            var ps = string.Join(", ", m.GetParameters().Select(p => $"{p.ParameterType.Name} {p.Name}"));
            Console.WriteLine($"  {m.ReturnType.Name} {m.Name}({ps})");
        }
        foreach (var p in t.GetProperties(BindingFlags.Public | BindingFlags.Instance | BindingFlags.DeclaredOnly))
        {
            Console.WriteLine($"  prop {p.PropertyType.Name} {p.Name}");
        }
        foreach (var f in t.GetFields(BindingFlags.Public | BindingFlags.Instance | BindingFlags.DeclaredOnly))
        {
            Console.WriteLine($"  field {f.FieldType.Name} {f.Name}");
        }
    }
}
