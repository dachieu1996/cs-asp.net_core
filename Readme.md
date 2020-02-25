# Asp.net Core Web API 
## Setup Project - EntityFramework
Install package:
* Microsoft.EntityFrameworkCore
* Microsoft.EntityFrameworkCore.SqlServer
* Microsoft.EntityFrameworkCore.Tools

Go to **appsettings.json**
```cs
{
  "ConnectionStrings": {
    "DefaultConnection": "Data Source=dachieu;Initial Catalog=Sparkz;User ID=sa;MultipleActiveResultSets=true"
  },

  "Logging": {
    "LogLevel": {
      "Default": "Information",
      "Microsoft": "Warning",
      "Microsoft.Hosting.Lifetime": "Information"
    }
  },
  "AllowedHosts": "*"
}
```

Create file **Data/ApplicationDbContext.cs**
```cs
public class ApplicationDbContext : DbContext
{
    public ApplicationDbContext(DbContextOptions<ApplicationDbContext> options) : base(options)
    {

    }

    public DbSet<NationalPark> NationalParks { get; set; }
}
```

Go to **Startup.cs**
```cs
public void ConfigureServices(IServiceCollection services)
{
    services.AddDbContext<ApplicationDbContext>
        (options => options.UseSqlServer(Configuration.GetConnectionString("DefaultConnection")));
    services.AddControllers();
}
```

## Repository
Add folder **Models/Repository and Models/Repository/IRepository**
Add file **IRepository/INationalParkRepository.cs**
Add file **NationalParkRepository.cs**
Add Dependency Injection in **Startup.cs**
```cs
public class Startup
{
    public void ConfigureServices(IServiceCollection services)
    {
        ...
        services.AddScoped<INationalParkRepository, NationalParkRepository>(); 
        ...
    }
}
```
```cs
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
```
## Dtos 
Create file **Models/Dtos/NationalParkDto.cs**
```cs
public class NationalParkDto
{
    public int Id { get; set; }
    public string Name { get; set; }
    public string State { get; set; }
    public DateTime Created { get; set; }
    public DateTime Established { get; set; }
}
```

## Automapper
Install package **AutoMapper**
Install package **AutoMapper.Extensions.Microsoft.DependencyInjection**
Create file **ParkyMapper/ParkyMappings.cs**
```cs
public class ParkyMappings : Profile
{
    public ParkyMappings()
    {
        CreateMap<NationalPark, NationalParkDto>().ReverseMap();
    }
}
```
```cs
public class Startup
{
    public void ConfigureServices(IServiceCollection services)
    {
        ...
        services.AddAutoMapper(typeof(ParkyMappings));
        ...
    }
}
```

## Create Controller
Create file **Controllers/NationalParkController.cs**
```cs
[Route("api/[controller]")]
[ApiController]
public class NationalParkController : ControllerBase
{
    private INationalParkRepository _repository;
    private readonly IMapper _mapper;

    public NationalParkController(INationalParkRepository repository, IMapper mapper)
    {
        _repository = repository;
        _mapper = mapper;
    }

    [HttpGet]
    public IActionResult GetNationalParks()
    {
        //var objList = _repository.GetNationalParks();
        //var objDto = new List<NationalParkDto>();
        //foreach (var obj in objList)
        //{
        //    objDto.Add(_mapper.Map<NationalParkDto>(obj));
        //}

        var objDto = _repository.GetNationalParks().Select(_mapper.Map<NationalPark, NationalParkDto>);

        return Ok(objDto);
    }

    [HttpGet("{id:int}")]
    public IActionResult GetNationPark(int id)
    {
        var obj = _repository.GetNationalPark(id);
        if (obj == null)
            return NotFound();

        var objDto = _mapper.Map<NationalParkDto>(obj);

        return Ok(objDto);
    }

    [HttpPost]
    public IActionResult CreateNationPark([FromBody] NationalParkDto nationalParkDto)
    {
        if (nationalParkDto == null)
            return BadRequest(ModelState);

        if (_repository.NationalParkExists(nationalParkDto.Name))
        {
            ModelState.AddModelError("", "National Park Exists!");
            return NotFound(ModelState);
        }

        var nationalParkObj = _mapper.Map<NationalPark>(nationalParkDto);
        if (!_repository.CreateNationPark(nationalParkObj))
        {
            ModelState.AddModelError("", $"Something went wrong when saving the record {nationalParkObj.Name}");
            return StatusCode(500, ModelState);
        }

        return CreatedAtRoute("GetNationPark", new { id = nationalParkObj.Id }, nationalParkObj);
    }

    [HttpPut("{nationalParkId:int}")]
    public IActionResult UpdateNationalPark(int nationalParkId, NationalParkDto nationalParkDto)
    {
        if (nationalParkDto == null || nationalParkId != nationalParkDto.Id)
            return BadRequest(ModelState);

        var nationalParkObj = _mapper.Map<NationalPark>(nationalParkDto);
        if (!_repository.UpdateNationPark(nationalParkObj))
        {
            ModelState.AddModelError("", $"Something went wrong when updating the record {nationalParkObj.Name}");
            return StatusCode(500, ModelState);
        }

        return NoContent();
    }
        
    [HttpDelete("{nationalParkId:int}")]
    public IActionResult DeleteNationalPark(int nationalParkId)
    {
        if (!_repository.NationParkExists(nationalParkId))
            return NotFound();

        var nationalParkObj = _repository.GetNationalPark(nationalParkId);
        if (!_repository.DeleteNationPark(nationalParkObj))
        {
            ModelState.AddModelError("", $"Something went wrong when deleting the record {nationalParkObj.Name}");
            return StatusCode(500, ModelState);
        }

        return NoContent();
    }
}
```

