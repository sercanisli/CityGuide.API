 using System;
using System.Collections.Generic;
using System.Linq;
using System.Security.Claims;
using System.Threading.Tasks;
using AutoMapper;
using CityGuide.API.Data;
using CityGuide.API.DTOs;
using CityGuide.API.Helpers;
using CityGuide.API.Models;
using CloudinaryDotNet;
using CloudinaryDotNet.Actions;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Options;

namespace CityGuide.API.Controllers
{
    [Produces("application/json")]
    [Route("api/cities/{id}/photos")]
    [ApiController]
    public class PhotosController : ControllerBase
    {
        private IAppRepository _appRepository;
        private IMapper _mapper;
        private IOptions<CloudinarySetting> _cloudinarySetting;
        private Cloudinary _cloudinary;
        
        public PhotosController(IAppRepository appRepository, IMapper mapper, IOptions<CloudinarySetting> cloudinarySetting)
        {
            _appRepository = appRepository;
            _mapper = mapper;
            _cloudinarySetting = cloudinarySetting;

            Account account = new Account(
                _cloudinarySetting.Value.CloudName,
                _cloudinarySetting.Value.APIKey,
                _cloudinarySetting.Value.APISecret
                );
            _cloudinary = new Cloudinary(account);
        }

        [HttpPost]
        public ActionResult AddPhotoForCity(int cityId,[FromBody]PhotoForCreationDTO photoForCreationDTO)
        {
            var city = _appRepository.GetCityById(cityId);
            if(city==null)
            {
                return BadRequest("Could not find the city");
            }
            var currentUserId = int.Parse( User.FindFirst(ClaimTypes.NameIdentifier).Value);
            if(currentUserId!=city.UserId)
            { 
                return Unauthorized();
            }
            var file = photoForCreationDTO.File;
            var uploadResult = new ImageUploadResult();
            if(file.Length>0)
            {
                using (var stream = file.OpenReadStream())
                {
                    var uploadParams = new ImageUploadParams
                    {
                        File = new FileDescription(file.Name, stream)
                    };
                    uploadResult = _cloudinary.Upload(uploadParams);
                }
            }
            photoForCreationDTO.Url = uploadResult.Uri.ToString();
            photoForCreationDTO.PublicId = uploadResult.PublicId;

            var photo = _mapper.Map<Photo>(photoForCreationDTO); 
            photo.City = city;

            if(!city.Photos.Any(p=>p.IsMain))
            {
                photo.IsMain = true;
            }
            city.Photos.Add(photo);

            if(_appRepository.SaveAll())
            {
                var photoToReturn = _mapper.Map<PhotoForReturnDTO>(photo);
                return CreatedAtRoute("GetPhoto", new { id = photo.Id, photoToReturn });
            }
            return BadRequest("Could not add the photo");
        }
        [HttpGet("{id}", Name ="GetPhoto")]
        public ActionResult GetPhoto(int id)
        {
            var photoFromDb = _appRepository.GetPhoto(id);
            var photo = _mapper.Map<PhotoForReturnDTO>(photoFromDb);
            return Ok(photo);
        }
           
    }
}