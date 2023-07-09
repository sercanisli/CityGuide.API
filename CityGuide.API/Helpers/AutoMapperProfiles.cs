using AutoMapper;
using CityGuide.API.DTOs;
using CityGuide.API.Models;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace CityGuide.API.Helpers
{
    public class AutoMapperProfiles:Profile
    {
        public AutoMapperProfiles()
        {
            CreateMap<City, CityForListDTO>()
                .ForMember(dest=>dest.PhotoUrl, opt=> 
                {
                    opt.MapFrom(src => src.Photos.FirstOrDefault(p => p.IsMain).Url);
                });
            CreateMap<City, CityForDetailDTO>();
            CreateMap<Photo, PhotoForCreationDTO>();
            CreateMap<PhotoForReturnDTO, Photo>();
        }
    }
}
