﻿using System;
using System.Collections.Generic;
using System.Text;
using System.Globalization;
using System.Net;
using System.Net.Sockets;
using System.Windows.Forms;
using System.Diagnostics;
using System.IO;
using NiceHashMiner.Configs;
using NiceHashMiner.Enums;
using NiceHashMiner.Devices;

namespace NiceHashMiner.Miners
{
    abstract public class ccminer : Miner
    {
        public ccminer(bool queryComputeDevices) : base(queryComputeDevices) { }

        public override void Start(Algorithm miningAlgorithm, string url, string username)
        {
            //if (ProcessHandle != null) return; // ignore, already running 

            CurrentMiningAlgorithm = miningAlgorithm;
            if (miningAlgorithm == null) return;

            LastCommandLine = "--algo=" + miningAlgorithm.MinerName +
                                  " --url=" + url +
                                  " --userpass=" + username + ":" + GetPassword(miningAlgorithm) +
                                  " --api-bind=" + APIPort.ToString() +
                                  " " + ExtraLaunchParameters +
                                  " " + miningAlgorithm.ExtraLaunchParameters +
                                  " --devices ";

            LastCommandLine += GetDevicesCommandString();

            Path = GetOptimizedMinerPath(miningAlgorithm.NiceHashID);

            ProcessHandle = _Start();
        }

        protected override void _Stop(bool willswitch) {
            Stop_cpu_ccminer_sgminer(willswitch);
        }

        protected override void QueryCDevs() {
            //catch (Exception e) {
            //    Helpers.ConsolePrint(MinerDeviceName, "Exception: " + e.ToString());

            //    MinerFileNotFoundDialog WarningDialog = new MinerFileNotFoundDialog(MinerDeviceName, Path);
            //    WarningDialog.ShowDialog();

            //    if (WarningDialog.DisableDetection) {
            //        if (this is ccminer_sm5x)
            //            ConfigManager.Instance.GeneralConfig.DeviceDetection.DisableDetectionNVidia5X = true;
            //        else if (this is ccminer_sm3x)
            //            ConfigManager.Instance.GeneralConfig.DeviceDetection.DisableDetectionNVidia3X = true;
            //        else if (this is ccminer_sm21)
            //            ConfigManager.Instance.GeneralConfig.DeviceDetection.DisableDetectionNVidia2X = true;
            //    }

            //    WarningDialog = null;

            //    return;
            //}
        }

        // new decoupled benchmarking routines
        #region Decoupled benchmarking routines
        protected override string BenchmarkCreateCommandLine(DeviceBenchmarkConfig benchmarkConfig, Algorithm algorithm, int time) {
            string CommandLine = " --algo=" + algorithm.MinerName +
                              " --benchmark" +
                              " --time-limit " + time.ToString() +
                              " " + ExtraLaunchParameters +
                              " " + algorithm.ExtraLaunchParameters +
                              " --devices ";

            CommandLine += GetDevicesCommandString();

            Path = GetOptimizedMinerPath(algorithm.NiceHashID);

            return CommandLine;
        }

        protected override bool BenchmarkParseLine(string outdata) {
            double lastSpeed = 0;
            if (double.TryParse(outdata, out lastSpeed)) {
                BenchmarkAlgorithm.BenchmarkSpeed = lastSpeed;
                return true;
            }
            return false;
        }

        protected override void BenchmarkOutputErrorDataReceivedImpl(string outdata) {
            CheckOutdata(outdata);
        }

        #endregion // Decoupled benchmarking routines

        public override APIData GetSummary() {
            return GetSummaryCPU_CCMINER();
        }

    }
}