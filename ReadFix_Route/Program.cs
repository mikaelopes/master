using System;
using System.Text.RegularExpressions;
using System.Collections.Generic;
using System.IO;
    

namespace ReadFix_Route
{
    class Program
    {
        static void Main(string[] args)
        {
            try
            {
                string path = @".\FixRoute.txt";
                //string space = " ";
                //string equal = "=";
                //string comma = ",";

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
                                    _short = matchValue.Value.Substring(4, matchValue.Value.Length - 4);
                                    if(_index == "")
                                    {
                                        Console.WriteLine(".");
                                    }
                                    if (route.Count == 0)
                                    {
                                        route.Add(new Route(_index, _short, hour));
                                    }
                                    else if (route.Exists(x => x._short == _short) == false)
                                    {
                                        route.Add(new Route(_index, _short, hour));
                                    }
                                    else if (route.Exists(x => x._short == _short) == true)
                                    {
                                        route.Find(x => x._short == _short)._changeCounter++;
                                    }
                                }
                            }
                        }
                    }

                    streamReader.Close();
                }


                using (StreamWriter streamWriter = new StreamWriter(@".\FixRoute_Result.txt"))
                {
                    foreach (string hour in hours)
                    {

                        List<Route> rf = route.FindAll(x => x._hour == hour);
                        streamWriter.WriteLine("|||||||||||||||||||||||||||||||||||||||||||||||||||||||||||||||||||");
                        streamWriter.WriteLine("Total fix route messages at {0}h: {1}", hour, rf.Count);
                        Console.WriteLine("|||||||||||||||||||||||||||||||||||||||||||||||||||||||||||||||||||");
                        Console.WriteLine("Total fix route messages at {0}h: {1}", hour, rf.Count);
                        foreach (Route fix in rf)
                        {
                            streamWriter.WriteLine("Node {0} changed parent {1} times", fix._short, fix._changeCounter);
                            Console.WriteLine("Node {0} changed parent {1} times", fix._short, fix._changeCounter);
                        }
                    }

                    streamWriter.Close();
                }
            }
            catch(Exception e)
            {
                Console.WriteLine(e);
            }
        }
    }

    public class Route
    {
        public string _index { get; set; }
        public string _short { get; set; }
        public int _changeCounter { get; set; }
        public string _hour { get; set; }

        public Route(string Index, string Short, string Hour)
        {
            this._index = Index;
            this._short = Short;
            this._hour = Hour;
            this._changeCounter = 1;
        }
    }
}

