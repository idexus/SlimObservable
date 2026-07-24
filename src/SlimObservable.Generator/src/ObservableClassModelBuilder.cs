//
// MIT License
// Copyright Pawel Krzywdzinski
//

using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading;
using Microsoft.CodeAnalysis;

namespace SlimObservable.Generator
{
    /// <summary>
    /// All the semantic analysis that used to live inside ObservableClassGenerator.
    /// Runs in the pipeline's 'transform' stage and yields nothing but an ObservableClassModel.
    /// </summary>
    internal static class ObservableClassModelBuilder
    {
        public const string DefaultValueAttributeString = "DefaultValueAttribute";
        public const string PropertyCallbackAttributeString = "PropertyCallbackAttribute";
        public const string ObservablePropertiesAttributeString = "ObservablePropertiesAttribute";

        // ----- diagnostics -----

        public static readonly DiagnosticDescriptor EmptyPropertyCallback = new DiagnosticDescriptor(
            id: "SLIMOBS001",
            title: "Empty PropertyCallback attribute",
            messageFormat: "PropertyCallback attribute on '{0}' must specify a callback method name",
            category: "SlimObservable",
            DiagnosticSeverity.Error,
            isEnabledByDefault: true);

        public static readonly DiagnosticDescriptor EmptyDefaultValue = new DiagnosticDescriptor(
            id: "SLIMOBS002",
            title: "Empty DefaultValue attribute",
            messageFormat: "DefaultValue attribute on '{0}' must specify a value",
            category: "SlimObservable",
            DiagnosticSeverity.Error,
            isEnabledByDefault: true);

        public static readonly DiagnosticDescriptor UnexpectedError = new DiagnosticDescriptor(
            id: "SLIMOBS003",
            title: "SlimObservable generator error",
            messageFormat: "Error while processing '{0}': {1}",
            category: "SlimObservable",
            DiagnosticSeverity.Error,
            isEnabledByDefault: true);

        // -----------------------------------------------------------------

        public static ObservableClassModelResult Build(INamedTypeSymbol symbol, CancellationToken ct)
        {
            try
            {
                return BuildCore(symbol, ct);
            }
            catch (OperationCanceledException)
            {
                throw;
            }
            catch (Exception ex)
            {
                return new ObservableClassModelResult(
                    null,
                    new EquatableArray<DiagnosticInfo>(new[]
                    {
                        new DiagnosticInfo(
                            UnexpectedError,
                            LocationInfo.CreateFrom(symbol),
                            new EquatableArray<string>(new[] { symbol.ToDisplayString(), ex.Message }))
                    }));
            }
        }

        static ObservableClassModelResult BuildCore(INamedTypeSymbol symbol, CancellationToken ct)
        {
            ct.ThrowIfCancellationRequested();

            var diagnostics = new List<DiagnosticInfo>();

            var model = new ObservableClassModel(
                Namespace: symbol.ContainingNamespace.ToDisplayString(),
                SymbolName: symbol.ToDisplayString().Split('.').Last(),
                FileName: Helpers.GetNormalizedFileName(symbol),
                PropertyChangedExistsInBases: PropertyChangedExistsInBases(symbol),
                Properties: BuildProperties(symbol, diagnostics, ct));

            return new ObservableClassModelResult(
                model,
                new EquatableArray<DiagnosticInfo>(diagnostics.ToArray()));
        }

        // -----------------------------------------------------------------

        static bool PropertyChangedExistsInBases(INamedTypeSymbol symbol)
        {
            var existsInBases = false;

            Helpers.LoopDownToObject(symbol, type =>
            {
                existsInBases = type
                    .GetMembers()
                    .Any(e =>
                        e.Kind == SymbolKind.Event &&
                        e.DeclaredAccessibility == Accessibility.Public &&
                        e.Name.Equals("PropertyChanged", StringComparison.Ordinal));

                return existsInBases;
            });

            return existsInBases;
        }

        static EquatableArray<ObservablePropertyModel> BuildProperties(
            INamedTypeSymbol symbol, List<DiagnosticInfo> diagnostics, CancellationToken ct)
        {
            var observableInterfaces = symbol
                .Interfaces
                .Where(e => e.GetAttributes().Any(a =>
                    a.AttributeClass != null &&
                    a.AttributeClass.Name.Equals(ObservablePropertiesAttributeString, StringComparison.Ordinal)));

            var result = new List<ObservablePropertyModel>();

            foreach (var inter in observableInterfaces)
            {
                ct.ThrowIfCancellationRequested();

                var properties = inter
                    .GetMembers()
                    .Where(e => e.Kind == SymbolKind.Property)
                    .Cast<IPropertySymbol>();

                foreach (var property in properties)
                {
                    var typeName = property.Type.ToDisplayString();

                    result.Add(new ObservablePropertyModel(
                        Name: property.Name,
                        CamelCaseName: Helpers.CamelCase(property.Name),
                        TypeName: typeName,
                        CallbackName: GetPropertyCallback(property, diagnostics),
                        DefaultValueString: GetDefaultValueString(property, typeName, diagnostics)));
                }
            }

            return new EquatableArray<ObservablePropertyModel>(result.ToArray());
        }

        // -----------------------------------------------------------------

        static string GetPropertyCallback(ISymbol symbol, List<DiagnosticInfo> diagnostics)
        {
            var attributeData = symbol.GetAttributes().FirstOrDefault(e =>
                e.AttributeClass != null &&
                e.AttributeClass.Name.Equals(PropertyCallbackAttributeString, StringComparison.Ordinal));

            if (attributeData == null)
                return null;

            if (attributeData.ConstructorArguments.Length == 0 || attributeData.ConstructorArguments[0].Value == null)
            {
                diagnostics.Add(new DiagnosticInfo(
                    EmptyPropertyCallback,
                    LocationInfo.CreateFrom(symbol),
                    new EquatableArray<string>(new[] { symbol.ToDisplayString() })));

                return null;
            }

            return (string)attributeData.ConstructorArguments[0].Value;
        }

        static string GetDefaultValueString(ISymbol symbol, string typeName, List<DiagnosticInfo> diagnostics)
        {
            var attributeData = symbol.GetAttributes().FirstOrDefault(e =>
                e.AttributeClass != null &&
                e.AttributeClass.Name.Equals(DefaultValueAttributeString, StringComparison.Ordinal));

            if (attributeData == null)
                return null;

            if (attributeData.ConstructorArguments.Length == 0 || attributeData.ConstructorArguments[0].Value == null)
            {
                diagnostics.Add(new DiagnosticInfo(
                    EmptyDefaultValue,
                    LocationInfo.CreateFrom(symbol),
                    new EquatableArray<string>(new[] { symbol.ToDisplayString() })));

                return null;
            }

            var value = attributeData.ConstructorArguments[0].Value.ToString();

            if (typeName.Equals("string", StringComparison.Ordinal))
                value = $"\"{value}\"";

            if (typeName.Equals("double", StringComparison.Ordinal) ||
                typeName.Equals("float", StringComparison.Ordinal) ||
                typeName.Equals("decimal", StringComparison.Ordinal))
                value = value.Replace(",", ".");

            return value;
        }
    }
}