## Api Document
Install package Swashbuckle.AspNetCore
Then config
```cs
public class Startup
{
    public void ConfigureServices(IServiceCollection services)
    {
        ...
        services.AddSwaggerGen(options =>
        {
            options.SwaggerDoc("ParkyOpenApiSpecNP",
                new Microsoft.OpenApi.Models.OpenApiInfo()
                {
                    Title = "Parky Api (National Park)",
                    Version = "1",
                    Description = "National Parks API"
                });

            options.SwaggerDoc("ParkyOpenApiSpecTrails",
                new Microsoft.OpenApi.Models.OpenApiInfo()
                {
                    Title = "Parky Api (Trails)",
                    Version = "1",
                    Description = "Trails API"
                });
        });
        ...
    }

    public void Configure(IApplicationBuilder app, IWebHostEnvironment env)
    {
        ...
        app.UseSwagger();
        app.UseSwaggerUI(options =>
        {
            options.SwaggerEndpoint("/swagger/ParkyOpenApiSpecNP/swagger.json", "National Parks");
            options.SwaggerEndpoint("/swagger/ParkyOpenApiSpecTrails/swagger.json", "Trails");
            options.RoutePrefix = ""; // Set Url
        });
        ...
    }
}
```
Go to Properties/lauchSettings.json comment this line
```json
//"launchUrl": "weatherforecast",
```
```cs
[ApiExplorerSettings(GroupName = "ParkyOpenApiSpecNP")]
public class NationalParksController : ControllerBase { }

[ApiExplorerSettings(GroupName = "ParkyOpenApiSpecTrails")]
public class TrailsController : ControllerBase { }
```

## Api Versioning - Api Document for versioning
Install package: 
* Microsoft.AspNetCore.Mvc.Versioning
* Microsoft.AspNetCore.Mvc.Versioning.ApiExplorer

Create file ConfigureSwaggerOptions.cs
```cs
public class ConfigureSwaggerOptions : IConfigureOptions<SwaggerGenOptions>
{
    private readonly IApiVersionDescriptionProvider _provider;

    public ConfigureSwaggerOptions(IApiVersionDescriptionProvider provider)
    {
        _provider = provider;
    }
    public void Configure(SwaggerGenOptions options)
    {
        foreach (var desc in _provider.ApiVersionDescriptions)
        {
            options.SwaggerDoc(
                desc.GroupName, new Microsoft.OpenApi.Models.OpenApiInfo()
                {
                    Title = $"Parky API {desc.ApiVersion}",
                    Version = desc.ApiVersion.ToString()
                });
        }
    }
}
```
```cs
namespace ParkyApi
{
    public class Startup
    { 
        public void ConfigureServices(IServiceCollection services)
        {
            ...
            services.AddApiVersioning(options =>
            {
                options.AssumeDefaultVersionWhenUnspecified = true;
                options.DefaultApiVersion = new ApiVersion(1, 0);
                options.ReportApiVersions = true;
            });
            services.AddVersionedApiExplorer(options => options.GroupNameFormat = "'v'VVV");
            services.AddTransient<IConfigureOptions<SwaggerGenOptions>, ConfigureSwaggerOptions>();
            services.AddSwaggerGen();
            ...
        }

        public void Configure(IApplicationBuilder app, IWebHostEnvironment env, IApiVersionDescriptionProvider provider)
        {
            ...
            app.UseSwagger();
            app.UseSwaggerUI(options =>
            {
                foreach (var desc in provider.ApiVersionDescriptions)
                {
                    options.SwaggerEndpoint($"/swagger/{desc.GroupName}/swagger.json",desc.GroupName.ToUpperInvariant());
                }

                options.RoutePrefix = "";
            });
            ...
        }
    }
}
```

