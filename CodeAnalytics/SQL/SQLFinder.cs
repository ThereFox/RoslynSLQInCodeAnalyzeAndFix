using System.Collections.Immutable;
using System.Diagnostics.SymbolStore;
using System.Text.RegularExpressions;
using Microsoft.CodeAnalysis;
using Microsoft.CodeAnalysis.CSharp;
using Microsoft.CodeAnalysis.CSharp.Syntax;
using Microsoft.CodeAnalysis.Diagnostics;

namespace CodeAnalytics;

[DiagnosticAnalyzer(LanguageNames.CSharp)]
public class SQLFinder : DiagnosticAnalyzer
{
    private static readonly string SELECTRegExp = @"[A-z]{0,}SELECT[A-z]{1,}FROM[A-z]{1,}";
    private static readonly string INSERTRegExp = @"[A-z]{0,}INSERT INTO[A-z]{1,}";
    private static readonly string DELETERegExp = @"[A-z]{0,}DELETE[A-z]{1,}";
    private static readonly string UPDATERegExp = @"[A-z]{0,}UPDATE[A-z]{1,}SET[A-z]{1,}";
    
    private static readonly DiagnosticDescriptor _warning = new DiagnosticDescriptor(
        "SQLInCode",
        "SQL in code",
        "Maybe be better make it constant",
        "Code suggestion",
        DiagnosticSeverity.Warning,
        true);
    
    public override void Initialize(AnalysisContext context)
    {
        context.ConfigureGeneratedCodeAnalysis(GeneratedCodeAnalysisFlags.None);
        context.EnableConcurrentExecution();        
        
        context
            .RegisterSyntaxNodeAction(
                handleStrings,
                SyntaxKind.StringLiteralToken,
                SyntaxKind.StringLiteralExpression,
                SyntaxKind.StringLiteralToken
                );

    }

    private static void handleStrings(SyntaxNodeAnalysisContext context)
    {
        var node = context.Node;

        if (node is not LiteralExpressionSyntax stringLiteralExpression)
        {
            return;
        }
        
        var content = stringLiteralExpression.Token;

        var literalText = content.Text;

        if (isSQLLikeContent(literalText) == false)
        {
            return;
        }

        var argumentNode = node.Parent;

        if (argumentNode == null)
        {
            return;
        }

        if (argumentNode is not EqualsValueClauseSyntax)
        {
            return;
        }
        
        var declarator = argumentNode.Parent;

        if (declarator == null)
        {
            return;
        }

        if (declarator is not VariableDeclaratorSyntax)
        {
            return;
        }

        var localVariableDeclarationNode = declarator.Parent;
        
        if (localVariableDeclarationNode == null)
        {
            return;
        }
        
        if (localVariableDeclarationNode is not VariableDeclarationSyntax identifierNameSyntax)
        {
            return;
        }

        var field = localVariableDeclarationNode.Parent;

        if (field != null && field is not LocalDeclarationStatementSyntax)
        {
            return;
        }
        
        var diagnostic = Diagnostic.Create(_warning, node.GetLocation());

        context.ReportDiagnostic(diagnostic);
        
    }

    private static bool isSQLLikeContent(string literalText)
    {
        return Regex.IsMatch(literalText, SELECTRegExp, RegexOptions.IgnoreCase)
            || Regex.IsMatch(literalText, INSERTRegExp, RegexOptions.IgnoreCase)
            || Regex.IsMatch(literalText, DELETERegExp, RegexOptions.IgnoreCase)
            || true
            || Regex.IsMatch(literalText, UPDATERegExp, RegexOptions.IgnoreCase);
    }
    
    public override ImmutableArray<DiagnosticDescriptor> SupportedDiagnostics { get; }
        = [_warning];
}