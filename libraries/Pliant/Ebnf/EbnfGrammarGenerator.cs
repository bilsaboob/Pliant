using Pliant.Automata;
using Pliant.Builders;
using Pliant.Grammars;
using Pliant.RegularExpressions;
using System.Collections.Generic;
using System.Text;
using System;
using Pliant.Tokens;
using Pliant.Tree;

namespace Pliant.Ebnf
{
    public class EbnfGrammarGenerator
    {
        private class DefinitionInfo
        {
            public DefinitionInfo(EbnfBlock block)
            {
                Block = block;
            }

            public EbnfBlock Block { get; set; }

            public EbnfQualifiedIdentifier NameIdentifier { get; set; }
            public string Name => NameIdentifier?.Identifier;

            public FullyQualifiedName FullyQualifiedName { get; set; }
            public string FullName => FullyQualifiedName?.FullName;

            public bool IsLex { get; set; }
            public bool IsRule { get; set; }
            public bool IsSetting { get; set; }
            public SymbolModel Model { get; set; }
        }

        readonly INfaToDfa _nfaToDfaAlgorithm;
        readonly IRegexToNfa _regexToNfaAlgorithm;
        private Dictionary<string, DefinitionInfo> _definitions;

        public EbnfGrammarGenerator()
        {
            _regexToNfaAlgorithm = new ThompsonConstructionAlgorithm();
            _nfaToDfaAlgorithm = new SubsetConstructionAlgorithm();
            _definitions = new Dictionary<string, DefinitionInfo>();
        }

        public IGrammar Generate(EbnfDefinition ebnf)
        {
            BuildDefinitions(ebnf);

            var grammarModel = new GrammarModel();
            Definition(ebnf, grammarModel);
            return grammarModel.ToGrammar();
        }

        #region Definitions
        private EbnfDefinition BuildEbnfDefinition(InternalTreeNode parseTree)
        {
            var ebnfVisitor = new EbnfVisitor();
            parseTree.Accept(ebnfVisitor);
            return ebnfVisitor.Definition;
        }

        private void BuildDefinitions(EbnfDefinition definition)
        {
            CollectRule(definition.Block);

            // process any remaining definitions
            if (definition.NodeType != EbnfNodeType.EbnfDefinitionConcatenation)
                return;

            var definitionConcatenation = definition as EbnfDefinitionConcatenation;
            BuildDefinitions(definitionConcatenation.Definition);
        }

        private void CollectRule(EbnfBlock block)
        {
            switch (block.NodeType)
            {
                case EbnfNodeType.EbnfBlockLexerRule:
                    var blockLexerRule = block as EbnfBlockLexerRule;
                    AddDefInfo(blockLexerRule.LexerRule.QualifiedIdentifier, new DefinitionInfo(block) { IsLex = true });
                    break;

                case EbnfNodeType.EbnfBlockRule:
                    var blockRule = block as EbnfBlockRule;
                    AddDefInfo(blockRule.Rule.QualifiedIdentifier, new DefinitionInfo(block) { IsRule = true });
                    break;
            }
        }

        private void AddDefInfo(EbnfQualifiedIdentifier ident, DefinitionInfo defInfo)
        {
            defInfo.NameIdentifier = ident;
            defInfo.FullyQualifiedName = GetFullyQualifiedNameFromQualifiedIdentifier(ident);
            _definitions[defInfo.FullName] = defInfo;
        }

        private bool TryGetDefinition(string name, out DefinitionInfo def)
        {
            return _definitions.TryGetValue(name, out def);
        }

        private bool IsLexDefinition(string name)
        {
            if (!_definitions.TryGetValue(name, out var def))
                return false;

            return def.IsLex;
        }

        private bool IsRuleDefinition(string name)
        {
            if (!_definitions.TryGetValue(name, out var def))
                return false;

            return def.IsRule;
        }

        private bool DefinitionExists(string name)
        {
            return _definitions.TryGetValue(name, out var def);
        }
        
        #endregion

        #region Generate
        
        private void Definition(EbnfDefinition definition, GrammarModel grammarModel)
        {
            Block(definition.Block, grammarModel);

            if (definition.NodeType != EbnfNodeType.EbnfDefinitionConcatenation)
                return;

            var definitionConcatenation = definition as EbnfDefinitionConcatenation;
            Definition(definitionConcatenation.Definition, grammarModel);                
        }

