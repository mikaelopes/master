using System;
using System.Text.RegularExpressions;
using System.Collections.Generic;
using System.Diagnostics;
using System.Threading;
using System.Linq;

namespace PingAll
{
    public class NodeInfo
    {
        public string IP { get; set; }
        public string Parent { get; set; }
        public string Lifetime { get; set; }
        public string LinkType { get; set; }
        public string PTx { get; set; }
        public string PRx { get; set; }
        public string PLoss { get; set; }
        public string PTime { get; set; }
        public string RTTmin { get; set; }
        public string RTTavg { get; set; }
        public string RTTmax { get; set; }
        public int ParentCounter { get; set; }
        public string LastParent { get; set; }
        public bool IsReachable { get; set; }
        public int Level { get; set; }
        public int ParentLevel { get; set; }
        public string MacAddress { get; set; }
        public string ShortMacAddress { get; set; }
        public List<string> Childs { get; set; }
        public List<string> Neighboors { get; set; }

        public NodeInfo(string _ip, string _parent, string _lifetime)
        {
            this.IP = _ip.ToUpper();
            this.Parent = _parent.ToUpper();
            this.Lifetime = _lifetime;
        }
        public NodeInfo(string _ip, string _parent, string _lifetime, int lvl, string _linkType)
        {
            this.IP = _ip.ToUpper();
            this.Parent = _parent.ToUpper();
            this.Lifetime = _lifetime;
            this.Level = lvl;
            this.LinkType = _linkType;
        }

        public NodeInfo(string _ip)
        {
            this.IP = _ip.ToUpper();
        }

        public NodeInfo(string _prx, string _ploss, string _ptime,
            string _rttmin, string _rttavg, string _rttmax)
        {
            this.PRx = _prx;
            this.PLoss = _ploss;
            this.PTime = _ptime;
            this.RTTmin = _rttmin;
            this.RTTavg = _rttavg;
            this.RTTmax = _rttmax;
        }

        public NodeInfo(string _ip, string _lastParent, int _parentCounter)
        {
            this.IP = _ip.ToUpper();
            this.LastParent = _lastParent.ToUpper();
            this.ParentCounter = _parentCounter;

        }

        public void ClearPing()
        {
            this.PLoss = null;
            this.PRx = null;
            this.PTx = null;
            this.RTTavg = null;
            this.RTTmin = null;
            this.RTTmax = null;
        }

        public void SavePing(Capture capture)
        {
            string value = ":";
            string value2 = ",";
            string t = "+PING6:3,3,0.0 %,293,84,84,86";
            string reachable;
            this.PTx = capture.Value.Substring(capture.Value.IndexOfNth(value, 0) + 1, (capture.Value.IndexOfNth(value2, 0) - capture.Value.IndexOfNth(value, 0) - 1));
            this.PRx = capture.Value.Substring(capture.Value.IndexOfNth(value2, 0) + 1, (capture.Value.IndexOfNth(value2, 1) - capture.Value.IndexOfNth(value2, 0) - 1));
            this.PLoss = capture.Value.Substring(capture.Value.IndexOfNth(value2, 1) + 1, (capture.Value.IndexOfNth(value2, 2) - capture.Value.IndexOfNth(value2, 1) - 2));
            this.PTime = capture.Value.Substring(capture.Value.IndexOfNth(value2, 2) + 1, (capture.Value.IndexOfNth(value2, 3) - capture.Value.IndexOfNth(value2, 2) - 1));
            this.RTTmin = capture.Value.Substring(capture.Value.IndexOfNth(value2, 3) + 1, (capture.Value.IndexOfNth(value2, 4) - capture.Value.IndexOfNth(value2, 3) - 1));
            this.RTTavg = t.Substring(t.IndexOfNth(value2, 4) + 1, (t.IndexOfNth(value2, 5) - t.IndexOfNth(value2, 4) - 1));
            this.RTTmax = capture.Value.Substring(capture.Value.IndexOfNth(value2, 5) + 1, (capture.Value.Length - capture.Value.IndexOfNth(value2, 5) - 1));
            if (this.PLoss == "100.0")
            {
                this.IsReachable = false;
                reachable = " - Node is not reachable.";
            }
            else if (this.PLoss == "0.0")
            {
                this.IsReachable = true;
                reachable = " - Node is reachable.";
            }
            else
            {
                this.IsReachable = true;
                reachable = " - Node is reachable, but connection may be unstable.";
            }
            Program.Stamp();
            Program.PrLog("Ping Result ", false);
            Program.PrLog("Node IP:  " + this.IP, false);
            Program.PrLog("Node Level:  " + this.Level, false);
            Program.PrLog("Packets sent: " + this.PTx + " - Packets Received: " + this.PRx + " - Packet Loss: " + this.PLoss + "%", true);
            Program.PrLog("RTTmin: " + this.RTTmin + "ms" + " - RTTmax: " + this.RTTmax + "ms" + " - RTTavg: " + this.RTTavg + "ms", true);
            Program.PrLog("Ping Total time: " + this.PTime + "ms" + reachable, true);
        }

