﻿using System.ComponentModel;

namespace ClearDashboard.Wpf.Application.ViewModels.Project;

public enum Tokenizers
{
    //[Description("Latin Sentence Tokenizer")]
    //LatinSentenceTokenizer = 0,

    //[Description("Latin Word Detokenizer")]
    //LatinWordDetokenizer,

    [Description("Latin Word TokenizedCorpus")]
    LatinWordTokenizer,

    //[Description("Line Segment Tokenizer")]
    //LineSegmentTokenizer,

    //[Description("Null Tokenizer")]
    //NullTokenizer,

    //[Description("Regex Tokenizer")]
    //RegexTokenizer,

    //[Description("String Detokenizer")]
    //StringDetokenizer,

    //[Description("String Tokenizer")]
    //StringTokenizer,

    //[Description("Whitespace Detokenizer")]
    //WhitespaceDetokenizer,

    [Description("Whitespace TokenizedCorpus")]
    // ReSharper disable once UnusedMember.Global
    WhitespaceTokenizer,

    //[Description("Zwsp Word Detokenizer")]
    //ZwspWordDetokenizer,

    // ReSharper disable once UnusedMember.Global
    [Description("Zwsp Word TokenizedCorpus")]
    ZwspWordTokenizer
}