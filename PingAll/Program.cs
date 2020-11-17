using System;
using System.IO.Ports;
using System.IO;
using System.Threading;
using System.Text.RegularExpressions;
using System.Collections.Generic;
using System.Linq;
using System.Runtime.InteropServices;
using System.Windows.Input;
using QuickGraph;

namespace PingAll
{
    public class Program
    {
        private const int MF_BYCOMMAND = 0x00000000;
        public const int SC_CLOSE = 0xF060;
        public const int SC_MINIMIZE = 0xF020;
        public const int SC_MAXIMIZE = 0xF030;
        public const int SC_SIZE = 0xF000;

        [DllImport("user32.dll")]
        public static extern int DeleteMenu(IntPtr hMenu, int nPosition, int wFlags);

        [DllImport("user32.dll")]
        private static extern IntPtr GetSystemMenu(IntPtr hWnd, bool bRevert);

        [DllImport("kernel32.dll", ExactSpelling = true)]
        private static extern IntPtr GetConsoleWindow();

        /*SET PORT NAME*/
        private static SerialPort testCom = new SerialPort("com1", 115200);

        public static SerialPort TestCom { get => testCom; set => testCom = value; }

        public static bool _continue = true;

        public static ConsoleColor primary = ConsoleColor.Green;
        public static ConsoleColor secondary = ConsoleColor.Yellow;
        public static ConsoleColor alert = ConsoleColor.Red;

        static void Main()
        {
            DeleteMenu(GetSystemMenu(GetConsoleWindow(), true), SC_CLOSE, MF_BYCOMMAND);
            DeleteMenu(GetSystemMenu(GetConsoleWindow(), true), SC_MINIMIZE, MF_BYCOMMAND);
            DeleteMenu(GetSystemMenu(GetConsoleWindow(), false), SC_MAXIMIZE, MF_BYCOMMAND);
            DeleteMenu(GetSystemMenu(GetConsoleWindow(), false), SC_SIZE, MF_BYCOMMAND);
            testCom.ReadBufferSize = 90000;
            /* DEFINES CONSOLE COLOR */
            ConsoleSetup();
            /* CREATE DHCP LIST */
            List<DHCP6> dhcp6s = new List<DHCP6>();
            /* CREATE LIST OF NODES*/
            List<NodeInfo> node = new List<NodeInfo>();

            NetworkInfo mesh = new NetworkInfo("0200");

            bool showMenu = true;
            testCom.PortName = SetPortName("COM1");
            while (showMenu)
            {
                showMenu = MainMenu(node, mesh, dhcp6s);
            }
        }

        public static void Header()
        {
            string top = " **********************************************************************************************************************";
            Console.ForegroundColor = primary;
            Console.WriteLine(top);
            Console.WriteLine(top);
            Console.ForegroundColor = secondary;
        }

        public static void KeyLine()
        {
            Console.ForegroundColor = primary;
            Console.Write(" **  -> ");
            Console.ForegroundColor = secondary;
        }

