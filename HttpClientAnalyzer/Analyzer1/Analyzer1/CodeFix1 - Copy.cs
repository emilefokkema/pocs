using System;
using System.Composition;
using System.Collections.Generic;
using System.Collections.Immutable;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using Microsoft.CodeAnalysis;
using Microsoft.CodeAnalysis.CodeFixes;
using Microsoft.CodeAnalysis.CodeActions;
using Microsoft.CodeAnalysis.CSharp;
using Microsoft.CodeAnalysis.CSharp.Syntax;
using Microsoft.CodeAnalysis.Rename;
using Microsoft.CodeAnalysis.Text;
using Microsoft.CodeAnalysis.Formatting;
using static Microsoft.CodeAnalysis.CSharp.SyntaxFactory;
namespace Analyzer1
{
    [ExportCodeFixProvider(LanguageNames.CSharp, Name = nameof(CodeFix2)), Shared]
    public class CodeFix2 : CodeFixProvider
    {
        // TODO: Replace with actual diagnostic id that should trigger this fix.
        public const string DiagnosticId = "EnforceSingletonHttpClientInstance";
        private const string title = @"¯\_(ツ)_/¯ Convert to static";

        public sealed override ImmutableArray<string> FixableDiagnosticIds
        {
            get
            {
                return ImmutableArray.Create(DiagnosticId);
            }
        }

        public sealed override FixAllProvider GetFixAllProvider()
        {
            return WellKnownFixAllProviders.BatchFixer;
        }

        public sealed override async Task RegisterCodeFixesAsync(CodeFixContext context)
        {
            var root =
              await context.Document.GetSyntaxRootAsync(context.CancellationToken)
              .ConfigureAwait(false);

            var diagnostic = context.Diagnostics.First();
            var diagnosticSpan = diagnostic.Location.SourceSpan;

            // Find the invocation expression identified by the diagnostic.
            var invocationExpr =
              root.FindToken(diagnosticSpan.Start).Parent.AncestorsAndSelf()
              .OfType<ObjectCreationExpressionSyntax>().First();

            // Register a code action that will invoke the fix.
            context.RegisterCodeFix(
              CodeAction.Create(title, c =>
              FixRegexAsync(context.Document, invocationExpr, c), equivalenceKey: title), diagnostic);
        }

        private async Task<Document> FixRegexAsync(Document document,
          ObjectCreationExpressionSyntax syntax,
          CancellationToken cancellationToken)
        {
            try
            {
                var originalSyntax = syntax.Parent.Parent.Parent.Parent;
                var pre = originalSyntax.GetLeadingTrivia().ToString();
                var newExpression = originalSyntax.ToFullString().Replace("private","private static");
                var newSyntax = CSharpSyntaxTree.ParseText($"class A {{{newExpression}}}");
                var aa = newSyntax.GetRoot().DescendantNodes().OfType<FieldDeclarationSyntax>().First();
                //var aa = newSyntax.
                var root = await document.GetSyntaxRootAsync();
               


                var newRoot = root.ReplaceNode(originalSyntax, aa).NormalizeWhitespace();
               
                var newDocument = document.WithSyntaxRoot(newRoot);

                return newDocument;
            }
            catch 
            {
                return document;
                
            }
        
        }
    }
}
