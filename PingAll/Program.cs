﻿using System;
using System.IO.Ports;
using System.IO;
using System.Threading;
using System.Text.RegularExpressions;
using System.Collections.Generic;
using System.Linq;
using System.Runtime.InteropServices;

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
            try
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

                bool showMenu = true;
                testCom.PortName = SetPortName("COM1");
                while (showMenu)
                {
                    showMenu = MainMenu(node, dhcp6s);
                }
            }
            catch (Exception e)
            {
                Console.WriteLine(e);
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
            try
            {
            Console.ForegroundColor = primary;
            Console.Write(" **  -> ");
            Console.ForegroundColor = secondary;
            }
            catch (Exception e)
            {
                Console.WriteLine(e);
            }
        }

        private static bool MainMenu(List<NodeInfo> node, List<DHCP6> dhcp6s)
        {
            try 
            {
                Console.Clear();
                Console.SetWindowSize(120, 30);
                Header();
                MenuLine("Serial Port selected: " + testCom.PortName.ToUpper());
                MenuLine("Choose one of the following options:");
                MenuLine(" 0 -> Select Serial Port");
                MenuLine(" 1 -> Get Nodes IP and Levels (RPL)");
                MenuLine(" 2 -> Get Nodes MAC Adresses (RPL DHCPS)");
                MenuLine(" 3 -> Ping a single node");
                MenuLine(" 4 -> Ping all nodes");
                MenuLine(" 5 -> Ping all nodes in a level");
                MenuLine(" 6 -> Ping all nodes all levels in order - Crescent");
                MenuLine(" 7 -> Ping all nodes all levels in order - Decrescent");
                MenuLine(" 8 -> Get Nodes IP per level");
                MenuLine(" 9 -> Get Nodes MAC per level");
                MenuLine(" 10 -> Get Next Push Time Table");
                Header();
                KeyLine();
                switch (Console.ReadLine())
                {
                    //Get Nodes IP and Levels (RPL)
                    case "1":
                        int[] r = SetRepetition();
                        for (int i = 0; i < r[0]; i++)
                        {
                            GetNodeInfo(node);
                            PathUnweighted.GetTopology(node);
                            if (r[0] - 1 > i)
                            {
                                WaitRep(r, i);
                            }
                        }
                        TaskFinished();
                        return true;
                    //Get Nodes MAC Adresses (RPL DHCPS)
                    case "2":
                        r = SetRepetition();
                        for (int i = 0; i < r[0]; i++)
                        {
                            GetNodeInfo(node);
                            PathUnweighted.GetTopology(node);
                            GetMacInfo(dhcp6s);
                            UpdateNodeMac(dhcp6s, node);
                            if (r[0] - 1 > i)
                            {
                                WaitRep(r, i);
                            }
                        }
                        TaskFinished();
                        return true;

                    //Ping a single node
                    case "3":
                        string _ip = SetIP();
                        int _count = SetCount();
                        int _size = SetSize();
                        int _timeout = SetTimeout();
                        int _lenght = SetLength();
                        r = SetRepetition();
                        for (int i = 0; i < r[0]; i++)
                        {
                            GetNodeInfo(node);
                            PathUnweighted.GetTopology(node);
                            PingNode(node, _ip, _count, _size, _timeout, _lenght);
                            if (r[0] - 1 > i)
                            {
                                WaitRep(r, i);
                            }
                        }
                        TaskFinished();
                        return true;
                    //Ping all nodes
                    case "4":
                        _count = SetCount();
                        _size = SetSize();
                        _timeout = SetTimeout();
                        _lenght = SetLength();
                        r = SetRepetition();
                        for (int i = 0; i < r[0]; i++)
                        {
                            GetNodeInfo(node);
                            PathUnweighted.GetTopology(node);
                            PingAllNodes(node, _count, _size, _timeout, _lenght);
                            if (r[0] - 1 > i)
                            {
                                WaitRep(r, i);
                            }
                        }
                        TaskFinished();
                        return true;
                    //Ping all nodes in a level
                    case "5":
                            int _lvl;
                            _lvl = SetLevel();
                            _count = SetCount();
                            _size = SetSize();
                            _timeout = SetTimeout();
                            _lenght = SetLength();
                            r = SetRepetition();
                            for (int i = 0; i < r[0]; i++)
                            {
                                GetNodeInfo(node);
                                PathUnweighted.GetTopology(node);
                                PingAllNodes(node, _count, _size, _timeout, _lenght, _lvl);
                                if (r[0] - 1 > i)
                                {
                                    WaitRep(r, i);
                                }
                            }
                        TaskFinished();
                        return true;
                    //Ping all nodes all levels - Crescent
                    case "6":
                        _count = SetCount();
                        _size = SetSize();
                        _timeout = SetTimeout();
                            _lenght = SetLength();
                            r = SetRepetition();
                        for (int i = 0; i < r[0]; i++)
                        {
                            GetNodeInfo(node);
                            PathUnweighted.GetTopology(node);
                            PingAllLevels(node, _count, _size, _timeout, _lenght, true);
                            if (r[0] - 1 > i)
                            {
                                WaitRep(r, i);
                            }
                        }
                        TaskFinished();
                        return true;
                    //Ping all nodes all levels - Decrescent
                    case "7":
                        _count = SetCount();
                        _size = SetSize();
                        _timeout = SetTimeout();
                            _lenght = SetLength();
                            r = SetRepetition();
                        for (int i = 0; i < r[0]; i++)
                        {
                            GetNodeInfo(node);
                            PathUnweighted.GetTopology(node);
                            PingAllLevels(node, _count, _size, _timeout, _lenght, false);
                            if (r[0] - 1 > i)
                            {
                                WaitRep(r, i);
                            }
                        }
                        TaskFinished();
                        return true;
                    //Get Nodes IP per level
                    case "8":
                        _lvl = SetLevel();
                        r = SetRepetition();
                        for (int i = 0; i < r[0]; i++)
                        {
                            GetNodeInfo(node);
                            PathUnweighted.GetTopology(node);
                            NodePerLevel(node, _lvl, false);
                            if (r[0] - 1 > i)
                            {
                                WaitRep(r, i);
                            }
                        }
                        TaskFinished();
                        return true;
                    //Get Nodes MAC per level
                    case "9":
                        _lvl = SetLevel();
                        r = SetRepetition();
                        for (int i = 0; i < r[0]; i++)
                        {
                            GetNodeInfo(node);
                            PathUnweighted.GetTopology(node);
                            GetMacInfo(dhcp6s);
                            UpdateNodeMac(dhcp6s, node);;
                            NodePerLevel(node, _lvl, true);
                            if (r[0] - 1 > i)
                            {
                                WaitRep(r, i);
                            }
                        }
                        TaskFinished();
                        return true;
                        //Get Nodes MAC per level
                    case "10":
                        GetNtpPush();
                        TaskFinished();
                        return true;
                        //Select Serial Port
                        case "0":
                        testCom.PortName = SetPortName("COM1");
                        return true;
                        default:
                        return true;
                }
            }
            catch (Exception e)
            {
                Console.WriteLine(e);
                return true;
            }
        }

        public static int SetTimeout()
        {
            try
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
            catch (Exception e)
            {
                Console.WriteLine(e);
                return 0;
            }
        }

        public static int[] SetRepetition()
        {
            try 
            { 
            string repetition = " Please type how many repetitions or press enter for default 1 repetition or press 0 to return to main menu:";
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
                string wait = " How long should be the wait time between iterations? Type the time in minutes";
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
            MenuLine("Executing task " + r + " time(s)");
            if (reps[0] > 1)
            {
                MenuLine("With a pause of " + w + " minutes between iterations");
            }
            MenuLine("Please wait execution");
            return reps;
            }
            catch (Exception e)
            {
                Console.WriteLine(e);
                int[] reps = new int[2];
                return reps;
            }
        }

        public static void WaitRep(int[] r, int i)
        {
            try
            { 
            if(r[1] >= 1)
            {
                long timeLimit = Extensions.NanoTime() + (r[1] * 60000 * 1000000L);
                int min = r[1];
                bool keypress = false;
                MenuLine("Starting a " + min + " minutes timer at " + DateTime.Now.ToString("HH:mm:ss dd/MM/yy"));
                MenuLine("Press any key to skip waiting");
                Console.WriteLine(" ");
                var spinner = new Spinner(Console.CursorLeft, Console.CursorTop);
                spinner.Start();
                while ((timeLimit > Extensions.NanoTime()) && (false == keypress))
                {
                    keypress = Console.KeyAvailable;
                    if(keypress == true)
                    {
                        spinner.Stop();
                        MenuLine("Key pressed, skipping wait time");
                    }
                }
                Console.ReadKey(false);
                spinner.Stop();
                Console.SetCursorPosition(Console.CursorLeft, Console.CursorTop);
                Console.Write("");
                Console.WriteLine("");
                MenuLine("Timer finished at " + DateTime.Now.ToString("HH:mm:ss dd/MM/yy"));
            }
            if(r[1] == 0)
            {
                MenuLine("Executing task " + (i+2) + " of " + r[0] + " time(s)");
                MenuLine("With no pause between iterations");
                MenuLine("Please wait execution");
            }
            }
            catch (Exception e)
            {
                Console.WriteLine(e);
            }
        }

        public static int SetSize()
        {
            try 
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
            catch (Exception e)
            {
                Console.WriteLine(e);
                int rets = new int();
                return rets;
            }
        }

        public static int SetLevel()
        {
            try
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
            catch (Exception e)
            {
                Console.WriteLine(e);
                int rets = new int();
                return rets;
            }
        }

        public static string SetIP()
        {
            try
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
            catch (Exception e)
            {
                Console.WriteLine(e);
                string rets = String.Empty;
                return rets;
            }
        }

        public static int SetCount()
        {
            try 
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
            catch (Exception e)
            {
                Console.WriteLine(e);
                int rets = new int();
                return rets;
            }
        }

        public static int SetLength()
        {
            try
            {
                int _length;

                MenuLine(" Please type the ping length (must be greater than timeout) or press enter for default:");
                KeyLine();
                string s = Console.ReadLine();
                if (s != "")
                {
                    _length = Convert.ToInt32(s);
                }
                else
                {
                    _length = 0;
                }
                return _length;
            }
            catch (Exception e)
            {
                Console.WriteLine(e);
                int rets = new int();
                return rets;
            }
        }

        public static void TaskFinished()
        {
            try 
            { 
            Header();
            string finished = " Task finished, please press any key twice to return";
            MenuLine(finished);
            Console.ReadKey();
            Console.ReadKey();
            KeyLine();
            }
            catch (Exception e)
            {
                Console.WriteLine(e);
            }
        }

        public static string SetPortName(string defaultPortName)
        {
            try
            {
            string portName;
            Console.Clear();
            Console.SetWindowSize(120, 30);
            Header();
            MenuLine("Available Ports:");
            foreach (string s in SerialPort.GetPortNames())
            {
                MenuLine("                  " + s);
            }

            MenuLine("Enter COM port value or press enter for default: ");
            MenuLine("Default Port: " + defaultPortName);
            Header();
            KeyLine();
            portName = Console.ReadLine();

            if (portName == "" || !(portName.ToLower()).StartsWith("com"))
            {
                portName = defaultPortName;
            }
            return portName;
            }
            catch (Exception e)
            {
                Console.WriteLine(e);
                string rets = string.Empty;
                return rets;
            }
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
            Console.Title = "Wisun AP Monitoring - v2.3";
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
            try {
                int listsize = dhcp6s.Count();
                List<string> dhcplistold = new List<string>();
                List<string> dhcplistnew = new List<string>();
                List<string> nodeupdate = new List<string>();
                string value2 = ",";
                Regex regex = new Regex(@"\[\b(VC\+RPD: )([A-Fa-f0-9](.*?)[,]){2}([0-9A-Fa-f]{2}[:-]){7}([0-9A-Fa-f]{2}){1},[0-9A-F]{4},[0-9A-F](.*),[0-9A-F](.*)\]");
                Regex regexMac = new Regex(@"([0-9A-Fa-f]{2}[:-]){7}([0-9A-Fa-f]{2}){1},[0-9A-F]{4},");
                Regex regexEnding = new Regex(@"\[\b(VC\+RPD END)\]");
                string endingRule = "[VC+RPD END]";
                string rplDhcps = "rpl dhcps";
                MatchCollection matches = regex.Matches(SerialComm(rplDhcps, endingRule, 40000, rplDhcps.Count(), regexEnding));
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
            catch (Exception e)
            {
                Console.WriteLine(e);
            }
        }

        public static void UpdateNodeMac(List<DHCP6> dhcp6s, List<NodeInfo> node)
        {
            try
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
                    PrLog("RPL DHCPS Result", false);
                    PrLog((t.ToString("{0:P}") + "% of all nodes has changed IP"), false);
                    PrLog((s.ToString() + " nodes have changed IP"), false);
                }

                dhcp6s = dhcp6s.Distinct().ToList();

            }
            catch (Exception e)
            {
                Console.WriteLine(e);

            }
        }

        public static void GetNodeInfo(List<NodeInfo> node)
        {
            try
            {
                int s = 0;
                Regex regex = new Regex(@"\[.{4}->.{4},(:?[ ,l,e,v]){6}\:(?:[ ,0-9]){1,4},(:?[ ,l,i,n,k,t,y,p,e]){9}\:(?:[ ,0-9]){1,3},(:?[ ,l,i,f,e,t,i,m]){9}\:.*\]", RegexOptions.IgnoreCase);
                Regex regexNodeIP = new Regex(@"(?:[A-Fa-f0-9]){4}(->)", RegexOptions.IgnoreCase);
                Regex regexNodeParent = new Regex(@"(>)(?:[A-Ta-t0-9]){4},", RegexOptions.IgnoreCase);
                Regex regexNodeLvl = new Regex(@"(:?[l,e,v]){5}\:(?:[ ,0-9]){1,4},", RegexOptions.IgnoreCase);
                Regex regexLifetime = new Regex(@"(:?[l,i,f,e,t,i,m]){8}\:.*s", RegexOptions.IgnoreCase);
                Regex regexLinkType = new Regex(@"(:?[l,i,n,k,t,y,p,e]){8}\:(?:[ ,0-9]){1,3},",RegexOptions.IgnoreCase);
                //Regex totalNodes = new Regex(@"\[ (?:[0-9])* (in total Routing link).*\]");
                Regex regexEnding = new Regex(@"(-){22}");
                string endingRule = "---------------------";
                string rplFromComm = SerialComm("rpl", endingRule, 40000, 3, regexEnding);
                MatchCollection matches = regex.Matches(rplFromComm);
                //MatchCollection matchTotal = totalNodes.Matches(rplFromComm);

                if (node.Count == 0)
                {
                    node.Add(new NodeInfo("0200", "ROOT", "0"));
                }

                if (matches.Count == 0)
                {
                    Stamp();
                    PrLog("RPL Result", false);
                    PrLog("No devices found, execute again!", true);
                }

                foreach (Match match in matches)
                {
                    MatchCollection matchIP = regexNodeIP.Matches(match.ToString());
                    MatchCollection matchParent = regexNodeParent.Matches(match.ToString());
                    MatchCollection matchLt = regexLifetime.Matches(match.ToString());
                    MatchCollection matchLvl = regexNodeLvl.Matches(match.ToString());
                    MatchCollection matchLinkT = regexLinkType.Matches(match.ToString());
                    char[] toTrimP = new char[4];
                    toTrimP[0] = '-';
                    toTrimP[1] = '>';
                    toTrimP[2] = ',';
                    toTrimP[3] = ' ';
                    char[] toTrimLvl = new char[5];
                    toTrimLvl[0] = 'l';
                    toTrimLvl[1] = 'e';
                    toTrimLvl[2] = 'v';
                    toTrimLvl[3] = ':';
                    toTrimLvl[4] = ',';
                    char[] toTrimLt = new char[15];
                    toTrimLt[0] = 'l';
                    toTrimLt[1] = 'i';
                    toTrimLt[2] = 't';
                    toTrimLt[3] = 'e';
                    toTrimLt[4] = 'm';
                    toTrimLt[5] = 'f';
                    toTrimLt[6] = 's';
                    toTrimLt[7] = ':';
                    toTrimLt[8] = ' ';
                    toTrimLt[9] = 'y';
                    toTrimLt[10] = 'p';
                    toTrimLt[11] = 'n';
                    toTrimLt[12] = 'k';
                    toTrimLt[13] = ',';
                    toTrimLt[14] = ']';

                    foreach (Capture capture in match.Captures)
                    {

                        string _ip = matchIP.First().ToString().ToLower().Trim(toTrimP).ToUpper();
                        string _parent = matchParent.First().ToString().ToLower().Trim(toTrimP).ToUpper();
                        string _lifetime = matchLt.First().ToString().ToLower().Trim(toTrimLt).ToUpper();
                        string _linkType = matchLinkT.First().ToString().ToLower().Trim(toTrimLt);
                        int _lvl = Convert.ToInt32(matchLvl.First().ToString().ToLower().Trim(toTrimLvl).ToUpper());

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
                                node.Find(x => x.IP == _ip).LinkType = _linkType;
                            }
                        }
                        else
                        {
                            node.Add(new NodeInfo(_ip, _parent, _lifetime, _lvl, _linkType));
                        }
                    }

                }

                int a = node.Count();
                double t = (double)s / (double)a;
                if (s != 0)
                {
                    Stamp();
                    PrLog("RPL Result", false);
                    PrLog(" " + (a.ToString() + " total nodes"), false);
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
            catch (Exception e)
            {
                Console.WriteLine(e);
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

        public static string SerialComm(string msg, string ending, int timeoutMillis, int length, Regex MatchEnding)
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
                    for (tries = 0; tries <= 4; tries++)
                    {
                        TestCom.Write(msg + "\r");
                        bool _continue = false;
                        bool _ismatch = false;
                        StampROL();
                        while ((timeLimit > Extensions.NanoTime()) && (true != _continue))
                        {
                            string stream = TestCom.ReadExisting();
                            LogToFileROL(stream);
                            buffer += stream;
                            _ismatch = buffer.Contains(ending);
                            MatchCollection endingMatches = MatchEnding.Matches(buffer);
                            _continue = _ismatch && (endingMatches.Count() > 0);

                            if (msg.Length != length)
                            {
                                Console.WriteLine("..");

                                if (buffer.Contains(msg) == false)
                                {
                                    Console.WriteLine(".");
                                }
                            }
                            Thread.Sleep(100);
                            if (_continue == true)
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

        public static string SerialComm(string msg, int timeoutMillis, Regex MatchEnding)
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
                    for (tries = 0; tries <= 4; tries++)
                    {
                        TestCom.Write(msg + "\r");
                        bool _continue = false;
                        StampROL();
                        while ((timeLimit > Extensions.NanoTime()) && (true != _continue))
                        {
                            string stream = TestCom.ReadExisting();
                            LogToFileROL(stream);
                            buffer += stream;
                            Match endingMatches = MatchEnding.Match(buffer);
                            _continue = (endingMatches.Success);
                            Thread.Sleep(100);
                            if (_continue == true)
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

        public static void PingNode(List<NodeInfo> node, string ip, int count, int payload, int timeout, int _lenght)
        {
            try
            {
                Regex regex = new Regex(@"\+(PING6:)(?:[0-9]*),(?:[0-9]*),(?:[0-9-%.]*),(?:[0-9]*),(?:[0-9]*),(?:[0-9]*),(?:[0-9]*)");
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
                    if (_lenght == 0)
                    {
                        _lenght = timeout;
                    }else if(_lenght < timeout)
                    {
                        _lenght = timeout;
                    }

                    if (ip != "0200")
                    {
                        string Ping6IP = "ping6 " + ip;
                        string Ping6Count =" c " + count;
                        string Ping6Size =" s " + payload;
                        string Ping6Timeout = " t " + timeout;
                        string Ping6Lenght = " l " + _lenght;
                        string msg = Ping6IP + Ping6Count + Ping6Size + Ping6Timeout + Ping6Lenght;
                        int lenght = msg.Length;
                        string com = SerialComm(msg, endingRule, count * (timeout) + 10000, lenght, regex);
                        MatchCollection matches = regex.Matches(com);
                        foreach (Match match in matches)
                        {
                            foreach (Capture capture in match.Captures)
                            {
                                node.Find(x => x.IP == ip).SavePing(capture);
                            }
                        }
                        if( matches.Count() == 0)
                        {
                            Console.WriteLine(".");
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

        public static void PingAllNodes(List<NodeInfo> node, int count, int payload, int timeout, int _lenght)
        {
            List<NodeInfo> WasPing = new List<NodeInfo>();
            try
            {
                foreach (NodeInfo nodes in node)
                {
                    PingNode(node, nodes.IP, count, payload, timeout, _lenght);
                    WasPing = node.FindAll(x => x.PTx != null);
                    PrLog(WasPing.Count().ToString("D4") + " Nodes pinged out of " + node.Count().ToString("D4") + " nodes", true);
                    if (WasPing.Count() % 100 == 0)
                    {
                        GetRPLNum();
                        node.Distinct().OrderBy(x => x.Level);
                        List<int> lvls = new List<int>();
                        foreach (NodeInfo nodeInfo in node)
                        {
                            int s = 0;
                            s = nodeInfo.Level;
                            lvls.Add(s);
                        }
                        lvls = lvls.OrderBy(x => x).Distinct().ToList();
                        foreach (int level in lvls)
                        {
                            PingResultsPerLevel(level, node);
                        }
                    }
                }
                if (WasPing.Count() == node.Count())
                {
                    PrLog("All selected nodes were pinged ", true);
                    GetRPLNum();
                    node.Distinct().OrderBy(x => x.Level);
                    List<int> lvls = new List<int>();
                    foreach (NodeInfo nodeInfo in node)
                    {
                        int s = 0;
                        s = nodeInfo.Level;
                        lvls.Add(s);
                    }
                    lvls = lvls.OrderBy(x => x).Distinct().ToList();
                    foreach (int level in lvls)
                    {
                        PingResultsPerLevel(level, node);
                    }
                }
                else
                {
                    GetRPLNum();
                    node.Distinct().OrderBy(x => x.Level);
                    List<int> lvls = new List<int>();
                    foreach (NodeInfo nodeInfo in node)
                    {
                        int s = 0;
                        s = nodeInfo.Level;
                        lvls.Add(s);
                    }
                    lvls = lvls.OrderBy(x => x).Distinct().ToList();
                    foreach (int level in lvls)
                    {
                        PingResultsPerLevel(level, node);
                    }
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

        public static void PingAllNodes(List<NodeInfo> node, int count, int payload, int timeout, int _lenght, int _lvl)
        {
            List <NodeInfo> WasPing = new List<NodeInfo>();
            try
            {
                List<NodeInfo> infos = new List<NodeInfo>();
                infos = node.FindAll(x => x.Level == _lvl);
                PrLog("Ping all nodes in Level " + _lvl.ToString("D2"), false);
                PrLog("Total nodes: " + infos.Count().ToString("D4"), true);
                foreach (NodeInfo nodes in infos)
                {
                    PingNode(node, nodes.IP, count, payload, timeout, _lenght);
                    WasPing = infos.FindAll(x => x.PTx != null);
                    PrLog(WasPing.Count().ToString("D4") + " Nodes pinged out of " + infos.Count().ToString("D4") + " total nodes in network", true);
                    if (WasPing.Count() % 100 == 0)
                    {
                        PingResultsPerLevel(_lvl, node);
                        GetRPLNum();
                    }
                }
                if (WasPing.Count() == node.Count())
                {
                    PrLog("All selected nodes were pinged ", true);
                    PingResultsPerLevel(_lvl, node);
                    GetRPLNum();
                }
                else
                {
                    PingResultsPerLevel(_lvl, node);
                    GetRPLNum();
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

        public static void PingAllLevels(List<NodeInfo> node, int count, int payload, int timeout, int _lenght, bool order)
        {
            try
            {
                if (order == true)
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
                        PingAllNodes(node, count, payload, timeout, _lenght, level);
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
            try
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
            catch (Exception e)
            {
                Console.WriteLine(e);
            }
        }

        public static void PingResults(int Ptx, int Prx, int Ploss, int Unstable)
        {
            try
            {
            PrLog("Total sent packets: " + Ptx.ToString("D2"), true);
            PrLog("Total received packets: " + Prx.ToString("D2"), true);
            double PRate = 1 - ((double)Prx / (double)Ptx);
            PrLog("Total package loss: " + (PRate).ToString("P"), true);
            PrLog("Total nodes not reachable: " + Ploss.ToString("D2"), true);
            PrLog("Total nodes with unstability: " + Unstable.ToString("D2"), true);
            }
            catch (Exception e)
            {
                Console.WriteLine(e);
            }
        }

        public static void PingResultsPerLevel(int _lvl, List<NodeInfo> node)
        {
            try
            {

                int Ptx = 0;
                int Prx = 0;
                int Ploss = 0;
                int Unstable = 0;
                List<NodeInfo> infos = new List<NodeInfo>();
                infos = node.FindAll(x => x.Level == _lvl);
                List<NodeInfo> WasPing = infos.FindAll(x => x.PTx != null);
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
                if (Ptx > 0 && Prx >= 0)
                {
                    PrLog("Ping Results for Level: " + _lvl.ToString("D2"), false);
                    int NodesPing = infos.Count() - (infos.Count() - WasPing.Count());
                    if (((infos.Count() - WasPing.Count()) != 0))
                    {
                        Console.WriteLine(".");
                    }
                    PrLog("Total nodes pinged: " + NodesPing.ToString("D2") + " out of " + infos.Count().ToString("D2") + " nodes in level " + _lvl.ToString("D2"), false);
                    PingResults(Ptx, Prx, Ploss, Unstable);
                }
            }
            catch (Exception e)
            {
                Console.WriteLine(e);
            }
        }

        public static void GetRPLNum()
        {
            try
            {
                Regex regex = new Regex(@"(?<=rpl num)\r\n\r(?:[0-9]*)\r\n");
                string rplFromComm = SerialComm("rpl num", 40000, regex);
                Match match = regex.Match(rplFromComm);
                Stamp();
                PrLog("RPL Num Result", false);
                PrLog("Total nodes in network: " + match.ToString().Trim(), true);
            }
            catch (Exception e)
            {
                Console.WriteLine(e);
            }
        }

        public static void GetNtpPush()
        {
            try
            {
                PrLog("Collecting NTP for Push Time Table from last and next 12 hours",false);
                //string endingRule = " seconds";
                //Regex regex = new Regex(@"NTP:.*time .* seconds");
                //Regex regexNTP = new Regex(@"time .* seconds");
                //Regex regexEnding = new Regex(@" seconds");
                //string ntpFromComm = SerialComm("ntp", endingRule, 40000, 3, regexEnding);
                //Match match = regexNTP.Match(ntpFromComm);
                //int w = Convert.ToInt32(Regex.Replace(match.Value, "[^0-9.]", "")); //Get Actual NIC NTP
                int w = System.Convert.ToInt32(System.DateTimeOffset.Now.ToUnixTimeSeconds());
                PrLog(" ", false);
                PrLog("Actual NTP is: " + w + " - Date: " + DateTimeOffset.FromUnixTimeSeconds(w).DateTime,false);
                int[] d = new int[] { 45, 50, 70 }; //Defines the push time accordingly with meter type
                List<String> m45 = new List<String>();
                List<String> m50 = new List<String>();
                List<String> m70 = new List<String>();
                PrLog("",false);
                PrLog("  Push 45 min          Push 50 min          Push 70 min", false);
                for (int i = w-43200; i <= w + 86400; i++) //Increment s in order to verify conditionadings
                {
                    foreach (int time in d)
                    {
                        if ((i % (time * 60)) == 0) //Compare if s is in d push time
                        {
                            if (time == 45)
                            {
                                DateTime result = DateTimeOffset.FromUnixTimeSeconds(i).DateTime; //Convert UNIX to Date Time
                                m45.Add(result.ToString());
                            }
                            if (time == 50)
                            {
                                DateTime result = DateTimeOffset.FromUnixTimeSeconds(i).DateTime; //Convert UNIX to Date Time
                                m50.Add(result.ToString());
                            }
                            if (time == 70)
                            {
                                DateTime result = DateTimeOffset.FromUnixTimeSeconds(i).DateTime; //Convert UNIX to Date Time
                                m70.Add(result.ToString());
                            }
                        }
                    }
                }
                for (int i = 0; i <= m70.Count - 1; i++) //Increment s in order to verify condition
                {
                    PrLog(m45[i] + "\t" + m50[i] + "\t" + m70[i],false);
                }
                PrLog("",false);
            }
            catch (Exception e)
            {
                Console.WriteLine(e);
            }
        }

    }
}
