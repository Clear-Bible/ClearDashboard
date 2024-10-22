﻿using System;
using Microsoft.EntityFrameworkCore;
using System.Linq;
using System.Threading.Tasks;
using Autofac;
using ClearDashboard.DAL.Interfaces;
using ClearDashboard.DataAccessLayer.Models;
using Xunit;
using Xunit.Abstractions;
using Corpus = ClearDashboard.DataAccessLayer.Models.Corpus;

namespace ClearDashboard.DAL.Tests
{
    public  class TokenComponentMetadataTests : TestBase
    {
        public TokenComponentMetadataTests(ITestOutputHelper output) : base(output)
        {
        }

        [Fact]
        public async Task QueryByMetadatumTests()
        {
            const string projectName = "TokenComponentMetadataTest";
            SetupProjectDatabase(projectName, false);

            var userProvider = Container!.Resolve<IUserProvider>();

            try
            {
                var corpus = await AddCorpus(userProvider);

                var tokenizedCorpus = await AddTokenizedCorpus(userProvider, corpus);

                var token = new Token
                {
                    BookNumber = 1,
                    ChapterNumber = 1,
                    VerseNumber = 1,
                    TokenizedCorpusId = tokenizedCorpus.Id,
                };
                token.Metadata.Add(new Metadatum { Key = "IsParallelCorpusToken", Value = true.ToString() });
                ProjectDbContext!.Tokens.Add(token);

                var token2 = new Token
                 {
                     BookNumber = 2,
                     ChapterNumber = 2,
                     VerseNumber = 2,
                     TokenizedCorpusId = tokenizedCorpus.Id,
                 };
                token2.Metadata.Add(new Metadatum { Key = "IsParallelCorpusToken", Value = false.ToString() });
                token2.Metadata.Add(new Metadatum { Key = "SomeKey", Value = "Some funky value" });
                ProjectDbContext.Tokens.Add(token2);

                var token3 = new Token
                {
                    BookNumber = 3,
                    ChapterNumber = 3,
                    VerseNumber = 3,
                    TokenizedCorpusId = tokenizedCorpus.Id,
                };
                token3.Metadata.Add(new Metadatum { Key = "SoloKey", Value = "There's only one of me!" });
                ProjectDbContext.Tokens.Add(token3);


                var token4 = new Token
                {
                    BookNumber = 4,
                    ChapterNumber = 4,
                    VerseNumber = 4,
                    TokenizedCorpusId = tokenizedCorpus.Id,
                };
                token4.Metadata.Add(new Metadatum { Key = "DuopolyKey", Value = "There's two of me!" });
                ProjectDbContext.Tokens.Add(token4);

                var token5 = new Token
                {
                    BookNumber = 5,
                    ChapterNumber = 5,
                    VerseNumber = 5,
                    TokenizedCorpusId = tokenizedCorpus.Id,
                };
                token5.Metadata.Add(new Metadatum { Key = "DuopolyKey", Value = "There's two of me!" });
                ProjectDbContext.Tokens.Add(token5);

                await ProjectDbContext.SaveChangesAsync();

                var roundTrippedTokens = await ProjectDbContext.Tokens
                    .Where(t=>t.Metadata.Any(m => m.Key == "IsParallelCorpusToken" && m.Value == true.ToString())).ToListAsync();
                Assert.NotNull(roundTrippedTokens);
                Assert.Single(roundTrippedTokens);

                var funkyRoundTrippedTokens = await ProjectDbContext.Tokens
                    .Where(t => t.Metadata.Any(m => m.Value == "Some funky value")).ToListAsync();
                Assert.Single(funkyRoundTrippedTokens);

                var soloToken = await ProjectDbContext.Tokens
                    .FirstOrDefaultAsync(t => t.Metadata.Any(m => m.Key == "SoloKey"));

                Assert.NotNull(soloToken);
                Assert.Equal("There's only one of me!", soloToken.Metadata.First(m => m.Key == "SoloKey").Value);

                var duopolyTokens = await ProjectDbContext.Tokens
                    .Where(t => t.Metadata.Any(m => m.Key=="DuopolyKey")).ToListAsync();
                Assert.Equal(2, duopolyTokens.Count);
            }
            catch (Exception e)
            {
                Output.WriteLine(e.ToString());
                throw;

            }
            finally
            {
                await DeleteDatabaseContext(projectName);
            }
        }

        private async Task<TokenizedCorpus> AddTokenizedCorpus(IUserProvider userProvider, Corpus corpus)
        {
            var tokenizedCorpus = new TokenizedCorpus
            {
                UserId = userProvider.CurrentUser!.Id,
                CorpusId = corpus.Id,

            };

            await ProjectDbContext!.TokenizedCorpora.AddAsync(tokenizedCorpus);
            return tokenizedCorpus;
        }

        private async Task<Corpus> AddCorpus(IUserProvider userProvider)
        {
            var corpus = new Corpus
            {
                UserId = userProvider.CurrentUser!.Id,
                Name = "Test Corpus",
                   
            };

            await ProjectDbContext!.Corpa.AddAsync(corpus);
            return corpus;
        }
    }
}
