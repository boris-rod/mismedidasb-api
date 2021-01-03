using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Text;

namespace MismeAPI.Common.DTO.Request
{
    public class SendEmailRequest
    {
        /// <summary>
        /// Id of the users who will receipt the email (-1 to send email to all users)
        /// </summary>
        public IEnumerable<int> UserIds { get; set; }

        /// <summary>
        /// Spanish email subject. Required
        /// </summary>
        [Required(AllowEmptyStrings = false)]
        public string Subject { get; set; }

        /// <summary>
        /// English email subject.
        /// </summary>
        public string SubjectEN { get; set; }

        /// <summary>
        /// Italian email subject.
        /// </summary>
        public string SubjectIT { get; set; }

        /// <summary>
        /// Spanish Emails body. Required
        /// </summary>
        [Required(AllowEmptyStrings = false)]
        public string Body { get; set; }

        /// <summary>
        /// English Emails body. Required
        /// </summary>
        public string BodyEN { get; set; }

        /// <summary>
        /// Italian Emails body. Required
        /// </summary>
        public string BodyIT { get; set; }
    }
}
