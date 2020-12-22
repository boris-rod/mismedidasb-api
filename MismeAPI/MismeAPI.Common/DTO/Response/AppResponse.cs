namespace MismeAPI.Common.DTO.Response
{
    public class AppResponse
    {
        public string Version { get; set; }
        public bool IsMandatory { get; set; }

        public string VersionIOS { get; set; }
        public bool IsMandatoryIOS { get; set; }
    }
}