# Asp.net Core MVC
## Setup Project
Razor runtime compilation: Install package Microsoft.AspNetCore.Mvc.Razor.RuntimeCompilation
```cs
public class Startup
{
    public void ConfigureServices(IServiceCollection services)
    {
        ...
        services.AddControllersWithViews().AddRazorRuntimeCompilation();
        services.AddHttpClient();
        ...
    }
}
```

## Create Repository
Create file Repository/Interface/IRepository.cs
```
public interface IRepository<T> where T : class
{
    Task<T> GetAsync(string url, int id);
    Task<IEnumerable<T>> GetAllAsync(string url);
    Task<bool> CreateAsync(string url, T objToCreate);
    Task<bool> UpdateAsync(string url, T objToUpdate);
    Task<bool> DeleteAsync(string url, int id);
}
```

Create file Repository/Repository.cs

# Authentication
Create file **Models/User.cs**
```cs
public class User
{
    public int Id { get; set; }
    public string UserName { get; set; }
    public string Password { get; set; }
    public string Role { get; set; }
    [NotMapped]
    public string Token { get; set; }
}
```


Modify **appsettings.json**
```json
  "AppSettings": {
    "Secret": "THIS IS USED TO SIGN AND VERIFY JWT TOKENS, REPLACE IT WITH YOUR OWN SECRET, IT CAN BE ANY STRING" 
  } 
```
Create file AppSettings.cs
```cs
public class AppSettings
{
    public string Secret { get; set; }
}
```

Install package Microsoft.AspNetCore.Authentication.JwtBearer

Go to Startup.cs

```cs
public void ConfigureServices(IServiceCollection services)
{
    ...
    var appSettingsSection = Configuration.GetSection("AppSettings");
    services.Configure<AppSettings>(appSettingsSection);
    var appSettings = appSettingsSection.Get<AppSettings>();
    var key = Encoding.ASCII.GetBytes(appSettings.Secret);

    services.AddAuthentication(x =>
        {
            x.DefaultAuthenticateScheme = JwtBearerDefaults.AuthenticationScheme;
            x.DefaultChallengeScheme = JwtBearerDefaults.AuthenticationScheme;
        })
        .AddJwtBearer(x =>
        {
            x.RequireHttpsMetadata = false;
            x.SaveToken = true;
            x.TokenValidationParameters = new TokenValidationParameters
            {
                ValidateIssuerSigningKey = true,
                IssuerSigningKey = new SymmetricSecurityKey(key),
                ValidateIssuer = false,
                ValidateAudience = false
            };
        });
    services.AddCors();

    ...
}
public void Configure(IApplicationBuilder app, IWebHostEnvironment env, IApiVersionDescriptionProvider provider)
{
    ...
    app.UseCors(x => x
        .AllowAnyOrigin()
        .AllowAnyMethod()
        .AllowAnyHeader());

    app.UseAuthentication();
    app.UseAuthorization();
    ...
}
```
Create file **IUserRepository.cs** and **UserRepository.cs**
```cs
public class UserRepository : IUserRepository
{
    private readonly ApplicationDbContext _db;
    private readonly AppSettings _appSettings;
    public UserRepository(ApplicationDbContext db, IOptions<AppSettings> appSettings)
    {
        _db = db;
        _appSettings = appSettings.Value;
    }

    public bool IsUniqueUser(string userName)
    {
        var user = _db.Users.SingleOrDefault(x => x.UserName == userName);
        if (user == null)
            return true;
        return false;
    }

    public User Authenticate(string userName, string password)
    {
        var user = _db.Users.SingleOrDefault(x => x.UserName == userName && x.Password == password);
        if (user == null)
            return null;

        var tokenHandler = new JwtSecurityTokenHandler();
        var key = Encoding.ASCII.GetBytes(_appSettings.Secret);
        var tokenDescriptor = new SecurityTokenDescriptor
        {
            Subject = new ClaimsIdentity(new Claim[]
            {
                new Claim(ClaimTypes.Name,user.Id.ToString()),
                new Claim(ClaimTypes.Role,user.Role) 
            }),
            Expires = DateTime.Now.AddDays(7),
            SigningCredentials = new SigningCredentials(new SymmetricSecurityKey(key), SecurityAlgorithms.HmacSha256Signature)
        };
        var token = tokenHandler.CreateToken(tokenDescriptor);
        user.Token = tokenHandler.WriteToken(token);
            


        return user;
    }

    public User Register(User user)
    {
        var userObj = new User()
        {
            UserName = user.UserName,
            Password = user.Password
        };

        _db.Users.Add(userObj);
        _db.SaveChanges();
        userObj.Password = "";
        return userObj;
    }
}
```
Add Bearer to Swagger UI
```cs
public class ConfigureSwaggerOptions : IConfigureOptions<SwaggerGenOptions>
{
    public void Configure(SwaggerGenOptions options)
    {
        ...
        options.AddSecurityDefinition("Bearer", new OpenApiSecurityScheme
        {
            Description = "JWT Authorization header using the Bearer scheme. \r\n\r\n" +
                            "Enter 'Bearer' [space] and then your token in the text input below. \r\n\r\n" +
                            "Example: \"Bearer 123456abcdef\"",
            Name = "Authorization",
            In = ParameterLocation.Header,
            Type = SecuritySchemeType.ApiKey,
            Scheme = "Bearer"
        });

        options.AddSecurityRequirement(new OpenApiSecurityRequirement()
        {
            {
                new OpenApiSecurityScheme
                {
                    Reference = new OpenApiReference
                    {
                        Type = ReferenceType.SecurityScheme,
                        Id="Bearer"
                    },
                    Scheme = "oauth2",
                    Name = "Bearer",
                    In = ParameterLocation.Header
                },
                new List<string>()
            }
        });
        ...
    }
}
```

