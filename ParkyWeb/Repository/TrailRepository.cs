using System;
using System.Collections.Generic;
using System.Linq;
using System.Net;
using System.Net.Http;
using System.Threading.Tasks;
using Newtonsoft.Json;
using ParkyWeb.Models;
using ParkyWeb.Repository.Interface;

namespace ParkyWeb.Repository
{
    public class TrailRepository : Repository<Trail>, ITrailRepository
    {
        public TrailRepository(IHttpClientFactory clientFactory) : base(clientFactory) { }

    }
}
