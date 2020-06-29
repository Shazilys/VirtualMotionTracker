﻿/*
MIT License

Copyright (c) 2020 gpsnmeajp

Permission is hereby granted, free of charge, to any person obtaining a copy
of this software and associated documentation files (the "Software"), to deal
in the Software without restriction, including without limitation the rights
to use, copy, modify, merge, publish, distribute, sublicense, and/or sell
copies of the Software, and to permit persons to whom the Software is
furnished to do so, subject to the following conditions:

The above copyright notice and this permission notice shall be included in all
copies or substantial portions of the Software.

THE SOFTWARE IS PROVIDED "AS IS", WITHOUT WARRANTY OF ANY KIND, EXPRESS OR
IMPLIED, INCLUDING BUT NOT LIMITED TO THE WARRANTIES OF MERCHANTABILITY,
FITNESS FOR A PARTICULAR PURPOSE AND NONINFRINGEMENT. IN NO EVENT SHALL THE
AUTHORS OR COPYRIGHT HOLDERS BE LIABLE FOR ANY CLAIM, DAMAGES OR OTHER
LIABILITY, WHETHER IN AN ACTION OF CONTRACT, TORT OR OTHERWISE, ARISING FROM,
OUT OF OR IN CONNECTION WITH THE SOFTWARE OR THE USE OR OTHER DEALINGS IN THE
SOFTWARE.
*/
using System;
using System.IO;
using System.IO.MemoryMappedFiles;
using System.Linq;
using System.Text;
using System.Text.Json;
using System.Text.Json.Serialization;
using System.Collections;
using System.Collections.ObjectModel;
using System.Collections.Generic;
using System.Reflection;
using System.Threading;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Data;
using System.Windows.Documents;
using System.Windows.Input;
using System.Windows.Media;
using System.Windows.Media.Imaging;
using System.Windows.Navigation;
using System.Windows.Shapes;
using System.Windows.Threading;
using Valve.VR;
using EasyLazyLibrary;
using System.Numerics;
using Rug.Osc;
using System.Reflection.Emit;
using System.Runtime.InteropServices;

namespace vmt_manager
{
    /// <summary>
    /// MainWindow.xaml の相互作用ロジック
    /// </summary>
public partial class MainWindow : Window
    {
        const string Version = "VMT_002";
        private DispatcherTimer dispatcherTimer;
        Random rnd;
        string title = "";
        bool detectOtherVersion = false;

        int aliveCnt = 0;
        EasyOpenVRUtil util;
        OSC osc;

        public MainWindow()
        {
            InitializeComponent();
        }

        private void Window_Loaded(object sender, RoutedEventArgs e)
        {
            try
            {
                Console.WriteLine("Loaded");
                title = this.Title;
                ManagerVersion.Text = Version;
                DriverVersion.Text = "-";

                dispatcherTimer = new DispatcherTimer();
                dispatcherTimer.Interval = new TimeSpan(0, 0, 0, 0, 100);
                dispatcherTimer.Tick += new EventHandler(GenericTimer);
                dispatcherTimer.Start();

                rnd = new Random();

                util = new EasyOpenVRUtil();
                if (!util.StartOpenVR())
                {
                    Close();
                    return;
                }

                osc = new OSC("127.0.0.1", 39571, 39570, OnBundle, OnMessage);
            }
            catch (Exception ex)
            {
                MessageBox.Show(ex.Message, title);
                Close();
                return;
            }

            string[] args = System.Environment.GetCommandLineArgs();
            if (args.Length > 1) {
                if (args[1] == "install")
                {
                    InstallButton(null, null);
                    Close();
                    return;
                }
                if (args[1] == "uninstall")
                {
                    UninstallButton(null, null);
                    Close();
                    return;
                }
            }
        }

