using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Threading;
using Autofac;
using ClearBible.Engine.Corpora;
using ClearDashboard.DAL.Alignment.Corpora;
using ClearDashboard.DAL.Alignment.Tests.Corpora.Handlers;
using MediatR;
using SIL.Machine.Corpora;
using SIL.Scripture;
using Xunit;
using Xunit.Abstractions;

namespace ClearDashboard.DAL.Alignment.Tests.Corpora
{
	public class ParallelCorpusTests
    {
		protected readonly ITestOutputHelper output_;
		protected readonly IComponentContext context_;
		protected readonly IMediator mediator_;
        protected readonly string projectName_ = "Test Project";
		public ParallelCorpusTests(ITestOutputHelper output)
		{
			output_ = output;
			mediator_ = new MediatorMock(); //FIXME: inject mediator

			var containerBuilder = new ContainerBuilder();
			containerBuilder.RegisterType<MediatorMock>().As<IMediator>();
			context_ = containerBuilder.Build();
		}

		[Fact]
		[Trait("Category", "Example")]
		public async void ParallelCorpus__GetAllParallelCorpusIds()
		{
			var parallelCorpusIds = await ParallelCorpus.GetAllParallelCorpusIdsAsync(context_, CancellationToken.None);
			Assert.True(parallelCorpusIds.Count() > 0);
		}


		[Fact]
		public void ParallelCorpus__GetGetRows_SameVerseRefOneToMany()
		{
			Versification.Table.Implementation.RemoveAllUnknownVersifications();
			string src = "&MAT 1:2-3 = MAT 1:2\nMAT 1:4 = MAT 1:3\n";
			ScrVers versification;
			using (var reader = new StringReader(src))
			{
				versification = Versification.Table.Implementation.Load(reader, "vers.txt", ScrVers.English, "custom");
			}

			var sourceCorpus = new DictionaryTextCorpus(
				new MemoryText("MAT", new[]
				{
					TextRow(new VerseRef("MAT 1:1", ScrVers.Original), "source chapter one, verse one ."),
					TextRow(new VerseRef("MAT 1:2", ScrVers.Original), "source chapter one, verse two ."),
					TextRow(new VerseRef("MAT 1:3", ScrVers.Original), "source chapter one, verse three .")
				}));
			var targetCorpus = new DictionaryTextCorpus(
				new MemoryText("MAT", new[]
				{
					TextRow(new VerseRef("MAT 1:1", versification), "target chapter one, verse one ."),
					TextRow(new VerseRef("MAT 1:2", versification),
						"target chapter one, verse two . target chapter one, verse three .", isInRange: true,
						isRangeStart: true),
					TextRow(new VerseRef("MAT 1:3", versification), isInRange: true),
					TextRow(new VerseRef("MAT 1:4", versification), "target chapter one, verse four .")
				}));

			var parallelCorpus = new ParallelTextCorpus(sourceCorpus, targetCorpus);
			ParallelTextRow[] rows = parallelCorpus.ToArray();
			Assert.Equal(3, rows.Length);
			Assert.Equal(Refs(new VerseRef("MAT 1:2", ScrVers.Original)), rows[1].SourceRefs.Cast<VerseRef>());
			Assert.Equal(Refs(new VerseRef("MAT 1:2", versification), new VerseRef("MAT 1:3", versification)), rows[1].TargetRefs.Cast<VerseRef>());
			Assert.Equal("source chapter one, verse two .".Split(), rows[1].SourceSegment);
			Assert.Equal("target chapter one, verse two . target chapter one, verse three .".Split(), rows[1].TargetSegment);
		}

		[Fact]
		[Trait("Category", "Example")]
		public async void ParallelCorpus__Create()
        {
			var sourceTokenizedTextCorpus = await TokenizedTextCorpus.Get( mediator_, new TokenizedTextCorpusId(new Guid()));

			var targetTokenizedTextCorpus = await TokenizedTextCorpus.Get(mediator_, new TokenizedTextCorpusId(new Guid()));

			var verseMappingList = TestDataHelpers.GetSampleTextCorpusSourceTextIdToVerseMappings(
					sourceTokenizedTextCorpus.Versification,
					targetTokenizedTextCorpus.Versification);

            var parallelTextCorpusWithTokenizedTextCorpuses = sourceTokenizedTextCorpus.EngineAlignRows(targetTokenizedTextCorpus, 
				new SourceTextIdToVerseMappingsFromVerseMappings(verseMappingList));

			var parallelTokenizedCorpus = await parallelTextCorpusWithTokenizedTextCorpuses.CreateAsync("parallelTextCorpusWithTokenizedTextCorpuses", context_);

            Assert.Equal(16, verseMappingList.Count());
			Assert.True(parallelTokenizedCorpus.SourceCorpus.Count() == 16);
			Assert.True(parallelTokenizedCorpus.TargetCorpus.Count() == 16);
		}


