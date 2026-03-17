// -----------------------------------------------------------------------
// <copyright file="XDocumentExtensions.cs" company="Altemiq">
// Copyright (c) Altemiq. All rights reserved.
// </copyright>
// -----------------------------------------------------------------------

namespace Altemiq.Patterns.Builder.Generator;

using System.Xml.Linq;
using Microsoft.CodeAnalysis.CSharp;
using Microsoft.CodeAnalysis.CSharp.Syntax;
using static Microsoft.CodeAnalysis.CSharp.SyntaxFactory;

/// <summary>
/// The <see cref="System.Xml.Linq"/> extensions.
/// </summary>
public static class XDocumentExtensions
{
    /// <summary>
    /// Converts the document in to <see cref="XmlNodeSyntax"/> elements.
    /// </summary>
    /// <param name="document">The document.</param>
    /// <returns>The transformed nodes.</returns>
    public static IEnumerable<XmlNodeSyntax> ToXmlNodes(this XDocument document) => document.Root?.ToXmlNodes() ?? [];

    /// <summary>
    /// Converts the container in to <see cref="XmlNodeSyntax"/> elements.
    /// </summary>
    /// <param name="container">The container.</param>
    /// <returns>The transformed nodes.</returns>
    public static IEnumerable<XmlNodeSyntax> ToXmlNodes(this XContainer container) => container.Nodes().ToXmlNodes();

    /// <summary>
    /// Converts the nodes in to <see cref="XmlNodeSyntax"/> elements.
    /// </summary>
    /// <param name="nodes">The nodes.</param>
    /// <returns>The transformed nodes.</returns>
    public static IEnumerable<XmlNodeSyntax> ToXmlNodes(this IEnumerable<XNode> nodes) => nodes.Select(node => node.ToXmlNode());

