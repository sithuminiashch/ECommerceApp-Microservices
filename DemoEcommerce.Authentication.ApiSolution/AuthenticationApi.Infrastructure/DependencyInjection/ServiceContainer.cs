using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using AuthenticationApi.Application.Interfaces;
using AuthenticationApi.Infrastructure.Data;
using eCommerce.SharedLibrary.DependencyInjection;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using AuthenticationApi.Infrastructure.Repositories;
using Microsoft.AspNetCore.Builder;

namespace AuthenticationApi.Infrastructure.DependencyInjection;

public static class ServiceContainer
{
    public static IServiceCollection AddInfrasructureService(this IServiceCollection services, IConfiguration config)
    {

        SharedServiceContainer.AddSharedServices<AuthenticationDbContext>(services, config, config["MySerilog:FileName"]!);

        services.AddScoped<IUser, UserRepository>();
        return services;
    }


    public static IApplicationBuilder UserInfrastructurePolicy(this IApplicationBuilder app)
    {

        SharedServiceContainer.UseSharedPolicies(app);
        return app;
    }

}
