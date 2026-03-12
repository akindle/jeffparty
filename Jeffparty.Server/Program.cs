using Jeffparty.Server.Hubs;
using Microsoft.AspNetCore.HttpOverrides;

var builder = WebApplication.CreateBuilder(args);

var corsOrigins = builder.Configuration.GetSection("CorsOrigins").Get<string[]>();

builder.Services.AddCors(options => options.AddPolicy("CorsPolicy",
    policy =>
    {
        policy.AllowAnyMethod()
            .AllowAnyHeader()
            .AllowCredentials();
        if (corsOrigins is { Length: > 0 })
            policy.WithOrigins(corsOrigins);
        else
            policy.SetIsOriginAllowed(_ => true);
    }));

builder.Services.AddSignalR();

var app = builder.Build();

if (!app.Environment.IsDevelopment())
{
    app.UseExceptionHandler("/Error");
    app.UseHsts();
}

app.UseForwardedHeaders(new ForwardedHeadersOptions
{
    ForwardedHeaders = ForwardedHeaders.XForwardedFor | ForwardedHeaders.XForwardedProto
});

app.UseBlazorFrameworkFiles();
app.UseStaticFiles();
app.UseCors("CorsPolicy");
app.UseRouting();

app.MapHub<ChatHub>("/chatHub");
app.MapFallbackToFile("index.html");

app.Run();