        public void ParentChange(Capture capture)
        {
            Regex regexNodeParent = new Regex(@" (?:[A-Fa-f0-9]){4} l");
            Regex regexNodeLvl = new Regex(@"   [(?:[A-Fa-f0-9].  ");
            Regex regexLifetime = new Regex(@"(lt: )[(?:[A-Fa-f0-9].* s\]");
            MatchCollection matchParent = regexNodeParent.Matches(capture.Value.ToString());
            MatchCollection matchLt = regexLifetime.Matches(capture.Value.ToString());
            MatchCollection matchLvl = regexNodeLvl.Matches(capture.Value.ToString());
            char[] toTrim = new char[9];
            toTrim[0] = 'l';
            toTrim[1] = 't';
            toTrim[3] = ':';
            toTrim[4] = 's';
            toTrim[5] = ']';
            toTrim[6] = '-';
            toTrim[7] = '[';
            toTrim[8] = ' ';
            string _parent = matchParent.First().ToString().Trim(toTrim).ToUpper();
            string _lifetime = matchLt.First().ToString().Trim(toTrim).ToUpper();
            int _lvl = Convert.ToInt32(matchLvl.First().ToString().Trim(toTrim).ToUpper());

            this.LastParent = this.Parent;
            this.Parent = _parent;
            this.ParentCounter++;
            this.Level = _lvl;
            this.Lifetime = _lifetime;
            Program.PrLog("Parent Change Result", false);
            Program.PrLog("Node IP: " + this.IP + " level: " + _lvl.ToString(), true);
            Program.PrLog("Node " + this.IP + " last parent was: " + this.LastParent, true);
            Program.PrLog("New parent is: " + this.Parent, true);
            Program.PrLog("Has changed parent approx.: " + this.ParentCounter + " times", true);
        }

    }

    public static class Extensions
    {
        public static int IndexOfNth(this string str,
                                string value,
                                int nth = 0)
        {
            if (nth < 0)
                throw new ArgumentException("Can not find a negative index of substring in string. Must start with 0");

            int offset = str.IndexOf(value);
            for (int i = 0; i < nth; i++)
            {
                if (offset == -1) return -1;
                offset = str.IndexOf(value, offset + 1);
            }

            return offset;
        }

        public static long NanoTime()
        {
            long nano = 10000L * Stopwatch.GetTimestamp();
            nano /= TimeSpan.TicksPerMillisecond;
            nano *= 100L;
            return nano;
        }

    }

    public class DHCP6
    {
        public string MacAdress { get; set; }
        public string ShortMac { get; set; }
        public string IP { get; set; }

        public DHCP6(string _mac, string _ip, string _shortmac)
        {
            this.MacAdress = _mac;
            this.IP = _ip;
            this.ShortMac = _shortmac;
        }

    }

    public class PathUnweighted
    { 

