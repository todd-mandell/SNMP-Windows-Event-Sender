using System;
using System.Diagnostics;
using System.Diagnostics.Eventing.Reader;
using System.Net;
using System.Text;
using System.Net.Sockets;
using System.Linq;

namespace SNMP_Event_Sender
{
    class Program
    {
        static void Main(string[] args)
        {

            Console.WriteLine("Type the eventlog you wish to monitor --");
            string SyslogType = Console.ReadLine();
            Console.WriteLine("Type the minimum XML event viewer log level --");
            string MinLogLevel = Console.ReadLine();

        EventLogWatcher watcher = null;

        try
        {

            EventLogQuery subscriptionQuery = new EventLogQuery(SyslogType, PathType.LogName, "*[System/Level<"+MinLogLevel+"]");

            watcher = new EventLogWatcher(subscriptionQuery);

            // Set watcher to listen for the EventRecordWritten
            // event.  When this event happens, the callback method
            // (EventLogEventRead) will be called.
            watcher.EventRecordWritten +=  new EventHandler<EventRecordWrittenEventArgs>(EventLogEventRead);

            // Begin subscribing to events the events
            watcher.Enabled = true;

            //Keep waiting for 25000 or more years
            for (int i = 0; i < 90000000; i++)
            {
                System.Threading.Thread.Sleep(900000000);
            }
        }
        catch (EventLogReadingException e)
        {
            Console.WriteLine("Error reading the log: {0}", e.Message);
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

    /// <summary>
    /// Callback method that gets executed when an event is
    /// reported to the subscription.
    /// </summary>
    public static void EventLogEventRead(object obj,
        EventRecordWrittenEventArgs arg)
    {
        // Make sure there was no error reading the event.
        if (arg.EventRecord != null)
        {
            //send the event data over udp to pre-specified SNMP message receiver at ipadd.parse address and port below
            Socket sock = new Socket(AddressFamily.InterNetwork, SocketType.Dgram,
            ProtocolType.Udp);
            IPAddress serverAddr = IPAddress.Parse("!!!INSERT-THE-IP-ADDRESS-OF-SNMP-LISTENER-EMAILER-HOST-HERE!!!");
            IPEndPoint endPoint = new IPEndPoint(serverAddr, 162);
            string messedge = "EventID " + arg.EventRecord.Id + " " + arg.EventRecord.FormatDescription();
            byte[] send_buffer = Encoding.ASCII.GetBytes(messedge);
            sock.SendTo(send_buffer, endPoint);
            sock.Close();
            
            Console.WriteLine("Received event {0} from the subscription.",arg.EventRecord.Id + " " + "Description: {0}", arg.EventRecord.FormatDescription());
            
        }
        else
        {
            Console.WriteLine("The event instance was null.");
        }

        }
    }
}