        private static bool MainMenu(List<NodeInfo> node, NetworkInfo mesh, List<DHCP6> dhcp6s)
        {
            Console.Clear();
            Console.SetWindowSize(120, 30);
            Header();
            MenuLine(" Serial Port selected: " + testCom.PortName.ToUpper());
            MenuLine(" Choose one of the following options:");
            MenuLine(" 1 -> Get Nodes IP and Levels (RPL)");
            MenuLine(" 2 -> Get Nodes MAC Adresses (RPL DHCPS)");
            MenuLine(" 3 -> Ping a single node");
            MenuLine(" 4 -> Ping all nodes");
            MenuLine(" 5 -> Ping all nodes in a level");
            MenuLine(" 6 -> Ping all nodes all levels in order - Crescent");
            MenuLine(" 7 -> Ping all nodes all levels in order - Decrescent");
            MenuLine(" 8 -> Get Nodes IP per level");
            MenuLine(" 9 -> Get Nodes MAC per level");
            MenuLine(" 0 -> Select Serial Port");
            Header();
            KeyLine();
            switch (Console.ReadKey().KeyChar)
            {
                //Get Nodes IP and Levels (RPL)
                case '1':
                    Console.WriteLine();
                    int[] r = SetRepetition();
                    for (int i = 0; i < r[0]; i++)
                    {
                        GetNodeInfo(node, mesh);
                        PathUnweighted.GetTopology(node);
                        if (r[0] - 1 > i)
                        {
                            WaitRep(r);
                        }
                    }
                    TaskFinished();
                    return true;
                //Get Nodes MAC Adresses (RPL DHCPS)
                case '2':
                    Console.WriteLine();
                    r = SetRepetition();
                    for (int i = 0; i < r[0]; i++)
                    {
                        GetNodeInfo(node, mesh);
                        PathUnweighted.GetTopology(node);
                        GetMacInfo(dhcp6s);
                        UpdateNodeMac(dhcp6s, node);
                        if (r[0] - 1 > i)
                        {
                            WaitRep(r);
                        }
                    }
                    TaskFinished();
                    return true;

                //Ping a single node
                case '3':
                    Console.WriteLine();
                    string _ip = SetIP();
                    int _count = SetCount();
                    int _size = SetSize();
                    int _timeout = SetTimeout();
                    r = SetRepetition();
                    for (int i = 0; i < r[0]; i++)
                    {
                        GetNodeInfo(node, mesh);
                        PathUnweighted.GetTopology(node);
                        PingNode(node, _ip, _count, _size, _timeout);
                        if (r[0] - 1 > i)
                        {
                            WaitRep(r);
                        }
                    }
                    TaskFinished();
                    return true;
                //Ping all nodes
                case '4':
                    Console.WriteLine();
                    _count = SetCount();
                    _size = SetSize();
                    _timeout = SetTimeout();
                    r = SetRepetition();
                    for (int i = 0; i < r[0]; i++)
                    {
                        GetNodeInfo(node, mesh);
                        PathUnweighted.GetTopology(node);
                        PingAllNodes(node, _count, _size, _timeout);
                        if (r[0] - 1 > i)
                        {
                            WaitRep(r);
                        }
                    }
                    TaskFinished();
                    return true;
                //Ping all nodes in a level
                case '5':
                    Console.WriteLine();
                    int _lvl;
                    _lvl = SetLevel();
                    _count = SetCount();
                    _size = SetSize();
                    _timeout = SetTimeout();
                    r = SetRepetition();
                    for (int i = 0; i < r[0]; i++)
                    {
                        GetNodeInfo(node, mesh);
                        PathUnweighted.GetTopology(node);
                        PingAllNodes(node, _count, _size, _timeout, _lvl);
                        if (r[0] - 1 > i)
                        {
                            WaitRep(r);
                        }
                    }
                    TaskFinished();
                    return true;
                //Ping all nodes all levels - Crescent
                case '6':
                    Console.WriteLine();
                    _count = SetCount();
                    _size = SetSize();
                    _timeout = SetTimeout();
                    r = SetRepetition();
                    for (int i = 0; i < r[0]; i++)
                    {
                        GetNodeInfo(node, mesh);
                        PathUnweighted.GetTopology(node);
                        PingAllLevels(node, _count, _size, _timeout, true);
                        if (r[0] - 1 > i)
                        {
                            WaitRep(r);
                        }
                    }
                    TaskFinished();
                    return true;
                //Ping all nodes all levels - Decrescent
                case '7':
                    Console.WriteLine();
                    _count = SetCount();
                    _size = SetSize();
                    _timeout = SetTimeout();
                    r = SetRepetition();
                    for (int i = 0; i < r[0]; i++)
                    {
                        GetNodeInfo(node, mesh);
                        PathUnweighted.GetTopology(node);
                        PingAllLevels(node, _count, _size, _timeout, false);
                        if (r[0] - 1 > i)
                        {
                            WaitRep(r);
                        }
                    }
                    TaskFinished();
                    return true;
                //Get Nodes IP per level
                case '8':
                    Console.WriteLine();
                    _lvl = SetLevel();
                    r = SetRepetition();
                    for (int i = 0; i < r[0]; i++)
                    {
                        GetNodeInfo(node, mesh);
                        PathUnweighted.GetTopology(node);
                        NodePerLevel(node, _lvl, false);
                        if (r[0] - 1 > i)
                        {
                            WaitRep(r);
                        }
                    }
                    TaskFinished();
                    return true;
                //Get Nodes MAC per level
                case '9':
                    Console.WriteLine();
                    _lvl = SetLevel();
                    r = SetRepetition();
                    for (int i = 0; i < r[0]; i++)
                    {
                        GetNodeInfo(node, mesh);
                        PathUnweighted.GetTopology(node);
                        GetMacInfo(dhcp6s);
                        UpdateNodeMac(dhcp6s, node);;
                        NodePerLevel(node, _lvl, true);
                        if (r[0] - 1 > i)
                        {
                            WaitRep(r);
                        }
                    }
                    TaskFinished();
                    return true;
                //Select Serial Port
                case '0':
                    Console.WriteLine();
                    testCom.PortName = SetPortName("COM1");
                    return true;
                default:
                    return true;
            }
        }

        public static int SetTimeout()
        {
            MenuLine(" Please type the ping timeout or press enter for default 3000 timeout:");
            int _timeout;
            KeyLine();
            string s = Console.ReadLine();
            if (s != "")
            {
                _timeout = Convert.ToInt32(s);
            }
            else
            {
                _timeout = 0;
            }
            return _timeout;
        }

