using Microsoft.AspNetCore.Builder;
using Microsoft.AspNetCore.HttpsPolicy;
using Microsoft.AspNetCore.Hosting;
using Microsoft.AspNetCore.ResponseCompression;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
using System.Linq;
using Refit;
using Blazorlingo.Server.Clients;
using System;
using System.Net.Http;
using System.Threading.Tasks;
using System.Threading;
using Microsoft.AspNetCore.Http;
using Microsoft.Extensions.DependencyInjection.Extensions;
using System.Diagnostics;
using System.Net.Http.Headers;
using System.Collections.Generic;
using Newtonsoft.Json;
using Newtonsoft.Json.Serialization;
using System.Net;
using System.Text;
using System.IO;
using Microsoft.Extensions.Options;

namespace Blazorlingo.Server
{
    
    public class Startup
    {
        public Startup(IConfiguration configuration)
        {
            Configuration = configuration;
        }

        public IConfiguration Configuration { get; }

        // This method gets called by the runtime. Use this method to add services to the container.
        // For more information on how to configure your application, visit https://go.microsoft.com/fwlink/?LinkID=398940
        public void ConfigureServices(IServiceCollection services)
        {

            services.Configure<DuolingoSettings>(Configuration.GetSection("Duolingo"));
            services.TryAddSingleton<IHttpContextAccessor, HttpContextAccessor>();
            services.AddTransient<DuolingoAuthorizationHandler>();

            services.AddRefitClient<IDuolingoApi>(new RefitSettings()
            {
                ContentSerializer = new NewtonsoftJsonContentSerializer(
                new JsonSerializerSettings
                {
                    ContractResolver = new CamelCasePropertyNamesContractResolver()
                })
            }).ConfigureHttpClient(c => {
                    
                    c.BaseAddress = new Uri("https://www.duolingo.com/2017-06-30");
            }).AddHttpMessageHandler<DuolingoAuthorizationHandler>();

            services.AddControllersWithViews();
            services.AddRazorPages();
        }

        // This method gets called by the runtime. Use this method to configure the HTTP request pipeline.
        public void Configure(IApplicationBuilder app, IWebHostEnvironment env)
        {
            if (env.IsDevelopment())
            {
                app.UseDeveloperExceptionPage();
                app.UseWebAssemblyDebugging();
            }
            else
            {
                app.UseExceptionHandler("/Error");
                // The default HSTS value is 30 days. You may want to change this for production scenarios, see https://aka.ms/aspnetcore-hsts.
                app.UseHsts();
            }

            app.UseHttpsRedirection();
            app.UseBlazorFrameworkFiles();
            app.UseStaticFiles();

            app.UseRouting();

            app.UseEndpoints(endpoints =>
            {
                endpoints.MapRazorPages();
                endpoints.MapControllers();
                endpoints.MapFallbackToFile("index.html");
            });
        }
    }

    public class DuolingoAuthorizationHandler : DelegatingHandler
    {
        private readonly IHttpContextAccessor _httpContextAccessor;
        private readonly IConfiguration _configuration;
        private readonly IOptions<DuolingoSettings> _settings;

        public DuolingoAuthorizationHandler(IHttpContextAccessor httpContextAccessor, IConfiguration configuration, IOptions<DuolingoSettings> settings)
        {
            _httpContextAccessor = httpContextAccessor;
            _configuration = configuration;
            _settings = settings;
            
        }

        protected override async Task<HttpResponseMessage> SendAsync(
            HttpRequestMessage request,
            CancellationToken cancellationToken)
        {

            request.Headers.Add("Authorization", _settings.Value.AuthHeader);

            if (request.Method.Method != "GET")
            {
                return new HttpResponseMessage(HttpStatusCode.OK) {
                    Content = new StringContent(HttpRequest(request.RequestUri.ToString(),
                    request.Method.Method != "GET" ? await request.Content.ReadAsStringAsync() : "", 
                    request.Method.Method, 
                    request.Headers.First(x => x.Key == "Authorization").Value.First()))
                };
            }

            return await base.SendAsync(request, cancellationToken);
        }

        readonly string[] types = new[] { "html", "text", "xml", "json", "txt", "x-www-form-urlencoded" };


        private string HttpRequest(string url, string body, string method, string authHeader)
        {

            var httpWebRequest = (HttpWebRequest)WebRequest.Create(url);
            httpWebRequest.ContentType = "application/json";
            httpWebRequest.Method = method;

            httpWebRequest.Headers["x-pxbmh"] = "0";
            httpWebRequest.UserAgent = "Mozilla/5.0 (Windows NT 10.0; WOW64) AppleWebKit/537.36 (KHTML, like Gecko) Chrome/68.0.3440.75 Safari/537.36";

            httpWebRequest.Headers["Authorization"] = authHeader;

            var response = string.Empty;

            if (!string.IsNullOrEmpty(body))
            {
                using (var req = httpWebRequest.GetRequestStream())
                {
                    var bodyBytes = Encoding.UTF8.GetBytes(body);
                    req.Write(bodyBytes, 0, bodyBytes.Length);
                }
            }

            try
            {
                using (var res = httpWebRequest.GetResponse())
                {
                    using (var str = res.GetResponseStream())
                    {
                        using (StreamReader reader = new StreamReader(str))
                        {
                            response = reader.ReadToEnd();
                            return response;
                        }
                    }
                }
            }
            catch (WebException ex)
            {
                using (var res = ex.Response)
                {
                    using (var str = res.GetResponseStream())
                    {
                        using (StreamReader reader = new StreamReader(str))
                        {
                            response = reader.ReadToEnd();
                            return response;
                        }
                    }
                }
            }

        }

        bool IsTextBasedContentType(HttpHeaders headers)
        {
            IEnumerable<string> values;
            if (!headers.TryGetValues("Content-Type", out values))
                return false;
            var header = string.Join(" ", values).ToLowerInvariant();

            return types.Any(t => header.Contains(t));
        }
    }
}
