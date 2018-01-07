using Pliant.Utilities;
using System;
using Pliant.Tokens;
using Pliant.Tree;

namespace Pliant.Ebnf
{
    public abstract class EbnfLexerRuleFactor : EbnfNode
    {     
    }

    public class EbnfLexerRuleFactorLiteral : EbnfLexerRuleFactor
    {
        public override EbnfNodeType NodeType { get { return EbnfNodeType.EbnfLexerRuleFactorLiteral; } }

        public string Value { get; private set; }

        public IToken ValueToken { get; private set; }

        private readonly int _hashCode;

        public EbnfLexerRuleFactorLiteral(string value)
            : this(value, null)
        {
        }

        public EbnfLexerRuleFactorLiteral(string value, IToken token)
        {
            Value = value;
            ValueToken = token;
            _hashCode = ComputeHashCode();
        }

        private int ComputeHashCode()
        {
            return HashCode.Compute(NodeType.GetHashCode(), Value.GetHashCode());
        }

        public override int GetHashCode()
        {
            return _hashCode;
        }

        public override bool Equals(object obj)
        {
            if ((object)obj == null)
                return false;
            var factor = obj as EbnfLexerRuleFactorLiteral;
            if ((object)factor == null)
                return false;
            return factor.NodeType == NodeType
                && factor.Value.Equals(Value);
        }
    }

    public class EbnfLexerRuleFactorRegex : EbnfLexerRuleFactor
    {
        public override EbnfNodeType NodeType { get { return EbnfNodeType.EbnfLexerRuleFactorRegex; } }
        private readonly int _hashCode;

        public RegularExpressions.Regex Regex { get; private set; }
        public IInternalTreeNode RegexNode { get; private set; }

        public EbnfLexerRuleFactorRegex(RegularExpressions.Regex regex)
            : this(regex, null)
        {
        }

        public EbnfLexerRuleFactorRegex(RegularExpressions.Regex regex, IInternalTreeNode regexNode)
        {
            Regex = regex;
            RegexNode = regexNode;

            _hashCode = ComputeHashCode();
        }

        int ComputeHashCode()
        {
            return HashCode.Compute(
                NodeType.GetHashCode(),
                Regex.GetHashCode());
        }

        public override int GetHashCode()
        {
            return _hashCode;
        }

        public override bool Equals(object obj)
        {
            if ((object)obj == null)
                return false;
            var factor = obj as EbnfFactorRegex;
            if ((object)factor == null)
                return false;
            return factor.NodeType == NodeType
                && factor.Regex.Equals(Regex);
        }
    }
}