using MismeAPI.Common.DTO.Response;
using MismeAPI.Common.DTO.Response.User;
using MismeAPI.Data.Entities;
using System.Collections.Generic;

namespace MismeAPI.Service.Utils
{
    public interface IHealthyHelper
    {
        UserEatHealtParametersResponse GetUserEatHealtParameters(User user);

        EatBalancedSummaryResponse IsBalancedPlan(User user, IEnumerable<Eat> dayPlan);
    }
}
