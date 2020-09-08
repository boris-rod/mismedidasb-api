using MismeAPI.Common.DTO.Response.User;
using MismeAPI.Data.Entities;
using System;
using System.Collections.Generic;
using System.Text;

namespace MismeAPI.Service.Utils
{
    public class HealthyHelper : IHealthyHelper
    {
        private readonly double _imc;
        private readonly double _dailyKCal;

        public HealthyHelper(double imc, double dailyKCal)
        {
            _imc = imc;
            _dailyKCal = dailyKCal;
        }

        public UserEatHealtParametersResponse GetUserEatHealtParameters(User user)
        {
            return new UserEatHealtParametersResponse
            {
                KCalOffSetVal = KCalOffSetVal,
                KCalMin = KCalMin,
                KCalMax = KCalMax,
                BreakFastCalVal = EatKCalVal(user.BreakFastKCalPercentage),
                BreakFastCalValExtra = EatKCalValExtra(user.BreakFastKCalPercentage),
                Snack1CalVal = EatKCalVal(user.Snack1KCalPercentage),
                Snack1CalValExtra = EatKCalValExtra(user.Snack1KCalPercentage),
                LunchCalVal = EatKCalVal(user.LunchKCalPercentage),
                LunchCalValExtra = EatKCalValExtra(user.LunchKCalPercentage),
                Snack2CalVal = EatKCalVal(user.Snack2KCalPercentage),
                Snack2CalValExtra = EatKCalValExtra(user.Snack2KCalPercentage),
                DinnerCalVal = EatKCalVal(user.DinnerKCalPercentage),
                DinnerCalValExtra = EatKCalValExtra(user.DinnerKCalPercentage),
            };
        }

        private double KCalOffSetVal
        {
            get => (_imc > 18.5 && _imc < 25) ? 100 : 500;
        }

        private double KCalMin
        {
            get => _imc <= 18.5 ? _dailyKCal : (_imc > 18.5 && _imc < 25 ? _dailyKCal - 100 : _dailyKCal - 500);
        }

        private double KCalMax
        {
            get => _imc <= 18.5 ? _dailyKCal + 500 : (_imc > 18.5 && _imc < 25 ? _dailyKCal + 100 : _dailyKCal);
        }

        private double EatKCalVal(int percent)
        {
            return _dailyKCal * percent / 100;
        }

        private double EatKCalValExtra(int percent)
        {
            return KCalOffSetVal * percent / 100;
        }
    }
}
