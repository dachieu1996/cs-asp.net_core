using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Mvc.Rendering;

namespace ParkyWeb.Models.ViewModels
{
    public class TrailViewModel
    {
        public Trail Trail { get; set; }
        public IEnumerable<SelectListItem> NationalParks { get; set; }
    }
}
