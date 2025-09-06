using MillenniumWebFixed.Models;
using System.Collections.Generic;

namespace MillenniumWebFixed.ViewModels
{
    public class DetailsExcelViewModel
    {
        public GeneralProjectData GeneralData { get; set; }
        public Dictionary<string, List<object>> Sections { get; set; }
        public List<ProjectImage> ProjectImages { get; set; }
    }
}