using System;
using System.Diagnostics;
using System.Diagnostics.Eventing.Reader;
using System.Net;
using System.Text;
using System.Net.Sockets;
using System.Linq;

namespace SNMP_Event_Sender
{
    public class Program
    {

        public static void Main(string[] args)
        {
            EventLogWatcher watcher = null;
            
            //string IPAddr = args.ElementAt(1);
            string SyslogType = args.First();
            int MinLogLevel = Convert.ToInt32(args.Last());

        try
        {
            Console.WriteLine("Log: " + args.First() + " -- Min Level: " + args.Last());

            //Min Log Level -- Higher Number is lower log level
            EventLogQuery subscriptionQuery = new EventLogQuery(SyslogType, PathType.LogName, "*[System/Level<=" + MinLogLevel + "]");
            watcher = new EventLogWatcher(subscriptionQuery);

            // (EventLogEventRead) will be called.
            watcher.EventRecordWritten += new EventHandler<EventRecordWrittenEventArgs>(EventLogEventRead);

            // Begin subscribing to events
            watcher.Enabled = true;

            //Keep waiting for 25000 or more years
            for (int i = 0; i < 90000000; i++)
            {
                System.Threading.Thread.Sleep(900000000);
            }

        }
        catch (EventLogReadingException e)
        {
            Console.WriteLine("Execute with arg 1 being System,Application,Security, etc and arg2 being the minimum log level OR Error reading the log: {0}", e.Message);
        }
        finally
        {
            // Stop listening to events
            watcher.Enabled = false;
            if (watcher != null)
            {
                watcher.Dispose();
        }
    }
}

        public static void EventLogEventRead(object obj, EventRecordWrittenEventArgs arg)
        {

            // Make sure there was no error reading the event.
            if (arg.EventRecord != null)
            {
                //send the event data over udp to pre-specified message receiver at ipadd.parse address and port below
                Socket sock = new Socket(AddressFamily.InterNetwork, SocketType.Dgram,
                ProtocolType.Udp);
                  IPAddress serverAddr = IPAddress.Parse("INSERT TARGET IP ADDRESS OF SNMP LISTENER EMAILER SERVER");
                IPEndPoint endPoint = new IPEndPoint(serverAddr, 162);
                string messedge = "EventID " + arg.EventRecord.Id + " " + arg.EventRecord.FormatDescription();
                byte[] send_buffer = Encoding.ASCII.GetBytes(messedge);
                sock.SendTo(send_buffer, endPoint);
                sock.Close();

                Console.WriteLine("Received event {0} at " + DateTime.Now, arg.EventRecord.Id, arg.EventRecord.FormatDescription());

            }
            else
            {
                Console.WriteLine("The event instance was null.");
            }
        }
    }
}

