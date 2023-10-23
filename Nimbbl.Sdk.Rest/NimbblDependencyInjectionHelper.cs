// ReSharper disable once CheckNamespace
// This namespace is added to enable easy integration when injecting 

namespace Microsoft.Extensions.DependencyInjection;

public static class NimbblDependencyInjectionHelper
{
    // public static IServiceCollection AddNimbbl(
    //     this IServiceCollection services,
    //     ServiceLifetime lifetime = ServiceLifetime.Transient)
    // {
    //     services.Add(new ServiceDescriptor(typeof(Cloudinary), (Func<IServiceProvider, object>) (c => (object) new Cloudinary()), lifetime));
    //     return services;
    // }
    //
    // public static IServiceCollection AddNimbbl(
    //     this IServiceCollection services,
    //     Account account,
    //     ServiceLifetime lifetime = ServiceLifetime.Transient)
    // {
    //     services.Add(new ServiceDescriptor(typeof(Cloudinary), (Func<IServiceProvider, object>) (c => (object) new Cloudinary(account)), lifetime));
    //     return services;
    // }
}