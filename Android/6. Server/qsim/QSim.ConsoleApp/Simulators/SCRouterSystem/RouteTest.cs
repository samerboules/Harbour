using log4net;
using QSim.ConsoleApp.DataTypes;
using QSim.ConsoleApp.Middleware;
using QSim.ConsoleApp.Utilities;
using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using PP = QSim.ConsoleApp.Utilities.PositionProvider;

namespace QSim.ConsoleApp.Simulators.SCRouterSystem
{
    class RouteTest
    {
        private static ILog _log = LogManager.GetLogger(typeof(RouteTest));
        private static List<QC> qcList;
        private static List<SC> scList;
        private static List<ASC> ascList;

        private static List<Position> wstpLeftParkPositions = new List<Position>();
        private static List<Position> wstpMidParkPositions = new List<Position>();
        private static List<Position> wstpRightParkPositions = new List<Position>();
        private static List<Position> qcPrePositions = new List<Position>();
        private static List<Position> qcWSLanePositions = new List<Position>();
        private static List<Position> qcLSLanePositions = new List<Position>();
        private static List<Position> qcWSPostPositions = new List<Position>();
        private static List<Position> qcLSPostPositions = new List<Position>();

        public static async Task ShowClaimsAndRoutes(List<QC> qcList, List<SC> scList, List<ASC> ascList)
        {
            RouteTest.qcList = qcList;
            RouteTest.scList = scList;
            RouteTest.ascList = ascList;
            SC sc1 = scList[0];

            CreateTps();
            await ShowTpClaims(sc1);
            await ShowHighways(sc1);
            await Task.Delay(500);

            while (true)
            {
                await ShowInitialRoutes(sc1);
                await ShowTpToTpRoutes(sc1);
                await ShowAcrossHighwayRoutes(sc1);
                await ShowAcrossHighwayHorizontalEndRoutes(sc1);
                await ShowFunnyStartRoutes(sc1);
            }
        }

        private static void CreateTps()
        {
            for (int ascId = 1; ascId <= PositionProvider.AscCount; ascId++)
            {
                for (int i = 0; i < 3; i++)
                {
                    int laneNumber = i == 0 ? 5 : i == 1 ? 3 : 1;
                    int slotNumber = 7;
                    Location wstpParkLocation = new Location(LocationType.WSTP, ascId, slotNumber, laneNumber, 0);
                    Position wstpParkPosition = PositionProvider.GetPosition(wstpParkLocation);
                    (i == 0 ? wstpLeftParkPositions : i == 1 ? wstpMidParkPositions : wstpRightParkPositions).Add(wstpParkPosition);
                }
            }

            for (int qcId = 1; qcId <= PositionProvider.QcCount; qcId++)
            {
                Position prePosition = SC.GetQctpPrePosition(qcId);
                qcPrePositions.Add(prePosition);

                for (int laneNumber = 1; laneNumber <= 2; laneNumber++)
                {
                    Position lanePosition = PositionProvider.GetPosition(new Location(LocationType.QCTP, qcId, 0, laneNumber, 0));
                    (laneNumber == 1 ? qcWSLanePositions : qcLSLanePositions).Add(lanePosition);

                    Position postPosition = SC.GetQctpPostPosition(qcId, laneNumber);
                    (laneNumber == 1 ? qcWSPostPositions : qcLSPostPositions).Add(postPosition);
                }
            }
        }

        private static async Task ShowTpClaims(SC sc)
        {
            List<Position> allPositions = new List<Position>();
            allPositions.AddRange(wstpLeftParkPositions);
            allPositions.AddRange(wstpMidParkPositions);
            allPositions.AddRange(wstpRightParkPositions);
            allPositions.AddRange(qcPrePositions);
            allPositions.AddRange(qcWSLanePositions);
            allPositions.AddRange(qcLSLanePositions);
            allPositions.AddRange(qcWSPostPositions);
            allPositions.AddRange(qcLSPostPositions);
            foreach (var position in allPositions)
            {
                _ = await AreaControl.Instance.RequestAccess(sc.GetOccupiedAreaAt(position), sc.Id);
            }

            // Show QC TP areas.
            for (int qcId = 1; qcId <= PositionProvider.QcCount; qcId++)
            {
                _ = await AreaControl.Instance.RequestAccess(qcList[qcId - 1].GetQctpClaim(), sc.Id);
            }
        }

