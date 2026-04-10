// -----------------------------------------------------------------------
// <copyright file="SyntaxFactsExtensions.cs" company="Altemiq">
// Copyright (c) Altemiq. All rights reserved.
// </copyright>
// -----------------------------------------------------------------------

namespace Altemiq.Patterns.Builder.Generators;

/// <summary>
/// The <see cref="SyntaxFacts"/> extensions.
/// </summary>
public static class SyntaxFactsExtensions
{
    extension(SyntaxFacts)
    {
        /// <summary>
        /// Escapes the text if it is a keyword.
        /// </summary>
        /// <param name="text">The text to test.</param>
        /// <returns>The escaped text if <paramref name="text"/> represents a keyword; otherwise <paramref name="text"/>.</returns>
        public static string EscapeKeyword(string text)
        {
            if (IsKeyword(text))
            {
                // prepend with an '@' symbol
                return "@" + text;
            }

            return text;

            static bool IsKeyword(string text)
            {
                if (string.IsNullOrWhiteSpace(text))
                {
                    return false;
                }

                return

                    // Check if it's a reserved keyword
                    SyntaxFacts.GetKeywordKind(text) is not SyntaxKind.None

                    // Check if it's a contextual keyword
                    || SyntaxFacts.GetContextualKeywordKind(text) is not SyntaxKind.None;
            }
        }
    }
}