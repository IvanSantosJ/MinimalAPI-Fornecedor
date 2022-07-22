using Microsoft.EntityFrameworkCore;
using Microsoft.OpenApi.Models;
using MinimalAPI.Data;

var builder = WebApplication.CreateBuilder(args);

#region Configure Services
builder.Services.AddDbContext<ContextDb>(options =>
    options.UseSqlServer(builder.Configuration.GetConnectionString("DefaultConnection")));


builder.Services.AddEndpointsApiExplorer();
builder.Services.AddSwaggerGen(doc =>
{
    doc.SwaggerDoc("v1", new OpenApiInfo
    {
        Title = "Minimal API Example",
        Description = "Developed by Ivan Santos - Based on desenvolvedor.io (Eduardo Pires) <a href='https://youtu.be/aXayqUfSNvw'>video</a> ",
        Contact = new OpenApiContact { Name = "Ivan Santos", Email = "ivanjr9782@gmail.com" },
        License = new OpenApiLicense { Name = "-", Url = new Uri("https://opensource.org/licenses/MIT") }
    });

    doc.AddSecurityDefinition("Bearer", new OpenApiSecurityScheme
    {
        Description = "Insira o token JWT desta maneira: Bearer {seu token}",
        Name = "Authorization",
        Scheme = "Bearer",
        BearerFormat = "JWT",
        In = ParameterLocation.Header,
        Type = SecuritySchemeType.ApiKey
    });

    doc.AddSecurityRequirement(new OpenApiSecurityRequirement
    {
        {
            new OpenApiSecurityScheme
            {
                Reference = new OpenApiReference
                {
                    Type = ReferenceType.SecurityScheme,
                    Id = "Bearer"
                }
            },
            new string[] {}
        }
    });
});

var app = builder.Build();
#endregion

#region Configure Pipeline
if (app.Environment.IsDevelopment())
{
    app.UseSwagger();
    app.UseSwaggerUI();
}

app.UseHttpsRedirection();
#endregion

#region Endpoints

#endregion

app.Run();