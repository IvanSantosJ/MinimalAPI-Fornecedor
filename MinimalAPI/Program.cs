using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Identity;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Options;
using Microsoft.OpenApi.Models;
using MinimalAPI.Data;
using MinimalAPI.Models;
using NetDevPack.Identity;
using NetDevPack.Identity.Jwt;
using NetDevPack.Identity.Model;

var builder = WebApplication.CreateBuilder(args);

#region Configure Services
builder.Services.AddIdentityEntityFrameworkContextConfiguration(options =>
    options.UseSqlServer(builder.Configuration.GetConnectionString("DefaultConnection"),
    b => b.MigrationsAssembly("MinimalAPI")));

builder.Services.AddDbContext<ContextDb>(options =>
    options.UseSqlServer(builder.Configuration.GetConnectionString("DefaultConnection")));

builder.Services.AddIdentityConfiguration();
builder.Services.AddJwtConfiguration(builder.Configuration, "AppSettings");

builder.Services.AddAuthorization(options =>
{
    options.AddPolicy("ManterFornecedor", policy => policy.RequireClaim("ManterFornecedor"));
});

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

app.UseAuthConfiguration();

app.UseHttpsRedirection();
#endregion

#region Endpoints
app.MapPost("/registro", [AllowAnonymous] async (SignInManager<IdentityUser> signInManager, UserManager<IdentityUser> userManager,
                                                    IOptions<AppJwtSettings> appJwtSettings, RegisterUser registerUser) =>
{
    if (registerUser == null)
        return Results.BadRequest("Usuário não informado");

    var user = new IdentityUser
    {
        UserName = registerUser.Email,
        Email = registerUser.Email,
        EmailConfirmed = true
    };

    var result = await userManager.CreateAsync(user, registerUser.Password);

    if (!result.Succeeded)
        return Results.BadRequest(result.Errors);

    var jwt = new JwtBuilder()
                    .WithUserManager(userManager)
                    .WithJwtSettings(appJwtSettings.Value)
                    .WithEmail(user.Email)
                    .WithJwtClaims()
                    .WithUserClaims()
                    .WithUserRoles()
                    .BuildUserResponse();

    return Results.Ok(jwt);
})
    .Produces<NetDevPack.Identity.Jwt.Model.UserResponse>(StatusCodes.Status200OK)
    .Produces<string>(StatusCodes.Status400BadRequest)
    .WithName("RegistroUsuario")
    .WithTags("Usuario");

app.MapPost("/login", [AllowAnonymous] async (SignInManager<IdentityUser> signInManager, UserManager<IdentityUser> userManager,
                                                IOptions<AppJwtSettings> appJwtSettings, LoginUser loginUser) =>
{
    if (loginUser == null)
        return Results.BadRequest("Usuário não informado");

    var result = await signInManager.PasswordSignInAsync(loginUser.Email, loginUser.Password, false, true);

    if (result.IsLockedOut)
        return Results.BadRequest("Usuário bloqueado");
    if (!result.Succeeded)
        return Results.BadRequest("Usuário ou senha inválidos");


    var jwt = new JwtBuilder()
                .WithUserManager(userManager)
                .WithJwtSettings(appJwtSettings.Value)
                .WithEmail(loginUser.Email)
                .WithJwtClaims()
                .WithUserClaims()
                .WithUserRoles()
                .BuildUserResponse();

    return Results.Ok(jwt);
})
    .Produces<NetDevPack.Identity.Jwt.Model.UserResponse>(StatusCodes.Status200OK)
    .Produces<string>(StatusCodes.Status400BadRequest)
    .WithName("LoginUsuario")
    .WithTags("Usuario");

app.MapGet("/fornecedor", [AllowAnonymous] async (ContextDb context) => await context.Fornecedores.ToListAsync())
    .WithName("GetFornecedor")
    .WithTags("Fornecedor");

app.MapGet("/fornecedor/{id}", [Authorize] async (ContextDb context, Guid id) =>
        await context.Fornecedores.FindAsync(id)
        is Fornecedor fornecedor ? Results.Ok(fornecedor) : Results.NotFound()
    )
    .Produces<Fornecedor>(StatusCodes.Status200OK)
    .Produces(StatusCodes.Status401Unauthorized)
    .Produces(StatusCodes.Status404NotFound)
    .WithName("GetFornecedorById")
    .WithTags("Fornecedor");

app.MapPost("/fornecedor", [Authorize] async (ContextDb context, Fornecedor fornecedor) =>
{
    context.Fornecedores.Add(fornecedor);
    var result = await context.SaveChangesAsync();

    return result > 0 ? Results.CreatedAtRoute("GetFornecedorById", new { id = fornecedor.Id }, fornecedor) : Results.BadRequest("Houve um problema ao salvar o registro.");
})
    .Produces<Fornecedor>(StatusCodes.Status201Created)
    .Produces(StatusCodes.Status400BadRequest)
    .Produces(StatusCodes.Status401Unauthorized)
    .Produces(StatusCodes.Status403Forbidden)
    .RequireAuthorization("ManterFornecedor")
    .WithName("PostFornecedor")
    .WithTags("Fornecedor");

app.MapPut("/fornecedor/{id}", [Authorize] async (ContextDb context, Guid id, Fornecedor fornecedor) =>
{
    var fornecedorBanco = await context.Fornecedores.AsNoTracking<Fornecedor>().FirstOrDefaultAsync(f => f.Id == id);
    if (fornecedorBanco == null)
        return Results.NotFound();

    context.Fornecedores.Update(fornecedor);
    var result = await context.SaveChangesAsync();

    return result > 0 ? Results.NoContent() : Results.BadRequest("Houve um problema ao salvar o registro.");
})
    .Produces<Fornecedor>(StatusCodes.Status204NoContent)
    .Produces(StatusCodes.Status400BadRequest)
    .Produces(StatusCodes.Status401Unauthorized)
    .Produces(StatusCodes.Status403Forbidden)
    .Produces(StatusCodes.Status404NotFound)
    .RequireAuthorization("ManterFornecedor")
    .WithName("PutFornecedor")
    .WithTags("Fornecedor");

app.MapDelete("/fornecedor/{id}", [Authorize] async (ContextDb context, Guid id) =>
{
    var fornecedor = await context.Fornecedores.FindAsync(id);
    if (fornecedor == null)
        return Results.NotFound();

    context.Fornecedores.Remove(fornecedor);
    var result = await context.SaveChangesAsync();

    return result > 0 ? Results.NoContent() : Results.BadRequest("Houve um problema ao remover o registro.");
})
    .Produces<Fornecedor>(StatusCodes.Status204NoContent)
    .Produces(StatusCodes.Status400BadRequest)
    .Produces(StatusCodes.Status401Unauthorized)
    .Produces(StatusCodes.Status403Forbidden)
    .Produces(StatusCodes.Status404NotFound)
    .RequireAuthorization("ManterFornecedor")
    .WithName("DeleteFornecedor")
    .WithTags("Fornecedor");
#endregion

app.Run();