        // function to form edge between 
        // two vertices source and dest
        public static void AddEdge(List<List<int>> adj,
                                    int i, int j)
        {
            int t = 0;
            try
            {
                if(i >= 0 && j >= 0)
                {
                    adj[i].Add(j);
                    adj[j].Add(i);
                }
                else
                {
                    
                        t++;
                }
            }
            catch(Exception e)
            {
                Console.WriteLine(e);
            }

        }

        // function to print the shortest 
        // distance and path between source 
        // vertex and destination vertex
        public static void PrintShortestDistance(List<List<int>> adj,
                                                  int s, int dest, int v, List<NodeInfo> node)
        {
            // predecessor[i] array stores 
            // predecessor of i and distance 
            // array stores distance of i
            // from s
            
            int[] pred = new int[v];
            int[] dist = new int[v];

            if (BFS(adj, s, dest,
                    v, pred, dist) == false)
            {
                Console.WriteLine("Given source and destination" +
                                  "are not connected");
                return;
            }

            // List to store path
            List<int> path = new List<int>();
            int crawl = dest;
            path.Add(crawl);

            while (pred[crawl] != -1)
            {
                path.Add(pred[crawl]);
                crawl = pred[crawl];
            }

            // Print distance
            Console.WriteLine("Node " + node.ElementAt(dest).IP + " level is: " +
                               dist[dest]);
            node.ElementAt(dest).Level = dist[dest];
            node.ElementAt(dest).ParentLevel = dist[dest] - 1;
            // Print path
            Console.WriteLine("Path to " + node.ElementAt(dest).IP + " is: ");

            for (int i = path.Count - 1;
                     i >= 0; i--)
            {
                if(i != 0)
                {
                    Console.Write(node.ElementAt(path[i]).IP + " --> ");
                }
                else
                {
                    Console.Write(node.ElementAt(path[i]).IP + " ");
                }

            }
            Console.WriteLine("");
        }

        public static void ShortestDistance(List<List<int>> adj,
                                                  int s, int dest, int v, List<NodeInfo> node)
        {
            // predecessor[i] array stores 
            // predecessor of i and distance 
            // array stores distance of i
            // from s

            int[] pred = new int[v];
            int[] dist = new int[v];

            if (BFS(adj, s, dest,
                    v, pred, dist) == false)
            {
                Console.WriteLine("Given source and destination" +
                                  "are not connected");
                return;
            }

            // List to store path
            List<int> path = new List<int>();
            int crawl = dest;
            path.Add(crawl);

            while (pred[crawl] != -1)
            {
                path.Add(pred[crawl]);
                crawl = pred[crawl];
            }

            // Save node distance
            node.ElementAt(dest).Level = dist[dest];
            node.ElementAt(dest).ParentLevel = dist[dest] - 1;
        }

        // a modified version of BFS that 
        // stores predecessor of each vertex 
        // in array pred and its distance 
        // from source in array dist
        public static bool BFS(List<List<int>> adj,
                                int src, int dest,
                                int v, int[] pred,
                                int[] dist)
        {
            // a queue to maintain queue of 
            // vertices whose adjacency list 
            // is to be scanned as per normal
            // BFS algorithm using List of int type
            List<int> queue = new List<int>();

            // bool array visited[] which 
            // stores the information whether 
            // ith vertex is reached at least 
            // once in the Breadth first search
            bool[] visited = new bool[v];

            // initially all vertices are 
            // unvisited so v[i] for all i 
            // is false and as no path is 
            // yet constructed dist[i] for 
            // all i set to infinity
            for (int i = 0; i < v; i++)
            {
                visited[i] = false;
                dist[i] = int.MaxValue;
                pred[i] = -1;
            }

            // now source is first to be 
            // visited and distance from 
            // source to itself should be 0
            visited[src] = true;
            dist[src] = 0;
            queue.Add(src);

            // bfs Algorithm
            while (queue.Count != 0)
            {
                int u = queue[0];
                queue.RemoveAt(0);

                for (int i = 0;
                         i < adj[u].Count; i++)
                {
                    if (visited[adj[u][i]] == false)
                    {
                        visited[adj[u][i]] = true;
                        dist[adj[u][i]] = dist[u] + 1;
                        pred[adj[u][i]] = u;
                        queue.Add(adj[u][i]);

                        // stopping condition (when we 
                        // find our destination)
                        if (adj[u][i] == dest)
                            return true;
                    }
                }
            }
            return false;
        }

