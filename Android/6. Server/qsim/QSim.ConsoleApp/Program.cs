using log4net;
using log4net.Config;
using QSim.ConsoleApp.Middleware;
using QSim.ConsoleApp.Middleware.Scheduling;
using QSim.ConsoleApp.Middleware.Scheduling.JobPool;
using QSim.ConsoleApp.Middleware.StackingSystem;
using QSim.ConsoleApp.Simulators;
using QSim.ConsoleApp.Simulators.SCRouterSystem;
using QSim.ConsoleApp.Utilities;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Reflection;
using System.Threading.Tasks;

namespace QSim.ConsoleApp
{
    class Program
    {
        private static ILog _log;
        private static readonly List<QC> _qcList = new List<QC>();
        private static readonly List<SC> _scList = new List<SC>();
        private static readonly List<ASC> _ascList = new List<ASC>();

        public static async Task Main(string[] args)
        {
            var logRepository = LogManager.GetRepository(Assembly.GetEntryAssembly());
            XmlConfigurator.ConfigureAndWatch(logRepository, new FileInfo("log4net.config"));

            _log = LogManager.GetLogger(typeof(Program));

            //check for -port=12345 in args
            var portString = args.FirstOrDefault(arg => arg.StartsWith("-port=", StringComparison.OrdinalIgnoreCase));
            if (portString == null || portString.Length < 7 || !int.TryParse(portString.Substring(6), out int port) ||
                port < 0)
            {
                port = 8096; //assign default port instead
            }

            var bridge = VisualizationBridge.Instance;
            bridge.StartServer(new TcpServer(port));
            _log.Info("Visualization bridge is started.");
            _log.Info(
                "Press 'g' start the test run, 'a' to show the claimable areas and routes or 'r' to test the routing.");
            var keyStroke = Console.ReadKey(true);
            char testCaseCode = keyStroke.KeyChar;

            for (int i = 0; i < PositionProvider.QcCount; i++)
            {
                _qcList.Add(new QC(i));
            }

            for (int i = 0; i < PositionProvider.ScCount; i++)
            {
                _scList.Add(new SC(i));
            }

            for (int i = 0; i < PositionProvider.AscCount; i++)
            {
                _ascList.Add(new ASC(i));
            }

            var scheduler = new MainScheduler(_qcList, _scList, _ascList);
            scheduler.SetMultiplier(2);

            var inputHandlerTask = Task.Run(() => HandleInput(scheduler));

            switch (testCaseCode)
            {
                case 'g':
                default:
                    _ = RunDemoJobs(scheduler);
                    break;
                case 'a':
                    _ = RouteTest.ShowClaimsAndRoutes(_qcList, _scList, _ascList);
                    break;
                case 'r':
                    _ = RouteTest.TestRoutes(_scList);
                    break;
            }

            _log.Info("Press Q or Escape to exit.");
            _log.Info("Press A, S or J for dumps from AreaControl, Stacking and JobPool respectively");
            await inputHandlerTask;
        }

        private static void HandleInput(MainScheduler scheduler)
        {
            while (true)
            {
                var keyStroke = Console.ReadKey(true);
                switch (keyStroke.Key)
                {
                    case ConsoleKey.Q:
                    case ConsoleKey.Escape:
                        //exit
                        return;
                    case ConsoleKey.R:
                        scheduler.ResetAllScs();
                        break;
                    case ConsoleKey.A:
                        AreaControl.Instance.DumpAreas();
                        break;
                    case ConsoleKey.J:
                        JobPool.Instance.DumpJobs();
                        break;
                    case ConsoleKey.S:
                        Stacking.Instance.DumpStack();
                        break;
                    default:
                        if (char.IsNumber(keyStroke.KeyChar))
                        {
                            double value = char.GetNumericValue(keyStroke.KeyChar);
                            if (value > 0)
                                scheduler.SetMultiplier(value);
                        }

                        break;
                }
            }
        }

        private static async Task RunDemoJobs(MainScheduler scheduler)
        {
            while (true)
            {
                Task[] fillStacks = new Task[]
                {
                    scheduler.RandomFillStack(0),
                    scheduler.RandomFillStowage(50)
                };

                await Task.WhenAll(fillStacks);

                await scheduler.DemoJobs(75);
            }
           
        }

    }
}
