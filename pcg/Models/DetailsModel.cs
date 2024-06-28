using System.ComponentModel.DataAnnotations;
using System.Collections.Generic;

namespace pcg.Models
{
    public class DetailsModel
    {
        public string TaskId { get; set; }
        public string SiteId { get; set; }
        public string Client { get; set; }
        public string Site { get; set; }
        public string SiteOM { get; set; }
        public string SiteSOM { get; set; }
        public string SiteSC { get; set; }
        public string SiteTK { get; set; }
        public string Details { get; set; }
        public string Task { get; set; }
        public string Description { get; set; }
        public string AddedBy { get; set; }
        public string Circulation { get; set; }
        public string AssignId { get; set; }
        public string Type { get; set; }
        public string Status { get; set; }
        public string ForwardId { get; set; }
    }
}
