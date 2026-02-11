namespace Sentinel.Application.Interfaces
{
    public interface IUnitOfWork : IDisposable
    {
        IDeviceRepository Devices { get; }
        IAlertRepository Alerts { get; }
        ITelemetryEventRepository TelemetryEvents { get; }
        Task<int> SaveChangesAsync(CancellationToken cancellationToken = default);
    }
}
