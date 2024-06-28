using System.ComponentModel.DataAnnotations;
using System.Collections.Generic;
using Microsoft.AspNetCore.Http;
using System;
using System.Web;

namespace pcg.Models
{
    public class VariationModel
    {
        [Required(ErrorMessage = "Task cannot be empty.")]
        public string Task { get; set; }
        public string TaskId { get; set; }
        public string Details { get; set; }
        [Required(ErrorMessage = "Description cannot be empty.")]
        public string Description { get; set; }
        public string Descquery { get; set; }
        public string Descvary { get; set; }
        public string Descdocreq { get; set; }
        public string AssignId { get; set; }
        public string SiteReqId { get; set; }
        public string AddedBy { get; set; }
        public string DateStart { get; set; }
        public string DateFwd { get; set; }
        public string DateRcv { get; set; }
        public string DateClr { get; set; }
        public string Process { get; set; }
        public string TaskType { get; set; }
        [DataType(DataType.Upload)]
        public IFormFile UploadFile { get; set; }
        [Required(ErrorMessage = "File name cannot be empty.")]
        public string FileAlias { get; set; }
    }
    public class ProcessModel
    {
        public string SiteId { get; set; }
        public string SiteReqId { get; set; }
        public string Process { get; set; }
        public string Code { get; set; }
        public string TaskId { get; set; }
        public string FwdId { get; set; }
        public string AssignId { get; set; }
        public string Task { get; set; }
        public string Details { get; set; }
        public string Status { get; set; }
        public string Circulation { get; set; }
        public string Description { get; set; }
        public string Descquery { get; set; }
        public string Descvary { get; set; }
        public string Descdocreq { get; set; }
        public string Comment { get; set; }
        public string SiteSC { get; set; }
        public string SiteTK { get; set; }
    }
    public class FileModel
    {
        public int FileId { get; set; }
        public string FileName { get; set; }
        public string FileAlias { get; set; }
        [DataType(DataType.Upload)]
        public IFormFile UploadFile { get; set; }
        public int SiteId { get; set; }
        public int TaskId { get; set; }
        public string Task { get; set; }
        public string Site { get; set; }
        public string Client { get; set; }
    }
    public class Downloads
    {
        public int FileId { get; set; }
        public string FileName { get; set; }
        public string FileAlias { get; set; }
        public string Task { get; set; }
        public int TaskId { get; set; }
        public string Name { get; set; }
        public DateTime DateAdded { get; set; }
        public string Status { get; set; }
    }
}