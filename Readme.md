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