        public static int[] SetRepetition()
        {
            string repetition = " Please type how many repetitions or press enter for default 1 repetition:";
            MenuLine(repetition);
            int r;
            int w = 0;
            KeyLine();
            string s = Console.ReadLine();
            if (s != "")
            {
                r = (int)Math.Round(Convert.ToDecimal(s));
            }
            else if( s=="0")
            {
                r = 1;
            }
            else
            {
                r = 1;
            }
            if (r > 1)
            {
                string wait = " How long should be the wait time between iterartions? Type the time in minutes";
                MenuLine(wait);
                string t;
                KeyLine();
                t = Console.ReadLine();
                if (t != "")
                {
                    w = (int)Math.Round(Convert.ToDecimal(t), MidpointRounding.ToZero);
                }
                else
                {
                    w = 0;
                }
            }
            int[] reps = new int[2];
            reps[0] = r;
            reps[1] = (int)w;
            MenuLine(" Executing task " + r + " time(s)");
            if (reps[0] > 1)
            {
                MenuLine(" With a pause of " + w + " minutes between iterations");
            }
            MenuLine(" Please wait execution");
            return reps;
        }

        public static void WaitRep(int[] r)
        {
            if(r[1] >= 1)
            {
                long timeLimit = Extensions.NanoTime() + (r[1] * 60000 * 1000000L);
                int min = r[1];
                bool keypress = false;
                MenuLine(" Starting a " + min + " minutes timer at " + DateTime.Now.ToString("HH:mm:ss dd/MM/yy"));
                MenuLine(" Press any key to skip waiting");
                Console.WriteLine(" ");
                var spinner = new Spinner(Console.CursorLeft, Console.CursorTop);
                spinner.Start();
                while ((timeLimit > Extensions.NanoTime()) && (false == keypress))
                {
                    keypress = Console.KeyAvailable;
                    if(keypress == true)
                    {
                        spinner.Stop();
                        MenuLine(" Key pressed, skipping wait time");
                    }
                }
                //Console.ReadKey(false);
                spinner.Stop();
                Console.SetCursorPosition(Console.CursorLeft, Console.CursorTop);
                Console.Write("");
                Console.WriteLine("");
                MenuLine(" Timer finished at " + DateTime.Now.ToString("HH:mm:ss dd/MM/yy"));
            }
            if(r[1] == 0)
            {
                MenuLine(" Executing task " + r[0] + " time(s)");
                MenuLine(" With no pause between iterations");
                MenuLine(" Please wait execution");
            }
        }

        public static int SetSize()
        {
            int _size;
            MenuLine(" Please type the ping size or press enter for default 32 bytes:");
            KeyLine();
            string s = Console.ReadLine();
            if (s != "")
            {
                _size = Convert.ToInt32(s);
            }
            else
            {
                _size = 0;
            }
            return _size;
        }

        public static int SetLevel()
        {
            MenuLine(" Please type the nodes level:");
            KeyLine();
            int _lvl = 0;
            string s = Console.ReadLine();
            if (s != "")
            {
                _lvl = Convert.ToInt32(s);
            }
            else
            {
                MenuLine(" Invalid value");
            }
            return _lvl;
        }

        public static string SetIP()
        {
            string _ip = string.Empty;
            MenuLine(" Please type node IP or MAC Address:");
            KeyLine();
            string s = Console.ReadLine();
            
            if (s != "")
            {
                _ip = s;
            }
            else
            {
                MenuLine(" Invalid value");
            }
            return _ip;
        }

        public static int SetCount()
        {
            int _count;

            MenuLine(" Please type the ping count or press enter for default 10 count:");
            KeyLine();
            string s = Console.ReadLine();
            if (s != "")
            {
                _count = Convert.ToInt32(s);
            }
            else
            {
                _count = 0;
            }
            return _count;
        }

        public static void TaskFinished()
        {
            Header();
            string finished = " Task finished, please press any key twice to return";
            MenuLine(finished);
            Console.ReadKey();
            Console.ReadKey();
            KeyLine();
        }

        public static string SetPortName(string defaultPortName)
        {
            string portName;
            Console.Clear();
            Console.SetWindowSize(120, 30);
            Header();
            MenuLine(" Available Ports:");
            foreach (string s in SerialPort.GetPortNames())
            {
                MenuLine("                  " + s);
            }

            MenuLine(" Enter COM port value or press enter for default: ");
            MenuLine(" Default Port: " + defaultPortName);
            Header();
            KeyLine();
            portName = Console.ReadLine();

            if (portName == "" || !(portName.ToLower()).StartsWith("com"))
            {
                portName = defaultPortName;
            }
            return portName;
        }

