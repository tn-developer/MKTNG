using System;
using System.ComponentModel.DataAnnotations;

namespace mktng.Models
{
    public class ClientModel
    {
        public int Id { get; set; }

        [Required]
        public string Prospect { get; set; }

        public string Designation { get; set; }

        [Required]
        public string Company { get; set; }

        public string Phone { get; set; }

        public string Email { get; set; }

        [Required]
        public string Status { get; set; }

        [Required]
        public string Type { get; set; }

        public string Notes { get; set; }

        public string Assessment { get; set; }

        public string AddedBy { get; set; }

        public DateTime? DateAdded { get; set; }

        public string DeletedBy { get; set; }

        [DataType(DataType.Date)]
        public DateTime? DateDeleted { get; set; }

        public string ReasonDel { get; set; }

        public string FlowStatus { get; set; }
        public string InquiryType { get; set; }
    }
}
