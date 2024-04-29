using AutoMapper;
using LearnAPI.Container;
using LearnAPI.Helper;
using LearnAPI.Modal;
using LearnAPI.Repos;
using LearnAPI.Repos.Models;
using LearnAPI.Service;
using Microsoft.AspNetCore.Authentication;
using Microsoft.AspNetCore.Authentication.JwtBearer;
using Microsoft.AspNetCore.RateLimiting;
using Microsoft.EntityFrameworkCore;
using Microsoft.IdentityModel.Tokens;
using Microsoft.OpenApi.Models;
using Serilog;
using System.Text;

var builder = WebApplication.CreateBuilder(args);

// Add services to the container.

builder.Services.AddControllers();
// Learn more about configuring Swagger/OpenAPI at https://aka.ms/aspnetcore/swashbuckle
builder.Services.AddEndpointsApiExplorer();
builder.Services.AddSwaggerGen(option =>
{
    option.AddSecurityDefinition("Bearer", new OpenApiSecurityScheme
    {
        In = ParameterLocation.Header,
        Description = "Please enter a valid token",
        Name = "Authorization",
        Type = SecuritySchemeType.Http,
        BearerFormat = "JWT",
        Scheme = "Bearer"
    });
    option.AddSecurityRequirement(new OpenApiSecurityRequirement
    {
        {
            new OpenApiSecurityScheme
            {
                Reference = new OpenApiReference
                {
                    Type=ReferenceType.SecurityScheme,
                    Id="Bearer"
                }
            },
            new string[]{}
        }
    });
});
builder.Services.AddTransient<ICustomerService, CustomerService>();
builder.Services.AddTransient<IRefreshHandler, RefreshHandler>();
builder.Services.AddDbContext<LearndataContext>(o => o.UseSqlServer(builder.Configuration.GetConnectionString("apicon")));

//authentication
//builder.Services.AddAuthentication("BasicAuthentication").AddScheme<AuthenticationSchemeOptions, BasicAuthenticationHandler>("BasicAuthentication", null);

var _authkey = builder.Configuration.GetValue<string>("JwtSettings:securitykey");
builder.Services.AddAuthentication(item =>
{
    item.DefaultAuthenticateScheme = JwtBearerDefaults.AuthenticationScheme;
    item.DefaultChallengeScheme = JwtBearerDefaults.AuthenticationScheme;

}).AddJwtBearer(item =>
{
    item.RequireHttpsMetadata = true;
    item.SaveToken = true;
    item.TokenValidationParameters = new TokenValidationParameters()
    {
        ValidateIssuerSigningKey = true,
        IssuerSigningKey = new SymmetricSecurityKey(Encoding.UTF8.GetBytes(_authkey)),
        ValidateIssuer = false,
        ValidateAudience = false,
        ClockSkew = TimeSpan.Zero

    };
});


//var automapper = new MapperConfiguration(item => item.AddProfile(new AutoMapperHandler()));
//IMapper mapper = automapper.CreateMapper();
//builder.Services.AddSingleton(mapper);

//mapper
builder.Services.AddAutoMapper(AppDomain.CurrentDomain.GetAssemblies());

//cors
builder.Services.AddCors(p=>p.AddPolicy("corspolicy", builder => {
    builder.WithOrigins("*").AllowAnyMethod().AllowAnyHeader();
}));

builder.Services.AddCors(p => p.AddPolicy("corspolicy", builder => {
    builder.WithOrigins("https://domain3.com").AllowAnyMethod().AllowAnyHeader();
}));

builder.Services.AddCors(p => p.AddDefaultPolicy(builder => {
    builder.WithOrigins("*").AllowAnyMethod().AllowAnyHeader();
}));

//Rate Limiter
builder.Services.AddRateLimiter(_ => _.AddFixedWindowLimiter(policyName: "fixedwindow", options =>
{
    options.Window = TimeSpan.FromSeconds(10);
    options.PermitLimit = 1;
    options.QueueLimit = 0;
    options.QueueProcessingOrder = System.Threading.RateLimiting.QueueProcessingOrder.OldestFirst;
}).RejectionStatusCode=401);

//logging
string logpath = builder.Configuration.GetSection("Logging:Logpath").Value;
var _logger = new LoggerConfiguration()
    .MinimumLevel.Information() //Debug or Error
    .MinimumLevel.Override("microsoft", Serilog.Events.LogEventLevel.Warning)
    .Enrich.FromLogContext()
    .WriteTo.File(logpath)
    .CreateLogger();
builder.Logging.AddSerilog(_logger);

//jwt
var _jwtsetting = builder.Configuration.GetSection("JwtSettings");
builder.Services.Configure<JwtSettings>(_jwtsetting);


var app = builder.Build();

//minimal api
app.MapGet("/minimalapi", () => "Nihira Techiees");

app.MapGet("/getchannel", (string channelname) => "Welcome to " +channelname).WithOpenApi(opt => 
{
    var parameter = opt.Parameters[0];
    parameter.Description = "Enter Channel Name";
    return opt;
});

app.MapGet("/getcustomer", async (LearndataContext db) => { 
    return await db.TblCustomers.ToListAsync();
});

app.MapGet("/getcustomerbycode/{code}", async (LearndataContext db,string code) => {
    return await db.TblCustomers.FindAsync(code);
});

app.MapPost("/createcustomer", async (LearndataContext db, TblCustomer customer) => {
    await db.TblCustomers.AddAsync(customer);
    await db.SaveChangesAsync();
});

app.MapPut("/updatecustomer/{code}", async (LearndataContext db, TblCustomer customer, string code) => {
    var existdata =  await db.TblCustomers.FindAsync(code);
    if(existdata != null)
    {
        existdata.Name = customer.Name;
        existdata.Email = customer.Email;
    }
    await db.SaveChangesAsync();
});

app.MapDelete("/removecustomer/{code}", async (LearndataContext db, string code) => {
    var existdata = await db.TblCustomers.FindAsync(code);
    if (existdata != null)
    {
       db.TblCustomers.Remove(existdata);
    }
    await db.SaveChangesAsync();
});

//rate limit
app.UseRateLimiter();

// Configure the HTTP request pipeline.
if (app.Environment.IsDevelopment())
{
    app.UseSwagger();
    app.UseSwaggerUI();
}

app.UseStaticFiles();
app.UseCors();


app.UseHttpsRedirection();

app.UseAuthentication();

app.UseAuthorization();

app.MapControllers();

app.Run();
