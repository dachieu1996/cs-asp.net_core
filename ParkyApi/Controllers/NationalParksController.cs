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
    [Route("api/[controller]")]
    [ApiController]
    [ApiExplorerSettings(GroupName = "ParkyOpenApiSpecNP")]
    public class NationalParksController : ControllerBase
    {
        private readonly INationalParkRepository _repository;
        private readonly IMapper _mapper;

        public NationalParksController(INationalParkRepository repository, IMapper mapper)
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

            var objDto = _repository.GetNationalParks().Select(_mapper.Map<NationalPark, NationalParkDto>);

            return Ok(objDto);
        }

        [HttpGet("{id:int}", Name = "GetNationPark")]
        public IActionResult GetNationPark(int id)
        {
            var obj = _repository.GetNationalPark(id);
            if (obj == null)
                return NotFound();

            var objDto = _mapper.Map<NationalParkDto>(obj);

            return Ok(objDto);
        }

        [HttpPost]
        public IActionResult CreateNationPark([FromBody] NationalParkDto nationalParkDto)
        {
            if (nationalParkDto == null)
                return BadRequest(ModelState);

            if (_repository.NationalParkExists(nationalParkDto.Name))
            {
                ModelState.AddModelError("", "National Park Exists!");
                return NotFound(ModelState);
            }

            var nationalParkObj = _mapper.Map<NationalPark>(nationalParkDto);
            if (!_repository.CreateNationPark(nationalParkObj))
            {
                ModelState.AddModelError("", $"Something went wrong when saving the record {nationalParkObj.Name}");
                return StatusCode(500, ModelState);
            }

            return CreatedAtRoute("GetNationPark", new { id = nationalParkObj.Id }, nationalParkObj);
        }

        [HttpPut("{nationalParkId:int}")]
        public IActionResult UpdateNationalPark(int nationalParkId, NationalParkDto nationalParkDto)
        {
            if (nationalParkDto == null || nationalParkId != nationalParkDto.Id)
                return BadRequest(ModelState);

            var nationalParkObj = _mapper.Map<NationalPark>(nationalParkDto);
            if (!_repository.UpdateNationPark(nationalParkObj))
            {
                ModelState.AddModelError("", $"Something went wrong when updating the record {nationalParkObj.Name}");
                return StatusCode(500, ModelState);
            }

            return NoContent();
        }
        
        [HttpDelete("{nationalParkId:int}")]
        public IActionResult DeleteNationalPark(int nationalParkId)
        {
            if (!_repository.NationParkExists(nationalParkId))
                return NotFound();

            var nationalParkObj = _repository.GetNationalPark(nationalParkId);
            if (!_repository.DeleteNationPark(nationalParkObj))
            {
                ModelState.AddModelError("", $"Something went wrong when deleting the record {nationalParkObj.Name}");
                return StatusCode(500, ModelState);
            }

            return NoContent();
        }
    }
}