        void Block(EbnfBlock block, GrammarModel grammarModel)
        {
            switch (block.NodeType)
            {
                case EbnfNodeType.EbnfBlockLexerRule:
                    var blockLexerRule = block as EbnfBlockLexerRule;
                    grammarModel.LexerRules.Add(LexerRule(blockLexerRule));
                    break;

                case EbnfNodeType.EbnfBlockRule:
                    var blockRule = block as EbnfBlockRule;
                    foreach (var production in Rule(blockRule.Rule))
                        grammarModel.Productions.Add(production);
                    break;

                case EbnfNodeType.EbnfBlockSetting:
                    var blockSetting = block as EbnfBlockSetting;
                    var settingKey = blockSetting.Setting.SettingIdentifier.Value?.TrimStart(':');
                    switch (settingKey)
                    {
                        case StartProductionSettingModel.SettingKey:
                            grammarModel.StartSetting = StartSetting(blockSetting);
                            break;

                        case IgnoreSettingModel.SettingKey:
                            var ignoreSettings = IgnoreSettings(blockSetting);
                            for (var i = 0; i < ignoreSettings.Count; i++)
                                grammarModel.IgnoreSettings.Add(ignoreSettings[i]);
                            break;

                        case TriviaSettingModel.SettingKey:
                            var triviaSettings = TriviaSettings(blockSetting);
                            for (var i = 0; i < triviaSettings.Count; i++)
                                grammarModel.TriviaSettings.Add(triviaSettings[i]);
                            break;
                    }
                    break;
            }
        }

        private LexerRuleModel LexerRule(EbnfBlockLexerRule blockLexerRule)
        {
            var ebnfLexerRule = blockLexerRule.LexerRule;

            var fullyQualifiedName = GetFullyQualifiedNameFromQualifiedIdentifier(
                ebnfLexerRule.QualifiedIdentifier);

            var lexerRule = LexerRuleExpression(
                fullyQualifiedName,
                ebnfLexerRule.Expression);

            return new LexerRuleModel(lexerRule);
        }

        private ILexerRule LexerRuleExpression(
            FullyQualifiedName fullyQualifiedName, 
            EbnfLexerRuleExpression ebnfLexerRule)
        {
            ILexerRule lexerRule = null;
            if (TryRecognizeSimpleLiteralExpression(fullyQualifiedName, ebnfLexerRule, out lexerRule))
                return lexerRule;

            var nfa = LexerRuleExpression(ebnfLexerRule);
            var dfa = _nfaToDfaAlgorithm.Transform(nfa);

            return new DfaLexerRule(dfa, fullyQualifiedName.FullName);
        }

        private bool TryRecognizeSimpleLiteralExpression(
            FullyQualifiedName fullyQualifiedName,
            EbnfLexerRuleExpression ebnfLexerRule, 
            out ILexerRule lexerRule)
        {
            lexerRule = null;

            if (ebnfLexerRule.NodeType != EbnfNodeType.EbnfLexerRuleExpression)
                return false;

            var term = ebnfLexerRule.Term;
            if (term.NodeType != EbnfNodeType.EbnfLexerRuleTerm)
                return false;

            var factor = term.Factor;
            if (factor.NodeType != EbnfNodeType.EbnfLexerRuleFactorLiteral)
                return false;

            var literal = factor as EbnfLexerRuleFactorLiteral;
            lexerRule = new StringLiteralLexerRule(
                literal.Value, 
                new TokenType(fullyQualifiedName.FullName));

            return true;
        }        

        INfa LexerRuleExpression(EbnfLexerRuleExpression expression)
        {
            var nfa = LexerRuleTerm(expression.Term);
            if (expression.NodeType == EbnfNodeType.EbnfLexerRuleExpressionAlteration)
            {
                var alteration = expression as EbnfLexerRuleExpressionAlteration;
                var alterationNfa = LexerRuleExpression(alteration.Expression);
                nfa = nfa.Union(alterationNfa);
            }
            return nfa;
        }

        INfa LexerRuleTerm(EbnfLexerRuleTerm term)
        {
            var nfa = LexerRuleFactor(term.Factor);
            if (term.NodeType == EbnfNodeType.EbnfLexerRuleTermConcatenation)
            {
                var concatenation = term as EbnfLexerRuleTermConcatenation;
                var concatNfa = LexerRuleTerm(concatenation.Term);
                nfa = nfa.Concatenation(concatNfa);
            }
            return nfa;
        }

        INfa LexerRuleFactor(EbnfLexerRuleFactor factor)
        {
            switch (factor.NodeType)
            {
                case EbnfNodeType.EbnfLexerRuleFactorLiteral:
                    return LexerRuleFactorLiteral(factor as EbnfLexerRuleFactorLiteral);                    

                case EbnfNodeType.EbnfLexerRuleFactorRegex:
                    return LexerRuleFactorRegex(factor as EbnfLexerRuleFactorRegex);

                default:
                    throw new InvalidOperationException(
                        $"Invalid EbnfLexerRuleFactor node type detected. Found {Enum.GetName(typeof(EbnfNodeType), factor.NodeType)}, expected EbnfLexerRuleFactorLiteral or EbnfLexerRuleFactorRegex");
            }
        }

