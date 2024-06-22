using Bicep.Core.Diagnostics;
using Bicep.Core.Semantics;
using Bicep.Core.Syntax;

namespace Bicep.Core.TypeSystem.Types
{
    public class Decorator : TypeSymbol
    {
        public Decorator(string name, FunctionOverload functionOverload, SyntaxBase syntax, ISemanticModel semanticModel)
            : base(name)
        {
            this.FunctionOverload = functionOverload;
            this.Syntax = syntax;
            this.SemanticModel = semanticModel;
        }

        public FunctionOverload FunctionOverload { get; }

        public SyntaxBase Syntax { get; }

        public ISemanticModel SemanticModel { get; }

        public override void Accept(SymbolVisitor visitor)
        {
            visitor.VisitDecorator(this);
        }

        public override SymbolKind Kind => SymbolKind.Decorator;

        // Emit a warning diagnostic if the decorator is marked as deprecated.
        public void EmitDeprecatedWarningIfNecessary(IDiagnosticWriter diagnosticWriter)
        {
            if (this.FunctionOverload.IsDeprecated && this.Syntax is DecoratorSyntax decoratorSyntax)
            {
                var message = this.FunctionOverload.DeprecationMessage ?? $"The decorator '{this.Name}' is deprecated.";
                diagnosticWriter.Write(decoratorSyntax, DiagnosticBuilder.ForPosition(decoratorSyntax).DeprecatedSymbol(message));
            }
        }
    }
}
