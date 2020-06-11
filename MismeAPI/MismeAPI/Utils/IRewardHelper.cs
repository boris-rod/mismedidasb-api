using MismeAPI.Common.DTO.Response.Reward;
using MismeAPI.Data.Entities;
using MismeAPI.Data.Entities.Enums;
using System.Collections.Generic;
using System.Threading.Tasks;

namespace MismeAPI.Utils
{
    public interface IRewardHelper
    {
        Task<RewardResponse> HandleRewardAsync(RewardCategoryEnum category, int targetUser, bool isPlus, object entity1, object entity2,
            NotificationTypeEnum notificationType = NotificationTypeEnum.SIGNAL_R, IEnumerable<Device> devices = null);
    }
}
