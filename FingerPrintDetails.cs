using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Net.Http;
using System.Net.Http.Headers;

namespace ScanAPIDemo
{
    class FingerPrintDetails
    {
        public int Id { get; set; }
        public int FP_ID { get; set; }
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