        public static void MenuLine(string _msg)
        {
            string bar = " ** ";
            string pad = new String(' ', (112 - (_msg).Length));
            Console.ForegroundColor = primary;
            Console.Write(bar);
            Console.ForegroundColor = secondary;
            Console.Write(_msg);
            Console.Write(pad);
            Console.ForegroundColor = primary;
            Console.Write(bar);
            Console.ForegroundColor = secondary;
            Console.Write("\n");
        }

        public static void MenuLineCenter(string _msg)
        {
            string bar = " ** ";
            string pad = new String(' ', (112 - (_msg).Length) / 2);
            //Console.Write("\n");
            Console.ForegroundColor = primary;
            Console.Write(bar);
            Console.Write(pad);
            Console.ForegroundColor = secondary;
            Console.Write(_msg);
            Console.ForegroundColor = primary;
            Console.Write(pad);
            Console.Write(bar);
            Console.ForegroundColor = secondary;
            Console.Write("\n");
        }

        public static void ConsoleSetup()
        {
            Console.Title = "Wisun AP Monitoring - v2.0";
            Console.ForegroundColor = secondary;
        }

        public static void Stamp()
        {
            string unixTimestamp = Convert.ToString(DateTime.Now);
            PrLog(unixTimestamp.ToString()+ " ", false);
        }

        public static void StampROL()
        {
            string unixTimestamp = Convert.ToString(DateTime.Now);
            LogToFileROL((unixTimestamp.ToString()));
        }

        public static void GetMacInfo(List<DHCP6> dhcp6s)
        {
            int listsize = dhcp6s.Count();
            List<string> dhcplistold = new List<string>();
            List<string> dhcplistnew = new List<string>();
            List<string> nodeupdate = new List<string>();
            string value2 = ",";
            Regex regex = new Regex(@"\[\b(VC\+RPD: )([A-Fa-f0-9](.*?)[,]){2}([0-9A-Fa-f]{2}[:-]){7}([0-9A-Fa-f]{2}){1},[0-9A-F]{4},[0-9A-F](.*),[0-9A-F](.*)\]");
            Regex regexMac = new Regex(@"([0-9A-Fa-f]{2}[:-]){7}([0-9A-Fa-f]{2}){1},[0-9A-F]{4},");
            string endingRule = "[VC+RPD END]";
            MatchCollection matches = regex.Matches(SerialComm("rpl dhcps", endingRule, 40000));
            if (dhcp6s.Count == 0)
            {
                dhcp6s.Add(new DHCP6("", "0200", "0:0"));
            }

            if (matches.Count == 0)
            {
                Stamp();
                Console.ForegroundColor = ConsoleColor.Red;
                PrLog("RPL DHCPS Result",false);
                PrLog("No devices found, execute again!",true);
                Console.ForegroundColor = ConsoleColor.Yellow;
            }

            foreach (Match match in matches)
            {
                MatchCollection matchMac = regexMac.Matches(match.ToString());

                foreach (Match matchM in matchMac)
                {
                    foreach (Capture capture in matchM.Captures)
                    {
                        int IndexInit = capture.Value.IndexOfNth(value2, 0) + 1;
                        int IndexEnd = capture.Value.IndexOfNth(value2, 1);
                        string[] separator = new string[] { "FF:FE:" };
                        string _mac = capture.Value.Substring(0, capture.Value.IndexOfNth(value2, 0)).ToUpper();
                        string _ip = capture.Value[IndexInit..IndexEnd].ToUpper();
                        string[] _shortmacvec = _mac.Split(separator, StringSplitOptions.RemoveEmptyEntries);
                        string _shortmac = _shortmacvec[0] + _shortmacvec[1];
                        if (dhcp6s.Exists(x => x.MacAdress == _mac) || dhcp6s.Exists(x => x.IP == _ip))
                        {
                            if (dhcp6s.Exists(x => x.MacAdress == _mac) && dhcp6s.Exists(x => x.IP == _ip))
                            {
                                dhcplistold.Add("Meter IP " + _ip + " and Mac " + _mac + " already in DHCP list.");
                            }
                            else if (dhcp6s.Exists(x => x.MacAdress == _mac))
                            {
                                if (dhcp6s.Find(x => x.MacAdress == _mac).IP != _ip)
                                {
                                    dhcp6s.Find(x => x.MacAdress == _mac).IP = _ip;
                                    nodeupdate.Add(("Meter IP " + _ip + " mac address is " + _mac));
                                }
                            }
                        }
                        else if ((dhcp6s.Exists(x => x.MacAdress == _mac)) == false)
                        {
                            dhcp6s.Add(new DHCP6(_mac, _ip, _shortmac));
                            dhcplistnew.Add("Meter IP " + _ip + " mac address is " + _mac);
                        }
                    }
                }
            }
            dhcp6s = dhcp6s.Distinct().ToList();
            if (dhcplistnew.Count() > 0)
            {
                Stamp();
                PrLog("MAC/IP UPDATE Result",false);
                PrLog("New nodes in DHCP list:",false);
                PrLog(dhcplistnew.Count().ToString(),false);
                //foreach (string line in dhcplistnew)
                //{
                    //PrLog(line,true);
                //}
            }
            if (nodeupdate.Count > 0)
            {
                Stamp();
                PrLog("MAC/IP UPDATE Result", false);
                PrLog("The following meter have changed the IP", false);
                foreach (string line in nodeupdate)
                {
                    PrLog(line,false);
                }
            }

        }

