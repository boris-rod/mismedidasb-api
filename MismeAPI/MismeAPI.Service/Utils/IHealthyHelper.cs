using MismeAPI.Common.DTO.Response.User;
using MismeAPI.Data.Entities;
using System;
using System.Collections.Generic;
using System.Text;

namespace MismeAPI.Service.Utils
{
    public interface IHealthyHelper
    {
        UserEatHealtParametersResponse GetUserEatHealtParameters(User user);
    }
}
