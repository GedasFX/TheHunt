// using System.Diagnostics;
// using Microsoft.CodeAnalysis;
//
// namespace TheHunt.MediaR.SourceGen
// {
//     [Generator]
//     public class RequestResponseGenerator : ISourceGenerator
//     {
//         public void Initialize(GeneratorInitializationContext context)
//         {
// #if DEBUG
//             if (!Debugger.IsAttached)
//             {
//                 Debugger.Launch();
//             }
// #endif 
//             // No init  
//         }
//
//         public void Execute(GeneratorExecutionContext context)
//         {
//             throw new System.NotImplementedException();
//         }
//     }
// }