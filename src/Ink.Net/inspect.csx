using System;
using System.Reflection;
using System.Linq;

var asm = Assembly.LoadFrom(@"C:\Users\Administrator\.nuget\packages\yoga.net\3.2.1\lib\net8.0\Yoga.Net.dll");
var types = asm.GetExportedTypes().OrderBy(t => t.FullName);
foreach (var t in types) {
    Console.WriteLine($"{t.FullName} ({(t.IsClass ? "class" : t.IsEnum ? "enum" : t.IsValueType ? "struct" : t.IsInterface ? "interface" : "other")})");
}
