using Microsoft.AspNetCore.Mvc;

namespace Harvest.Web.Controllers
{
    public abstract class SuperController : Controller
    {
        private const string TempDataMessageKey = "Message";
        private const string TempDataErrorMessageKey = "ErrorMessage";
       
        public string Message
        {
            get => TempData[TempDataMessageKey] as string;
            set => TempData[TempDataMessageKey] = value;
        }

        public string ErrorMessage
        {
            get => TempData[TempDataErrorMessageKey] as string;
            set => TempData[TempDataErrorMessageKey] = value;
        }
    }
}
