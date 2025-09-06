
namespace MillenniumWebFixed.Helpers
{
    public static class SpinnerHelper
    {
        public static string ShowSpinnerScript()
        {
            return "<script>document.getElementById('spinner').style.display = 'block';</script>";
        }

        public static string HideSpinnerScript()
        {
            return "<script>document.getElementById('spinner').style.display = 'none';</script>";
        }
    }
}
