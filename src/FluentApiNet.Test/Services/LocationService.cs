using System.Linq;
using FluentApiNet.Core;
using FluentApiNet.Test.Database;
using FluentApiNet.Test.Entities;
using FluentApiNet.Test.Models;

namespace FluentApiNet.Test.Services
{
    /// <summary>
    /// Location service class
    /// </summary>
    /// <seealso cref="FluentApiNet.Core.ServiceBase{FluentApiNet.Test.Models.LocationModel, FluentApiNet.Test.Entities.Location, FluentApiNet.Test.Database.TestDbContext}" />
    public class LocationService : ServiceBase<LocationModel, Location, TestDbContext>
    {
        /// <summary>
        /// Initializes a new instance of the <see cref="LocationService"/> class.
        /// </summary>
        /// <param name="context">The context.</param>
        public LocationService(TestDbContext context) : base()
        {
            Context = context;
            AddMapping(x => x.Id, x => x.Id, true);
            AddMapping(x => x.Name, x => x.Name);
        }

        /// <summary>
        /// Orders the query if none order by is passed by operations.
        /// </summary>
        /// <param name="query"></param>
        /// <returns></returns>
        protected override IOrderedQueryable<Location> OrderQuery(IQueryable<Location> query)
        {
            return query.OrderBy(x => x.Id);
        }
    }
}
