using MillenniumWebFixed.Models;
using System.Collections.Generic;

namespace MillenniumWebFixed.ViewModels
{
    public class DetailsJsonViewModel
    {
        public GeneralQuoteData GeneralData { get; set; }
        public Dictionary<string, List<object>> Sections { get; set; }
        public List<ProjectImage> ProjectImages { get; set; }
    }
}
