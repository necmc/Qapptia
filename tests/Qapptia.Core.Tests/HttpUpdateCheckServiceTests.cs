using System;
using System.Net;
using System.Net.Http;
using System.Text;
using System.Threading;
using System.Threading.Tasks;
using Qapptia.Core.Abstractions;
using Qapptia.Core.Services;
using Xunit;

namespace Qapptia.Core.Tests;

public class HttpUpdateCheckServiceTests
{
    private sealed class MockHttpMessageHandler : HttpMessageHandler
    {
        private readonly Func<HttpRequestMessage, HttpResponseMessage> _handler;

        public MockHttpMessageHandler(Func<HttpRequestMessage, HttpResponseMessage> handler)
        {
            _handler = handler;
        }

        protected override Task<HttpResponseMessage> SendAsync(HttpRequestMessage request, CancellationToken cancellationToken)
        {
            return Task.FromResult(_handler(request));
        }
    }

    [Fact]
    public async Task CheckForUpdatesAsyncWhenRemoteIsHigherReturnsUpdateAvailable()
    {
        var jsonResponse = """
        {
            "version": "2.1.0",
            "siteDownload": "https://example.com/download",
            "changelog": ["Nuevas funciones añadidas"],
            "platform": {
                "windows": { "isAvailable": true }
            }
        }
        """;

        using var handler = new MockHttpMessageHandler(_ => new HttpResponseMessage(HttpStatusCode.OK)
        {
            Content = new StringContent(jsonResponse, Encoding.UTF8, "application/json")
        });
        using var httpClient = new HttpClient(handler);
        using var service = new HttpUpdateCheckService(httpClient, currentVersion: "2.0.0");

        var result = await service.CheckForUpdatesAsync(ct: CancellationToken.None);

        Assert.Equal(UpdateCheckStatus.UpdateAvailable, result.Status);
        Assert.NotNull(result.LatestRelease);
        Assert.Equal("2.1.0", result.LatestRelease.Version);
        Assert.Equal("https://example.com/download", result.LatestRelease.SiteDownload);
    }

    [Fact]
    public async Task CheckForUpdatesAsyncWhenRemoteIsSameOrLowerReturnsUpToDate()
    {
        var jsonResponse = """
        {
            "version": "2.0.0",
            "downloadSite": "https://example.com/download",
            "changelog": ["Versión actual"]
        }
        """;

        using var handler = new MockHttpMessageHandler(_ => new HttpResponseMessage(HttpStatusCode.OK)
        {
            Content = new StringContent(jsonResponse, Encoding.UTF8, "application/json")
        });
        using var httpClient = new HttpClient(handler);
        using var service = new HttpUpdateCheckService(httpClient, currentVersion: "2.0.0");

        var result = await service.CheckForUpdatesAsync(ct: CancellationToken.None);

        Assert.Equal(UpdateCheckStatus.UpToDate, result.Status);
        Assert.NotNull(result.LatestRelease);
        Assert.Equal("2.0.0", result.LatestRelease.Version);
    }

    [Fact]
    public async Task CheckForUpdatesAsyncWhenHttpErrorReturnsServerUnavailable()
    {
        using var handler = new MockHttpMessageHandler(_ => new HttpResponseMessage(HttpStatusCode.InternalServerError));
        using var httpClient = new HttpClient(handler);
        using var service = new HttpUpdateCheckService(httpClient, currentVersion: "2.0.0");

        var result = await service.CheckForUpdatesAsync(ct: CancellationToken.None);

        Assert.Equal(UpdateCheckStatus.ServerUnavailable, result.Status);
        Assert.Null(result.LatestRelease);
        Assert.Equal("Servidor no disponible (500).", result.ErrorMessage);
    }

    [Fact]
    public async Task CheckForUpdatesAsyncWhenMalformedJsonReturnsInvalidPayload()
    {
        var malformedJson = "{ invalid_json }";

        using var handler = new MockHttpMessageHandler(_ => new HttpResponseMessage(HttpStatusCode.OK)
        {
            Content = new StringContent(malformedJson, Encoding.UTF8, "application/json")
        });
        using var httpClient = new HttpClient(handler);
        using var service = new HttpUpdateCheckService(httpClient, currentVersion: "2.0.0");

        var result = await service.CheckForUpdatesAsync(ct: CancellationToken.None);

        Assert.Equal(UpdateCheckStatus.InvalidPayload, result.Status);
    }

    [Fact]
    public async Task CheckForUpdatesAsyncWhenTimeoutReturnsTimeoutErrorMessage()
    {
        using var handler = new MockHttpMessageHandler(_ => throw new TaskCanceledException());
        using var httpClient = new HttpClient(handler);
        using var service = new HttpUpdateCheckService(httpClient, currentVersion: "2.0.0");

        var result = await service.CheckForUpdatesAsync(ct: CancellationToken.None);

        Assert.Equal(UpdateCheckStatus.ServerUnavailable, result.Status);
        Assert.Equal(Constants.UpdateErrorTimeout, result.ErrorMessage);
    }

