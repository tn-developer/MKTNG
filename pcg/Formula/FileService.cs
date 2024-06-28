using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using System.IO;
using System.Threading.Tasks;
using System;
using System.Linq;


namespace pcg.Formula
{
    public class FileService
    {
        private readonly string _filePath;

        public FileService(string filePath)
        {
            _filePath = filePath;
        }

        public async Task<(string FilePath, string DirectoryPath, string ErrorMessage)> UploadFileAsync(IFormFile file, string FileName)
        {
            if (file == null || file.Length == 0)
            {
                return (null, null, "Please select a file to upload.");
            }

            var allowedExtensions = new[] { ".xls", ".xlsx", ".docx", ".png", ".jpg", ".jpeg", ".pdf" };
            var extension = Path.GetExtension(file.FileName).ToLower();

            if (!allowedExtensions.Contains(extension))
            {
                return (null, null, "Invalid file type. Only .xls, .xlsx, .docx, .png, .jpg, .jpeg, and .pdf files are allowed.");
            }

            if (extension == ".xlsx")
            {
                FileName = Path.ChangeExtension(FileName, ".xls");
            }
            else
            {
                FileName = Path.ChangeExtension(FileName, extension);
            }

            string datePart = "(" + DateTime.Now.ToString("MMddyy") + ")";
            string customFileName = $"{datePart}{FileName}";
            var filePath = Path.Combine(_filePath, customFileName);

            var directory = Path.GetDirectoryName(filePath);
            if (!Directory.Exists(directory))
            {
                Directory.CreateDirectory(directory);
            }

            using (var fileStream = new FileStream(filePath, FileMode.Create))
            {
                await file.CopyToAsync(fileStream);
            }
            return (filePath, customFileName, null);
        }
        public async Task<FileContentResult> DownloadFileAsync(string fileName)
        {
            var filePath = Path.Combine(_filePath, fileName);

            if (!System.IO.File.Exists(filePath))
            {
                throw new FileNotFoundException("The file was not found.", fileName);
            }

            var fileBytes = await System.IO.File.ReadAllBytesAsync(filePath);
            var contentType = "application/octet-stream"; 

            return new FileContentResult(fileBytes, contentType)
            {
                FileDownloadName = fileName
            };
        }
        public async Task<bool> DeleteFileAsync(string fileName)
        {
            var filePath = Path.Combine(_filePath, fileName);

            if (System.IO.File.Exists(filePath))
            {
                try
                {
                    System.IO.File.Delete(filePath);
                    return true;
                }
                catch (Exception ex)
                {
                    Console.WriteLine($"Error deleting file: {ex.Message}");
                    return false;
                }
            }

            return false;
        }
        public async Task<(bool Success, string ErrorMessage)> RenameFileAsync(string oldFileName, string newFileName)
        {
            var oldFilePath = Path.Combine(_filePath, oldFileName);
            string datePart = "(" + DateTime.Now.ToString("MMddyy") + ")";
            var fileExtension = Path.GetExtension(oldFileName);
            var forname = datePart + newFileName + fileExtension;
            var newFilePath = Path.Combine(_filePath, forname);

            if (!System.IO.File.Exists(oldFilePath))
            {
                return (false, $"File '{oldFileName}' not found.");
            }

            if (System.IO.File.Exists(newFilePath))
            {
                return (false, $"File with name '{newFileName}' already exists.");
            }

            try
            {
                System.IO.File.Move(oldFilePath, newFilePath);
                return (true, null);
            }
            catch (Exception ex)
            {
                return (false, $"Error renaming file: {ex.Message}");
            }
        }
    }
}
