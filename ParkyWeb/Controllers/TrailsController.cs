using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Rendering;
using ParkyWeb.Models;
using ParkyWeb.Models.ViewModels;
using ParkyWeb.Repository.Interface;

namespace ParkyWeb.Controllers
{
    [Authorize]
    public class TrailsController : Controller
    {
        private readonly ITrailRepository _trailRepository;
        private readonly INationalParkRepository _npRepository;

        public TrailsController(ITrailRepository trailRepository, INationalParkRepository npRepository)
        {
            _trailRepository = trailRepository;
            _npRepository = npRepository;
        }
        public IActionResult Index()
        {
            return View();
        }

        [Authorize(Roles = "Admin")]
        public async Task<IActionResult> Create()
        {
            var obj = new TrailViewModel
            {
                Trail = new Trail(),
                NationalParks = await GetAllNationalParkName()
            };
            return View("Upsert", obj);
        }

        [Authorize(Roles = "Admin")]
        public async Task<IActionResult> Update(int id)
        {
            var trail = await _trailRepository.GetAsync(StaticDetails.TrailAPIUrl, id, HttpContext.Session.GetString("JWToken"));
            if (trail == null)
                return NotFound();

            
            var trailViewModel = new TrailViewModel
            {
                Trail = trail,
                NationalParks = await GetAllNationalParkName()
            };

            return View("Upsert", trailViewModel);
        }

        public async Task<IEnumerable<SelectListItem>> GetAllNationalParkName()
        {
            var npList = await _npRepository.GetAllAsync(StaticDetails.NationalParkAPIUrl, HttpContext.Session.GetString("JWToken"));
            return npList.Select(n => new SelectListItem
            {
                Text = n.Name,
                Value = n.Id.ToString()
            });
        }
        public async Task<IActionResult> GetAllTrail()
        {
            return Json(new { data = await _trailRepository.GetAllAsync(StaticDetails.TrailAPIUrl, HttpContext.Session.GetString("JWToken")) });
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Save(TrailViewModel trailViewModel)
        {
            if (!ModelState.IsValid)
            {
                trailViewModel.NationalParks = await GetAllNationalParkName();
                return View("Upsert", trailViewModel);
            }


            if (trailViewModel.Trail.Id == 0)
                await _trailRepository.CreateAsync(StaticDetails.TrailAPIUrl, trailViewModel.Trail, HttpContext.Session.GetString("JWToken"));
            else
                await _trailRepository.UpdateAsync(StaticDetails.TrailAPIUrl + trailViewModel.Trail.Id, trailViewModel.Trail, HttpContext.Session.GetString("JWToken"));

            return RedirectToAction("Index");
        }

        [HttpDelete]
        public async Task<IActionResult> Delete(int id)
        {
            var status = await _trailRepository.DeleteAsync(StaticDetails.TrailAPIUrl, id, HttpContext.Session.GetString("JWToken"));
            if (status)
                return Json(new { success = true, message = "Delete Successful" });
            return Json(new { success = false, message = "Delete Not Successful" });
        }
    }
}