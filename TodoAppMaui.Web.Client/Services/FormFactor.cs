using TodoAppMaui.Shared.Services;

namespace TodoAppMaui.Web.Client.Services
{
    public class FormFactor : IFormFactor
    {
        public string GetFormFactor()
        {
            return "WebAssembly";
        }

        public string GetPlatform()
        {
            //Environment.
            return Environment.OSVersion.ToString();
        }
    }
}
