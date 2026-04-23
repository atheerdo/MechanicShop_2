namespace MechanicShop.Client.Hubs;

using Microsoft.AspNetCore.Components;
using Microsoft.AspNetCore.Components.WebAssembly.Hosting;
using Microsoft.AspNetCore.SignalR.Client;

/// <summary>
/// Client for managing SignalR hub connections to WorkOrders.
/// </summary>
public sealed class WorkOrderHubClient : IAsyncDisposable
{
    private readonly HubConnection hubConnection;
    private bool isStarted;
    private bool isDisposed;

    /// <summary>
    /// Initializes a new instance of the <see cref="WorkOrderHubClient"/> class.
    /// </summary>
    /// <param name="env">The WebAssembly host environment.</param>
    public WorkOrderHubClient(IWebAssemblyHostEnvironment env)
    {
        var baseUrl = env.BaseAddress;

        this.hubConnection = new HubConnectionBuilder()
            .WithUrl($"{baseUrl}hubs/workorders")
            .WithAutomaticReconnect()
            .Build();
    }

    /// <summary>
    /// Starts the hub connection and registers handlers.
    /// </summary>
    /// <param name="onWorkOrdersChanged">Callback to invoke when work orders change.</param>
    /// <returns>A task representing the asynchronous operation.</returns>
    public async Task StartAsync(Func<Task> onWorkOrdersChanged)
    {
        if (this.isDisposed || this.isStarted)
        {
            return;
        }

        this.hubConnection.On("WorkOrdersChanged", async () =>
        {
            if (!this.isDisposed)
            {
                await onWorkOrdersChanged.Invoke();
            }
        });

        await this.hubConnection.StartAsync();
        this.isStarted = true;
    }

    /// <summary>
    /// Disposes the hub connection and releases resources.
    /// </summary>
    /// <returns>A value task representing the asynchronous operation.</returns>
    public async ValueTask DisposeAsync()
    {
        if (this.isDisposed)
        {
            return;
        }

        this.isDisposed = true;

        if (this.hubConnection.State is HubConnectionState.Connected or HubConnectionState.Connecting)
        {
            await this.hubConnection.StopAsync();
        }

        await this.hubConnection.DisposeAsync();
    }
}