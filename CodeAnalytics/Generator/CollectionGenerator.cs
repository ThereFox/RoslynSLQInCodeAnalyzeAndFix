using System.Collections.Immutable;
using System.Text;
using Microsoft.CodeAnalysis;
using Microsoft.CodeAnalysis.CSharp;
using Microsoft.CodeAnalysis.CSharp.Syntax;

namespace CodeAnalytics.Generator;

[Generator]
public class CollectionGenerator : IIncrementalGenerator
{
    public void Initialize(IncrementalGeneratorInitializationContext context)
    {
        var collectableClass = context.SyntaxProvider.CreateSyntaxProvider(
            isCollectableClass, (ex, token) => (ClassDeclarationSyntax)ex.Node 
            )
            .Where(ex => ex is not null)
            .Collect();
        
        context.RegisterSourceOutput(collectableClass, generateCollectionClasses);
        
    }

    private void generateCollectionClasses(SourceProductionContext context, ImmutableArray<ClassDeclarationSyntax> syntax)
    {
        foreach (var element in syntax)
        {
            generateCollectionClass(context, element);
        }
    }

    private void generateCollectionClass(SourceProductionContext context, ClassDeclarationSyntax syntax)
    {
        var className = syntax.Identifier.ValueText;

        var whiteSpaceTrivia = SyntaxFactory.SyntaxTrivia(SyntaxKind.WhitespaceTrivia, " ");
        var newLineTrivia = SyntaxFactory.SyntaxTrivia(SyntaxKind.EndOfLineTrivia, "\n");
        
        var generatedAttribute = SyntaxFactory.Attribute(
            SyntaxFactory.ParseName("System.CodeDom.Compiler.GeneratedCodeAttribute")
            );
        
        var attributeList = SyntaxFactory.AttributeList(
            SyntaxFactory.SingletonSeparatedList(generatedAttribute)
            ).WithTrailingTrivia(newLineTrivia);
        var attributes = SyntaxFactory.List<AttributeListSyntax>([attributeList]);

        var internalModifier = SyntaxFactory.Token(SyntaxKind.InternalKeyword).WithTrailingTrivia(whiteSpaceTrivia);
        var internalModifierList = SyntaxFactory.TokenList([internalModifier]);
        var collectionClassName = SyntaxFactory.ParseToken($"{className}Collection")
            .WithLeadingTrivia(whiteSpaceTrivia)
            .WithTrailingTrivia(whiteSpaceTrivia);

        var constrainClaus = SyntaxFactory.List<TypeParameterConstraintClauseSyntax>();
        
        var emptyMembers = SyntaxFactory.List<MemberDeclarationSyntax>();

        var baseClass = SyntaxFactory.ParseTypeName($"List<{className}>",0, true);
        var baseClassType = (BaseTypeSyntax)SyntaxFactory.SimpleBaseType(baseClass).WithLeadingTrivia(whiteSpaceTrivia);
        var separetedBaseType = SyntaxFactory.SeparatedList([baseClassType]);
        var baseClassList = SyntaxFactory.BaseList(separetedBaseType).WithTrailingTrivia(newLineTrivia);
        
        var listClass = SyntaxFactory.ClassDeclaration(
            attributes, 
            internalModifierList, 
            collectionClassName, 
            null, 
            baseClassList, 
            constrainClaus,
            emptyMembers
            );
        
        var classDeclarationText = listClass.GetText(Encoding.Default);
        
        context.AddSource($"{className}Collection.cs", classDeclarationText);
    }
    
    private bool isCollectableClass(SyntaxNode node, CancellationToken token)
    {
        if (node is not ClassDeclarationSyntax classDeclarationSyntax)
        {
            return false;
        }

        var attributesList = classDeclarationSyntax.AttributeLists;

        if (attributesList.Count == 0)
        {
            return false;
        }
        
        return attributesList.Any(
            ex => ex.Attributes.Any(
                subEx => subEx.Name.ToString() == "Collectable"
            )
        );
    }
}