        private static async Task ShowHighways(SC sc)
        {
            foreach (var highway in PositionProvider.GetHighways())
            {
                _ = await AreaControl.Instance.RequestAccess(sc.GetOccupiedAreaAt(highway.from, highway.from.DistanceTo(highway.to)), sc.Id);
            }
        }

        private static async Task ShowInitialRoutes(SC sc)
        {
            List<Guid> claimIds = new List<Guid>();

            // We can only show initial route for first SC because each SC has its own claim.
            for (int i = 0; i < 1; i++)
            {
                Position start = PositionProvider.GetEquipmentPosition(PositionProvider.IndexToId("SC", i));
                claimIds.AddRange(await ClaimRoute(sc, start, wstpLeftParkPositions[0]));
            }

            await Task.Delay(10 * 1000);
            ReleaseClaims(claimIds);
        }

        private static async Task ShowTpToTpRoutes(SC sc)
        {
            List<Guid> claimIds = new List<Guid>();

            claimIds.AddRange(await ClaimRoute(sc, wstpLeftParkPositions[0], qcPrePositions[0]));
            claimIds.AddRange(await ClaimRoute(sc, qcWSPostPositions[0], wstpLeftParkPositions[2]));
            claimIds.AddRange(await ClaimRoute(sc, qcPrePositions[0], wstpRightParkPositions[1]));
            claimIds.AddRange(await ClaimRoute(sc, qcPrePositions[2], qcWSLanePositions[2]));
            claimIds.AddRange(await ClaimRoute(sc, wstpMidParkPositions[5], wstpMidParkPositions[6]));
            claimIds.AddRange(await ClaimRoute(sc, wstpMidParkPositions[3], wstpMidParkPositions[2]));
            claimIds.AddRange(await ClaimRoute(sc, qcLSPostPositions[1], qcPrePositions[3]));
            claimIds.AddRange(await ClaimRoute(sc, qcLSPostPositions[3], qcPrePositions[2]));
            claimIds.AddRange(await ClaimRoute(sc, qcWSPostPositions[3], wstpRightParkPositions[9]));

            await Task.Delay(10 * 1000);
            ReleaseClaims(claimIds);
        }

        private static async Task ShowAcrossHighwayRoutes(SC sc)
        {
            List<Guid> claimIds = new List<Guid>();

            claimIds.AddRange(await ShowAcrossHighwayRoute(sc, wstpMidParkPositions[4], 0, PP.NORTH));
            claimIds.AddRange(await ShowAcrossHighwayRoute(sc, wstpRightParkPositions[4], 1, PP.NORTH));
            claimIds.AddRange(await ShowAcrossHighwayRoute(sc, wstpLeftParkPositions[4], -1, PP.NORTH));

            claimIds.AddRange(await ShowAcrossHighwayRoute(sc, wstpLeftParkPositions[5], SC.TURNING_RADIUS - 1, PP.NORTH));
            claimIds.AddRange(await ShowAcrossHighwayRoute(sc, wstpRightParkPositions[3], -(SC.TURNING_RADIUS - 1), PP.NORTH));

            claimIds.AddRange(await ShowAcrossHighwayRoute(sc, wstpRightParkPositions[5], SC.TURNING_RADIUS, PP.NORTH));
            claimIds.AddRange(await ShowAcrossHighwayRoute(sc, wstpLeftParkPositions[3], -SC.TURNING_RADIUS, PP.NORTH));

            claimIds.AddRange(await ShowAcrossHighwayRoute(sc, wstpLeftParkPositions[6], SC.TURNING_RADIUS + 1, PP.NORTH));
            claimIds.AddRange(await ShowAcrossHighwayRoute(sc, wstpRightParkPositions[2], -(SC.TURNING_RADIUS + 1), PP.NORTH));

            claimIds.AddRange(await ShowAcrossHighwayRoute(sc, wstpRightParkPositions[6], SC.TURNING_RADIUS * 2, PP.NORTH));
            claimIds.AddRange(await ShowAcrossHighwayRoute(sc, wstpLeftParkPositions[2], -(SC.TURNING_RADIUS * 2), PP.NORTH));

            await Task.Delay(10 * 1000);
            ReleaseClaims(claimIds);
        }

