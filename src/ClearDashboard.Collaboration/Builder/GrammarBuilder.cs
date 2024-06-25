using ClearDashboard.Collaboration.Model;
using ClearDashboard.DataAccessLayer.Data;
using Models = ClearDashboard.DataAccessLayer.Models;

namespace ClearDashboard.Collaboration.Builder;

public class GrammarBuilder : GeneralModelBuilder<Models.Grammar>
{
    public override string IdentityKey => BuildPropertyRefName();

	public override IReadOnlyDictionary<string, Type> AddedPropertyNamesTypes =>
		new Dictionary<string, Type>()
		{
			{ BuildPropertyRefName(), typeof(string) }
		};

	public Func<ProjectDbContext, IEnumerable<Models.Grammar>> GetGrammar = (projectDbContext) =>
    {
        return projectDbContext.Grammars.OrderBy(e => e.ShortName).ToList();
    };

    public override IEnumerable<GeneralModel<Models.Grammar>> BuildModelSnapshots(BuilderContext builderContext)
    {
		var modelSnapshots = new GeneralListModel<GeneralModel<Models.Grammar>>();

		GetGrammar(builderContext.ProjectDbContext).ToList().ForEach(e =>
		{
			var modelSnapshotProperties = ExtractUsingModelRefs(
				e,
				builderContext,
				builderContext.CommonIgnoreProperties.Union(new List<string>() { nameof(Models.IdentifiableEntity.Id) }));

			var snapshot = new GeneralModel<Models.Grammar>(IdentityKey, HashPartsToRef(nameof(Models.Grammar), e.ShortName));
			AddPropertyValuesToGeneralModel(snapshot, modelSnapshotProperties);

			modelSnapshots.Add(snapshot);
		});

        return modelSnapshots;
    }
}