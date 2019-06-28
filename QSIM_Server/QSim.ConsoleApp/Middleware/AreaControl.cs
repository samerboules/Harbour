using log4net;
using QSim.ConsoleApp.DataTypes;
using QSim.ConsoleApp.Utilities;
using QSim.ConsoleApp.Utilities.Clipper;
using System;
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.Drawing;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;

namespace QSim.ConsoleApp.Middleware
{
    public sealed class AreaControl
    {
        private static readonly Lazy<AreaControl> lazy = new Lazy<AreaControl>(() => new AreaControl());
        private readonly ILog _log;
        private readonly VisualizationBridge _bridge;

        private readonly Dictionary<Guid, Area> _areas = new Dictionary<Guid, Area>();
        private readonly SemaphoreSlim _areaListLock = new SemaphoreSlim(1, 1);
        private readonly ConcurrentDictionary<Guid, SemaphoreSlim> _waitingForAccessSemaphores = new ConcurrentDictionary<Guid, SemaphoreSlim>();

        public static AreaControl Instance { get { return lazy.Value; } }

        private AreaControl()
        {
            _log = LogManager.GetLogger(GetType());
            _bridge = VisualizationBridge.Instance;
        }

        private IEnumerable<Area> GetIntersectingAreas(List<IntPoint> r)
        {
            return _areas.Values.Where(x => x.Intersects(r));
        }

        public async Task<Guid?> RequestAccess(Rectangle r, string equipmentId, int timeoutMilliseconds = 0)
        {
            return await RequestAccess(r.ToIntPoints(), equipmentId, timeoutMilliseconds);
        }

        public async Task<Guid?> RequestAccess(Area area, int timeoutMilliseconds = 0)
        {
            return await RequestAccess(area.GetPolygon(), area.Owner, timeoutMilliseconds);
        }

        public async Task<Guid?> RequestAccess(List<IntPoint> claimPolygon, string equipmentId, int timeoutMilliseconds = 0)
        {
            _log.Debug($"RequestAccessLocation: {claimPolygon.ToRectangle()} by {equipmentId}");
            await VisualizeWaitingClaim(claimPolygon, equipmentId);

            CancellationTokenSource timeOutCancellationSource = (timeoutMilliseconds > 0) ? new CancellationTokenSource(timeoutMilliseconds) : new CancellationTokenSource();
            CancellationToken cancellationToken = timeOutCancellationSource.Token;

            SemaphoreSlim waitForRelinquish = new SemaphoreSlim(1, 1);
            Guid waitForRelinquishId = Guid.NewGuid();
            while (true)
            {
                try
                {
                    // If cancellationToken is canceled:
                    //  - waitForRelinquish might be entered (but the corresponding semaphore is removed in the catch block)
                    //  - _areaListLock cannot have been entered, as either waiting for the first or the second enter failed.
                    await waitForRelinquish.WaitAsync(cancellationToken);
                    await _areaListLock.WaitAsync(cancellationToken);
                    var intersectingAreas = GetIntersectingAreas(claimPolygon).Where(x => x.Owner != equipmentId);
                    if (intersectingAreas.Any())
                    {
                        _log.Debug($"RequestAccessLocation: WAITING on {claimPolygon.ToRectangle()} by {equipmentId}.");
                        _waitingForAccessSemaphores.TryAdd(waitForRelinquishId, waitForRelinquish);
                    }
                    else
                    {
                        Guid id = Guid.NewGuid();
                        _log.Debug($"RequestAccessLocation: Granted {claimPolygon.ToRectangle()} by {equipmentId}. Id {id}");
                        _areas.Add(id, new Area(claimPolygon, equipmentId));
                        _areaListLock.Release();
                        await VisualizeWaitingClaim(claimPolygon, equipmentId, true);
                        await VisualizeClaim(id);
                        return id;
                    }
                    _areaListLock.Release();
                }
                catch(OperationCanceledException)
                {
                    _log.Debug($"RequestAccessLocation: CANCELLED waiting on {claimPolygon.ToRectangle()} by {equipmentId}.");
                    _waitingForAccessSemaphores.TryRemove(waitForRelinquishId, out _);
                    await VisualizeWaitingClaim(claimPolygon, equipmentId, true);
                    return null;
                }
            }
        }

        public void RelinquishAccess(Guid id)
        {
            _log.Debug($"RelinquishAccess: {id}");
            _areaListLock.Wait();
            _areas.Remove(id);
            _ = VisualizeClaim(id, true);
            var copyOfwaitingForAccessSemaphores = _waitingForAccessSemaphores.Values.ToList();
            // Clear all semaphores to prevent double release later on; if equipment is still interested in access it can re-register itself
            _waitingForAccessSemaphores.Clear();
            copyOfwaitingForAccessSemaphores.ForEach(x => x.Release()); // Let all equipment try to get the requested claim
            _areaListLock.Release();
        }

        public void DumpAreas()
        {
            _log.Info("======================");
            _log.Info(" AREA DUMP ");
            _log.Info("======================");
            _areas.Values.ToList().ForEach(a => _log.Info(a));
            _log.Info("======================");
        }

        private async Task VisualizeClaim(Guid guid, bool delete = false)
        {
            string id = guid.ToString();
            if (delete)
            {
                await _bridge.Delete(id);
                return;
            }

            await _bridge.Claim(id, 0x6600DD00, _areas[guid].GetPolygon());
        }

        private async Task VisualizeWaitingClaim(List<IntPoint> polygon, string equipment, bool delete = false)
        {
            string id = "DESIRED_CLAIM_" + equipment;

            if (delete)
            {
                await _bridge.Delete(id);
                return;
            }

            await _bridge.Claim(id, 0x66DD0000, polygon);
        }
    }
}