        public static void UpdateNodeMac(List<DHCP6> dhcp6s, List<NodeInfo> node)
        {
            int s = 0;
            foreach (NodeInfo nodeInfo in node)
            {
                if (nodeInfo.MacAddress == null && dhcp6s.Exists(x => x.IP == nodeInfo.IP))
                {
                    nodeInfo.MacAddress = dhcp6s.Find(x => x.IP == nodeInfo.IP).MacAdress;
                    nodeInfo.ShortMacAddress = dhcp6s.Find(x => x.IP == nodeInfo.IP).ShortMac;
                }
                if (dhcp6s.Exists(x => x.IP == nodeInfo.IP))
                {
                    if (dhcp6s.Find(x => x.MacAdress == nodeInfo.MacAddress).IP != nodeInfo.IP)
                    {
                        dhcp6s.Find(x => x.MacAdress == nodeInfo.MacAddress).IP = nodeInfo.IP;
                        s++;
                    }
                }
            }
            int a = dhcp6s.Count();
            double t = (double)s / (double)a;
            if (s != 0)
            {
                Stamp();
                PrLog("RPL DHCPS Result",  false);
                PrLog((t.ToString("{0:P}") + "% of all nodes has changed IP"), false);
                PrLog((s.ToString() + " nodes have changed IP"), false);
            }

            dhcp6s = dhcp6s.Distinct().ToList();
        }

        public static void GetNodeInfo(List<NodeInfo> node, NetworkInfo mesh)
        {
            int s = 0;
            Regex regex = new Regex(@"\[[(?:[A-Fa-f0-9]{4} -> (?:[A-Fa-f0-9]{4}) (lv:   )(?:[0-9]*)   (lt: )(?:[0-9].*)\]");
            Regex regexNodeIP = new Regex(@"\[(?:[A-Fa-f0-9]){4} -");
            Regex regexNodeParent = new Regex(@" (?:[A-Fa-f0-9]){4} l");
            Regex regexNodeLvl = new Regex(@"   [(?:[A-Fa-f0-9].  ");
            Regex regexLifetime = new Regex(@"(lt: )[(?:[A-Fa-f0-9].* s\]");
            string endingRule = "---------------------";
            MatchCollection matches = regex.Matches(SerialComm("rpl", endingRule, 40000));

            if (node.Count == 0)
            {
                node.Add(new NodeInfo("0200", "0", "0"));
            }

            if (matches.Count == 0)
            {
                Stamp();
                Console.ForegroundColor = alert;
                PrLog("RPL Result", false);
                PrLog("No devices found, execute again!", true);
                Console.ForegroundColor = secondary;
            }

            foreach (Match match in matches)
            {
                MatchCollection matchIP = regexNodeIP.Matches(match.ToString());
                MatchCollection matchParent = regexNodeParent.Matches(match.ToString());
                MatchCollection matchLt = regexLifetime.Matches(match.ToString());
                MatchCollection matchLvl = regexNodeLvl.Matches(match.ToString());
                char[] toTrim = new char[9];
                toTrim[0] = 'l';
                toTrim[1] = 't';
                toTrim[3] = ':';
                toTrim[4] = 's';
                toTrim[5] = ']';
                toTrim[6] = '-';
                toTrim[7] = '[';
                toTrim[8] = ' ';


                foreach (Capture capture in match.Captures)
                {
                    string _ip = matchIP.First().ToString().Trim(toTrim).ToUpper();
                    string _parent = matchParent.First().ToString().Trim(toTrim).ToUpper();
                    string _lifetime = matchLt.First().ToString().Trim(toTrim).ToUpper();                 
                    int _lvl = Convert.ToInt32(matchLvl.First().ToString().Trim(toTrim).ToUpper());

                    if (node.Exists(x => x.IP == _ip))
                    {
                        if (node.Find(x => x.IP == _ip).IP == _ip)
                        {
                            if (node.Find(x => x.IP == _ip).Parent != _parent)
                            {
                                node.Find(x => x.IP == _ip).ParentChange(capture);
                                s++;
                            }
                            node.Find(x => x.IP == _ip).Lifetime = _lifetime;
                            node.Find(x => x.IP == _ip).Level = _lvl;
                        }
                    }
                    else
                    {
                        node.Add(new NodeInfo(_ip, _parent, _lifetime, _lvl));
                    }
                }

            }
            int a = node.Count();
            mesh.TotalNodes = a;
            double t = (double)s / (double)a;
            if (s != 0)
            {
                Stamp();
                PrLog("RPL Result", false);
                PrLog((t.ToString("P") + " of all nodes has changed parent"), false);
                PrLog((s.ToString() + " nodes have changed parent"), false);
            }
            else
            {
                Stamp();
                PrLog("RPL Result", false);
                PrLog(" " + (a.ToString() + " nodes were found"), false);
            }
        }

