using System.Reflection;
using FluentValidation;
using Microsoft.AspNetCore.Components.WebAssembly.Hosting;
using Microsoft.AspNetCore.Identity;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.DependencyInjection.Extensions;
using MudBlazor;
using MudBlazor.Services;
using Unshackled.Kitchen.Core.Data;
using Unshackled.Kitchen.My.Components;
using Unshackled.Kitchen.My.Extensions;
using Unshackled.Kitchen.My.Features;
using Unshackled.Kitchen.My.Middleware;
using Unshackled.Kitchen.My.Services;
using Unshackled.Studio.Core.Client.Configuration;
using Unshackled.Studio.Core.Client.Extensions;
using Unshackled.Studio.Core.Client.Services;
using Unshackled.Studio.Core.Data;
using Unshackled.Studio.Core.Data.Entities;
using Unshackled.Studio.Core.Server.Middleware;
using Unshackled.Studio.Core.Server.Services;
using Unshackled.Studio.Core.Server.Utils;

var builder = WebApplication.CreateBuilder(args);

ConnectionStrings connectionStrings = new ConnectionStrings
{
	DefaultDatabase = builder.Configuration.GetConnectionString("DefaultDatabase") ?? string.Empty
};
builder.Services.AddSingleton(connectionStrings);

SiteConfiguration siteConfig = new();
builder.Configuration.GetSection("SiteConfiguration").Bind(siteConfig);
builder.Services.AddSingleton(siteConfig);

HashIdConfiguration hashConfig = new();
builder.Configuration.GetSection("HashIdConfiguration").Bind(hashConfig);
HashIdSettings.Configure(hashConfig.Alphabet, hashConfig.Salt, hashConfig.MinLength);

SmtpSettings smtpSettings = new();
builder.Configuration.GetSection("SmtpSettings").Bind(smtpSettings);
builder.Services.AddSingleton(smtpSettings);

DbConfiguration dbConfig = new DbConfiguration();
builder.Configuration.GetSection("DbConfiguration").Bind(dbConfig);
builder.Services.AddSingleton(dbConfig);

StorageSettings storageSettings = new();
builder.Configuration.GetSection("StorageSettings").Bind(storageSettings);
builder.Services.AddSingleton(storageSettings);

AuthenticationProviderConfiguration authProviderConfig = new();
builder.Configuration.GetSection("AuthenticationProviders").Bind(authProviderConfig);

switch (dbConfig.DatabaseType?.ToLower())
{
	case DbConfiguration.MSSQL:
		builder.Services.AddDbContext<KitchenDbContext, MsSqlServerDbContext>();
		break;
	case DbConfiguration.MYSQL:
		builder.Services.AddDbContext<KitchenDbContext, MySqlServerDbContext>();
		break;
	case DbConfiguration.POSTGRESQL:
		builder.Services.AddDbContext<KitchenDbContext, PostgreSqlServerDbContext>();
		break;
	case DbConfiguration.SQLITE:
		FileUtils.EnsureDataSourceDirectoryExists(connectionStrings.DefaultDatabase);
		builder.Services.AddDbContext<KitchenDbContext, SqliteDbContext>();
		break;
	default:
		break;
}
builder.Services.AddDatabaseDeveloperPageExceptionFilter();

builder.Services.AddCascadingAuthenticationState();
builder.Services.AddScoped<IdentityUserAccessor>();
builder.Services.AddScoped<IdentityRedirectManager>();

builder.Services.AddAuthorization();
builder.Services.AddAuthentication(options => {
	options.DefaultScheme = IdentityConstants.ApplicationScheme;
	options.DefaultSignInScheme = IdentityConstants.ExternalScheme;
})
	.AddMicrosoftAccount(authProviderConfig)
	.AddGoogleAccount(authProviderConfig)
	.AddIdentityCookies();

builder.Services.ConfigureApplicationCookie(o =>
{
	o.LoginPath = "/account/login";
	o.LogoutPath = "/account/logout";
	o.ExpireTimeSpan = TimeSpan.FromDays(14);
	o.SlidingExpiration = true;
	o.Cookie.Name = siteConfig.SiteName?.RemoveNonAlphaNumeric() ?? "UnshackledKitchen";
});

builder.Services.Configure<DataProtectionTokenProviderOptions>(o => o.TokenLifespan = TimeSpan.FromHours(3));