        private static async Task ShowAcrossHighwayHorizontalEndRoutes(SC sc)
        {
            List<Guid> claimIds = new List<Guid>();

            claimIds.AddRange(await ShowAcrossHighwayRoute(sc, wstpLeftParkPositions[0], -(SC.TURNING_RADIUS * 2), PP.EAST));
            claimIds.AddRange(await ShowAcrossHighwayRoute(sc, wstpRightParkPositions[0], -(SC.TURNING_RADIUS + 501), PP.EAST));
            claimIds.AddRange(await ShowAcrossHighwayRoute(sc, wstpLeftParkPositions[1], -(SC.TURNING_RADIUS + 500), PP.EAST));
            claimIds.AddRange(await ShowAcrossHighwayRoute(sc, wstpRightParkPositions[1], -(SC.TURNING_RADIUS + 499), PP.EAST));
            claimIds.AddRange(await ShowAcrossHighwayRoute(sc, wstpLeftParkPositions[2], -SC.TURNING_RADIUS, PP.EAST));
            claimIds.AddRange(await ShowAcrossHighwayRoute(sc, wstpRightParkPositions[2], 0, PP.EAST));
            claimIds.AddRange(await ShowAcrossHighwayRoute(sc, wstpLeftParkPositions[3], SC.TURNING_RADIUS + 500, PP.EAST));
            claimIds.AddRange(await ShowAcrossHighwayRoute(sc, wstpLeftParkPositions[4], 2 * SC.TURNING_RADIUS, PP.EAST));
            claimIds.AddRange(await ShowAcrossHighwayRoute(sc, wstpLeftParkPositions[5], 3 * SC.TURNING_RADIUS, PP.EAST));
            claimIds.AddRange(await ShowAcrossHighwayRoute(sc, wstpLeftParkPositions[6], 4 * SC.TURNING_RADIUS, PP.EAST));

            await Task.Delay(10 * 1000);
            ReleaseClaims(claimIds);
        }

        private static async Task<List<Guid>> ShowAcrossHighwayRoute(SC sc, Position from, int deltaX, double finalOrientation)
        {
            Position to = from.Copy();
            to.y = qcPrePositions[0].y;
            to.x += deltaX;
            to.phi = finalOrientation;
            return await ClaimRoute(sc, from, to);
        }

        private static async Task ShowFunnyStartRoutes(SC sc)
        {
            List<Guid> claimIds = new List<Guid>();

            Position start1 = new Position(wstpLeftParkPositions[0].x, PositionProvider.GetHighwayInOrientation(PP.EAST).from.y + 500, 0, PP.NORTH);
            claimIds.Add((await AreaControl.Instance.RequestAccess(sc.GetOccupiedAreaAt(start1), sc.Id)).Value);
            claimIds.AddRange(await ClaimRoute(sc, start1, wstpLeftParkPositions[1]));

            Position start2 = new Position(wstpRightParkPositions[2].x, PositionProvider.GetHighwayInOrientation(PP.WEST).from.y, 0, PP.NORTH);
            claimIds.Add((await AreaControl.Instance.RequestAccess(sc.GetOccupiedAreaAt(start2), sc.Id)).Value);
            claimIds.AddRange(await ClaimRoute(sc, start2, wstpRightParkPositions[1]));

            Position start3 = new Position(wstpLeftParkPositions[3].x, PositionProvider.GetHighwayInOrientation(PP.WEST).from.y, 0, PP.NORTH);
            claimIds.Add((await AreaControl.Instance.RequestAccess(sc.GetOccupiedAreaAt(start3), sc.Id)).Value);
            claimIds.AddRange(await ClaimRoute(sc, start3, wstpLeftParkPositions[4]));

            Position start4 = new Position(wstpRightParkPositions[5].x, PositionProvider.GetHighwayInOrientation(PP.EAST).from.y, 0, PP.NORTH);
            claimIds.Add((await AreaControl.Instance.RequestAccess(sc.GetOccupiedAreaAt(start4), sc.Id)).Value);
            claimIds.AddRange(await ClaimRoute(sc, start4, wstpRightParkPositions[4]));

            Position start5 = new Position(wstpLeftParkPositions[6].x, PositionProvider.GetHighwayInOrientation(PP.EAST).from.y, 0, PP.NORTH);
            claimIds.Add((await AreaControl.Instance.RequestAccess(sc.GetOccupiedAreaAt(start5), sc.Id)).Value);
            claimIds.AddRange(await ClaimRoute(sc, start5, qcPrePositions[3]));

            await Task.Delay(10 * 1000);
            ReleaseClaims(claimIds);
        }

        private static async Task<List<Guid>> ClaimRoute(SC sc, Position from, Position to)
        {
            SCRouter router = new SCRouter();

            List<RoutePoint> route = router.GetRoute(from, to);
            List<Guid> claimIds = new List<Guid>();
            foreach (var point in route)
            {
                Guid? claimId = await AreaControl.Instance.RequestAccess(sc.GetOccupiedAreaAt(point.Position), sc.Id);
                if (claimId == null)
                {
                    _log.Info($"Failed to get claim at {point.Position} for {sc.Id}");
                }
                else
                {
                    claimIds.Add(claimId.Value);
                }
            }
            return claimIds;
        }

