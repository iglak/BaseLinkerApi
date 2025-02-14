﻿using System.Runtime.CompilerServices;
using System.Threading.RateLimiting;
using BaseLinkerApi.Common;
using Microsoft.Extensions.Options;

[assembly:InternalsVisibleTo("BaseLinkerApi.Tests")]

namespace BaseLinkerApi.Extensions.DependencyInjection;

internal class InjectableBaseLinkerApiClient : IBaseLinkerApiClient
{
    private readonly BaseLinkerApiClient _client;

    public InjectableBaseLinkerApiClient(HttpClient httpClient, IOptions<BaseLinkerOptions> options)
    {
        if (options.Value.Token == null) throw new ArgumentNullException(nameof(options.Value.Token));
        _client = new BaseLinkerApiClient(httpClient, options.Value.Token)
        {
            ThrowExceptions = options.Value.ThrowExceptions,
            UseRequestLimit = options.Value.UseRequestLimit,
            TimeLimiter = new FixedWindowRateLimiter(new FixedWindowRateLimiterOptions
            {
                Window = TimeSpan.FromMinutes(1),
                PermitLimit = options.Value.MaxRequestsPerMinute
            })
        };
    }

    public Task<TOutput> SendAsync<TOutput>(IRequest<TOutput> request, CancellationToken cancellationToken = default) where TOutput : ResponseBase
        => _client.SendAsync(request, cancellationToken);
}