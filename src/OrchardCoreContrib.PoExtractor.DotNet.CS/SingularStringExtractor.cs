using Microsoft.CodeAnalysis;
using Microsoft.CodeAnalysis.CSharp;
using Microsoft.CodeAnalysis.CSharp.Syntax;
using System;
using System.Linq;

namespace OrchardCoreContrib.PoExtractor.DotNet.CS;

/// <summary>
/// Extracts <see cref="LocalizableStringOccurence"/> with the singular text from the C# AST node
/// </summary>
/// <remarks>
/// The localizable string is identified by the name convention - T["TEXT TO TRANSLATE"]
/// </remarks>
/// <remarks>
/// Creates a new instance of a <see cref="SingularStringExtractor"/>.
/// </remarks>
/// <param name="metadataProvider">The <see cref="IMetadataProvider{TNode}"/>.</param>
public class SingularStringExtractor(IMetadataProvider<SyntaxNode> metadataProvider) : LocalizableStringExtractor<SyntaxNode>(metadataProvider)
{

    /// <inheritdoc/>
    public override bool TryExtract(SyntaxNode node, out LocalizableStringOccurence result)
    {
        ArgumentNullException.ThrowIfNull(node);

        result = null;

        if (node is ElementAccessExpressionSyntax accessor &&
            accessor.Expression is IdentifierNameSyntax identifierName &&
            LocalizerAccessors.LocalizerIdentifiers.Contains(identifierName.Identifier.Text) &&
            accessor.ArgumentList != null)
        {

            var argument = accessor.ArgumentList.Arguments.FirstOrDefault();
            if (argument != null)
            {
                if (TryEvaluateStringExpression(argument.Expression, out var text))
                {
                    result = CreateLocalizedString(text, null, node);
                    return true;
                }

                if (argument.Expression is LiteralExpressionSyntax literal && literal.IsKind(SyntaxKind.StringLiteralExpression))
                {
                    result = CreateLocalizedString(literal.Token.ValueText, null, node);
                    return true;
                }
            }
        }

        return false;
    }

    private static bool TryEvaluateStringExpression(ExpressionSyntax expr, out string value)
    {
        value = null;
        if (expr == null) return false;

        // String literal
        if (expr is LiteralExpressionSyntax lit && lit.IsKind(SyntaxKind.StringLiteralExpression))
        {
            value = lit.Token.ValueText;
            return true;
        }

        // Parenthesized expression: evaluate inner
        if (expr is ParenthesizedExpressionSyntax par)
        {
            return TryEvaluateStringExpression(par.Expression, out value);
        }

        // Binary concatenation: left + right
        if (expr is BinaryExpressionSyntax bin && bin.IsKind(SyntaxKind.AddExpression))
        {
            if (TryEvaluateStringExpression(bin.Left, out var left) && TryEvaluateStringExpression(bin.Right, out var right))
            {
                value = left + right;
                return true;
            }

            return false;
        }

        // Interpolated string with no interpolations
        if (expr is InterpolatedStringExpressionSyntax interpolated)
        {
            // Accept only when there are no interpolation expressions
            if (!interpolated.Contents.OfType<InterpolationSyntax>().Any())
            {
                var parts = interpolated.Contents.OfType<InterpolatedStringTextSyntax>().Select(p => p.TextToken.ValueText);
                value = string.Concat(parts);
                return true;
            }
        }

        return false;
    }
}
