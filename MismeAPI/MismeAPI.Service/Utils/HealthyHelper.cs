using MismeAPI.Common.DTO.Response;
using MismeAPI.Common.DTO.Response.User;
using MismeAPI.Data.Entities;
using MismeAPI.Data.Entities.Enums;
using System.Collections.Generic;

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

        public EatBalancedSummaryResponse IsBalancedPlan(User user, IEnumerable<Eat> dayPlan)
        {
            var parameters = GetUserEatHealtParameters(user);

            double currentCaloriesSum = 0;
            double currentProteinsSum = 0;
            double currentCarbohydratesSum = 0;
            double currentFatSum = 0;
            double currentFiberSum = 0;

            bool breakfastKcal = false;
            bool snack1Kcal = false;
            bool lunchKcal = false;
            bool snack2Kcal = false;
            bool dinnerKcal = false;

            double breakfastKcalPer = 0.0;
            double snack1KcalPer = 0.0;
            double lunchKcalPer = 0.0;
            double snack2KcalPer = 0.0;
            double dinnerKcalPer = 0.0;

            foreach (var eat in dayPlan)
            {
                double eatTypeCalories = 0;

                foreach (var eatDish in eat.EatDishes)
                {
                    var dishCals = eatDish.Dish.Calories.HasValue ? eatDish.Dish.Calories.Value : 0;
                    var calories = dishCals * eatDish.Qty;
                    eatTypeCalories += calories;

                    var dishProteins = eatDish.Dish.Proteins.HasValue ? eatDish.Dish.Proteins.Value : 0;
                    var proteins = dishProteins * eatDish.Qty;
                    currentProteinsSum += proteins;

                    var dishCarbs = eatDish.Dish.Carbohydrates.HasValue ? eatDish.Dish.Carbohydrates.Value : 0;
                    var carbohydrates = dishCarbs * eatDish.Qty;
                    currentCarbohydratesSum += carbohydrates;

                    var dishFats = eatDish.Dish.Fat.HasValue ? eatDish.Dish.Fat.Value : 0;
                    var fats = dishFats * eatDish.Qty;
                    currentFatSum += fats;

                    var dishFibs = eatDish.Dish.Fiber.HasValue ? eatDish.Dish.Fiber.Value : 0;
                    var fibers = dishFibs * eatDish.Qty;
                    currentFiberSum += fibers;
                }

                foreach (var eatCompoundDish in eat.EatCompoundDishes)
                {
                    foreach (var dishCompoundDish in eatCompoundDish.CompoundDish.DishCompoundDishes)
                    {
                        var dishCals = dishCompoundDish.Dish.Calories.HasValue ? dishCompoundDish.Dish.Calories.Value : 0;
                        var calories = dishCals * eatCompoundDish.Qty * dishCompoundDish.DishQty;
                        eatTypeCalories = calories;

                        var dishProteins = dishCompoundDish.Dish.Proteins.HasValue ? dishCompoundDish.Dish.Proteins.Value : 0;
                        var proteins = dishProteins * eatCompoundDish.Qty * dishCompoundDish.DishQty;
                        currentProteinsSum += proteins;

                        var dishCarbs = dishCompoundDish.Dish.Carbohydrates.HasValue ? dishCompoundDish.Dish.Carbohydrates.Value : 0;
                        var carbohydrates = dishCarbs * eatCompoundDish.Qty * dishCompoundDish.DishQty;
                        currentProteinsSum += carbohydrates;

                        var dishFats = dishCompoundDish.Dish.Fat.HasValue ? dishCompoundDish.Dish.Fat.Value : 0;
                        var fats = dishFats * eatCompoundDish.Qty * dishCompoundDish.DishQty;
                        currentFatSum += fats;

                        var dishFibs = dishCompoundDish.Dish.Fiber.HasValue ? dishCompoundDish.Dish.Fiber.Value : 0;
                        var fibers = dishFibs * eatCompoundDish.Qty * dishCompoundDish.DishQty;
                        currentFiberSum += fibers;
                    }
                }

                currentCaloriesSum += eatTypeCalories;

                switch (eat.EatType)
                {
                    case EatTypeEnum.BREAKFAST:
                        breakfastKcalPer = GetEatTypeKcalPer(eatTypeCalories, parameters.BreakFastCalVal, parameters.BreakFastCalValExtra);
                        breakfastKcal = GetEatTypeKcal(eatTypeCalories, parameters.BreakFastCalVal, parameters.BreakFastCalValExtra, breakfastKcalPer);

                        break;

                    case EatTypeEnum.SNACK1:
                        snack1KcalPer = GetEatTypeKcalPer(eatTypeCalories, parameters.Snack1CalVal, parameters.Snack1CalValExtra);
                        snack1Kcal = GetEatTypeKcal(eatTypeCalories, parameters.Snack1CalVal, parameters.Snack1CalValExtra, snack1KcalPer);
                        break;

                    case EatTypeEnum.LUNCH:
                        lunchKcalPer = GetEatTypeKcalPer(eatTypeCalories, parameters.LunchCalVal, parameters.LunchCalValExtra);
                        lunchKcal = GetEatTypeKcal(eatTypeCalories, parameters.LunchCalVal, parameters.LunchCalValExtra, lunchKcalPer);
                        break;

                    case EatTypeEnum.SNACK2:
                        snack2KcalPer = GetEatTypeKcalPer(eatTypeCalories, parameters.Snack2CalVal, parameters.Snack2CalValExtra);
                        snack2Kcal = GetEatTypeKcal(eatTypeCalories, parameters.Snack2CalVal, parameters.Snack2CalValExtra, snack2KcalPer);
                        break;

                    case EatTypeEnum.DINNER:
                        dinnerKcalPer = GetEatTypeKcalPer(eatTypeCalories, parameters.DinnerCalVal, parameters.DinnerCalValExtra);
                        dinnerKcal = GetEatTypeKcal(eatTypeCalories, parameters.DinnerCalVal, parameters.DinnerCalValExtra, dinnerKcalPer);
                        break;

                    default:
                        break;
                }
            }

            bool isKcalAllOk = currentCaloriesSum > parameters.KCalMin && currentCaloriesSum <= parameters.KCalMax;

            double proteinPer = currentProteinsSum * 100 / (parameters.KCalMax * 25 / 100);
            bool isProteinsOk = proteinPer <= 100 && proteinPer > (12 * 100 / 25);

            double carbohydratesPer = currentCarbohydratesSum * 100 / (parameters.KCalMax * 55 / 100);
            bool isCarbohydratesOk = carbohydratesPer <= 100 && carbohydratesPer > 35 * 100 / 55;

            double fatPer = currentFatSum * 100 / (parameters.KCalMax * 35 / 100);
            bool isFatOk = fatPer <= 100 && fatPer > 20 * 100 / 35;

            double fiberPer = currentFiberSum * 100 / 50;
            bool isFiberOk = fiberPer <= 100 && fiberPer > 30 * 100 / 50;

            bool isBalanced = isKcalAllOk && isProteinsOk && isCarbohydratesOk && isFatOk && isFiberOk && breakfastKcal && snack1Kcal && lunchKcal && snack2Kcal && dinnerKcal;

            var summary = new EatBalancedSummaryResponse
            {
                BreakfastKcalPer = breakfastKcalPer,
                CarbohydratesPer = carbohydratesPer,
                CurrentCaloriesSum = currentCaloriesSum,
                CurrentCarbohydratesSum = currentCarbohydratesSum,
                CurrentFatSum = currentFatSum,
                CurrentFiberSum = currentFiberSum,
                CurrentProteinsSum = currentProteinsSum,
                DinnerKcalPer = dinnerKcalPer,
                FatPer = fatPer,
                FiberPer = fiberPer,
                IsBalanced = isBalanced,
                IsBreakfastKcalOk = breakfastKcal,
                IsCarbohydratesOk = isCarbohydratesOk,
                IsDinnerKcalOk = dinnerKcal,
                IsFatOk = isFatOk,
                IsFiberOk = isFiberOk,
                IsKcalAllOk = isKcalAllOk,
                IsLunchKcalOk = lunchKcal,
                IsProteinsOk = isProteinsOk,
                IsSnack1KcalOk = snack1Kcal,
                IsSnack2KcalOk = snack2Kcal,
                LunchKcalPer = lunchKcalPer,
                ProteinPer = proteinPer,
                Snack1KcalPer = snack1KcalPer,
                Snack2KcalPer = snack2KcalPer
            };

            return summary;
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

        private double GetEatTypeKcalPer(double calories, double calVal, double calValExtra)
        {
            return calories * 100 / (calVal + calValExtra);
        }

        private bool GetEatTypeKcal(double calories, double calVal, double calValExtra, double caloriesPer)
        {
            return calories >= (calVal - calValExtra) && caloriesPer < 101;
        }
    }
}