        public static void GetTopology (List<NodeInfo> node)
        {
            // No of vertices
            int v = node.Count();
            

            // Adjacency list for storing 
            // which vertices are connected
            List<List<int>> adj =
                      new List<List<int>>(v);

            for (int i = 0; i < v; i++)
            {
                adj.Add(new List<int>());
            }

            // Creating graph given in the 
            // above diagram. add_edge 
            // function takes adjacency list, 
            // source and destination vertex 
            // as argument and forms an edge 
            // between them.

            foreach (NodeInfo info in node)
            {
                if (info.IP != "0200")
                {
                    int _ip = node.FindIndex(x => x.IP == info.IP);
                    int _parent = node.FindIndex(x => x.IP == info.Parent);
                    AddEdge(adj, _parent, _ip);
                }
            }

            foreach (NodeInfo info in node)
            {
                if (info.IP != "0200")
                {
                    int _ip = node.FindIndex(x => x.IP == info.IP);
                    int _parent = node.FindIndex(x => x.IP == info.Parent);
                    ShortestDistance(adj, 0,
                                          _ip, v, node);
                }
            }

            int LastHop = node.Max(x => x.Level);
            for (int i = 0; i <= LastHop; i++)
            {
                int NodesLvl = node.FindAll(x => x.Level == i).Count;
                Program.PrLog(" Level " + i.ToString("D2") + " ---> " + NodesLvl.ToString("D4") + " Nodes ", false);
            }
        }


        public static void GetTopologyPath(List<NodeInfo> node)
        {
            // No of vertices
            int v = node.Count();

            // Adjacency list for storing 
            // which vertices are connected
            List<List<int>> adj =
                      new List<List<int>>(v);

            for (int i = 0; i < v; i++)
            {
                adj.Add(new List<int>());
            }

            // Creating graph given in the 
            // above diagram. add_edge 
            // function takes adjacency list, 
            // source and destination vertex 
            // as argument and forms an edge 
            // between them.

            foreach (NodeInfo info in node)
            {
                if (info.IP != "0200")
                {
                    int _ip = node.FindIndex(x => x.IP == info.IP);
                    int _parent = node.FindIndex(x => x.IP == info.Parent);
                    AddEdge(adj, _parent, _ip);
                }
            }

            foreach (NodeInfo info in node)
            {
                if (info.IP != "0200")
                {
                    int _ip = node.FindIndex(x => x.IP == info.IP);
                    int _parent = node.FindIndex(x => x.IP == info.Parent);
                    PrintShortestDistance(adj, 0,
                                          _ip, v, node);
                }
            }
        }

    }

    public class Spinner : IDisposable
    {
        private const string Sequence = @"/-\|";
        private int counter = 0;
        private readonly int left;
        private readonly int top;
        private readonly int delay;
        private bool active;
        private readonly Thread thread;

        public Spinner(int left, int top, int delay = 100)
        {
            this.left = left;
            this.top = top;
            this.delay = delay;
            thread = new Thread(Spin);
        }

        public void Start()
        {
            active = true;
            if (!thread.IsAlive)
                thread.Start();
        }

        public void Stop()
        {
            active = false;
            Console.SetCursorPosition(this.left, this.top);
            Draw(' ');
        }

        private void Spin()
        {
            while (active)
            {
                Turn();
                Thread.Sleep(delay);
            }
        }

        private void Draw(char c)
        {
            Console.SetCursorPosition(this.left, this.top);
            Console.ForegroundColor = ConsoleColor.Green;
            Console.Write(c);
        }

        private void Turn()
        {
            Draw(Sequence[++counter % Sequence.Length]);
        }

        public void Dispose()
        {
            Stop();
        }
    }
}

