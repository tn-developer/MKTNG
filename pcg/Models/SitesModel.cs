using System.ComponentModel.DataAnnotations;
using System.Collections.Generic;

namespace pcg.Models
{
    public class SitesModel
    {
        public string SiteId { get; set; }
        [Required(ErrorMessage = "Client is required.")]
        public string Client { get; set; }
        public string Clientcheck { get; set; }
        [Required(ErrorMessage = "Site is required.")]
        public string Site { get; set; }
        public string SiteOM { get; set; }
        public string SiteSOM { get; set; }
        public string SiteSC { get; set; }
        public string SiteTK { get; set; }
    }
}
