using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Linq;
using System.Web;

namespace API.Models
{
    public class FingerPrintDetails
    {
        [Key]
        public int Id { get; set; }
        [Required]
        public int FP_ID { get; set; }
        [Required]
        public String FP_NAME { get; set; }
        public byte[] FP_BLOB01 { get; set; }
        public byte[] FP_BLOB02 { get; set; }
        public byte[] FP_BLOB03 { get; set; }
        public byte[] FP_BLOB04 { get; set; }
        public byte[] FP_BLOB05 { get; set; }
        public byte[] FP_BLOB06 { get; set; }
        public byte[] FP_BLOB07 { get; set; }
        public byte[] FP_BLOB08 { get; set; }
        public byte[] FP_BLOB09 { get; set; }
        public byte[] FP_BLOB10 { get; set; }
    }
}