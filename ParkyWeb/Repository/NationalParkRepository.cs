using System;
using System.Collections.Generic;
using System.Linq;
using System.Net.Http;
using System.Threading.Tasks;
using ParkyWeb.Models;
using ParkyWeb.Repository.Interface;

namespace ParkyWeb.Repository
{
    public class NationalParkRepository : Repository<NationalPark>, INationalParkRepository
    {
        public NationalParkRepository(IHttpClientFactory clientFactory) : base(clientFactory)
        {
        }
    }
}
