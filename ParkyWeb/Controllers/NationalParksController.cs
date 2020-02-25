using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using ParkyWeb.Models;
using ParkyWeb.Repository.Interface;

namespace ParkyWeb.Controllers
{
    [Authorize]
    public class NationalParksController : Controller
    {
        private readonly INationalParkRepository _repository;

        public NationalParksController(INationalParkRepository repository)
        {
            _repository = repository;
        }
        public IActionResult Index()
        {
            return View();
        }
        
        [Authorize(Roles = "Admin")]
        public IActionResult Create()
        {
            var obj = new NationalPark();
            return View("Upsert", obj);
        }

        [Authorize(Roles = "Admin")]
        public async Task<IActionResult> Update(int id)
        {
            var obj = await _repository.GetAsync(StaticDetails.NationalParkAPIUrl, id, HttpContext.Session.GetString("JWToken"));
            if (obj == null)
                return NotFound();
            return View("Upsert", obj);
        }

        public async Task<IActionResult> GetAllNationalPark()
        {
            return Json(new {data = await _repository.GetAllAsync(StaticDetails.NationalParkAPIUrl, HttpContext.Session.GetString("JWToken")) });
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult>Save(NationalPark nationalPark)
        {
            if (!ModelState.IsValid)
                return View("Upsert", nationalPark);

            var files = HttpContext.Request.Form.Files;
            if (files.Count > 0)
            {
                byte[] p = null;
                using (var fs = files[0].OpenReadStream())
                    using (var ms = new MemoryStream())
                    {
                        fs.CopyTo(ms);
                        p = ms.ToArray();
                    }
                nationalPark.Picture = p;
            }
            else
            {
                var nationParkFromDb = await _repository.GetAsync(StaticDetails.NationalParkAPIUrl, nationalPark.Id, HttpContext.Session.GetString("JWToken"));
                nationalPark.Picture = nationParkFromDb.Picture;
            }


            if (nationalPark.Id == 0)
                await _repository.CreateAsync(StaticDetails.NationalParkAPIUrl, nationalPark, HttpContext.Session.GetString("JWToken"));
            else
                await _repository.UpdateAsync(StaticDetails.NationalParkAPIUrl + nationalPark.Id, nationalPark, HttpContext.Session.GetString("JWToken"));

            return RedirectToAction("Index");
        }

        [Authorize(Roles = "Admin")]
        [HttpDelete]
        public async Task<IActionResult> Delete(int id)
        {
            var status = await _repository.DeleteAsync(StaticDetails.NationalParkAPIUrl, id, HttpContext.Session.GetString("JWToken"));
            if (status)
                return Json(new {success = true, message = "Delete Successful"});
            return Json(new {success = false, message = "Delete Not Successful"});
        }
    }
}