        private INfa LexerRuleFactorLiteral(EbnfLexerRuleFactorLiteral ebnfLexerRuleFactorLiteral)
        {
            var literal = ebnfLexerRuleFactorLiteral.Value;
            var states = new NfaState[literal.Length + 1];
            for (var i = 0; i < states.Length; i++)
            {
                var current = new NfaState();
                states[i] = current;

                if (i == 0)
                    continue;

                var previous = states[i - 1];
                previous.AddTransistion(
                    new TerminalNfaTransition(
                        new CharacterTerminal(literal[i - 1]), current));
            }
            return new Nfa(states[0], states[states.Length - 1]);
        }
        
        private INfa LexerRuleFactorRegex(EbnfLexerRuleFactorRegex ebnfLexerRuleFactorRegex)
        {
            var regex = ebnfLexerRuleFactorRegex.Regex;
            return _regexToNfaAlgorithm.Transform(regex);
        }

        private StartProductionSettingModel StartSetting(EbnfBlockSetting blockSetting)
        {
            var productionName =  GetFullyQualifiedNameFromQualifiedIdentifier(
                blockSetting.Setting.QualifiedIdentifier);
            return new StartProductionSettingModel(productionName);
        }

        private IReadOnlyList<TriviaSettingModel> TriviaSettings(EbnfBlockSetting blockSetting)
        {
            var fullyQualifiedName = GetFullyQualifiedNameFromQualifiedIdentifier(blockSetting.Setting.QualifiedIdentifier);
            var triviaSettingModel = new TriviaSettingModel(fullyQualifiedName);
            return new[] { triviaSettingModel};
        }

        private IReadOnlyList<IgnoreSettingModel> IgnoreSettings(EbnfBlockSetting blockSetting)
        {
            var fullyQualifiedName = GetFullyQualifiedNameFromQualifiedIdentifier(blockSetting.Setting.QualifiedIdentifier);
            var ignoreSettingModel = new IgnoreSettingModel(fullyQualifiedName);
            return new[] { ignoreSettingModel };
        }

        IEnumerable<ProductionModel> Rule(EbnfRule rule)
        {
            var nonTerminal = GetFullyQualifiedNameFromQualifiedIdentifier(rule.QualifiedIdentifier);
            var productionModel = new ProductionModel(nonTerminal);
            foreach(var production in Expression(rule.Expression, productionModel))
                yield return production;
            yield return productionModel;           
        }

        IEnumerable<ProductionModel> Expression(EbnfExpression expression, ProductionModel currentProduction)
        {
            foreach (var production in Term(expression.Term, currentProduction))
                yield return production;

            if (expression.NodeType != EbnfNodeType.EbnfExpressionAlteration)
                yield break;

            var expressionAlteration = expression as EbnfExpressionAlteration;
            currentProduction.Lambda();

            foreach (var production in Expression(expressionAlteration.Expression, currentProduction))
                yield return production;            
        }

        IEnumerable<ProductionModel> Grouping(EbnfFactorGrouping grouping, ProductionModel currentProduction)
        {
            var name = grouping.ToString();
            var nonTerminal = new NonTerminal(name);
            var groupingProduction = new ProductionModel(nonTerminal);

            currentProduction.AddWithAnd(new NonTerminalModel(nonTerminal));

            var expression = grouping.Expression;           
            foreach (var production in Expression(expression, groupingProduction))
                yield return production; 

            yield return groupingProduction;
        }

        IEnumerable<ProductionModel> Optional(EbnfFactorOptional optional, ProductionModel currentProduction)
        {
            var name = optional.ToString();
            var nonTerminal = new NonTerminal(name);
            var optionalProduction = new ProductionModel(nonTerminal);

            currentProduction.AddWithAnd(new NonTerminalModel(nonTerminal));

            var expression = optional.Expression;
            foreach (var production in Expression(expression, optionalProduction))
                yield return production;

            optionalProduction.Lambda();
            yield return optionalProduction;
        }

        IEnumerable<ProductionModel> Repetition(EbnfFactorRepetition repetition, ProductionModel currentProduction)
        {
            // build the repetition production
            var repeatProductionName = repetition.ToString() + "_many";
            var repeatNonTerminal = new NonTerminal(repeatProductionName);
            var repeatProduction = new ProductionModel(repeatNonTerminal);
            currentProduction.AddWithAnd(repeatProduction);

            // build the leaf productions
            var leafProductionName = repetition.ToString();
            var leafNonTerminal = new NonTerminal(leafProductionName);
            var leafProduction = new ProductionModel(leafNonTerminal);
            
            // my_many -> leaf
            repeatProduction.AddWithAnd(leafProduction);
            
            // my_many -> leaf my_many (the leaf and then repeat with self)
            repeatProduction.AddWithOr(leafProduction);
            repeatProduction.AddWithAnd(repeatProduction);

            // my_many -> (empty for optional)
            repeatProduction.Lambda();

            // process the repetition expressions
            var expression = repetition.Expression;
            foreach (var production in Expression(expression, leafProduction))
                yield return production;

            /*currentProduction.AddWithAnd(new NonTerminalModel(leafNonTerminal));

            var expression = repetition.Expression;
            foreach (var production in Expression(expression, leafProduction))
                yield return production;

            leafProduction.AddWithOr(new NonTerminalModel(leafNonTerminal));
            leafProduction.Lambda();*/

            yield return leafProduction;
            yield return repeatProduction;
        }

