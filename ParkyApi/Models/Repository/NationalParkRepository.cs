using System;
using System.Collections.Generic;
using System.Linq;
using ParkyApi.Data;
using ParkyApi.Models.Repository.IRepository;

namespace ParkyApi.Models.Repository
{
    public class NationalParkRepository : INationalParkRepository
    {
        private readonly ApplicationDbContext _db;

        public NationalParkRepository(ApplicationDbContext db)
        {
            _db = db;
        }
        public ICollection<NationalPark> GetNationalParks()
        {
            return _db.NationalParks.OrderBy(n => n.Name).ToList();
        }

        public NationalPark GetNationalPark(int nationalParkId)
        {
            return _db.NationalParks.FirstOrDefault(n => n.Id == nationalParkId);;
        }

        public bool NationalParkExists(string name)
        {
            return _db.NationalParks.Any(n => n.Name.ToLower().Trim() == name.ToLower().Trim());
        }

        public bool NationParkExists(int id)
        {
            return _db.NationalParks.Any(n => n.Id == id);
        }

        public bool CreateNationPark(NationalPark nationalPark)
        {
            _db.NationalParks.Add(nationalPark);
            return Save();
        }

        public bool UpdateNationPark(NationalPark nationalPark)
        {
            _db.NationalParks.Update(nationalPark);
            return Save();
        }

        public bool DeleteNationPark(NationalPark nationalPark)
        {
            _db.NationalParks.Remove(nationalPark);
            return Save();
        }

        public bool Save()
        {
            return _db.SaveChanges() >= 0 ? true : false;
        }
    }
}