        private static void ReleaseClaims(List<Guid> claimIds)
        {
            foreach (var claimId in claimIds)
            {
                AreaControl.Instance.RelinquishAccess(claimId);
            }
        }

        public static async Task TestRoutes(List<SC> scList)
        {
            //await TestVariousRoutes(scList);
            //await TestWstpToWstp(scList);
            //await TestQcToQc(scList);
            await TestQcToWstp(scList);
        }

        private static async Task TestVariousRoutes(List<SC> scList)
        {
            _log.Info("Starting various route tests.");

            RouteTest.scList = scList;
            SC sc1 = scList[0];
            SC sc2 = scList[1];
            SC sc3 = scList[2];

            Task[] tasks1 = new Task[]
            {
                DriveTo(sc1, new Location(LocationType.WSTP, 1, 7, 5, 0)),
                DriveTo(sc2, new Location(LocationType.WSTP, 4, 7, 5, 0)),
                DriveTo(sc3, new Location(LocationType.SCPARK, 1, 0, 0, 0))
            };
            await Task.WhenAll(tasks1);
            await Task.Delay(1000);

            Position onHighway1 = new Position(sc1.Position.x, PP.GetHighwayInOrientation(PP.WEST).from.y, 0, PP.NORTH);
            Task[] tasks2 = new Task[]
            {
                DriveTo(sc1, onHighway1),
                DriveTo(sc2, SC.GetQctpPrePosition(1)),
                DriveTo(sc3, new Location(LocationType.SCPARK, 2, 0, 0, 0))
            };
            await Task.WhenAll(tasks2);
            await Task.Delay(1000);

            Position wstp971 = PP.GetPosition(new Location(LocationType.WSTP, 4, 7, 5, 0));
            Task[] tasks3 = new Task[]
            {
                DriveTo(sc1, new Position(wstp971.x, PP.GetHighwayInOrientation(PP.EAST).from.y, 0, PP.EAST)),
                DriveTo(sc2, new Location(LocationType.QCTP, 1, 0, 1, 0))
            };
            await Task.WhenAll(tasks3);
            await Task.Delay(1000);

            Task[] tasks4 = new Task[]
            {
                DriveTo(sc1, new Location(LocationType.SCPARK, 3, 0, 0, 0)),
                DriveTo(sc2, new Location(LocationType.SCPARK, 1, 0, 0, 0))
            };
            await Task.WhenAll(tasks4);

            _log.Info("Various route tests finished.");
        }

        private static async Task TestWstpToWstp(List<SC> scList)
        {
            _log.Info("Starting WSTP to WSTP route tests.");

            RouteTest.scList = scList;
            SC sc1 = scList[0];

            await DriveTo(sc1, new Location(LocationType.WSTP, 1, 7, 5, 0));
            await Task.Delay(250);
            await DriveTo(sc1, new Location(LocationType.WSTP, 1, 1, 5, 0));
            await Task.Delay(250);
            await DriveTo(sc1, new Location(LocationType.WSTP, 1, 7, 5, 0));
            await Task.Delay(250);
            await DriveTo(sc1, new Location(LocationType.WSTP, 1, 7, 4, 0));
            await Task.Delay(250);
            await DriveTo(sc1, new Location(LocationType.WSTP, 1, 7, 2, 0));
            await Task.Delay(250);
            await DriveTo(sc1, new Location(LocationType.WSTP, 2, 7, 5, 0));
            await Task.Delay(250);
            await DriveTo(sc1, new Location(LocationType.WSTP, 2, 7, 2, 0));
            await Task.Delay(250);
            await DriveTo(sc1, new Location(LocationType.WSTP, 3, 7, 5, 0));
            await Task.Delay(250);
            await DriveTo(sc1, new Location(LocationType.WSTP, 3, 7, 1, 0));
            await Task.Delay(250);
            await DriveTo(sc1, new Location(LocationType.WSTP, 4, 7, 1, 0));
            await Task.Delay(250);
            await DriveTo(sc1, new Location(LocationType.WSTP, 3, 7, 1, 0));
            await Task.Delay(250);
            await DriveTo(sc1, new Location(LocationType.WSTP, 3, 7, 5, 0));
            await Task.Delay(250);
            await DriveTo(sc1, new Location(LocationType.WSTP, 2, 7, 2, 0));
            await Task.Delay(250);
            await DriveTo(sc1, new Location(LocationType.WSTP, 2, 7, 5, 0));
            await Task.Delay(250);
            await DriveTo(sc1, new Location(LocationType.WSTP, 1, 7, 2, 0));
            await Task.Delay(250);
            await DriveTo(sc1, new Location(LocationType.WSTP, 1, 7, 4, 0));
            await Task.Delay(250);
            await DriveTo(sc1, new Location(LocationType.WSTP, 1, 7, 5, 0));

            _log.Info("WSTP to WSTP route tests finished.");
        }

