using Common.AppSettings;
using Services.Implementation;
using Services.Interfaces;
using Services.Repositories;
using StackExchange.Redis;

namespace RideMatch_Api.Extension
{
    public static class Dependencies
    {
        /// <summary>
        /// Register Dependencies
        /// </summary>
        /// <param name="services">services as IServiceCollection.</param>
        public static void RegisterDependencies(this IServiceCollection services)
        {
            //services.AddTransient(typeof(IDataRepository<>), typeof(DataRepository<>));
            services.AddTransient<IUserRepository, UserRepository>();
            services.AddTransient<IDriverRepository, DriverRepository>();
            services.AddTransient<IRideRepository, RideRepository>();

            services.AddMemoryCache();

            // Register it For Service Locator Pattern
            services.AddSingleton<IHttpContextAccessor, HttpContextAccessor>();

            ServiceLocator.Initialize(services.BuildServiceProvider().GetService<IHttpContextAccessor>());

            // Register Service Dependencies
            services.RegisterServiceDependencies();

            services.AddDistributedMemoryCache();
            services.AddSession(options =>
            {
                options.IdleTimeout = TimeSpan.FromMinutes(50);
            });
        }

        public static IServiceCollection RegisterServiceDependencies(this IServiceCollection services)
        {

            services.AddTransient<ILoggerManager, LoggerManager>();
            services.AddSingleton<RideMatchQueue>();
            services.AddSingleton<IRideMatchQueue>(provider => provider.GetRequiredService<RideMatchQueue>());
            services.AddTransient<IRideMatchService, RideMatchService>();
            services.AddTransient<IDriverService, DriverService>();
            services.AddTransient<IRiderRideService, RiderRideService>();
            services.AddHostedService<RideMatchWorker>();
            return services;
        }

        public static void RegisterConfiguration(this IServiceCollection services, IConfiguration configuration)
        {
            services.Configure<AppConfiguration>(configuration.GetSection("AppConfiguration"));
        }

        public static IServiceCollection AddRedis(this IServiceCollection services, IConfiguration configuration)
        {
            var connectionString = configuration.GetConnectionString("Redis") ?? "localhost:6379";
            services.AddSingleton<IConnectionMultiplexer>(_ =>
            {
                var options = ConfigurationOptions.Parse(connectionString);
                options.AbortOnConnectFail = false;
                return ConnectionMultiplexer.Connect(options);
            });
            services.AddSingleton<IRedisService, RedisService>();
            return services;
        }
    }
}







