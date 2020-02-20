using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace ParkyApi.Models.Repository.IRepository
{
    public interface INationalParkRepository
    {
        ICollection<NationalPark> GetNationalParks();
        NationalPark GetNationalPark(int nationalParkId);
        bool NationalParkExists(string name);
        bool NationParkExists(int id);
        bool CreateNationPark(NationalPark nationalPark);
        bool UpdateNationPark(NationalPark nationalPark);
        bool DeleteNationPark(NationalPark nationalPark);
        bool Save();
    }
}
