using System;
using System.Collections.Generic;
using System.Linq;
using System.Net;
using System.Net.Http;
using System.Text;
using System.Threading.Tasks;
using Newtonsoft.Json;
using ParkyWeb.Models;
using ParkyWeb.Repository.Interface;

namespace ParkyWeb.Repository
{
    public class AccountRepository : Repository<User>, IAccountRepository
    {
        public AccountRepository(IHttpClientFactory clientFactory) : base(clientFactory)
        {
        }

        public async Task<User> LoginAsync(string url, User user)
        {
            if (user == null)
                return new User();

            var request = new HttpRequestMessage(HttpMethod.Post, url)
            {
                Content = new StringContent(JsonConvert.SerializeObject(user), Encoding.UTF8, "application/json")
            };
            var client = _clientFactory.CreateClient();
            var response = await client.SendAsync(request);

            if (response.StatusCode != HttpStatusCode.OK)
                return new User();

            var jsonString = await response.Content.ReadAsStringAsync();
            return JsonConvert.DeserializeObject<User>(jsonString);
        }

        public async Task<bool> RegisterAsync(string url, User user)
        {
            if (user == null)
                return false;

            var request = new HttpRequestMessage(HttpMethod.Post, url)
            {
                Content = new StringContent(JsonConvert.SerializeObject(user), Encoding.UTF8, "application/json")
            };
            var client = _clientFactory.CreateClient();
            var response = await client.SendAsync(request);

            return response.StatusCode == HttpStatusCode.OK;
        }
    }
}
