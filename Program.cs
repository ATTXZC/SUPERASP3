using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Options;
using Eco_life.Models;
using Eco_life.Services;
using Microsoft.AspNetCore.Identity;

var builder = WebApplication.CreateBuilder(args);

// Configuração da string de conexão com o banco de dados
var connectionString = builder.Configuration.GetConnectionString("DefaultConnection");
if (string.IsNullOrEmpty(connectionString))
{
    throw new InvalidOperationException("A string de conexão 'DefaultConnection' não está configurada.");
}

// Adiciona o contexto do banco de dados ao contêiner de serviços
builder.Services.AddDbContext<ApplicationDbContext>(options =>
    options.UseMySql(connectionString, ServerVersion.AutoDetect(connectionString)));

// Configuração do Identity
builder.Services.AddIdentity<ApplicationUser, IdentityRole>()
    .AddEntityFrameworkStores<ApplicationDbContext>()
    .AddDefaultTokenProviders();

// Configuração dos serviços de e-mail
builder.Services.Configure<SmtpSettings>(builder.Configuration.GetSection("SmtpSettings"));
builder.Services.AddTransient<IEmailSender, EmailSender>(); // Apenas uma vez

// Configuração de autenticação e autorização
builder.Services.AddAuthentication();
builder.Services.AddAuthorization();

// Adiciona serviços de sessão
builder.Services.AddSession(options =>
{
    options.IdleTimeout = TimeSpan.FromMinutes(30);
    options.Cookie.HttpOnly = true;
    options.Cookie.IsEssential = true;
});

// Adiciona suporte a Razor Pages
builder.Services.AddRazorPages();

var app = builder.Build();

// Configurações de ambiente
if (!app.Environment.IsDevelopment())
{
    app.UseExceptionHandler("/Error");
    app.UseHsts();
}
else
{
    app.UseDeveloperExceptionPage();
}

app.UseHttpsRedirection();
app.UseStaticFiles();
app.UseRouting();
app.UseAuthentication();
app.UseAuthorization();

// Adiciona middleware de sessão
app.UseSession();

app.MapRazorPages();

// Enviar token ao iniciar a aplicação
Task.Run(async () =>
{
    var scope = app.Services.CreateScope();
    var services = scope.ServiceProvider;
    var userManager = services.GetRequiredService<UserManager<ApplicationUser>>();
    var emailSender = services.GetRequiredService<IEmailSender>();

    var user = new ApplicationUser { UserName = "Testes Guigas", Email = "guigastestes@gmail.com" }; // Altere para o email desejado
    var result = await userManager.CreateAsync(user, "UbroadNine9"); // Altere a senha conforme necessário
    if (result.Succeeded)
    {
        var token = await userManager.GenerateEmailConfirmationTokenAsync(user);
        var callbackUrl = "http://localhost:5103/Account/ConfirmEmail"; // Altere conforme necessário
        await emailSender.SendEmailAsync(user.Email, "Confirme seu email", $"Confirme sua conta clicando <a href='{callbackUrl}?userId={user.Id}&code={token}'>aqui</a>.");
    }
}).Wait();

app.Run();

TestDatabaseConnection(app);

// Remova a função se não precisar dela
void TestDatabaseConnection(WebApplication app)
{
    using (var scope = app.Services.CreateScope())
    {
        var services = scope.ServiceProvider;
        try
        {
            var context = services.GetRequiredService<ApplicationDbContext>();
            if (context.Database.CanConnect())
            {
                Console.WriteLine("Conexão com o banco de dados bem-sucedida!");
            }
            else
            {
                Console.WriteLine("Falha na conexão com o banco de dados.");
            }
        }
        catch (Exception ex)
        {
            Console.WriteLine($"Ocorreu uma exceção: {ex.Message}");
        }
    }
}