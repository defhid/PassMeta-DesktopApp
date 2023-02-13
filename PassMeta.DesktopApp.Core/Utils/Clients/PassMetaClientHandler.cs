using System;
using System.Linq;
using System.Net;
using System.Net.Http;
using System.Threading;
using System.Threading.Tasks;
using PassMeta.DesktopApp.Common.Abstractions.AppContext;
using PassMeta.DesktopApp.Common.Abstractions.Utils.Logging;
using PassMeta.DesktopApp.Core.Services.Extensions;

namespace PassMeta.DesktopApp.Core.Utils.Clients;

internal class PassMetaClientHandler : HttpClientHandler
{
    private static readonly SemaphoreSlim CookiesRefreshSemaphore = new(1, 1);
    private readonly IAppContextManager _appContextManager;
    private readonly ILogsWriter _logger;
    private readonly IDisposable _appContextSubscription;

    public PassMetaClientHandler(IAppContextManager appContextManager, ILogsWriter logger)
    {
        CookieContainer = new CookieContainer(3);

        _appContextManager = appContextManager;
        _logger = logger;
        _appContextSubscription = appContextManager.CurrentObservable
            .Subscribe(appContext => 
                CookieHelper.RefillCookieContainer(CookieContainer, appContext.Cookies));

        CookieHelper.RefillCookieContainer(CookieContainer, _appContextManager.Current.Cookies);
    }

    protected override HttpResponseMessage Send(HttpRequestMessage request, CancellationToken cancellationToken)
        => throw new NotImplementedException();

    protected override async Task<HttpResponseMessage> SendAsync(HttpRequestMessage request, CancellationToken cancellationToken)
    {
        var response = await base.SendAsync(request, cancellationToken);

        if (response.Headers.TryGetValues("Set-Cookie", out var newCookies) && newCookies.Any())
        {
            await RefreshCookiesAsync();
        }

        return response;
    }

    protected override void Dispose(bool disposing)
    {
        _appContextSubscription.Dispose();
        base.Dispose(disposing);
    }

    private async ValueTask RefreshCookiesAsync()
    {
        await CookiesRefreshSemaphore.WaitAsync();
        try
        {
            await _appContextManager.ApplyAsync(appContext =>
            {
                var currentCookies = appContext.Cookies;
                var freshCookies = CookieContainer.GetAllCookies();

                appContext.Cookies = CookieHelper.JoinCookies(currentCookies, freshCookies).ToList();
            });
        }
        catch (Exception ex)
        {
            _logger.Error(ex, "Failed to refresh cookie container");
        }
        finally
        {
            CookiesRefreshSemaphore.Release();
        }
    }
}