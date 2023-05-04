using System;
using Splat;

namespace ScoreBridge.Client.Extensions;

public static class ResolverExtension
{
    public static T GetRequiredService<T>(this IReadonlyDependencyResolver resolver)
    {
        var service = resolver.GetService<T>();

        if (service is null)
        {
            throw new InvalidOperationException($"Failed to resolve object of type {typeof(T)}");
        }

        return service;
    }
}