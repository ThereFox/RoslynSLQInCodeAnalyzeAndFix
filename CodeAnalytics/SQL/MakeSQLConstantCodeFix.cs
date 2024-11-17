using System.Collections.Immutable;
using Microsoft.CodeAnalysis;
using Microsoft.CodeAnalysis.CodeActions;
using Microsoft.CodeAnalysis.CodeFixes;
using Microsoft.CodeAnalysis.CSharp;
using Microsoft.CodeAnalysis.CSharp.Syntax;
using Microsoft.CodeAnalysis.Editing;

namespace CodeAnalytics;

[ExportCodeFixProvider(LanguageNames.CSharp, Name = nameof(MakeSqlConstantCodeFix))]
public class MakeSqlConstantCodeFix : CodeFixProvider
{
    public override async Task RegisterCodeFixesAsync(CodeFixContext context)
    {
        var document = context.Document;
        var diagnostic = context.Diagnostics.Single();
        
        var diagnosticSpan = diagnostic.Location.SourceSpan;

        var errorNodeRoot = await context.Document.GetSyntaxRootAsync(context.CancellationToken).ConfigureAwait(false);

        var errorNode = errorNodeRoot.FindNode(diagnosticSpan);

        if (errorNode == null || errorNode is not LiteralExpressionSyntax)
        {
            return;
        }
        
        
        var codeAction = CodeAction.Create(
            "Make SQL Constant",
            (token) => MakeSQLConstantFix(document, errorNode, token),
            "MakeSQLConstant"
            );
        
        context.RegisterCodeFix(codeAction, diagnostic);
    }

    public async Task<Solution> MakeSQLConstantFix(
        Document document, SyntaxNode warningNode,
        CancellationToken cancellationToken = default(CancellationToken)
        )
    {
        
        if (warningNode is not LiteralExpressionSyntax literalExpression)
        {
            throw new InvalidCastException("error 1");
        }

        var localDefinition = warningNode?.Parent?.Parent?.Parent;

        if (
            localDefinition == null || 
            localDefinition is not VariableDeclarationSyntax variableDeclarationSyntax
            )
        {
            throw new InvalidCastException("error 2");
        }

        if (
            variableDeclarationSyntax.Parent == null ||
            variableDeclarationSyntax.Parent is not LocalDeclarationStatementSyntax localDeclarationStatementSyntax
            )
        {
            throw new InvalidCastException("error 3");
        }
        
        
        if (localDeclarationStatementSyntax.Parent is not BlockSyntax methodBody)
        {
            throw new InvalidCastException("error 4");
        }
        
        if (methodBody.Parent is not MethodDeclarationSyntax methodDeclarationSyntax)
        {
            throw new InvalidCastException("error 5");
        }
        
        if (methodDeclarationSyntax.Parent is not ClassDeclarationSyntax classDeclarationSyntax)
        {
            throw new InvalidCastException("error 6");
        }

        if (classDeclarationSyntax == null)
        {
            throw new InvalidCastException("error 7");
        }
        
        var privateToken = SyntaxFactory.Token(SyntaxKind.PrivateKeyword);
        var readonlyToken = SyntaxFactory.Token(SyntaxKind.ConstKeyword);
        var tokensList = SyntaxFactory.TokenList([privateToken, readonlyToken]);
        
        var attributeList = SyntaxFactory.List<AttributeListSyntax>([]);

        var variableDecrarationSyntax = SyntaxFactory.VariableDeclaration(SyntaxFactory.ParseTypeName("string"),
            variableDeclarationSyntax.Variables);
        
        var constantField = SyntaxFactory.FieldDeclaration(
            attributeList, 
            tokensList, 
            variableDecrarationSyntax
            );

        
        var editorProxi = new SolutionEditor(document.Project.Solution);

        var editor = await editorProxi.GetDocumentEditorAsync(document.Id, cancellationToken);


        editor.RemoveNode(localDeclarationStatementSyntax, SyntaxRemoveOptions.KeepNoTrivia);
        editor.InsertMembers(classDeclarationSyntax, 0, [constantField]);


        return editorProxi.GetChangedSolution();
    }
    
    private readonly static string SqlInCodeDiagnosticId = "SQLInCode";

    public override ImmutableArray<string> FixableDiagnosticIds { get; } = [SqlInCodeDiagnosticId];
}