		[Fact]
		[Trait("Category", "Example")]
		public async void ParallelCorpus_Get()
		{
			var parallelCorpus = await ParallelCorpus.GetAsync( context_, new ParallelCorpusId(new Guid()));
			Assert.NotNull(parallelCorpus.SourceTextIdToVerseMappings);
			Assert.Equal(1, parallelCorpus.SourceTextIdToVerseMappings?.GetVerseMappings()?.Count() ?? 0); // should be 1 and not 16: EngineParallelTextCorpus should not have used sil versification to initialize.
			Assert.True(parallelCorpus.SourceCorpus.Count() > 0);
			Assert.True(parallelCorpus.TargetCorpus.Count() > 0);

			//since there is only one mapped verse, there should only be one that displays
			foreach (var engineParallelTextRow in parallelCorpus.Cast<EngineParallelTextRow>())
			{
				//display verse info
				var verseRefStr = engineParallelTextRow.Ref.ToString();
				output_.WriteLine(verseRefStr);

				//display source
				var sourceVerseText = string.Join(" ", engineParallelTextRow.SourceSegment);
				output_.WriteLine($"Source: {sourceVerseText}");
				var sourceTokenIds = string.Join(" ", engineParallelTextRow.SourceTokens?
					.Select(token => token.TokenId.ToString()) ?? new string[] { "NONE" });
				output_.WriteLine($"SourceTokenIds: {sourceTokenIds}");

				//display target
				var targetVerseText = string.Join(" ", engineParallelTextRow.TargetSegment);
				output_.WriteLine($"Target: {targetVerseText}");
				var targetTokenIds = string.Join(" ", engineParallelTextRow.TargetTokens?
					.Select(token => token.TokenId.ToString()) ?? new string[] { "NONE" });
				output_.WriteLine($"TargetTokenIds: {targetTokenIds}");
			}
		}

		[Fact]
		[Trait("Category", "Example")]
		public async void ParallelCorpus_Get_ChangeVerseMappings_SaveNewParallelVersion()
		{
			var parallelCorpus = await ParallelCorpus.GetAsync(context_, new ParallelCorpusId(new Guid()));
            Assert.NotNull(parallelCorpus.SourceTextIdToVerseMappings);

			var verseMappingList = parallelCorpus.SourceTextIdToVerseMappings?.GetVerseMappings()?.ToList();

			Assert.NotNull(verseMappingList);
            Assert.Single(verseMappingList); // should be 1 and not 16: EngineParallelTextCorpus should not have used sil versification to initialize.
            Assert.True(parallelCorpus.SourceCorpus.Count() > 0);
            Assert.True(parallelCorpus.TargetCorpus.Count() > 0);

//			var sourceTextIdToVerseMappings = new SourceTextIdToVerseMappingsFromVerseMappings();
            verseMappingList.Add(
				new VerseMapping(
					new List<Verse>() { new Verse("MAT", 1, 2) },
					new List<Verse>() { new Verse("MAT", 1, 2) })); //already checked for null, this should never happen.

			parallelCorpus.SourceTextIdToVerseMappings = new SourceTextIdToVerseMappingsFromVerseMappings(verseMappingList);

            Assert.Equal(2, parallelCorpus.SourceTextIdToVerseMappings.GetVerseMappings().Count()); // should be 1 and not 16: EngineParallelTextCorpus should not have used sil versification to initialize.

			//since there is only one mapped verse, there should only be one that displays
			foreach (var engineParallelTextRow in parallelCorpus.Cast<EngineParallelTextRow>())
			{
				//display verse info
				var verseRefStr = engineParallelTextRow.Ref.ToString();
				output_.WriteLine(verseRefStr);

				//display source
				var sourceVerseText = string.Join(" ", engineParallelTextRow.SourceSegment);
				output_.WriteLine($"Source: {sourceVerseText}");
				var sourceTokenIds = string.Join(" ", engineParallelTextRow.SourceTokens?
					.Select(token => token.TokenId.ToString()) ?? new string[] { "NONE" });
				output_.WriteLine($"SourceTokenIds: {sourceTokenIds}");

				//display target
				var targetVerseText = string.Join(" ", engineParallelTextRow.TargetSegment);
				output_.WriteLine($"Target: {targetVerseText}");
				var targetTokenIds = string.Join(" ", engineParallelTextRow.TargetTokens?
					.Select(token => token.TokenId.ToString()) ?? new string[] { "NONE" });
				output_.WriteLine($"TargetTokenIds: {targetTokenIds}");
			}
		}