    [Fact]
    public async Task CheckForUpdatesAsyncWhenSocketNetworkDownReturnsNoInternet()
    {
        var socketEx = new System.Net.Sockets.SocketException((int)System.Net.Sockets.SocketError.NetworkDown);
        using var handler = new MockHttpMessageHandler(_ => throw new HttpRequestException("Network down", socketEx));
        using var httpClient = new HttpClient(handler);
        using var service = new HttpUpdateCheckService(httpClient, currentVersion: "2.0.0");

        var result = await service.CheckForUpdatesAsync(ct: CancellationToken.None);

        Assert.Equal(UpdateCheckStatus.NoInternet, result.Status);
        Assert.Equal(Constants.UpdateErrorNoInternet, result.ErrorMessage);
    }

    [Fact]
    public async Task CheckForUpdatesAsyncWhenUserCancelledReturnsCancelledErrorMessage()
    {
        using var cts = new CancellationTokenSource();
        cts.Cancel();

        using var handler = new MockHttpMessageHandler(_ => throw new OperationCanceledException(cts.Token));
        using var httpClient = new HttpClient(handler);
        using var service = new HttpUpdateCheckService(httpClient, currentVersion: "2.0.0");

        var result = await service.CheckForUpdatesAsync(ct: cts.Token);

        Assert.Equal(UpdateCheckStatus.ConnectionFailed, result.Status);
        Assert.Equal(Constants.UpdateErrorCheckCancelled, result.ErrorMessage);
    }

    [Fact]
    public async Task CheckForUpdatesAsyncWhenPlatformSchemaProvidedReturnsUpdateAvailableWithDetails()
    {
        var jsonResponse = """
        {
            "version": "2.1.0",
            "isEnabled": true,
            "siteDownload": "https://example.com/download",
            "siteSponsor": "https://example.com/sponsor",
            "changelog": [
                "Mejora 1",
                "Mejora 2"
            ],
            "platform": {
                "windows": {
                    "isAvailable": true,
                    "variants": {
                        "installer": {
                            "filename": "Qapptia-setup.exe",
                            "url": "https://example.com/download",
                            "sha256": "9f86d081884c7d659a2feaa0c55ad015a3bf4f1b2b0b822cd15d6c15b0f00a08"
                        }
                    }
                },
                "linux": { "isAvailable": false },
                "macos": { "isAvailable": false }
            }
        }
        """;

        using var handler = new MockHttpMessageHandler(_ => new HttpResponseMessage(HttpStatusCode.OK)
        {
            Content = new StringContent(jsonResponse, Encoding.UTF8, "application/json")
        });
        using var httpClient = new HttpClient(handler);
        using var service = new HttpUpdateCheckService(httpClient, currentVersion: "2.0.0");

        var result = await service.CheckForUpdatesAsync(ct: CancellationToken.None);

        Assert.Equal(UpdateCheckStatus.UpdateAvailable, result.Status);
        Assert.NotNull(result.LatestRelease);
        Assert.Equal("2.1.0", result.LatestRelease.Version);
        Assert.Equal("https://example.com/download", result.LatestRelease.SiteDownload);
        Assert.Equal("https://example.com/sponsor", result.LatestRelease.SiteSponsor);
        Assert.Equal(2, result.LatestRelease.Changelog?.Count);
        Assert.Contains("Mejora 1", result.LatestRelease.Changelog![0]);
        Assert.Equal("9f86d081884c7d659a2feaa0c55ad015a3bf4f1b2b0b822cd15d6c15b0f00a08", result.LatestRelease.Platform?.Windows?.Variants?.Installer?.Sha256);
    }

    [Fact]
    public async Task CheckForUpdatesAsyncWhenCurrentPlatformUnavailableReturnsUpToDate()
    {
        var jsonResponse = """
        {
            "version": "2.2.0",
            "isEnabled": true,
            "platform": {
                "windows": { "isAvailable": false },
                "linux": { "isAvailable": true },
                "macos": { "isAvailable": false }
            }
        }
        """;

        using var handler = new MockHttpMessageHandler(_ => new HttpResponseMessage(HttpStatusCode.OK)
        {
            Content = new StringContent(jsonResponse, Encoding.UTF8, "application/json")
        });
        using var httpClient = new HttpClient(handler);
        using var service = new HttpUpdateCheckService(httpClient, currentVersion: "2.0.0");

        var result = await service.CheckForUpdatesAsync(ct: CancellationToken.None);

        // On Windows test environment, windows isAvailable is false, so it must return UpToDate
        if (OperatingSystem.IsWindows())
        {
            Assert.Equal(UpdateCheckStatus.UpToDate, result.Status);
        }
    }

    [Fact]
    public async Task CheckForUpdatesAsyncWhenVersionDisabledReturnsUpToDate()
    {
        var jsonResponse = """
        {
            "version": "2.5.0",
            "isEnabled": false,
            "platform": {
                "windows": { "isAvailable": true }
            }
        }
        """;

        using var handler = new MockHttpMessageHandler(_ => new HttpResponseMessage(HttpStatusCode.OK)
        {
            Content = new StringContent(jsonResponse, Encoding.UTF8, "application/json")
        });
        using var httpClient = new HttpClient(handler);
        using var service = new HttpUpdateCheckService(httpClient, currentVersion: "2.0.0");

        var result = await service.CheckForUpdatesAsync(ct: CancellationToken.None);

        Assert.Equal(UpdateCheckStatus.UpToDate, result.Status);
    }
}