builder.Services.AddIdentityCore<UserEntity>(options => {
	options.SignIn.RequireConfirmedAccount = siteConfig.RequireConfirmedAccount;
	options.Password.RequireDigit = siteConfig.PasswordStrength.RequireDigit;
	options.Password.RequireLowercase = siteConfig.PasswordStrength.RequireLowercase;
	options.Password.RequireNonAlphanumeric = siteConfig.PasswordStrength.RequireNonAlphanumeric;
	options.Password.RequireUppercase = siteConfig.PasswordStrength.RequireUppercase;
	options.Password.RequiredLength = siteConfig.PasswordStrength.RequiredLength;
	options.Password.RequiredUniqueChars = siteConfig.PasswordStrength.RequiredUniqueChars;
})
	.AddEntityFrameworkStores<KitchenDbContext>()
	.AddSignInManager()
	.AddDefaultTokenProviders();

builder.Services.AddRazorComponents()
	.AddInteractiveWebAssemblyComponents()
	.AddAuthenticationStateSerialization();

builder.Services.AddControllers();

builder.Services.AddMudServices(config =>
{
	config.SnackbarConfiguration.PositionClass = Defaults.Classes.Position.TopCenter;

	config.SnackbarConfiguration.PreventDuplicates = false;
	config.SnackbarConfiguration.NewestOnTop = false;
	config.SnackbarConfiguration.ShowCloseIcon = true;
	config.SnackbarConfiguration.VisibleStateDuration = 3000;
	config.SnackbarConfiguration.HideTransitionDuration = 200;
	config.SnackbarConfiguration.ShowTransitionDuration = 200;
	config.SnackbarConfiguration.SnackbarVariant = Variant.Filled;
});

builder.Services.AddMediatR(cfg => cfg.RegisterServicesFromAssemblies([
			Assembly.GetExecutingAssembly(),
			typeof(BaseController).Assembly
		]));
builder.Services.AddAutoMapper([
	Assembly.GetExecutingAssembly(),
			typeof(BaseController).Assembly
]);
builder.Services.AddValidatorsFromAssemblies([
	Assembly.GetExecutingAssembly(),
			typeof(BaseController).Assembly
]);

builder.Services.TryAddScoped<IWebAssemblyHostEnvironment, ServerHostEnvironment>();
builder.Services.AddHttpContextAccessor();
builder.Services.AddScoped<IRenderStateService, Unshackled.Studio.Core.Server.Services.RenderStateService>();
builder.Services.AddSingleton<IEmailSender<UserEntity>, SmtpService>();
builder.Services.AddScoped<IFileStorageService, LocalFileStorageService>();

var app = builder.Build();

// Configure the HTTP request pipeline.
if (app.Environment.IsDevelopment())
{
	app.UseWebAssemblyDebugging();
	app.UseMigrationsEndPoint();
}
else
{
	app.UseExceptionHandler("/Error", createScopeForErrors: true);
	// The default HSTS value is 30 days. You may want to change this for production scenarios, see https://aka.ms/aspnetcore-hsts.
	app.UseHsts();
}

if (siteConfig.ApplyMigrationsOnStartup)
{
	using var scope = app.Services.CreateScope();
	switch (dbConfig.DatabaseType?.ToLower())
	{
		case DbConfiguration.MSSQL:
			scope.ServiceProvider.GetRequiredService<MsSqlServerDbContext>()
				.Database.Migrate();
			break;
		case DbConfiguration.MYSQL:
			scope.ServiceProvider.GetRequiredService<MySqlServerDbContext>()
				.Database.Migrate();
			break;
		case DbConfiguration.POSTGRESQL:
			scope.ServiceProvider.GetRequiredService<PostgreSqlServerDbContext>()
				.Database.Migrate();
			break;
		case DbConfiguration.SQLITE:
			scope.ServiceProvider.GetRequiredService<SqliteDbContext>()
				.Database.Migrate();
			break;
		default:
			break;
	}
}

app.UseHttpsRedirection();

app.MapStaticAssets();
app.UseAntiforgery();

app.UseMiddleware<AuthorizedUserMiddleware>();
app.UseMiddleware<AuthorizedMemberMiddleware>();

app.MapRazorComponents<App>()
	.AddInteractiveWebAssemblyRenderMode()
	.AddAdditionalAssemblies(
		typeof(Unshackled.Studio.Core.Client._Imports).Assembly,
		typeof(Unshackled.Kitchen.My.Client._Imports).Assembly);

// Add additional endpoints required by the Identity /account Razor components.
app.MapAdditionalIdentityEndpoints();

app.MapControllers();

app.Run();
