using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using AutoMapper;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using ParkyApi.Models;
using ParkyApi.Models.Dtos;
using ParkyApi.Models.Repository.IRepository;

namespace ParkyApi.Controllers
{
    [Route("api/v{version:apiVersion}/nationalparks")]
    [ApiVersion("2.0")]
    [ApiController]
    //[ApiExplorerSettings(GroupName = "ParkyOpenApiSpecNP")]
    public class NationalParksV2Controller : ControllerBase
    {
        private readonly INationalParkRepository _repository;
        private readonly IMapper _mapper;

        public NationalParksV2Controller(INationalParkRepository repository, IMapper mapper)
        {
            _repository = repository;
            _mapper = mapper;
        }

        [HttpGet]
        public IActionResult GetNationalParks()
        {
            //var objList = _repository.GetNationalParks();
            //var objDto = new List<NationalParkDto>();
            //foreach (var obj in objList)
            //{
            //    objDto.Add(_mapper.Map<NationalParkDto>(obj));
            //}

            var objDto = _repository.GetNationalParks()
                .Select(_mapper.Map<NationalPark, NationalParkDto>)
                .FirstOrDefault();

            return Ok(objDto);
        }
    }
}