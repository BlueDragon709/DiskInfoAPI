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
        private List<Disk> _Disks = new List<Disk>();

        public DiskService()
        {
            DriveInfo[] DriveInfos = DriveInfo.GetDrives();
            int i = 0;
            foreach (DriveInfo d in DriveInfos)
            {
                i++;
                Disk disk = new Disk();
                disk.Id = i;
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

        public DiskInfo GetDrive(int id)
        {
            Disk disk = _Disks.SingleOrDefault(d => d.Id == id);
            if (disk == null)
            {
                return null;
            }

            DriveInfo info = new DriveInfo(disk.Name);

            DiskInfo drive = new DiskInfo();
            drive.Id = id;
            drive.Name = info.Name;
            drive.VolumeLable = info.VolumeLabel;
            drive.IsReady = info.IsReady;
            drive.DriveType = info.DriveType.ToString();
            drive.DriveFormat = info.DriveFormat;
            drive.TotalSize = info.TotalSize;
            drive.TotalFreeSpace = info.TotalFreeSpace;
            drive.AvailableFreeSpace = info.AvailableFreeSpace;

            return drive;
        }
    }
}
