using System.Collections.Immutable;
using System.Text;
using Microsoft.CodeAnalysis;
using Microsoft.CodeAnalysis.CSharp;
using Microsoft.CodeAnalysis.CSharp.Syntax;
using Microsoft.CodeAnalysis.Rename;
using Microsoft.CodeAnalysis.Text;

namespace CodeAnalytics.Generator;

[Generator]
public class SimpleInrementalGenerator : IIncrementalGenerator
{
    public void Initialize(IncrementalGeneratorInitializationContext context)
    {
        var emptyClasses = context.SyntaxProvider
            .CreateSyntaxProvider(
                EmptyClassSyntaxSelector, ClassSemanticSelector
                )
            .Where(ex => ex is not null)
            .Collect();
     
        context.RegisterSourceOutput(emptyClasses, generator);
        
    }

    private static void generator(
        SourceProductionContext context,
        ImmutableArray<ClassDeclarationSyntax> sources)
    {
        if (sources.Any() == false)
        {
            return;
        }
        
        foreach (var emptyClass in sources)
        {
            var NameToken = SyntaxFactory.ParseToken(" NameOfClass");
            var tokens = SyntaxFactory.ParseTokens("public const ");
            var tokensList = SyntaxFactory.TokenList(tokens);
        
            var attributeList = SyntaxFactory.List<AttributeListSyntax>(null);

            var variable = SyntaxFactory.VariableDeclarator(NameToken);
            var variableList = SyntaxFactory
                .SeparatedList<VariableDeclaratorSyntax>(
                    new VariableDeclaratorSyntax[]{ variable });
            
            var variableDecrarationSyntax = SyntaxFactory
                .VariableDeclaration(
                    SyntaxFactory.ParseTypeName("string"),
                    variableList);
        
            var constantField = SyntaxFactory.FieldDeclaration(
                attributeList, 
                tokensList, 
                variableDecrarationSyntax
            );
            
            var updatedClass = emptyClass.AddMembers(constantField);

            var newIdentifier = SyntaxFactory.Identifier(
                $"Generated_{updatedClass.Identifier.ValueText}"
                );
            
            var classWithNewName = SyntaxFactory.ClassDeclaration(
                updatedClass.AttributeLists,
                updatedClass.Modifiers,
                updatedClass.Keyword,
                newIdentifier,
                updatedClass.TypeParameterList,
                updatedClass.BaseList,
                updatedClass.ConstraintClauses,
                updatedClass.OpenBraceToken,
                updatedClass.Members,
                updatedClass.CloseBraceToken,
                updatedClass.SemicolonToken
            );
            
            var sourceCode = classWithNewName.GetText(Encoding.UTF8);
            
            context.AddSource(
                $"Test.{updatedClass.Identifier.ValueText}.cs",
                sourceCode
            );
        }
        
    }
    
    private bool EmptyClassSyntaxSelector(SyntaxNode node, CancellationToken cancellationToken)
    {
        if (node is not ClassDeclarationSyntax classDeclarationSyntax)
        {
            return false;
        }

        if (classDeclarationSyntax.DescendantNodes().Count() > 0)
        {
            return false;
        }

        return true;
    }

    private ClassDeclarationSyntax ClassSemanticSelector(GeneratorSyntaxContext context,
        CancellationToken cancellationToken)
    {
        var node = context.Node;

        if (node is not ClassDeclarationSyntax classDeclarationSyntax)
        {
            throw new Exception("Error");
        }
        
        return classDeclarationSyntax;
        
    }
}