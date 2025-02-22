using System;
using System.Collections.Generic;
using System.Diagnostics;

namespace PerfectHash
{
    /// <summary>
    /// Implements a graph with 'N' vertices.  First, you connect the graph with
    /// edges, which have a desired value associated.  Then the vertex values
    /// are assigned, which will fail if the graph is cyclic.  The vertex values
    /// are assigned such that the two values corresponding to an edge add up to
    /// the desired edge value (mod N).
    /// </summary>
    public class Graph
    {

        /// <summary>
        /// maps a vertex number to the list of tuples (vertex, edge value)
        /// to which it is connected by edges.
        /// </summary>
        public Dictionary<int, List<Tuple<int, int>>> adjacent;

        /// <summary>
        /// number of vertices
        /// </summary>
        public int N;

        public int[] vertex_values;

        public Graph(int N)
        {
            this.N = N;
            this.adjacent = new Dictionary<int, List<Tuple<int, int>>>();
            for (int i = 0; i < N; i++)
            {
                adjacent[i] = new List<Tuple<int, int>>();
            }
        }

        /// <summary>
        /// Connect 'vertex1' and 'vertex2' with an edge, with associated
        /// value 'value'
        /// </summary>
        /// <param name="vertex1"></param>
        /// <param name="vertex2"></param>
        /// <param name="edge_value"></param>
        public void connect(int vertex1, int vertex2, int edge_value)
        {
            // Add vertices to each other's adjacent list
            this.adjacent[vertex1].Add(new Tuple<int, int>(vertex2, edge_value));
            this.adjacent[vertex2].Add(new Tuple<int, int>(vertex1, edge_value));
        }

        /// <summary>
        /// Try to assign the vertex values, such that, for each edge, you can
        /// add the values for the two vertices involved and get the desired
        /// value for that edge, i.e. the desired hash key.
        /// This will fail when the graph is cyclic.
        ///
        /// This is done by a Depth-First Search of the graph.  If the search
        /// finds a vertex that was visited before, there's a loop and False is
        /// returned immediately, i.e. the assignment is terminated.
        /// On success (when the graph is acyclic) True is returned.
        /// </summary>
        public virtual bool assign_vertex_values()
        {
            vertex_values = new int[N];
            Array.Fill(vertex_values, -1);

            var visited = new bool[N];

            // Loop over all vertices, taking unvisited ones as roots.
            for (int root = 0; root < N; root++)
            {
                if (visited[root])
                {
                    continue;
                }

                // explore tree starting at 'root'
                this.vertex_values[root] = 0;
                // Stack of vertices to visit, a list of tuples (parent, vertex)
                var tovisit = new Stack<Tuple<int, int>>();
                tovisit.Push(new Tuple<int, int>(-1, root));
                while (tovisit.Count > 0)
                {
                    var (parent, vertex) = tovisit.Pop();
                    visited[vertex] = true;
                    // Loop over adjacent vertices, but skip the vertex we arrived
                    // here from the first time it is encountered.
                    var skip = true;
                    foreach (var (neighbor, edge_value) in this.adjacent[vertex])
                    {
                        if (skip && neighbor == parent)
                        {
                            skip = false;
                            continue;
                        }

                        if (visited[neighbor])
                        {
                            // We visited here before, so the graph is cyclic.
                            return false;
                        }

                        tovisit.Push(new Tuple<int, int>(vertex, neighbor));
                        // Set new vertex's value to the desired edge value,
                        // minus the value of the vertex we came here from.
                        this.vertex_values[neighbor] = ((edge_value - this.vertex_values[vertex]) % this.N + N) % N;
                    }
                }
            }

            // check if all vertices have a valid value
            for (int vertex = 0; vertex < N; vertex++)
            {
                Debug.Assert(this.vertex_values[vertex] >= 0);
            }

            // We got though, so the graph is acyclic,
            // and all values are now assigned.
            return true;
        }
    }
}