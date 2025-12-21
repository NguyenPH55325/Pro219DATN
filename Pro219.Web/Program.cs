using Blazored.LocalStorage;
using MudBlazor;
using MudBlazor.Services;
using Pro219.Web.Components;
using Pro219.Web.Services;

var builder = WebApplication.CreateBuilder(args);

// Add services to the container.
builder.Services.AddRazorComponents()
    .AddInteractiveServerComponents();

builder.Services.AddMudServices(config =>
{
    config.SnackbarConfiguration.PositionClass = Defaults.Classes.Position.TopRight;

    config.SnackbarConfiguration.PreventDuplicates = false;
    config.SnackbarConfiguration.NewestOnTop = false;
    config.SnackbarConfiguration.ShowCloseIcon = true;
    config.SnackbarConfiguration.VisibleStateDuration = 3000;
    config.SnackbarConfiguration.HideTransitionDuration = 500;
    config.SnackbarConfiguration.ShowTransitionDuration = 500;
    config.SnackbarConfiguration.SnackbarVariant = Variant.Filled;
});

builder.Services.AddScoped<ProductService>();
builder.Services.AddScoped<AccountService>();
builder.Services.AddScoped<AuthService>();
builder.Services.AddScoped<BrandService>();
builder.Services.AddScoped<CartService>();
builder.Services.AddScoped<CategoryService>();
builder.Services.AddScoped<ColorService>();
builder.Services.AddScoped<CouponService>();
builder.Services.AddScoped<SaleService>();
builder.Services.AddScoped<SizeService>();
builder.Services.AddScoped<CustomerService>();
builder.Services.AddScoped<DrawerService>();
builder.Services.AddScoped<ProductImageService>();
builder.Services.AddScoped<ProductVariantService>(); 
builder.Services.AddScoped<CartItemService>(); 
builder.Services.AddScoped<CartStateService>();
builder.Services.AddScoped<AddressSerivce>();
builder.Services.AddScoped<OrderService>();
builder.Services.AddScoped<StatisticalService>();

builder.Services.AddScoped(http => new HttpClient
{
    BaseAddress = new Uri("https://localhost:7179/")
});

builder.Services.AddBlazoredLocalStorage();

var app = builder.Build();

// Configure the HTTP request pipeline.
if (!app.Environment.IsDevelopment())
{
    app.UseExceptionHandler("/Error", createScopeForErrors: true);
    // The default HSTS value is 30 days. You may want to change this for production scenarios, see https://aka.ms/aspnetcore-hsts.
    app.UseHsts();
}
app.UseStatusCodePagesWithRedirects("/error/{0}");
app.UseHttpsRedirection();

app.UseStaticFiles();
app.UseAntiforgery();

app.MapRazorComponents<App>()
    .AddInteractiveServerRenderMode();

app.Run();
