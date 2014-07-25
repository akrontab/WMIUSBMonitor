/*
 * Very Helpful stackoverflow posts
 *              http://stackoverflow.com/questions/5278860/using-wmi-to-identify-which-device-caused-a-win32-devicechangeevent Using WMI to identify which device caused a Win32_DeviceChangeEvent
 *              http://stackoverflow.com/questions/4793139/detect-iphone-using-wmi-in-c-sharp?rq=1  Detect iPhone using WMI in C# 
 * WMI reference: 
 *              http://msdn.microsoft.com/en-us/library/ms257340.aspx WMI .NET Overview
 * 
 * References:  http://msdn.microsoft.com/en-us/library/aa394504(v=vs.85).aspx  Win32_USBController class
 *              http://msdn.microsoft.com/en-us/library/aa394504(v=vs.85).aspx  Computer System Hardware Classes
 *              http://msdn.microsoft.com/en-us/library/aa394084(v=vs.85).aspx  Win32 Classes
 *              http://msdn.microsoft.com/en-us/library/gg145024.aspx           System.Management Namespaces
 *              http://msdn.microsoft.com/en-us/library/system.management.managementclass.aspx  ManagementClass Class
 *              
 * WMI Queries:
 *              http://www.codeproject.com/Articles/46390/WMI-Query-Language-by-Example
 *              
 * WMI Object stuff:
 *              http://stackoverflow.com/questions/16866456/listing-usb-devices-with-c-sharp-and-wmi?rq=1 Listing USB Devices with C# and WMI
 *              http://stackoverflow.com/questions/2837353/opening-my-application-when-a-usb-device-is-inserted-on-windows-using-wmi Opening my application when a USB device is inserted on Windows using WMI
 *              http://stackoverflow.com/questions/19067178/hardware-device-monitoring-using-c-sharp Hardware Device Monitoring using c#
 *              
 */

using System;
using System.Collections.Generic;
using System.Text;
using System.Management;
using System.Diagnostics;

namespace WMIUSBMonitor
{
    class Program
    {
        static ManagementEventWatcher w = null;
        static TimeSpan pollInterval = new TimeSpan(0, 0, 1); 

        static void Main(string[] args)
        {
            AddRemoveUSBHandler();
            while (true)
            {
                var command = Console.ReadLine();

                switch (command.ToLower())
                {
                    case "clear":
                        Console.Clear();
                        break;
                    case "exit":
                    case "quit":
                        return;
                    default:
                        break;
                }

                if (command.ToLower() == "clear")
                {
                    Console.Clear();
                }
            }
        }

        static void AddRemoveUSBHandler()
        {
            WqlEventQuery q;

            try
            {
                q = new WqlEventQuery();
                q.EventClassName = "__InstanceDeletionEvent";
                q.WithinInterval = pollInterval;
                //q.Condition = "TargetInstance ISA 'Win32_USBControllerdevice'";
                q.Condition = "TargetInstance ISA 'Win32_PnPEntity'";
                //w = new ManagementEventWatcher(scope, q);
                w = new ManagementEventWatcher(q);
                w.EventArrived += USBRemoved;

                w.Start();
            }
            catch (Exception e)
            {
                Console.WriteLine(e.Message);
                if (w != null)
                {
                    w.Stop();
                }
            }
        }


        // TODO : use a Pinvoke to call LockWorkstation in user32.dll
        static void USBRemoved(object sender, EventArrivedEventArgs e)
        {
            var instance = ((PropertyData)(e.NewEvent.Properties["TargetInstance"]));
            var obj = (ManagementBaseObject)instance.Value;
            Console.WriteLine();

            var PropEnum = obj.Properties.GetEnumerator();
            while (PropEnum.MoveNext())
            {
                Console.WriteLine("{0} : {1}", PropEnum.Current.Name, PropEnum.Current.Value);

                if (PropEnum.Current.Name == "Name" && PropEnum.Current.Value.ToString() == "Galaxy Nexus")
                {
                    Process.Start(@"C:\WINDOWS\system32\rundll32.exe", "user32.dll,LockWorkStation");
                }
            }
        }
    }
}