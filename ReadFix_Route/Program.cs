using System;
using System.Text.RegularExpressions;
using System.Collections.Generic;
using System.IO;
using System.Threading;


namespace ReadFix_Route
{
    class Program
    {
        public class Spinner : IDisposable
        {
            private string Sequence { get; set; }
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
                Draw(" ");
            }

            public void Update(string percent)
            {
                this.Sequence = percent;
            }

            private void Spin()
            {
                while (active)
                {
                    Turn();
                    Thread.Sleep(delay);
                }
            }

            private void Draw(string c)
            {
                Console.SetCursorPosition(this.left, this.top);
                Console.ForegroundColor = ConsoleColor.Green;
                Console.Write(c);
            }

            private void Turn()
            {
                Draw(Sequence);
            }

            public void Dispose()
            {
                Stop();
            }
        }
        static void Main()
        {
            try
            {
                string path = @".\FixRoute.txt";
                //string space = " ";
                //string equal = "=";
                //string comma = ",";
                Console.WriteLine("Reading Log file - FixRoute.txt");
                var spinner = new Spinner(Console.CursorLeft, Console.CursorTop);
                spinner.Update("0%");
                spinner.Start();
                string percent = @"0";
                Regex _rgxFixRoute = new Regex(@"\[(.*?[0-9]){2}:(.*?[0-9]){2}:(.*?[0-9]){2}\](fix_route: index = )(.*?[0-9]), (short = )(.*?)$");
                Regex _indexValue = new Regex(@"(= )(.*?[0 - 9])");
                Regex _shortValue = new Regex(@"(t = )([a-f0-9])(.*)$");
                Regex _timestamp = new Regex(@"\[(.*?[0-9]){2}:(.*?[0-9]){2}:(.*?[0-9]){2}\]");
                string[] hours = new string[24];
                for(int i = 0; i <= 23; i++)
                {
                    hours[i] = i.ToString("D2");
                }
                List<Route> route = new List<Route>();
                using (StreamReader streamReader = new StreamReader(path))
                {
                    string s = "";
                    double lines = File.ReadAllLines(path).Length;
                    double readlines = 0;
                    while( (s = streamReader.ReadLine()) != null)
                    {
                        MatchCollection matches = _rgxFixRoute.Matches(s);
                        foreach( Match match in matches)
                        {
                            foreach(string hour in hours)
                            {
                                string h = _timestamp.Match(s).Value.TrimStart('[').TrimEnd(']').Substring(0, 2);
                                if (h == hour)
                                {
                                    string _index = string.Empty;
                                    string _short = string.Empty;
                                    Match matchIndex = _indexValue.Match(match.Value);
                                    Match matchValue = _shortValue.Match(match.Value);
                                    _index = matchIndex.Value.ToString().TrimStart('=').TrimEnd(',').Trim().TrimEnd(',');
                                    _short = matchValue.Value[4..];
                                    if(_index == "")
                                    {
                                        Console.WriteLine(".");
                                    }
                                    if (route.Count == 0)
                                    {
                                        route.Add(new Route(_index, _short, hour));
                                    }
                                    else if (route.Exists(x => x.Short == _short) == false)
                                    {
                                        route.Add(new Route(_index, _short, hour));
                                    }
                                    else if (route.Exists(x => x.Short == _short) == true)
                                    {
                                        route.Find(x => x.Short == _short).ChangeCounter++;
                                    }
                                }
                            }
                        }
                        readlines++;
                        double v = (readlines / lines);
                        percent = v.ToString("P");
                        spinner.Update(percent);
                    }

                    streamReader.Close();
                    spinner.Stop();
                }

                Console.WriteLine("Writing Results file - FixRoute_Result.txt");
                using (StreamWriter streamWriter = new StreamWriter(@".\FixRoute_Result.txt"))
                {
                    streamWriter.WriteLine("node,count,hour");
                    foreach (string hour in hours)
                    {
                        List<Route> rf = route.FindAll(x => x.Get_hour() == hour);
                        int totalFix = 0;
                        foreach (Route fix in rf)
                        {
                            totalFix += fix.ChangeCounter;
                        }
                        streamWriter.WriteLine(",{1},{0}h", hour, totalFix.ToString("D3"));
                        Console.WriteLine("*---------------------------*");
                        Console.WriteLine("Total fix route messages at {0}h: {1}", hour, rf.Count.ToString("D3"));
                        foreach (Route fix in rf)
                        {
                            streamWriter.WriteLine("{0},{1},{2}h", fix.Short.ToUpper(), fix.ChangeCounter.ToString("D3"), hour);
                            Console.WriteLine("Node {0} changed parent {1} times at {2}h", fix.Short.ToUpper(), fix.ChangeCounter.ToString("D3"), hour);
                        }
                    }

                    streamWriter.Close();
                }
                Console.WriteLine("Task Finished");
            }
            catch(Exception e)
            {
                Console.WriteLine(e);
            }
        }
    }

    public class Route
    {
        public string Index { get; set; }
        public string Short { get; set; }
        public int ChangeCounter { get; set; }

        private string _hour1;

        public string Get_hour()
        {
            return _hour1;
        }

        public void Set_hour(string value)
        {
            _hour1 = value;
        }

        public Route(string Index, string Short, string Hour)
        {
            this.Index = Index;
            this.Short = Short;
            this.Set_hour(Hour);
            this.ChangeCounter = 1;
        }
    }
}

