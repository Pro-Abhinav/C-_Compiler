using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using Compiler.CodeAnalysis;
using Compiler.CodeAnalysis.Binding;
using Compiler.CodeAnalysis.Syntax;
using Compiler.CodeAnalysis.Text;

namespace Compiler
{
    internal static class Program {
        private static void Main () {

            var showTree = false;
            var variables = new Dictionary<VariableSymbol , object>();
            var textBuilder = new StringBuilder();
            Console.WriteLine("Commands:");
            Console.WriteLine("#cls: To clear the screen");
            Console.WriteLine("#showParseTree: To show/hide Parse Trees\n");

            while (true) 
            {
                if (textBuilder.Length == 0)
                    Console.Write ("-> ");
                else
                    Console.Write ("| ");

                var input = Console.ReadLine ();
                var isBlank = string.IsNullOrWhiteSpace (input);  
                
                if(textBuilder.Length == 0)
                {
                    if(isBlank)
                    {
                        break;
                    }
                    else if(input == "#showParseTree")
                    {
                        showTree = !showTree;
                        Console.WriteLine(showTree ? "Showing Parse Trees" : "Not showing Parse trees");
                        continue;
                    }
                    else if(input  == "#cls")
                    {
                        Console.Clear();
                        continue;
                    }
                }

                textBuilder.AppendLine(input);
                var text = textBuilder.ToString();

                var syntaxTree = SyntaxTree.Parse(text);

                if (!isBlank && syntaxTree.Diagnostics.Any())
                    continue;

                var compilation = new Compilation(syntaxTree);
                var result = compilation.Evaluate(variables);

                var diagnostics = result.Diagnostics;
                
                if(showTree)
                {
                    Console.ForegroundColor = ConsoleColor.DarkGray; 
                    syntaxTree.Root.WriteTo(Console.Out);
                    Console.ResetColor();
                }

                if (!diagnostics.Any())
                {
                    Console.WriteLine(result.Value);
                }
                else
                {
                    foreach (var diagnostic in diagnostics)
                    {
                        var lineIndex = syntaxTree.Text.GetLineIndex(diagnostic.Span.Start);
                        var line = syntaxTree.Text.Lines[lineIndex];                        
                        var lineNumber = lineIndex +1;
                        var character = diagnostic.Span.Start - line.Start + 1;

                        Console.WriteLine();
                        
                        Console.ForegroundColor = ConsoleColor.DarkRed;
                        Console.Write($"({lineNumber}, {character}): ");
                        Console.WriteLine(diagnostic);
                        Console.ResetColor();                        
                        
                        var prefixSpan = TextSpan.FromBounds(line.Start,diagnostic.Span.Start);
                        var suffixSpan = TextSpan.FromBounds(diagnostic.Span.End, line.End);                        
                        
                        var prefix = syntaxTree.Text.ToString(prefixSpan);
                        var error = syntaxTree.Text.ToString(diagnostic.Span);
                        var suffix = syntaxTree.Text.ToString(suffixSpan);
                        
                        Console.Write("    ");
                        Console.Write(prefix);
                        
                        Console.ForegroundColor = ConsoleColor.DarkRed;
                        Console.WriteLine(error);
                        Console.ResetColor();
                        
                        Console.Write(suffix);
                        
                        Console.WriteLine();
                    }
                    
                    Console.WriteLine();
                }

                textBuilder.Clear();
            }
        }
    }
} 