        public static string SerialComm(string msg, string ending, int timeoutMillis)
        {
            testCom.Dispose();
            string buffer = string.Empty;
            int tries;
            try
            {
                if (TestCom.IsOpen == false)
                {
                    TestCom.Open();
                    long timeLimit = Extensions.NanoTime() + (timeoutMillis * 1000000L);
                    for(tries = 0; tries <= 4; tries++)
                    {
                        TestCom.Write(msg + "\r");
                        bool _continue = false;
                        StampROL();
                        while ((timeLimit > Extensions.NanoTime()) && (true != _continue))
                        {
                            string stream = TestCom.ReadExisting();
                            LogToFileROL(stream);
                            buffer += stream;
                            _continue = buffer.Contains(ending);
                            Thread.Sleep(100);
                            if(_continue == true)
                            {
                                tries = 5;
                            }
                        }
                        if (_continue == false)
                        {
                            StampROL();
                            Stamp();
                            Console.ForegroundColor = alert;
                            PrLog("\r", false);
                            PrLog("Timeout serial for Command: ", true);
                            PrLog(msg, true);
                            PrLog("\r", false);
                            TestCom.Write("\r");
                            TestCom.Write("\r");
                            Console.ForegroundColor = secondary;
                            tries++;
                            PrLog("Trying again ");
                            Thread.Sleep(600);
                            timeLimit = Extensions.NanoTime() + (timeoutMillis * 1000000L);
                        }
                    }
                    string unixTimestamp = Convert.ToString(DateTime.Now);
                    //Console.WriteLine("* ------- " + unixTimestamp.ToString() + " ------- *");
                    //Console.WriteLine(buffer);
                    TestCom.Close();
                }
                else
                {
                    TestCom.Close();
                }
            }
            catch (Exception e)
            {
                Console.WriteLine(e);
                _continue = false;
            }

            return buffer;
        }

        public static void PingNode(List<NodeInfo> node, string ip, int count, int payload, int timeout)
        {
            try
            {
                Regex regex = new Regex(@"\+\b(PING6:)(.*?),(.*?)\%,(.*?),(.*?),(.*?),(.*?)$\B");

                string endingRule = "+PING6:";
                if(node.Exists(x=>x.IP == ip))
                {
                    node.Find(x => x.IP == ip).ClearPing();
                    if (count == 0)
                    {
                        count = 10;
                    }
                    if (payload == 0)
                    {
                        payload = 32;
                    }
                    if (timeout == 0)
                    {
                        timeout = 3000;
                    }
                    if (ip != "0200")
                    {
                        string com = SerialComm("ping6 " + ip + " c " + count + " s " + payload + " t " + timeout, endingRule, count * (timeout) + 10000);
                        MatchCollection matches = regex.Matches(com);
                        foreach (Match match in matches)
                        {
                            foreach (Capture capture in match.Captures)
                            {
                                node.Find(x => x.IP == ip).SavePing(capture);
                            }
                        }
                    }
                }
                else
                {
                    Stamp();
                    Console.ForegroundColor = alert;
                    PrLog("This IP does not exist in actual RPL list ", false);
                    Console.ForegroundColor = secondary;
                }
            }
            catch (Exception e)
            {
                Console.WriteLine(e);
            }

        }

        public static void LogToFile(string msg)
        {
            try
            {
                string path = @".\log" + DateTime.Now.ToString("yyyyMMdd");
                using StreamWriter sw = File.AppendText(path);
                sw.WriteLine(msg);
            }
            catch (Exception e)
            {
                Console.WriteLine(e);
            }
        }

        public static void LogToFileROL(string msg)
        {
            try
            {
                string path = @".\logROL" + DateTime.Now.ToString("yyyyMMdd");
                using StreamWriter sw = File.AppendText(path);
                sw.WriteLine(msg);
            }
            catch (Exception e)
            {
                Console.WriteLine(e);
            }

        }

        public static void PrLog(string msg, bool selector)
        {

            try
            {
                if (selector == true)
                {
                    MenuLine(msg);
                    LogToFile(msg);
                }
                else
                {
                    MenuLineCenter(msg);
                    LogToFile(msg);
                }

            }
            catch (Exception e)
            {
                Console.WriteLine(e);
            }
        }

