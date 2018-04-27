using background.Service;
using CustomMiddleware;
using Microsoft.AspNetCore.Builder;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using System;
using System.Collections.Generic;
using System.ServiceModel;
using System.Text;

namespace background
{
    public class Startup
    {
        public Startup(IConfiguration configuration)
        {
            Configuration = configuration;
        }

        public IConfiguration Configuration { get; }

        // This method gets called by the runtime. Use this method to add services to the container.
        public void ConfigureServices(IServiceCollection services)
        {
            services.AddScoped<CalculatorService>();           
        }

        // This method gets called by the runtime. Use this method to configure the HTTP request pipeline.
        public void Configure(IApplicationBuilder app)
        {
            //加入一个/CalculatorService.svc 地址，绑定Http
            app.UseSOAPMiddleware<CalculatorService>("/CalculatorService.svc", new BasicHttpBinding());
            
        }
    }

}
