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
        public LocationService(TestDbContext context)
        {
            Context = context;
            AddMapping(x => x.Id, x => x.Id, true);
            AddMapping(x => x.Name, x => x.Name);
        }

    }
}