		[Fact]
		[Trait("Category", "Example")]
		public async void ParallelCorpus_Get_ChangeTokenizedCorpuses_SaveNewParallelTokenizedCorpus()
		{
			var parallelCorpus = await ParallelCorpus.GetAsync(context_, new ParallelCorpusId(new Guid()));
            Assert.NotNull(parallelCorpus.SourceTextIdToVerseMappings);
			Assert.Equal(1, parallelCorpus.SourceTextIdToVerseMappings?.GetVerseMappings()?.Count() ?? 0); // should be 1 and not 16: EngineParallelTextCorpus should not have used sil versification to initialize.
            Assert.True(parallelCorpus.SourceCorpus.Count() > 0);
            Assert.True(parallelCorpus.TargetCorpus.Count() > 0);

            //since there is only one mapped verse, there should only be one that displays
            foreach (var engineParallelTextRow in parallelCorpus.Cast<EngineParallelTextRow>())
			{
				//display verse info
				var verseRefStr = engineParallelTextRow.Ref.ToString();
				output_.WriteLine(verseRefStr);

				//display source
				var sourceVerseText = string.Join(" ", engineParallelTextRow.SourceSegment);
				output_.WriteLine($"Source: {sourceVerseText}");
				var sourceTokenIds = string.Join(" ", engineParallelTextRow.SourceTokens?
					.Select(token => token.TokenId.ToString()) ?? new string[] { "NONE" });
				output_.WriteLine($"SourceTokenIds: {sourceTokenIds}");

				//display target
				var targetVerseText = string.Join(" ", engineParallelTextRow.TargetSegment);
				output_.WriteLine($"Target: {targetVerseText}");
				var targetTokenIds = string.Join(" ", engineParallelTextRow.TargetTokens?
					.Select(token => token.TokenId.ToString()) ?? new string[] { "NONE" });
				output_.WriteLine($"TargetTokenIds: {targetTokenIds}");
			}
		}

		private static TextRow TextRow(int key, string text = "", bool isSentenceStart = true,
			bool isInRange = false, bool isRangeStart = false)
		{
			return new TextRow(new RowRef(key))
			{
				Segment = text.Length == 0 ? Array.Empty<string>() : text.Split(),
				IsSentenceStart = isSentenceStart,
				IsInRange = isInRange,
				IsRangeStart = isRangeStart,
				IsEmpty = text.Length == 0
			};
		}

		private static TextRow TextRow(VerseRef vref, string text = "", bool isSentenceStart = true,
			bool isInRange = false, bool isRangeStart = false)
		{
			return new TextRow(vref)
			{
				Segment = text.Length == 0 ? Array.Empty<string>() : text.Split(),
				IsSentenceStart = isSentenceStart,
				IsInRange = isInRange,
				IsRangeStart = isRangeStart,
				IsEmpty = text.Length == 0
			};
		}

		private static IEnumerable<RowRef> Refs(params int[] keys)
		{
			return keys.Select(key => new RowRef(key));
		}

		private static IEnumerable<VerseRef> Refs(params VerseRef[] verseRefs)
		{
			return verseRefs;
		}

		private static AlignmentRow AlignmentRow(int key, params AlignedWordPair[] pairs)
		{
			return new AlignmentRow(new RowRef(key))
			{
				AlignedWordPairs = new HashSet<AlignedWordPair>(pairs)
			};
		}
	}
}