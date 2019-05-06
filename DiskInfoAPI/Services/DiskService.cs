using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using DiskInfoAPI.Models;
using System.IO;

namespace DiskInfoAPI.Services
{
    public class DiskService
    {
        private List<Disk> _Disks;

        public DiskService()
        {
            DriveInfo[] DriveInfos = DriveInfo.GetDrives();
            _Disks = new List<Disk>();

            foreach (DriveInfo d in DriveInfos)
            {
                Disk disk = new Disk();
                disk.Name = d.Name;
                disk.DriveType = d.DriveType.ToString();
                if (d.IsReady)
                {
                    disk.DriveFormat = d.DriveFormat;
                    disk.TotalSize = (d.TotalSize / 1000000000);
                    disk.TotalFreeSpace = (d.TotalFreeSpace / 1000000000);
                }

                _Disks.Add(disk);
            }
        }

        public List<Disk> Get()
        {
            return _Disks;
        }
    }
}
