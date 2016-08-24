using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Mvc.Rendering;
using SkyDriftCoreWeb.Controllers;

namespace SkyDriftCoreWeb.Models.StaticsModels
{
    public class RecordQueryModel
    {
        public List<Record> Records { get; set; }
        public Course Course { get; set; }
    }
}
