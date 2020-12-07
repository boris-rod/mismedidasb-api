using MismeAPI.Data.Entities;
using System;
using System.Threading.Tasks;

namespace MismeAPI.Service.Utils
{
    public interface IProfileHelthHelper
    {
        Task<(User user, double kcal, double IMC, DateTime? firstHealtMeasured)> GetUserProfileUseAsync(int loggedUser);

        Task<(double kcal, double imc, DateTime? firstHealthMeasure)> GetCurrentKCalIMCFirstHeltMeasureAsync(int userId);
    }
}