        public static void PrLog(string msg)
        {

            try
            {
                    Console.WriteLine(msg);
                    LogToFile(msg);
            }
            catch (Exception e)
            {
                Console.WriteLine(e);
            }
        }

        public static void WrLog(string msg)
        {
            try
            {
                MenuLine(msg);
                LogToFile(msg);
            }
            catch (Exception e)
            {
                Console.WriteLine(e);
            }
        }

        public static void PingAllNodes(List<NodeInfo> node, int count, int payload, int timeout)
        {
            int Ptx = 0;
            int Prx = 0;
            int Ploss = 0;
            int Unstable = 0;
            List<NodeInfo> WasPing = new List<NodeInfo>();
            try
            {
                foreach (NodeInfo nodes in node)
                {
                    PingNode(node, nodes.IP, count, payload, timeout);
                    WasPing = node.FindAll(x => x.PTx != null);
                    PrLog(WasPing.Count().ToString("D4") + " Nodes pinged out of " + node.Count().ToString("D4") + " nodes", true);
                    if(WasPing.Count() % 100 == 0)
                    {
                        foreach(NodeInfo ping in WasPing)
                        {
                            Ptx += Convert.ToInt32(ping.PTx);
                            Prx += Convert.ToInt32(ping.PRx);
                            if(ping.PLoss == "100.0")
                            {
                                Ploss++;
                            }
                            else if (ping.PLoss != "0.0" && ping.PLoss != null)
                            {
                                Unstable++;
                            }

                        }
                        if(Ptx > 0 && Prx >0)
                        {
                            PingResults(Ptx, Prx, Ploss, Unstable);
                        }

                    }
                }
                if (WasPing.Count() == node.Count())
                {
                    Ptx = 0;
                    Prx = 0;
                    foreach (NodeInfo ping in WasPing)
                    {
                        Ptx += Convert.ToInt32(ping.PTx);
                        Prx += Convert.ToInt32(ping.PRx);
                        if (ping.PLoss == "100.0")
                        {
                            Ploss++;
                        }
                        else if (ping.PLoss != "0.0" && ping.PLoss != null)
                        {
                            Unstable++;
                        }

                    }
                    PrLog("All selected nodes were pinged ", true);
                    PingResults(Ptx, Prx, Ploss, Unstable);
                }
                else
                {
                    Ptx = 0;
                    Prx = 0;
                    foreach (NodeInfo ping in WasPing)
                    {
                        Ptx += Convert.ToInt32(ping.PTx);
                        Prx += Convert.ToInt32(ping.PRx);
                        if (ping.PLoss == "100.0")
                        {
                            Ploss++;
                        }
                        else if (ping.PLoss != "0.0" && ping.PLoss != null)
                        {
                            Unstable++;
                        }

                    }
                    PrLog(WasPing.Count().ToString("D4") + " nodes were pinged ", true);
                    PingResults(Ptx, Prx, Ploss, Unstable);
                }
                foreach (NodeInfo clear in WasPing)
                {
                    node.Find(x => x.IP == clear.IP).PTx = null;
                    node.Find(x => x.IP == clear.IP).PRx = null;
                    node.Find(x => x.IP == clear.IP).PLoss = null;
                }
            }
            catch (Exception e)
            {
                Console.WriteLine(e);
            }

        }

