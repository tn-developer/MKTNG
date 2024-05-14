using System.ComponentModel.DataAnnotations;
using System.Collections.Generic;

namespace pcg.Models
{
    public class AdminModel
    {
        public string Stringa { get; set; }
        public string Stringb { get; set; }
        public string Stringc { get; set; }
    }
    public class SitesAdmin
    {
        public string SiteId { get; set; }
        [Required(ErrorMessage = "Site Name is required.")]
        public string SiteName { get; set; }
        [Required(ErrorMessage = "Site Location is required.")]
        public string SiteLoc { get; set; }
        public List<SitesAdmin> AdminSitelist { get; set; }
        public string Selectedsite { get; set; }
    }
}
