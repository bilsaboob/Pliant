using Pliant.Diagnostics;
using Pliant.Utilities;
using System;
using Pliant.Tokens;

namespace Pliant.Ebnf
{
    public class EbnfQualifiedIdentifier : EbnfNode
    {
        private readonly int _hashCode;

        public string Identifier { get; private set; }

        public IToken IdentifierToken { get; private set; }

        public EbnfQualifiedIdentifier(string identifier)
            : this(identifier, null)
        {
        }

        public EbnfQualifiedIdentifier(string identifier, IToken token)
        {
            Assert.IsNotNull(identifier, nameof(identifier));
            Identifier = identifier;
            IdentifierToken = token;
            _hashCode = ComputeHashCode();
        }

        public override EbnfNodeType NodeType
        {
            get
            {
                return EbnfNodeType.EbnfQualifiedIdentifier;
            }
        }

        public override bool Equals(object obj)
        {
            if ((object)obj == null)
                return false;
            var qualifiedIdentifier = obj as EbnfQualifiedIdentifier;
            if ((object)qualifiedIdentifier == null)
                return false;
            return qualifiedIdentifier.NodeType == NodeType
                && qualifiedIdentifier.Identifier.Equals(Identifier);
        }

        int ComputeHashCode()
        {
            return HashCode.Compute(NodeType.GetHashCode(), Identifier.GetHashCode());
        }

        public override int GetHashCode()
        {
            return _hashCode;
        }

        public override string ToString()
        {
            return Identifier;
        }
    }

    public class EbnfQualifiedIdentifierConcatenation : EbnfQualifiedIdentifier
    {
        private readonly int _hashCode;

        public EbnfQualifiedIdentifier QualifiedIdentifier { get; private set; }
        public IToken IdentifierToken { get; private set; }

        public EbnfQualifiedIdentifierConcatenation(
            string identifier,
            EbnfQualifiedIdentifier qualifiedIdentifier)
            : this(identifier, null, qualifiedIdentifier)
        {
        }

        public EbnfQualifiedIdentifierConcatenation(
            string identifier,
            IToken identifierToken,
            EbnfQualifiedIdentifier qualifiedIdentifier)
            : base(identifier, identifierToken)
        {
            QualifiedIdentifier = qualifiedIdentifier;
            _hashCode = ComputeHashCode();
        }

        public override EbnfNodeType NodeType
        {
            get
            {
                return EbnfNodeType.EbnfQualifiedIdentifierConcatenation;
            }
        }

        public override bool Equals(object obj)
        {
            if ((object)obj == null)
                return false;
            var qualifiedIdentifier = obj as EbnfQualifiedIdentifierConcatenation;
            if ((object)qualifiedIdentifier == null)
                return false;
            return qualifiedIdentifier.NodeType == NodeType
                && qualifiedIdentifier.Identifier.Equals(Identifier)
                && qualifiedIdentifier.QualifiedIdentifier.Equals(QualifiedIdentifier);
        }

        int ComputeHashCode()
        {
            return HashCode.Compute(
                NodeType.GetHashCode(),
                Identifier.GetHashCode(), 
                QualifiedIdentifier.GetHashCode());
        }

        public override int GetHashCode()
        {
            return _hashCode;
        }

        public override string ToString()
        {
            return $"{QualifiedIdentifier}.{Identifier}";
        }
    }
}