        private void OnMessage(OscMessage message)
        {
            try
            {
                if (message.Address == "/VMT/Out/Log")
                {
                    int stat = (int)message[0];
                    string msg = (string)message[1];

                    switch (stat)
                    {
                        case 0:
                            MessageBox.Show(msg, title, MessageBoxButton.OK, MessageBoxImage.Information);
                            break;
                        case 1:
                            MessageBox.Show(msg, title, MessageBoxButton.OK, MessageBoxImage.Warning);
                            break;
                        case 2:
                            MessageBox.Show(msg, title, MessageBoxButton.OK, MessageBoxImage.Error);
                            break;
                        default:
                            MessageBox.Show(msg, title, MessageBoxButton.OK);
                            break;
                    }
                }
                else if (message.Address == "/VMT/Out/Alive")
                {
                    this.Dispatcher.Invoke(() =>
                    {
                        DriverVersion.Text = (string)message[0];
                        ControlDock.IsEnabled = true;

                        if ((string)message[0] != Version)
                        {
                            DriverVersion.Foreground = new SolidColorBrush(Color.FromRgb(255, 100, 100));
                            detectOtherVersion = true;
                        }
                        else {
                            DriverVersion.Foreground = new SolidColorBrush(Color.FromRgb(0, 255, 0));
                        }

                        aliveCnt = 0;
                    });
                    //Keep Alive
                }
                else {
                    //Do noting
                }

            }
            catch (Exception ex)
            {
                MessageBox.Show(ex.Message, title);
                Close();
                return;
            }
        }

        private void OnBundle(OscBundle bundle)
        {
            for (int i = 0; i < bundle.Count; i++)
            {
                switch (bundle[i])
                {
                    //Messageを受信した
                    case OscMessage msg:
                        OnMessage(msg);
                        break;
                    default:
                        //Do noting
                        break;
                }
            }
        }

        private void GenericTimer(object sender, EventArgs e)
        {
            HmdMatrix34_t m = new HmdMatrix34_t();
            OpenVR.ChaperoneSetup.GetWorkingStandingZeroPoseToRawTrackingPose(ref m);

            RoomMatrixTextBox.Text =
                String.Format("{0:0.00}, {1:0.00}, {2:0.00}, {3:0.00}\n{4:0.00}, {5:0.00}, {6:0.00}, {7:0.00}\n{8:0.00}, {9:0.00}, {10:0.00}, {11:0.00}\n", m.m0, m.m1, m.m2, m.m3, m.m4, m.m5, m.m6, m.m7, m.m8, m.m9, m.m10,m.m11);
            if (
                m.m0 == 0 &&
                m.m1 == 0 &&
                m.m2 == 0 &&
                m.m3 == 0 &&
                m.m4 == 0 &&
                m.m5 == 0 &&
                m.m6 == 0 &&
                m.m7 == 0 &&
                m.m8 == 0 &&
                m.m9 == 0 &&
                m.m10 == 0 &&
                m.m11 == 0
                )
            {
                RoomMatrixTextBox.Background = new SolidColorBrush(Color.FromRgb(255, 100, 100));
                SetRoomMatrixButtonName.IsEnabled = false;
            }
            else {
                RoomMatrixTextBox.Background = new SolidColorBrush(Color.FromRgb(0, 255, 0));
                SetRoomMatrixButtonName.IsEnabled = true;
            }


            var t1 = util.GetTransformBySerialNumber("VMT_0");
            if (t1 != null) {
                string roomPos = String.Format("{0:0.00}, {1:0.00}, {2:0.00}", t1.position.X, t1.position.Y, t1.position.Z);
                CheckPositionTextBox.Text = roomPos;
                if (roomPos == "1.00, 1.00, 1.00") {
                    CheckPositionTextBox.Background = new SolidColorBrush(Color.FromRgb(0, 255, 0));
                }
                else {
                    CheckPositionTextBox.Background = new SolidColorBrush(Color.FromRgb(255, 100, 100));
                }
            }
            var t2 = util.GetTransformBySerialNumberRaw("VMT_0");
            if (t2 != null) {
                CheckPositionRawTextBox.Text = String.Format("{0:0.00}, {1:0.00}, {2:0.00}", t2.position.X, t2.position.Y, t2.position.Z);
            }

            if (aliveCnt > 90)
            {
                DriverVersion.Text = "-";
                DriverVersion.Foreground = new SolidColorBrush(Color.FromRgb(255, 255, 255));
                ControlDock.IsEnabled = false;
            }
            else {
                aliveCnt++;
            }
        }


        private void Window_Closed(object sender, EventArgs e)
        {
            //クローズ
            Console.WriteLine("Closed");
            osc.Dispose();
        }

        private void ResetButton(object sender, RoutedEventArgs e)
        {
            osc.Send(new OscMessage("/VMT/Reset"));
        }

        private void ReloadJsonButton(object sender, RoutedEventArgs e)
        {
            osc.Send(new OscMessage("/VMT/LoadSetting"));
        }