        public static void PingAllNodes(List<NodeInfo> node, int count, int payload, int timeout, int _lvl)
        {
            int Ptx = 0;
            int Prx = 0;
            int Ploss = 0;
            int Unstable = 0;
            List <NodeInfo> WasPing = new List<NodeInfo>();
            try
            {
                List<NodeInfo> infos = new List<NodeInfo>();
                infos = node.FindAll(x => x.Level == _lvl);
                PrLog("Nodes in Level " + _lvl.ToString("D2"), false);
                PrLog("Total nodes: " + infos.Count().ToString("D4"), true);
                foreach (NodeInfo nodes in infos)
                {
                    PingNode(node, nodes.IP, count, payload, timeout);
                    WasPing = infos.FindAll(x => x.PTx != null);
                    PrLog(WasPing.Count().ToString("D4") + " Nodes pinged out of " + infos.Count().ToString("D4") + " nodes", true);
                    if (WasPing.Count() % 100 == 0)
                    {
                        foreach (NodeInfo ping in WasPing)
                        {
                            Ptx += Convert.ToInt32(ping.PTx);
                            Prx += Convert.ToInt32(ping.PRx);
                            if (ping.PLoss == "100.0")
                            {
                                Ploss++;
                            }
                            else if (ping.PLoss != "0.0" && ping.PLoss != null)
                            {
                                Unstable++;
                            }

                        }
                        if (Ptx > 0 && Prx > 0)
                        {
                            PrLog("Total sent packets: " + Ptx.ToString(), true);
                            PingResults(Ptx, Prx, Ploss, Unstable); ;
                        }

                    }
                }
                if (WasPing.Count() == node.Count())
                {
                    Ptx = 0;
                    Prx = 0;
                    foreach (NodeInfo ping in WasPing)
                    {
                        Ptx += Convert.ToInt32(ping.PTx);
                        Prx += Convert.ToInt32(ping.PRx);
                        if (ping.PLoss == "100.0")
                        {
                            Ploss++;
                        }
                        else if (ping.PLoss != "0.0" && ping.PLoss != null)
                        {
                            Unstable++;
                        }

                    }
                    PrLog("All selected nodes were pinged ", true);
                    PingResults(Ptx, Prx, Ploss, Unstable);
                }
                else
                {
                    Ptx = 0;
                    Prx = 0;
                    foreach (NodeInfo ping in WasPing)
                    {
                        Ptx += Convert.ToInt32(ping.PTx);
                        Prx += Convert.ToInt32(ping.PRx);
                        if (ping.PLoss == "100.0")
                        {
                            Ploss++;
                        }
                        else if (ping.PLoss != "0.0" && ping.PLoss != null)
                        {
                            Unstable++;
                        }

                    }
                    PrLog(WasPing.Count().ToString("D4") + " nodes were pinged ", true);
                    PingResults(Ptx, Prx, Ploss, Unstable);
                }
                foreach (NodeInfo clear in WasPing)
                {
                    node.Find(x => x.IP == clear.IP).PTx = null;
                    node.Find(x => x.IP == clear.IP).PRx = null;
                    node.Find(x => x.IP == clear.IP).PLoss = null;
                }

            }
            catch (Exception e)
            {
                Console.WriteLine(e);
            }

        }

        public static void PingAllLevels(List<NodeInfo> node, int count, int payload, int timeout, bool order)
        {
            try
            {
                if(order == true)
                {
                    node.Distinct().OrderBy(x => x.Level);
                    List<int> lvls = new List<int>();
                    foreach(NodeInfo nodeInfo in node)
                    {
                        int s = 0;
                        s = nodeInfo.Level;
                        lvls.Add(s);
                    }
                    lvls = lvls.OrderBy(x => x).Distinct().ToList();
                    foreach(int level in lvls)
                    {
                        PingAllNodes(node, count, payload, timeout, level);
                    }
                }
                else
                {
                    node.Distinct().OrderByDescending(x => x.Level);
                    List<int> lvls = new List<int>();
                    foreach (NodeInfo nodeInfo in node)
                    {
                        int s = 0;
                        s = nodeInfo.Level;
                        lvls.Add(s);
                    }
                    lvls = lvls.OrderByDescending(x => x).Distinct().ToList();
                    foreach (int level in lvls)
                    {
                        PingAllNodes(node, count, payload, timeout, level);
                    }
                }
            }
            catch (Exception e)
            {
                Console.WriteLine(e);
            }

        }

        public static void NodePerLevel(List<NodeInfo> node, int _lvl, bool selector)
        {
            PathUnweighted.GetTopology(node);
            List<NodeInfo> infos = new List<NodeInfo>();
            infos = node.FindAll(x => x.Level == _lvl);
            Stamp();
            string msg = string.Empty;
            if (selector == true)
            {
                PrLog("Nodes MAC in level " + _lvl.ToString("D2") + "  ", false);
                int i = 1;
                msg = string.Empty;
                foreach (NodeInfo info in infos)
                {
                    msg += (info.ShortMacAddress.Replace(":", "") + "\t");
                    if (i % 7 == 0)
                    {
                        //msg += ("\r");
                        Console.Write("\r");
                        PrLog(msg);
                        msg = string.Empty;
                    }
                    i++;
                }

            }
            else
            {
                PrLog("Nodes IP in level " + _lvl.ToString("D2") + " ", false);
                int i = 1;
                foreach (NodeInfo info in infos)
                {
                    msg += (info.IP + "\t");
                    if (i % 15 == 0)
                    {
                        //msg += ("\r");
                        Console.Write("\r");
                        PrLog(msg);
                        msg = string.Empty;
                    }
                    i++;
                }
            }
            PrLog(msg);
            PrLog("Total Nodes in level " + _lvl.ToString("D2") + ":  " + infos.Count() + " ", false);
        }

        public static void PingResults(int Ptx, int Prx, int Ploss, int Unstable)
        {
            PrLog("Total sent packets: " + Ptx.ToString(), true);
            PrLog("Total received packets: " + Prx.ToString(), true);
            double PRate = 1 - ((double)Prx / (double)Ptx);
            PrLog("Total package loss: " + (PRate).ToString("P"), true);
            PrLog("Total nodes not reachable: " + Ploss.ToString(), true);
            PrLog("Total nodes with unstability: " + Unstable.ToString(), true);
        }
    }
}
