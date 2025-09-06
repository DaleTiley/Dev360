
using System.Web.Mvc;

namespace MillenniumWebFixed.Helpers
{
    public static class ToastHelper
    {
        public static void SetToastMessage(this Controller controller, string message, bool isError = false)
        {
            if (isError)
                controller.TempData["Error"] = message;
            else
                controller.TempData["Message"] = message;
        }
    }
}
