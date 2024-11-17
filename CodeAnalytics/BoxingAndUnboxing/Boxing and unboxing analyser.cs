using System.Collections.Immutable;
using Microsoft.CodeAnalysis;
using Microsoft.CodeAnalysis.CSharp;
using Microsoft.CodeAnalysis.CSharp.Syntax;
using Microsoft.CodeAnalysis.Diagnostics;
using Exception = System.Exception;

namespace CodeAnalytics.BoxingAndUnboxing;

[DiagnosticAnalyzer(LanguageNames.CSharp)]
public class BoxingAndUnboxingAnalyser : DiagnosticAnalyzer
{
    private static readonly DiagnosticDescriptor boxingWarning = new(
        "BoxingAndUnboxing","Variable boxing was found", "Variable boxing was found",
        "Boxing and unboxing", DiagnosticSeverity.Warning, true
        );
    
    private static readonly DiagnosticDescriptor boxedValueWarning = new(
        "BoxingAndUnboxing","In this used boxed variable", "Variable boxing was found",
        "Boxing and unboxing", DiagnosticSeverity.Warning, true
    );
    
    public override void Initialize(AnalysisContext context)
    {
        context.EnableConcurrentExecution();
        context.ConfigureGeneratedCodeAnalysis(GeneratedCodeAnalysisFlags.None);
        
        context.RegisterSemanticModelAction(handleAssingment);
        context.RegisterSemanticModelAction(handleDeclaration);
    }

    private void handleAssingment(SemanticModelAnalysisContext context)
    {
        var model = context.SemanticModel;
        var treeRoot = context.FilterTree.GetRoot();
        var equals = treeRoot
            .DescendantNodes()
            .OfType<EqualsValueClauseSyntax>();

        var assingments = treeRoot.DescendantNodes()
            .OfType<AssignmentExpressionSyntax>();
        
        
        foreach (var assignment in assingments)
        {
            var variable = assignment.Left;
            var value = assignment.Right;

            var variableType = context.SemanticModel.GetTypeInfo(variable).Type;
            var valueType = context.SemanticModel.GetTypeInfo(value).Type;

            if (variableType.IsReferenceType && valueType.IsValueType)
            {
                context
                    .ReportDiagnostic(
                        Diagnostic.Create(
                            boxingWarning,
                            assignment.GetLocation()
                        )
                    );
            }
            else if (variableType.IsValueType && valueType.IsValueType)
            {
                context
                    .ReportDiagnostic(
                        Diagnostic.Create(
                            boxedValueWarning,
                            treeRoot.GetLocation()
                        )
                    );    
            }
            
                
            
            
        }
    }
    
    private void handleDeclaration(SemanticModelAnalysisContext context)
    {
        var treeRoot = context.FilterTree.GetRoot();

        var declarations = 
            treeRoot
                .DescendantNodes()
                .OfType<VariableDeclarationSyntax>();

        foreach (var declaration in declarations)
        {

            var declaretedType = declaration.Type;

            if (declaretedType.IsVar)
            {
                continue;
            }
            
            var variableType = context.SemanticModel.GetSymbolInfo(declaretedType).Symbol as ITypeSymbol;

            foreach (var variable in declaration.Variables)
            {
                if (variable.Initializer == null)
                {
                    continue;
                }
                
                var valueType = context.SemanticModel.GetTypeInfo(variable.Initializer.Value).Type as ITypeSymbol;
                
                if (variableType.IsReferenceType && valueType.IsValueType)
                {
                    context
                        .ReportDiagnostic(
                            Diagnostic.Create(
                                boxingWarning,
                                declaration.GetLocation()
                            )
                        );
                }
                else if (variableType.IsValueType && valueType.IsValueType)
                {
                    context
                        .ReportDiagnostic(
                            Diagnostic.Create(
                                boxedValueWarning,
                                declaration.GetLocation()
                            )
                        );    
                }
                
            }
            
        }
        
    }
    
    private static readonly DiagnosticDescriptor debug =
        new DiagnosticDescriptor(
            "Test",
            "Test",
            "{0}",
            "Test",
            DiagnosticSeverity.Warning, true);
    
    public override ImmutableArray<DiagnosticDescriptor> SupportedDiagnostics { get; }
        = [boxingWarning, debug, boxedValueWarning];
}