using System;
using System.ComponentModel.DataAnnotations;

namespace GTI_Solutionx.Models.Dashboard
{
    public class ErrorViewModel
    {
        [Key]
        public string RequestId { get; set; }

        public bool ShowRequestId => !string.IsNullOrEmpty(RequestId);
    }
}