#### Role
Add "new Claim(ClaimTypes.Role,user.Role)" to UserRepository.cs  
```cs
public User Authenticate(string userName, string password)
{
    ...
    var tokenDescriptor = new SecurityTokenDescriptor
    {
        Subject = new ClaimsIdentity(new Claim[]
        {
            new Claim(ClaimTypes.Name,user.Id.ToString()),
            new Claim(ClaimTypes.Role,user.Role), // ADD THIS LINE
        }),
        Expires = DateTime.Now.AddDays(7),
        SigningCredentials = new SigningCredentials(new SymmetricSecurityKey(key), SecurityAlgorithms.HmacSha256Signature)
    };
    ...
}
```
Add Annotaion to function 
```cs
[Authorize(Roles = "Admin")]
public IActionResult GetTrail(int id) { }
```

## Authentication ASP.NET Core MVC
Create file **AccountRepository.cs**
```cs
public class AccountRepository : Repository<User>, IAccountRepository
{
    public AccountRepository(IHttpClientFactory clientFactory) : base(clientFactory)
    {
    }

    public async Task<User> LoginAsync(string url, User user)
    {
        if (user == null)
            return null;

        var request = new HttpRequestMessage(HttpMethod.Post, url)
        {
            Content = new StringContent(JsonConvert.SerializeObject(user), Encoding.UTF8, "application/json")
        };
        var client = _clientFactory.CreateClient();
        var response = await client.SendAsync(request);

        if (response.StatusCode != HttpStatusCode.OK)
            return null;

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
```
Go to file **Startup.cs**
```cs
public class Startup
{
    public void ConfigureServices(IServiceCollection services)
    {
        ...
        services.AddSession(options =>
        {
            // Set a short timeout for easy testing
            options.IdleTimeout = TimeSpan.FromMinutes(10);
            options.Cookie.HttpOnly = true;
            // Make the session cookie essential
            options.Cookie.IsEssential = true;
        });
        services.AddAuthentication(CookieAuthenticationDefaults.AuthenticationScheme)
            .AddCookie(options =>
            {
                options.Cookie.HttpOnly = true;
                options.LoginPath = "/Home/Login";
                options.AccessDeniedPath = "/Home/AccessDenied";
                options.SlidingExpiration = true;
            });
        services.AddHttpContextAccessor();
    }

    public void Configure(IApplicationBuilder app, IWebHostEnvironment env)
    {
        ...
        app.UseRouting();

        app.UseCors(x => x
            .AllowAnyOrigin()
            .AllowAnyMethod()
            .AllowAnyHeader());

        app.UseSession();

        app.UseAuthentication();

        app.UseAuthorization();
        ...
    }
```