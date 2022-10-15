﻿using System.Collections.Generic;
using System.Linq;
using System.Text;
using Microsoft.CodeAnalysis;
using Microsoft.CodeAnalysis.CSharp.Syntax;

namespace TheHunt.MediaR.SourceGen
{
    [Generator]
    public class RpcMediatorSourceGenerator : ISourceGenerator
    {
        public void Execute(GeneratorExecutionContext context)
        {
            if (context.SyntaxReceiver is not RpcServiceReceiver syntaxReceiver)
                return;

            var set = new HashSet<(string Namespace, string Request, string Result)>();
            foreach (var service in syntaxReceiver.Services)
            {
                foreach (var method in service.Members.Where(m => m is MethodDeclarationSyntax).Cast<MethodDeclarationSyntax>())
                {
                    var requestType = (QualifiedNameSyntax)method.ParameterList.Parameters[0].Type!;
                    var requestNamespace = requestType.Left.ToString().Substring(8);
                    var requestClassType = requestType.Right.ToString();

                    var resultType = ((GenericNameSyntax)((QualifiedNameSyntax)method.ReturnType).Right).TypeArgumentList.Arguments[0].ToString();

                    set.Add((requestNamespace, requestClassType, resultType));
                }
            }

            foreach (var namespaceGroup in set.GroupBy(s => s.Namespace))
            {
                var sb = new StringBuilder();
                foreach (var (_, requestClass, resultType) in namespaceGroup)
                    sb.Append($"    public partial class {requestClass} : MediatR.IRequest<{resultType}> {{ }}\n");

                var source = @$"// <auto-generated />
namespace {namespaceGroup.Key}
{{
{sb}
}}";
                context.AddSource($"{namespaceGroup.Key.Replace('.', '_')}.g.cs", source);
            }
        }

        public void Initialize(GeneratorInitializationContext context)
        {
            context.RegisterForSyntaxNotifications(() => new RpcServiceReceiver());
        }
    }


    public class RpcServiceReceiver : ISyntaxReceiver
    {
        public IList<ClassDeclarationSyntax> Services { get; } = new List<ClassDeclarationSyntax>();

        public void OnVisitSyntaxNode(SyntaxNode syntaxNode)
        {
            if (syntaxNode is not AliasQualifiedNameSyntax { Alias.Identifier.Text: "grpc", Name.Identifier.Text: "BindServiceMethod" }) return;

            var parent = syntaxNode.Parent;
            while (parent is not ClassDeclarationSyntax and not null)
            {
                parent = parent.Parent;
            }

            if (parent != null)
                Services.Add((ClassDeclarationSyntax)parent);
        }
    }
}