        private void SetRoomMatrixButton(object sender, RoutedEventArgs e)
        {
            HmdMatrix34_t m = new HmdMatrix34_t();
            OpenVR.ChaperoneSetup.GetWorkingStandingZeroPoseToRawTrackingPose(ref m);
            osc.Send(new OscMessage("/VMT/SetRoomMatrix", 
                m.m0, m.m1, m.m2, m.m3, 
                m.m4, m.m5, m.m6, m.m7, 
                m.m8, m.m9, m.m10, m.m11));
        }

        private void ResetRoomMatrixButton(object sender, RoutedEventArgs e)
        {
            osc.Send(new OscMessage("/VMT/SetRoomMatrix", 
                1f,0f,0f,0f,
                0f,1f,0f,0f,
                0f,0f,1f,0f));
        }

        private void CheckPositionButton(object sender, RoutedEventArgs e)
        {
            osc.Send(new OscMessage("/VMT/Room/Driver",
                0, 1, 0f,
                1f, 1f, 1f,
                0f, 0f, 0f, 1f));
        }

        private void InstallButton(object sender, RoutedEventArgs e)
        {
            if (detectOtherVersion) {
                MessageBox.Show("Please remove other version VMT before install.\nインストールを続ける前に、他のバージョンのVMTを削除してください", title,MessageBoxButton.OK,MessageBoxImage.Error);
                return;
            }

            try
            {
                string driverPath_rel = System.IO.Path.GetDirectoryName(Assembly.GetEntryAssembly().Location) + @"\..\vmt";
                string driverPath = System.IO.Path.GetFullPath(driverPath_rel);
                Console.WriteLine(OpenVR.RuntimePath() + @"\bin\win64");
                Console.WriteLine(driverPath);

                System.Diagnostics.Process process = new System.Diagnostics.Process();
                process.StartInfo.WorkingDirectory = OpenVR.RuntimePath() + @"\bin\win64";
                process.StartInfo.FileName = OpenVR.RuntimePath() + @"\bin\win64\vrpathreg.exe";
                process.StartInfo.Arguments = "adddriver \"" + driverPath + "\"";
                process.StartInfo.UseShellExecute = false;
                process.Start();
                process.WaitForExit();

                MessageBox.Show("OK (ExitCode=" + process.ExitCode + ")", title);
            }
            catch (Exception ex) {
                MessageBox.Show(ex.Message, title);
            }
        }

        private void UninstallButton(object sender, RoutedEventArgs e)
        {
            if (detectOtherVersion)
            {
                var res = MessageBox.Show("This manager can not uninstall currently installed VMT version.\nPlease use installed manager version.\nTry it anyway?\n\nこのバージョンのManagerでは現在インストールされているバージョンのVMTをアンインストールすることはできません。\nインストールされているバージョンのManagerを使用してください。\nそれでも実行しますか？", title, MessageBoxButton.OKCancel, MessageBoxImage.Error);
                if (res != MessageBoxResult.OK)
                {
                    return;
                }
            }

            try
            {
                string driverPath_rel = System.IO.Path.GetDirectoryName(Assembly.GetEntryAssembly().Location) + @"\..\vmt";
                string driverPath = System.IO.Path.GetFullPath(driverPath_rel);
                Console.WriteLine(OpenVR.RuntimePath() + @"\bin\win64");
                Console.WriteLine(driverPath);

                System.Diagnostics.Process process = new System.Diagnostics.Process();
                process.StartInfo.WorkingDirectory = OpenVR.RuntimePath() + @"\bin\win64";
                process.StartInfo.FileName = OpenVR.RuntimePath() + @"\bin\win64\vrpathreg.exe";
                process.StartInfo.Arguments = "removedriver \"" + driverPath + "\"";
                process.StartInfo.UseShellExecute = false;
                process.Start();
                process.WaitForExit();

                MessageBox.Show("OK (ExitCode=" + process.ExitCode + ")", title);
            }
            catch (Exception ex)
            {
                MessageBox.Show(ex.Message, title);
            }
        }

        private void ShowAllButton(object sender, RoutedEventArgs e)
        {
            for (int i = 0; i < 58; i++) {
                osc.Send(new OscMessage("/VMT/Room/Driver",
                    i, 1, 0f,
                    (float)rnd.NextDouble()-0.5f, (float)rnd.NextDouble() - 0.5f, (float)rnd.NextDouble() - 0.5f,
                    0f, 0f, 0f, 1f));
            }
        }
    }
}
