using CityGuide.API.Models;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace CityGuide.API.DTOs
{
    public class CityForDetailDTO
    {
        public int Id { get; set; }
        public string Name { get; set; }
        public string Descriptions { get; set; }

        public List<Photo> Photos { get; set; }
    }
}