        private static async Task TestQcToQc(List<SC> scList)
        {
            _log.Info("Starting QC to QC route tests.");

            RouteTest.scList = scList;
            SC sc1 = scList[0];

            await DriveTo(sc1, SC.GetQctpPrePosition(1));
            await Task.Delay(250);
            await DriveTo(sc1, new Location(LocationType.QCTP, 1, 0, 1, 0));
            await Task.Delay(250);
            await DriveTo(sc1, SC.GetQctpPostPosition(1, 1));

            await Task.Delay(250);
            await DriveTo(sc1, SC.GetQctpPrePosition(2));
            await Task.Delay(250);
            await DriveTo(sc1, new Location(LocationType.QCTP, 2, 0, 2, 0));
            await Task.Delay(250);
            await DriveTo(sc1, SC.GetQctpPostPosition(2, 2));

            await Task.Delay(250);
            await DriveTo(sc1, SC.GetQctpPrePosition(3));
            await Task.Delay(250);
            await DriveTo(sc1, SC.GetQctpPrePosition(4));
            await Task.Delay(250);
            await DriveTo(sc1, SC.GetQctpPrePosition(3));

            await Task.Delay(250);
            await DriveTo(sc1, SC.GetQctpPrePosition(1));
            await Task.Delay(250);
            await DriveTo(sc1, new Location(LocationType.QCTP, 1, 0, 1, 0));
            await Task.Delay(250);
            await DriveTo(sc1, SC.GetQctpPostPosition(1, 1));
            await Task.Delay(250);
            Position shifted = SC.GetQctpPrePosition(2).Copy();
            shifted.x = sc1.Position.x + 500;
            await DriveTo(sc1, shifted);

            _log.Info("QC to QC route test finished.");
        }

        private static async Task TestQcToWstp(List<SC> scList)
        {
            _log.Info("Starting QC to WSTP route tests.");

            RouteTest.scList = scList;
            SC sc1 = scList[0];

            await DriveTo(sc1, SC.GetQctpPrePosition(2));
            for (int block = 2; block <= 4; block++)
            {
                for (int lane = 5; lane >= 1; lane--)
                {
                    await Task.Delay(250);
                    await DriveTo(sc1, new Location(LocationType.WSTP, block, 7, lane, 0));
                    await Task.Delay(250);
                    await DriveTo(sc1, SC.GetQctpPrePosition(2));
                }
            }

            await Task.Delay(250);
            await DriveTo(sc1, SC.GetQctpPostPosition(2, 1));
            for (int block = 3; block <= 5; block++)
            {
                for (int lane = 5; lane >= 1; lane--)
                {
                    await Task.Delay(250);
                    await DriveTo(sc1, new Location(LocationType.WSTP, block, 7, lane, 0));
                    await Task.Delay(250);
                    await DriveTo(sc1, SC.GetQctpPostPosition(2, 1));
                }
            }

            _log.Info("QC to WSTP route test finished.");
        }

        private static async Task DriveTo(SC sc, Location location)
        {
            await DriveTo(sc, PositionProvider.GetPosition(location));
        }

        private static async Task DriveTo(SC sc, Position position)
        {
            _log.Info($"{sc.Id} driving from {sc.Position} to {position}");

            Guid? claimId = null;
            while (claimId == null)
            {
                claimId = await AreaControl.Instance.RequestAccess(sc.GetOccupiedAreaAt(position), sc.Id, 1000);
            }
            _log.Info($"{sc.Id} obtained destination claim for {sc.Position}");

            bool arrived = false;
            while (!arrived)
            {
                arrived = await sc.DriveTo(position);
                _log.Info($"{sc.Id} did not yet arrive at {sc.Position}");
            }
            AreaControl.Instance.RelinquishAccess(claimId.Value);
            _log.Info($"{sc.Id} arrived at {sc.Position}");
        }
    }
}