        IEnumerable<ProductionModel> Term(EbnfTerm term, ProductionModel currentProduction)
        {
            foreach (var production in Factor(term.Factor, currentProduction))
                yield return production;

            if (term.NodeType != EbnfNodeType.EbnfTermConcatenation)
                yield break;

            var concatenation = term as EbnfTermConcatenation;
            foreach(var production in Term(concatenation.Term, currentProduction))
                yield return production;                    
        }

        IEnumerable<ProductionModel> Factor(EbnfFactor factor, ProductionModel currentProduction)
        {
            switch (factor.NodeType)
            {
                case EbnfNodeType.EbnfFactorGrouping:
                    var grouping = factor as EbnfFactorGrouping;
                    foreach (var production in Grouping(grouping, currentProduction))
                        yield return production;
                    break;

                case EbnfNodeType.EbnfFactorOptional:
                    var optional = factor as EbnfFactorOptional;
                    foreach (var production in Optional(optional, currentProduction))
                        yield return production;
                    break;

                case EbnfNodeType.EbnfFactorRepetition:
                    var repetition = factor as EbnfFactorRepetition;
                    foreach (var production in Repetition(repetition, currentProduction))
                        yield return production;
                    break;

                case EbnfNodeType.EbnfFactorIdentifier:
                    var identifier = factor as EbnfFactorIdentifier;
                    var nonTerminal = GetFullyQualifiedNameFromQualifiedIdentifier(identifier.QualifiedIdentifier);

                    if (TryGetDefinition(nonTerminal.FullName, out var def))
                    {
                        if (def.IsLex)
                        {
                            // Either use the cached lexer model or build it and cache it
                            var lexerRuleModel = def.Model as LexerRuleModel;
                            if (lexerRuleModel == null)
                            {
                                var lexRuleBlock = def.Block as EbnfBlockLexerRule;
                                lexerRuleModel = LexerRule(lexRuleBlock);
                                def.Model = lexerRuleModel;
                            }
                            currentProduction.AddWithAnd(lexerRuleModel);
                        }
                        else
                        {
                            // add a non terminal, we don't need to evaluat it or anything
                            currentProduction.AddWithAnd(new NonTerminalModel(nonTerminal));
                        }
                    }
                    else
                    {
                        //throw new Exception($"Unresolved reference to '{nonTerminal.FullName}'");
                    }
                    
                    break;

                case EbnfNodeType.EbnfFactorLiteral:
                    var literal = factor as EbnfFactorLiteral;
                    var stringLiteralRule = new StringLiteralLexerRule(literal.Value);
                    currentProduction.AddWithAnd( new LexerRuleModel(stringLiteralRule));
                    break;

                case EbnfNodeType.EbnfFactorRegex:
                    var regex = factor as EbnfFactorRegex;
                    var nfa = _regexToNfaAlgorithm.Transform(regex.Regex);
                    var dfa = _nfaToDfaAlgorithm.Transform(nfa);
                    var dfaLexerRule = new DfaLexerRule(dfa, regex.Regex.ToString());
                    currentProduction.AddWithAnd(new LexerRuleModel(dfaLexerRule));
                    break;                
            }            
        }
                        
        private static FullyQualifiedName GetFullyQualifiedNameFromQualifiedIdentifier(EbnfQualifiedIdentifier qualifiedIdentifier)
        {
            var @namespace = new StringBuilder();
            var currentQualifiedIdentifier = qualifiedIdentifier;
            var index = 0;
            while (currentQualifiedIdentifier.NodeType == EbnfNodeType.EbnfQualifiedIdentifierConcatenation)
            {
                if (index > 0)
                    @namespace.Append(".");
                @namespace.Append(currentQualifiedIdentifier.Identifier);
                currentQualifiedIdentifier = (currentQualifiedIdentifier as EbnfQualifiedIdentifierConcatenation).QualifiedIdentifier;
                index++;
            }
            return new FullyQualifiedName(@namespace.ToString(), currentQualifiedIdentifier.Identifier);
        }
        #endregion

        private static Exception UnreachableCodeException()
        {
            return new InvalidOperationException("Unreachable Code Detected");
        }
    }
}