    /// <summary>
    /// Converts the node into a <see cref="XmlNodeSyntax"/>.
    /// </summary>
    /// <param name="node">The node.</param>
    /// <returns>The transformed node.</returns>
    public static XmlNodeSyntax ToXmlNode(this XNode node)
    {
        return node switch
        {
            // Text
            XText text => XmlText(text.Value),

            // documentation specific attributes
            XElement { Name.LocalName: DocumentationCommentXmlNames.Elements.Parameter } element => XmlParamElement(GetName(element), element.ToXmlNodes().ToArray()),
            XElement { Name.LocalName: DocumentationCommentXmlNames.Elements.ParameterReference } element => XmlParamRefElement(GetName(element)),
            XElement { Name.LocalName: DocumentationCommentXmlNames.Elements.Summary } element => XmlSummaryElement(element.ToXmlNodes().ToArray()),
            XElement { Name.LocalName: DocumentationCommentXmlNames.Elements.Returns } element => XmlReturnsElement(element.ToXmlNodes().ToArray()),
            XElement { Name.LocalName: DocumentationCommentXmlNames.Elements.Remarks } element => XmlRemarksElement(element.ToXmlNodes().ToArray()),
            XElement { Name.LocalName: DocumentationCommentXmlNames.Elements.Para } element => XmlParaElement(element.ToXmlNodes().ToArray()),
            XElement { Name.LocalName: DocumentationCommentXmlNames.Elements.See } element => CreateSeeElement(element),
            XElement { Name.LocalName: DocumentationCommentXmlNames.Elements.SeeAlso } element => CreateSeeAlsoElement(element),
            XElement { Name.LocalName: DocumentationCommentXmlNames.Elements.Exception } element => XmlExceptionElement(CreateCrefElement(element), element.ToXmlNodes().ToArray()),
            XElement { Name.LocalName: DocumentationCommentXmlNames.Elements.Permission } element => XmlPermissionElement(CreateCrefElement(element), element.ToXmlNodes().ToArray()),
            XElement { Name.LocalName: DocumentationCommentXmlNames.Elements.Example } element => XmlExampleElement(element.ToXmlNodes().ToArray()),
            XElement { Name.LocalName: DocumentationCommentXmlNames.Elements.Value } element => XmlValueElement(element.ToXmlNodes().ToArray()),
            XElement { Name.LocalName: DocumentationCommentXmlNames.Elements.Placeholder } element => XmlPlaceholderElement(element.ToXmlNodes().ToArray()),
            XElement { Name.LocalName: DocumentationCommentXmlNames.Elements.ThreadSafety } => XmlThreadSafetyElement(),

            // Generic
            XElement { Name.LocalName: DocumentationCommentXmlNames.Elements.TypeParameter } element => XmlElement(DocumentationCommentXmlNames.Elements.TypeParameter, List(element.ToXmlNodes())).AddStartTagAttributes(XmlNameAttribute(GetName(element))),
            XElement { Name.LocalName: DocumentationCommentXmlNames.Elements.TypeParameterReference } element => XmlElement(DocumentationCommentXmlNames.Elements.TypeParameterReference, List(element.ToXmlNodes())).AddStartTagAttributes(XmlNameAttribute(GetName(element))),

            // List
            XElement { Name.LocalName: DocumentationCommentXmlNames.Elements.List } element => XmlElement(DocumentationCommentXmlNames.Elements.List, List(element.ToXmlNodes())).AddStartTagAttributes(XmlTextAttribute(DocumentationCommentXmlNames.Attributes.Type, GetAttributeValue(element, DocumentationCommentXmlNames.Attributes.Type))),
            XElement { Name.LocalName: DocumentationCommentXmlNames.Elements.ListHeader } element => XmlElement(DocumentationCommentXmlNames.Elements.ListHeader, List(element.ToXmlNodes())),
            XElement { Name.LocalName: DocumentationCommentXmlNames.Elements.Item } element => XmlElement(DocumentationCommentXmlNames.Elements.Item, List(element.ToXmlNodes())),
            XElement { Name.LocalName: DocumentationCommentXmlNames.Elements.Term } element => XmlElement(DocumentationCommentXmlNames.Elements.Term, List(element.ToXmlNodes())),
            XElement { Name.LocalName: DocumentationCommentXmlNames.Elements.Description } element => XmlElement(DocumentationCommentXmlNames.Elements.Description, List(element.ToXmlNodes())),

            // Just translate directly
            XElement { Name.LocalName: { } name, HasAttributes: true } element => XmlElement(name, List(element.ToXmlNodes())).AddStartTagAttributes([.. GetAttributes(element)]),
            XElement { Name.LocalName: { } name } element => XmlElement(name, List(element.ToXmlNodes())),

            _ => throw new NotSupportedException(),
        };

        static string GetName(XElement element)
        {
            return GetAttributeValue(element, DocumentationCommentXmlNames.Attributes.Name);
        }

        static string GetAttributeValue(XElement element, string name)
        {
            return element.Attribute(name)!.Value;
        }

        static bool TryGetAttributeValue(XElement element, string name, out string value)
        {
            if (element.Attribute(name) is { } attribute)
            {
                value = attribute.Value;
                return true;
            }

            value = string.Empty;
            return false;
        }

        static IEnumerable<XmlAttributeSyntax> GetAttributes(XElement element)
        {
            return element.Attributes().Select<XAttribute, XmlAttributeSyntax>(attribute => attribute switch
            {
                { Name.LocalName: DocumentationCommentXmlNames.Attributes.Name, Value: { } value } => XmlNameAttribute(value),
                { Name.LocalName: DocumentationCommentXmlNames.Attributes.Cref, Value: { } value } => XmlCrefAttribute(GetCrefElement(value)),
                { Name.LocalName: { } name, Value: { } value } => XmlTextAttribute(name, value),
            });
        }

        static XmlEmptyElementSyntax CreateSeeElement(XElement element)
        {
            if (TryGetAttributeValue(element, DocumentationCommentXmlNames.Attributes.Cref, out var cref))
            {
                return XmlSeeElement(GetCrefElement(cref));
            }

            if (TryGetAttributeValue(element, DocumentationCommentXmlNames.Attributes.Langword, out var keyword))
            {
                return XmlEmptyElement(DocumentationCommentXmlNames.Elements.See).AddAttributes(
                    XmlTextAttribute(DocumentationCommentXmlNames.Attributes.Langword, keyword));
            }

            if (TryGetAttributeValue(element, DocumentationCommentXmlNames.Attributes.Href, out var href))
            {
                return XmlEmptyElement(DocumentationCommentXmlNames.Elements.See).AddAttributes(
                    XmlTextAttribute(DocumentationCommentXmlNames.Attributes.Href, href));
            }

            throw new InvalidOperationException();
        }

        static XmlEmptyElementSyntax CreateSeeAlsoElement(XElement element)
        {
            if (TryGetAttributeValue(element, DocumentationCommentXmlNames.Attributes.Cref, out var cref))
            {
                return XmlSeeAlsoElement(GetCrefElement(cref));
            }

            if (TryGetAttributeValue(element, DocumentationCommentXmlNames.Attributes.Href, out var href))
            {
                return XmlEmptyElement(DocumentationCommentXmlNames.Elements.See).AddAttributes(
                    XmlTextAttribute(DocumentationCommentXmlNames.Attributes.Href, href));
            }

            throw new InvalidOperationException();
        }

        static CrefSyntax CreateCrefElement(XElement element)
        {
            return TryGetAttributeValue(element, DocumentationCommentXmlNames.Attributes.Cref, out var cref)
                ? GetCrefElement(cref)
                : throw new InvalidOperationException();
        }

        static CrefSyntax GetCrefElement(string cref)
        {
            var colonIndex = cref.IndexOf(':');
            if (colonIndex < 0)
            {
                return NameMemberCref(SyntaxFactory.QualifiedName(cref));
            }

            var type = cref[..colonIndex];
            colonIndex++;
            return type switch
            {
                "T" => TypeCref(SyntaxFactory.QualifiedName(cref[colonIndex..])),
                _ => NameMemberCref(SyntaxFactory.QualifiedName(cref[colonIndex..])),
            };
        }
    }
}