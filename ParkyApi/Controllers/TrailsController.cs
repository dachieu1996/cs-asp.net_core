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
    [Route("api/trails")]
    [ApiController]
    [ApiExplorerSettings(GroupName = "ParkyOpenApiSpecTrails")]
    public class TrailsController : ControllerBase
    {
        private readonly ITrailRepository _repository;
        private readonly IMapper _mapper;

        public TrailsController(ITrailRepository repository, IMapper mapper)
        {
            _repository = repository;
            _mapper = mapper;
        }

        [HttpGet]
        public IActionResult GetTrails()
        {
            //var objList = _repository.GetTrails();
            //var objDto = new List<TrailDto>();
            //foreach (var obj in objList)
            //{
            //    objDto.Add(_mapper.Map<TrailDto>(obj));
            //}

            var objDto = _repository.GetTrails().Select(_mapper.Map<Trail, TrailDto>);

            return Ok(objDto);
        }

        [HttpGet("{id:int}", Name = "GetTrail")]
        public IActionResult GetTrail(int id)
        {
            var obj = _repository.GetTrail(id);
            if (obj == null)
                return NotFound();

            var objDto = _mapper.Map<TrailDto>(obj);

            return Ok(objDto);
        }

        [HttpPost]
        public IActionResult CreateTrail([FromBody] TrailCreateDto trailCreateDto)
        {
            if (trailCreateDto == null)
                return BadRequest(ModelState);
            if (!_repository.NationParkExists(trailCreateDto.NationalParkId))
            {
                ModelState.AddModelError("", $"National Park Id {trailCreateDto.NationalParkId} doesn't exist");
                return NotFound(ModelState);
            }

            if (_repository.TrailExists(trailCreateDto.Name))
            {
                ModelState.AddModelError("", "Trail Exists!");
                return NotFound(ModelState);
            }

            var trailObj = _mapper.Map<Trail>(trailCreateDto);
            if (!_repository.CreateTrail(trailObj))
            {
                ModelState.AddModelError("", $"Something went wrong when saving the record {trailObj.Name}");
                return StatusCode(500, ModelState);
            }

            return CreatedAtRoute("GetTrail", new { id = trailObj.Id }, trailObj);
        }

        [HttpPut("{trailId:int}")]
        public IActionResult UpdateTrail(int trailId, TrailUpdateDto trailUpdateDto)
        {
            if (trailUpdateDto == null || trailId != trailUpdateDto.Id)
                return BadRequest(ModelState);
            if (!_repository.NationParkExists(trailUpdateDto.NationalParkId))
            {
                ModelState.AddModelError("", $"National Park Id {trailUpdateDto.NationalParkId} doesn't exist");
                return NotFound(ModelState);
            }

            var trailObj = _mapper.Map<Trail>(trailUpdateDto);
            if (!_repository.UpdateTrail(trailObj))
            {
                ModelState.AddModelError("", $"Something went wrong when updating the record {trailObj.Name}");
                return StatusCode(500, ModelState);
            }

            return NoContent();
        }
        
        [HttpDelete("{trailId:int}")]
        public IActionResult DeleteTrail(int trailId)
        {
            if (!_repository.TrailExists(trailId))
                return NotFound();

            var trailObj = _repository.GetTrail(trailId);
            if (!_repository.DeleteTrail(trailObj))
            {
                ModelState.AddModelError("", $"Something went wrong when deleting the record {trailObj.Name}");
                return StatusCode(500, ModelState);
            }

            return NoContent();
        }
    }
}