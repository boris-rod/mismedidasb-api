using Microsoft.AspNetCore.Hosting;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Threading.Tasks;

namespace MismeAPI.Utils
{
    public static class EmailTemplateHelper
    {
        /// <summary>
        /// Create email template string with footer and header
        /// </summary>
        /// <param name="templateName">One of the Templates/template.html files</param>
        /// <param name="headerSubject">Main subject of the email</param>
        /// <param name="_env">Required Host Enviroment parameter</param>
        /// <returns>String template ready to remplace body variables before sending it</returns>
        public static string GetEmailTemplateString(string templateName, string headerSubject, IWebHostEnvironment _env)
        {
            var header = GetHeaderString(_env);
            header = header.ToHeaderEmail(headerSubject);
            var footer = GetFooterString(_env);

            var templateString = GetStringTemplate(templateName, _env);

            return templateString.SetHeaderFooterToTemplate(header, footer);
        }

        private static string GetHeaderString(IWebHostEnvironment _env)
        {
            var templateName = "_header.html";
            var stringTemplate = GetStringTemplate(templateName, _env);

            return stringTemplate;
        }

        private static string GetFooterString(IWebHostEnvironment _env)
        {
            var templateName = "_header.html";
            var stringTemplate = GetStringTemplate(templateName, _env);

            return stringTemplate;
        }

        private static string GetStringTemplate(string templateName, IWebHostEnvironment _env)
        {
            var resource = _env.ContentRootPath
                      + Path.DirectorySeparatorChar.ToString()
                      + "Templates"
                      + Path.DirectorySeparatorChar.ToString()
                      + templateName;
            var reader = new StreamReader(resource);

            var stringTemplate = reader.ReadToEnd();

            reader.Dispose();

            return stringTemplate;
        }
    }
}
