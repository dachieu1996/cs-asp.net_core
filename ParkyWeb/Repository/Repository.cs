using System;
using System.Collections.Generic;
using System.Linq;
using System.Net;
using System.Net.Http;
using System.Net.Http.Headers;
using System.Text;
using System.Text.Json.Serialization;
using System.Threading.Tasks;
using Newtonsoft.Json;
using ParkyWeb.Repository.Interface;

namespace ParkyWeb.Repository
{
    public class Repository<T> : IRepository<T> where T : class
    {
        public readonly IHttpClientFactory _clientFactory;

        public Repository(IHttpClientFactory clientFactory)
        {
            _clientFactory = clientFactory;
        }
        public async Task<bool> CreateAsync(string url, T objToCreate, string token)
        {
            if (objToCreate == null)
                return false;

            var request = new HttpRequestMessage(HttpMethod.Post, url)
            {
                Content = new StringContent(JsonConvert.SerializeObject(objToCreate), Encoding.UTF8, "application/json")
            };
            var client = _clientFactory.CreateClient();
            if(!string.IsNullOrEmpty(token))
                client.DefaultRequestHeaders.Authorization = new AuthenticationHeaderValue("Bearer",token);

            var response = await client.SendAsync(request);

            return response.StatusCode == HttpStatusCode.Created;
        }

        public async Task<bool> DeleteAsync(string url, int id, string token)
        {
            var request = new HttpRequestMessage(HttpMethod.Delete, url + id);
            var client = _clientFactory.CreateClient();
            if (!string.IsNullOrEmpty(token))
                client.DefaultRequestHeaders.Authorization = new AuthenticationHeaderValue("Bearer", token);

            var response = await client.SendAsync(request);

            return response.StatusCode == HttpStatusCode.NoContent;
        }

        public async Task<IEnumerable<T>> GetAllAsync(string url, string token)
        {
            var request = new HttpRequestMessage(HttpMethod.Get, url);
            var client = _clientFactory.CreateClient();
            if (!string.IsNullOrEmpty(token))
                client.DefaultRequestHeaders.Authorization = new AuthenticationHeaderValue("Bearer", token);

            var response = await client.SendAsync(request);

            if (response.StatusCode != HttpStatusCode.OK)
                return null;
            var jsonString = await response.Content.ReadAsStringAsync();

            return JsonConvert.DeserializeObject<IEnumerable<T>>(jsonString);
        }

        public async Task<T> GetAsync(string url, int id, string token)
        {
            var request = new HttpRequestMessage(HttpMethod.Get, url + id);
            var client = _clientFactory.CreateClient();
            if (!string.IsNullOrEmpty(token))
                client.DefaultRequestHeaders.Authorization = new AuthenticationHeaderValue("Bearer", token);

            var response = await client.SendAsync(request);

            if (response.StatusCode != HttpStatusCode.OK)
                return null;
            var jsonString = await response.Content.ReadAsStringAsync();

            return JsonConvert.DeserializeObject<T>(jsonString);
        }

        public async Task<bool> UpdateAsync(string url, T objToUpdate, string token)
        {
            if (objToUpdate == null)
                return false;

            var request = new HttpRequestMessage(HttpMethod.Put, url)
            {
                Content = new StringContent(JsonConvert.SerializeObject(objToUpdate), Encoding.UTF8, "application/json")
            };
            var client = _clientFactory.CreateClient();
            if (!string.IsNullOrEmpty(token))
                client.DefaultRequestHeaders.Authorization = new AuthenticationHeaderValue("Bearer", token);

            var response = await client.SendAsync(request);

            return response.StatusCode == HttpStatusCode.NoContent;
        }
    }
}
