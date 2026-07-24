//
// MIT License
// Copyright Pawel Krzywdzinski
//

using System.Linq;
using Microsoft.CodeAnalysis;
using Microsoft.CodeAnalysis.Text;

namespace SlimObservable.Generator
{
    // ---------------------------------------------------------------------
    // Models - ONLY plain, immutable data.
    // No ISymbol / SyntaxNode / Compilation here: they hold a reference
    // to the compilation and destroy the pipeline cache.
    // ---------------------------------------------------------------------

    internal sealed record ObservablePropertyModel(
        string Name,                    // propertySymbol.Name
        string CamelCaseName,           // Helpers.CamelCase(Name)
        string TypeName,                // propertySymbol.Type.ToDisplayString()
        string CallbackName,            // null when no [PropertyCallback]
        string DefaultValueString);     // null when no [DefaultValue]

    internal sealed record ObservableClassModel(
        string Namespace,               // mainSymbol.ContainingNamespace.ToDisplayString()
        string SymbolName,              // symbol.ToDisplayString().Split('.').Last()
        string FileName,                // Helpers.GetNormalizedFileName(symbol)
        bool PropertyChangedExistsInBases,
        EquatableArray<ObservablePropertyModel> Properties);

    // ---------------------------------------------------------------------
    // Diagnostics - Location holds a SyntaxTree, so only the raw coordinates
    // are stored and the Location is rebuilt inside RegisterSourceOutput.
    // ---------------------------------------------------------------------

    internal sealed record LocationInfo(
        string FilePath,
        TextSpan TextSpan,
        LinePositionSpan LineSpan)
    {
        public Location ToLocation() => Location.Create(FilePath, TextSpan, LineSpan);

        public static LocationInfo CreateFrom(ISymbol symbol)
        {
            var location = symbol.Locations.FirstOrDefault(e => e.IsInSource);
            if (location == null) return null;

            var span = location.GetLineSpan();
            return new LocationInfo(span.Path, location.SourceSpan, span.Span);
        }
    }

    internal sealed record DiagnosticInfo(
        DiagnosticDescriptor Descriptor,
        LocationInfo Location,
        EquatableArray<string> MessageArgs)
    {
        public Diagnostic ToDiagnostic() => Diagnostic.Create(
            Descriptor,
            Location?.ToLocation() ?? Microsoft.CodeAnalysis.Location.None,
            MessageArgs.Cast<object>().ToArray());
    }

    /// <summary>
    /// Result of analysing a single class: the model to generate (or null) plus diagnostics.
    /// </summary>
    internal sealed record ObservableClassModelResult(
        ObservableClassModel Model,
        EquatableArray<DiagnosticInfo> Diagnostics);
}
