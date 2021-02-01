using System;
using System.Collections.Generic;
using System.Windows;
using System.Threading;
using System.Windows.Threading;
using OpenHardwareMonitor.Hardware;

namespace HardwareMonitor
{
    /// <summary>
    /// MainWindow.xaml 的交互逻辑
    /// </summary>
    public partial class MainWindow : Window
    {
        private List<object> cpuList;
        private Timer threadTimer;
        private readonly UpdateVisitor updateVisitor = new UpdateVisitor();
        private readonly Computer computer = new Computer();

        public MainWindow()
        {
            InitializeComponent();
        }

        private void Window_Loaded(object sender, RoutedEventArgs e)
        {

            cpuList = new List<object>();

            cpus.ItemsSource = cpuList;
            RunAddThread();

            computer.Open();
            computer.CPUEnabled = true;
            computer.GPUEnabled = true;
            DataContext = this;
        }

        private void DealCPU(IHardware 硬件)
        {
            cpuList.Clear();
            for (var 属性序号 = 0; 属性序号 < 硬件.Sensors.Length; 属性序号++)
            {
                var 属性 = 硬件.Sensors[属性序号];
                if (属性.SensorType == SensorType.Load)
                {
                    if (属性.Index == 0)
                    {
                        var process = 属性.Value / 100 * 50;
                        var 颜色 = "Yellow";
                        if (process >= 32)
                        {
                            颜色 = "Red";
                        }
                        else if (process >= 16)
                        {
                            颜色 = "YellowGreen";
                        }

                        CPUCanvas.DataContext = new
                        {
                            color = 颜色,
                            process,
                        };
                    }
                    else
                    {
                        var height = 属性.Value / 100 * 15;
                        var 颜色 = "Yellow";
                        if (height >= 10)
                        {
                            颜色 = "Red";
                        }
                        else if (height >= 5)
                        {
                            颜色 = "YellowGreen";
                        }
                        cpuList.Add(new
                        {
                            height,
                            top = 15 - height,
                            color = 颜色
                        });
                    }
                }
                else if (属性.SensorType == SensorType.Temperature)
                {
                    if (属性.Index == 0)
                    {
                        cpuTem.Text = $"{属性.Value:00}℃";
                    }
                }
            }
        }

        private void DealGPU(IHardware 硬件)
        {
            for (var 属性序号 = 0; 属性序号 < 硬件.Sensors.Length; 属性序号++)
            {
                var 属性 = 硬件.Sensors[属性序号];
                if (属性.SensorType == SensorType.Temperature)
                {
                    if (属性.Index == 0)
                    {
                        gpuTem.Text = $"{属性.Value:00}℃";
                    }
                }
                else if (属性.SensorType == SensorType.SmallData)
                {
                    if (属性.Index == 1)
                    {
                        gpuFree.Text = $"{属性.Value} MB";
                    }
                }
                else if (属性.SensorType == SensorType.Load)
                {
                    if (属性.Index == 0)
                    {
                        var process = 属性.Value / 100 * 50;
                        var 颜色 = "Yellow";
                        if (process >= 32)
                        {
                            颜色 = "Red";
                        }
                        else if (process >= 16)
                        {
                            颜色 = "YellowGreen";
                        }

                        GPUCanvas.DataContext = new
                        {
                            color = 颜色,
                            process,
                        };
                    }
                    else if (属性.Index == 4)
                    {
                        var process = 属性.Value / 100 * 50;
                        var 颜色 = "Yellow";
                        if (process >= 32)
                        {
                            颜色 = "Red";
                        }
                        else if (process >= 16)
                        {
                            颜色 = "YellowGreen";
                        }

                        GPUM.DataContext = new
                        {
                            color = 颜色,
                            process,
                        };
                    }
                }
            }
        }

        private void DealText(Computer computer)
        {
            for (var 硬件序号 = 0; 硬件序号 < computer.Hardware.Length; 硬件序号++)
            {
                var 硬件 = computer.Hardware[硬件序号];
                if (硬件.HardwareType == HardwareType.CPU)
                {
                    DealCPU(硬件);
                }
                else if (硬件.HardwareType == HardwareType.GpuNvidia)
                {
                    DealGPU(硬件);
                }
            }
        }

        private void RunAddThread()
        {
            threadTimer = new Timer((object state) =>
            {
                computer.Accept(updateVisitor);
                this.Dispatcher.BeginInvoke(DispatcherPriority.SystemIdle, (Action)delegate ()
                {
                    DealText(computer);
                    cpus.Items.Refresh();
                    Topmost = true;

                });
            }, this.cpus, 1000, 1000);
        }

        private void Window_Closed(object sender, EventArgs e)
        {
            computer.Close();
            threadTimer.Dispose();
        }

        private void TextBlock_MouseLeftButtonDown(object sender, System.Windows.Input.MouseButtonEventArgs e)
        {
            DragMove();
        }
    }
}
