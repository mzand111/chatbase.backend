using ChatBase.Backend.Infrastructure;
using ChatBase.Backend.Infrastructure.Chat;
using ChatBase.Backend.Service;
using Microsoft.Extensions.DependencyInjection;
using MZBase.Infrastructure;

namespace ChatBase.Backend.Lib
{
    public static class ServiceCollectionExtensions
    {
        public static IServiceCollection AddBusinessServices(this IServiceCollection services)
        {
            services.AddScoped<IDateTimeProviderService, DateTimeProviderService>();
            #region Chat
            //Repos
            services.AddScoped<IChatMessageRepository, ChatMessageRepository>();
            //UoW
            services.AddScoped<IChatUnitOfWork, ChatUnitOfWork>();
            services.AddScoped<ChatUnitOfWork, ChatUnitOfWork>();
            //Services
            services.AddScoped<ChatMessageStorageService, ChatMessageStorageService>();
            #endregion
            return services;
        }
    }
}
