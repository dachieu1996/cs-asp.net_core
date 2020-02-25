using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace ParkyWeb
{
    public static class StaticDetails
    {
        public static string APIBaseUrl = "https://localhost:44319/";
        public static string NationalParkAPIUrl = APIBaseUrl + "api/v1/nationalparks/";
        public static string TrailAPIUrl = APIBaseUrl + "api/v1/trails/";
        public static string AccountAPIUrl = APIBaseUrl + "api/v1/users/";
    }
}
