using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using DiskInfoAPI.Models;
using DiskInfoAPI.Services;
using System.IO;

namespace DiskInfoAPI.Controllers
{
    [Route("api/[controller]")]
    [ApiController]
    public class DiskController : ControllerBase
    {
        private readonly DiskService _diskService;

        public DiskController(DiskService diskService)
        {
            _diskService = diskService;
        }

        [HttpGet]
        public List<Disk> Get()
        {
            return _diskService.Get();
        }

        [HttpGet("{id}")]
        public ActionResult<DiskInfo> GetDriveInfo(int id)
        {
            DiskInfo drive = _diskService.GetDrive(id);

            if(drive == null)
            {
                return NotFound();
            }

            return drive;
        }
    }
}