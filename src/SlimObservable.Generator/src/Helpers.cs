//
// MIT License
// Copyright Pawel Krzywdzinski
//

using System;
using Microsoft.CodeAnalysis;
using System.Collections.Generic;
using System.Linq;
using System.Diagnostics;
using System.Threading;
using System.Threading.Tasks;

namespace SlimObservable.Generator
{
    public static class Helpers
    {
        static readonly string[] keywords = { "class", "switch", "event" };

        public static void WaitForDebugger(CancellationToken cancellationToken)
        {
#if DEBUG
            if (!Debugger.IsAttached)
            {
                Debugger.Launch();
            }
#endif
        }

        public static string CamelCase(string str)
        {
            var camelCaseName = $"{str.Substring(0, 1).ToLower()}{str.Substring(1)}";
            if (keywords.Contains(camelCaseName)) camelCaseName = $"@{camelCaseName}";
            return camelCaseName;
        }

        public static string GetNormalizedFileName(INamedTypeSymbol type)
        {
            var tail = type.IsGenericType ? $".{type.TypeArguments.FirstOrDefault().Name}" : "";
            return $"{type.Name}{tail}";
        }

        public static string GetNormalizedClassName(INamedTypeSymbol type)
        {
            var tail = type.IsGenericType ? $"Of{type.TypeArguments.FirstOrDefault().Name}" : "";
            return $"{type.Name}{tail}";
        }
    }
}

