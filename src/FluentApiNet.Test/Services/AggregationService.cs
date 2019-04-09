using FluentApiNet.Core;
using FluentApiNet.Domain;
using FluentApiNet.Test.Entities;
using FluentApiNet.Test.Models;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace FluentApiNet.Test.Services
{
    /// <summary>
    /// Aggregation service class
    /// </summary>
    /// <seealso cref="FluentApiNet.Core.AggregationServiceBase{FluentApiNet.Test.Models.AggregationModel, FluentApiNet.Test.Services.AggregationJoinResult}" />
    public class AggregationService : AggregationServiceBase<AggregationModel, AggregationJoinResult>
    {
        /// <summary>
        /// Initializes a new instance of the <see cref="AggregationService"/> class.
        /// </summary>
        /// <param name="locationService">The location service.</param>
        /// <param name="userService">The user service.</param>
        public AggregationService(LocationService locationService, UserService userService) : base()
        {
            this.LocationService = locationService;
            this.UserService = userService;
            Mappings = new List<MappingAggregator>
            {
                MappingAggregator.Init<AggregationModel, AggregationService>(x => x.User, x => x.UserService),
                MappingAggregator.Init<AggregationModel, AggregationService>(x => x.Locations, x => x.LocationService),
            };
        }

        /// <summary>
        /// Gets or sets the location service.
        /// </summary>
        /// <value>
        /// The location service.
        /// </value>
        public LocationService LocationService { get; set; }

        /// <summary>
        /// Gets or sets the user service.
        /// </summary>
        /// <value>
        /// The user service.
        /// </value>
        public UserService UserService { get; set; }

        /// <summary>
        /// Joins the query.
        /// </summary>
        /// <returns></returns>
        protected override IQueryable<AggregationJoinResult> JoinQuery()
        {
            var queryUser = RetrieveQuery<User>(x => x.User);
            var queryLocation = RetrieveQuery<Location>(x => x.Locations);

            var query = queryUser;
            if(HaveClause(x => x.Locations))
            {
                query = query.Where(x => queryLocation.Any(y => y.UserId == x.Id));
            }
            return query
                .GroupJoin(queryLocation,
                    x => x.Id,
                    x => x.UserId,
                    (x, Locations) => new AggregationJoinResult { User = x, Locations = Locations });
        }

        /// <summary>
        /// Orders the query.
        /// </summary>
        /// <param name="query"></param>
        /// <returns></returns>
        protected override IOrderedQueryable<AggregationJoinResult> OrderQuery(IQueryable<AggregationJoinResult> query)
        {
            var operations = new Operations<AggregationModel>();
            operations.OrderBy.Add(x => x.User.IdModel);
            return query.OrderBy(x => x.User.Id);
        }
    }

    /// <summary>
    /// Aggregation join result class
    /// </summary>
    public class AggregationJoinResult
    {
        /// <summary>
        /// Gets or sets the user.
        /// </summary>
        /// <value>
        /// The user.
        /// </value>
        public User User { get; set; }

        /// <summary>
        /// Gets or sets the locations.
        /// </summary>
        /// <value>
        /// The locations.
        /// </value>
        public IEnumerable<Location> Locations { get; set; }
    }
}
