using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Reflection;
using System.Web;
using DocumentFormat.OpenXml.Office2010.ExcelAc;
using PriceCare.Web.Constants;
using PriceCare.Web.Models;

namespace PriceCare.Web.Repository
{
    public class FileRepository
    {
        public FileRepository()
        {
  
        }

        public string StoreFile(HttpPostedFile file)
        {
            return null;
        }

        public void DeleteFile(string storageName)
        {
            
        }

        public byte[] DownloadFile(string storageName)
        {
            return null;
        }

        public string GetStringContent(string storageName)
        {
            return null;
        }

        public List<XlsTemplateViewModel> GetXlsTemplates(string path)
        {
            var result = new List<XlsTemplateViewModel>();
            var files = Directory.GetFiles(path);
            
            foreach (var file in files)
            {
                var fileName = file.Split('\\').Last();
                var templateName = fileName.Split('.')[0];
                var extension = "."+fileName.Split('.')[1];
                result.Add(new XlsTemplateViewModel
                {
                    DisplayName = templateName + string.Format(" ({0})", extension), 
                    FileName = fileName,
                    Extension = extension
                });   
            }

            return result;
        }

        private string FindConstantName<T>(Type containingType, T value)
        {
            EqualityComparer<T> comparer = EqualityComparer<T>.Default;

            foreach (FieldInfo field in containingType.GetFields
                     (BindingFlags.Static | BindingFlags.Public))
            {
                if (field.FieldType == typeof(T) &&
                    comparer.Equals(value, (T)field.GetValue(null)))
                {
                    return field.Name; // There could be others, of course...
                }
            }
            return null; // Or throw an exception
        }
    }
}