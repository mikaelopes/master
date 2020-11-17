using System;
using System.Collections.Generic;
using QuickGraph;


namespace GraphTopology
{
    class Program
    {

        public static void Main(String[] args)
        {
            var g = new AdjacencyGraph<int, TaggedEdge<int, string>>();
            var e2 = new TaggedEdge<int, string>(1, 2,"hello");
            var e3 = new TaggedEdge<int, string>(1, 2, "hello");

            g.AddVerticesAndEdge(e2);

            g.AddVerticesAndEdge(e3);

        }
    }
 
}
