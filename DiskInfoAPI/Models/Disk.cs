using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace DiskInfoAPI.Models
{
    public class Disk
    {
        public int Id { get; set; }
        public string Name { get; set; }
        public string DriveType { get; set; }
        public string DriveFormat { get; set; }
        public long TotalSize { get; set; }
        public long TotalFreeSpace { get; set; }
    }

    public class DiskInfo
    {
        public int Id { get; set; }
        public string Name { get; set; }
        public string VolumeLable { get; set; }
        public bool IsReady { get; set; }
        public string DriveType { get; set; }
        public string DriveFormat { get; set; }
        public long TotalSize { get; set; }
        public long TotalFreeSpace { get; set; }
        public long AvailableFreeSpace { get; set; }
    }
}
