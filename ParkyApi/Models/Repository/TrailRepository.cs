using System;
using System.Collections.Generic;
using System.Linq;
using Microsoft.EntityFrameworkCore;
using ParkyApi.Data;
using ParkyApi.Models.Repository.IRepository;

namespace ParkyApi.Models.Repository
{
    public class TrailRepository : ITrailRepository
    {
        private readonly ApplicationDbContext _db;

        public TrailRepository(ApplicationDbContext db)
        {
            _db = db;
        }
        public ICollection<Trail> GetTrails()
        {
            return _db.Trails.Include(t => t.NationalPark).OrderBy(n => n.Name).ToList();
        }

        public ICollection<Trail> GetTrailsInNationalPark(int nationParkId)
        {
            return _db.Trails.Include(t => t.NationalPark).Where(t => t.NationalParkId == nationParkId).ToList();
        }

        public Trail GetTrail(int trailId)
        {
            return _db.Trails.Include(t => t.NationalPark).FirstOrDefault(t => t.Id == trailId);

        }
        
        public bool TrailExists(string name)
        {
            return _db.Trails.Any(n => n.Name.ToLower().Trim() == name.ToLower().Trim());
        }

        public bool TrailExists(int id)
        {
            return _db.Trails.Any(n => n.Id == id);
        }

        public bool CreateTrail(Trail trail)
        {
            _db.Trails.Add(trail);
            return Save();
        }

        public bool UpdateTrail(Trail trail)
        {
            _db.Trails.Update(trail);
            return Save();
        }

        public bool DeleteTrail(Trail trail)
        {
            _db.Trails.Remove(trail);
            return Save();
        }
        public bool NationParkExists(int id)
        {
            return _db.NationalParks.Any(n => n.Id == id);

        }

        public bool Save()
        {
            return _db.SaveChanges() >= 0 ? true : false;
        }
    }
}
