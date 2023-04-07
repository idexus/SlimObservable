//
// MIT License
// Copyright Pawel Krzywdzinski
//

using System.Collections.Generic;
using System.Linq;
using Microsoft.CodeAnalysis;

namespace SlimObservable.Generator
{
    [Generator]
    public class Builder : ISourceGenerator
    {
        public const string ObservableObjectAttributeString = "ObservableObjectAttribute";

        public void Initialize(GeneratorInitializationContext context) { }

        public void Execute(GeneratorExecutionContext context)
        {
            //Helpers.WaitForDebugger(context.CancellationToken);

            var observableObjectSymbols = context.Compilation.GetSymbolsWithName((s) => true, filter: SymbolFilter.Type)
                .Where(e => !e.IsStatic && e.GetAttributes().FirstOrDefault(e => e.AttributeClass.Name.Equals(ObservableObjectAttributeString)) != null)
                .Select(e => e as INamedTypeSymbol);

            foreach (var symbol in observableObjectSymbols)
                new ObservableClassGenerator(context, symbol).Build();
        }
    }
}