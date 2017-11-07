﻿using System;
using System.ServiceProcess;
using HOLMS.PBXConnector.Support;
using HOLMS.PBXConnector.Connector;
using HOLMS.Application.Client;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Logging.EventLog;

namespace HOLMS.PBXConnector.ServiceRunner {
    public partial class PBXConnectorService : ServiceBase {
        private readonly ILogger _log;
        private RegistryConfigurationProvider _config;
        private PBXConnection _connection;

        public PBXConnectorService() {
            ServiceName = "HOLMS.PBXConnector.ServiceRunner";
            _log = GetProductionLogger();
        }

        protected override void OnStart(string[] args) {
            _log.LogInformation("Starting PBXConnector service");

            try {
                _config = new RegistryConfigurationProvider(_log);
            } catch (Exception ex) {
                _log.LogError($"Error starting PBXConnectorService: {ex}");
                throw;
            }
            _log.LogInformation($"Completed Configuration Read");

            var ac = new ApplicationClient(new PBXApplicationClientConfigurationProvider(_config), 
                _log, "CJASDBYCOKYIWBWNFPQHOBGIQPEJUBSYNEOUEKJZTOSWWCPGCRWNYGBOOUZE");
            _connection = new PBXConnection(_log, _config, ac);
            _log.LogInformation($"Initialized PBX Connection Object");
            _connection.Start();
        }

        protected override void OnStop() {
            _connection.Stop();
        }

        public static ILogger GetProductionLogger() {
            var lf = new LoggerFactory();
            var els = new EventLogSettings {
                LogName = "HOLMS",
                SourceName = "PBXConnector"
            };
            lf.AddEventLog(els);

            return lf.CreateLogger("Scheduler");
        }
    }
}