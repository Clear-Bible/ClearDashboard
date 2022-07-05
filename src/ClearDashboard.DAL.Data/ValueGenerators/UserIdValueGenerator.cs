using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.ChangeTracking;
using Microsoft.EntityFrameworkCore.ValueGeneration;

namespace ClearDashboard.DataAccessLayer.Data.ValueGenerators
{
    
    internal class UserIdValueGenerator : ValueGenerator<Guid>
    {
        public override bool GeneratesTemporaryValues => false;
        public override Guid Next(EntityEntry entry) => GetUserId(entry.Context);
        Guid GetUserId(DbContext context) => ((ProjectDbContext)context).UserProvider.CurrentUser.Id;
    }
}
