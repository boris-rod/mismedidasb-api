using System;
using System.Collections.Generic;
using System.Text;

namespace MismeAPI.Common.DTO.Request.Reward
{
    public class RewardManualCoinsRequest
    {
        /// <summary>
        /// Id of the users who will receipt the coins (-1 to give the reward to all users)
        /// </summary>
        public IEnumerable<int> UserIds { get; set; }

        /// <summary>
        /// Id of the groups who will receipt the email
        /// </summary>
        public IEnumerable<int> GroupIds { get; set; }

        /// <summary>
        /// Coins of the reward
        /// </summary>
        public int Coins { get; set; }

        /// <summary>
        /// A push notification will be sent to each user using a default title if this parameter is
        /// not provided. Default: "Has recibido una recompensa por ser tester"
        /// </summary>
        public string Title { get; set; }

        /// <summary>
        /// PN title in english
        /// </summary>
        public string TitleEN { get; set; }

        /// <summary>
        /// PN title in Italian
        /// </summary>
        public string TitleIT { get; set; }

        /// <summary>
        /// A push notification will be sent to each user using a default body if this parameter is
        /// not provided. Default: "Gracias por tu aporte. Has recibido {{X}} monedas."
        /// </summary>
        public string Body { get; set; }

        /// <summary>
        /// PN body in english
        /// </summary>
        public string BodyEN { get; set; }

        /// <summary>
        /// PN body in italian
        /// </summary>
        public string BodyIT { get; set; }
    }
}
