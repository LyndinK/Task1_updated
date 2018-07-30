using System;
using System.Collections.Generic;
using System.Text;
using System.Linq;
using System.IO;

namespace Matrix_shortest
{
    class Program
    {
        // ----------------------------------------------------------------------- //
        // ----------------------------------------------------------------------- //
        // -------------- This block of code contains service functions------------//
        // ----------------------------------------------------------------------- //
        // ----------------------------------------------------------------------- //

        // ------------------------Reads field from file-------------------------- //
        public static string[,] GetMap(string path)
        {
            string[,] field = { {".", ".", "#", ".", ".", ".", ".", ".", ".", ".", },
                                {".", ".", "#", ".", ".", ".", ".", ".", ".", ".", },
                                {".", ".", "#", ".", "#", ".", ".", ".", ".", ".", },
                                {".", ".", "#", ".", "#", ".", ".", ".", ".", ".", },
                                {".", ".", "#", "X", "#", ".", ".", ".", ".", ".", },
                                {".", ".", "#", "#", "#", ".", "#", "#", "#", "#", },
                                {".", ".", "#", ".", ".", ".", ".", ".", ".", ".", },
                                {".", ".", "#", "#", "#", ".", "#", "#", "#", ".", },
                                {".", ".", ".", ".", ".", ".", ".", ".", ".", ".", },
                                {".", ".", ".", ".", "0", ".", ".", ".", ".", ".", },};
            try
            {
                string[] lines = System.IO.File.ReadAllLines(path);
                var res = new string[10, 10];
                for (int a = 0; a < res.GetLength(0); a++)
                {
                    for (int j = 0; j < res.GetLength(1); j++)
                    {

                        var symbol = lines[a].Split(' ')[j];
                        if (symbol != "." && symbol != "#" && symbol != "0" && symbol != "X")
                        {
                            throw new Exception("ERROR, file is corrupted and cannot be used");
                        }
                        else
                        {
                            res[a, j] = symbol;
                        }
                    }
                }
                if (findvalue("X", res).Count > 2 || findvalue("X", res).Count == 0 || findvalue("0", res).Count > 2 || findvalue("X", res).Count == 0)
                    throw new Exception("ERROR: file has multiple starts or multiple goals");
                Console.WriteLine("Successfully loaded file from: " +path+ "\n-----");
                return res;
            }
            catch (FileNotFoundException)
            {
                Console.WriteLine("ERROR: file not found");
                Console.WriteLine("Loading default field\n-----");
                return field;
            }
            catch (Exception ex)
            {
                Console.WriteLine(ex.Message);
                Console.WriteLine("Loading default field\n-----");
                return field;
            }

        }

        //----------------------------Saves result-------------------------------- //
        public static void PrintResult(string[,] res)
        {
            StringBuilder to_write = new StringBuilder();
            for (int a = 0; a < res.GetLength(0); a++)
            {
                for (int j = 0; j < res.GetLength(0); j++)
                {
                    to_write.AppendFormat(res[a, j] + " ");
                }
                to_write.AppendFormat(Environment.NewLine);
            }
            System.IO.File.WriteAllText((Directory.GetCurrentDirectory() + @"\result.txt"), to_write.ToString());
            Console.WriteLine("saving to " + Directory.GetCurrentDirectory());

        }
        // -------------Finds start and goal points------------------------------- //
        public static List<int> findvalue(string to_find, string[,] arr)
        {
            List<int> res = new List<int>();
            for (int a = 0; a < arr.GetLength(0); a++)
            {
                for (int j = 0; j < arr.GetLength(1); j++)
                {
                    if (String.Equals(arr[a, j], to_find))
                    {
                        res.Add(a);
                        res.Add(j);
                    }
                }
            }
            return res;
        }
        // ----------------------------------------------------------------------- //
        // ----------------------------------------------------------------------- //
        // ----------- This block of code contains A* method components------------//
        // ----------------------------------------------------------------------- //
        // ----------------------------------------------------------------------- //

        //----------------class to represent a point-------------------------------//
        public class Waypoint
        {
            public int x { get; set; }
            public int y { get; set; }
            public Waypoint camefrom { get; set; }
            public int WayFromStart { get; set; }
            public int Estimated { get; set; }
            public int EstimatedFull
            {
                get
                {
                    return this.Estimated + this.WayFromStart;
                }
            }
        }
        //-----------------method to find neighbors --------------------------------//
        private static List<Waypoint> GetNeighbors(Waypoint chosen, int xgoal, int ygoal, string[,] patharr)
        {
            var res = new List<Waypoint>();
            if (chosen.x + 1<patharr.GetLength(0))             
            {
                if (patharr[chosen.x +1, chosen.y] != "#")
                {
                    res.Add(new Waypoint
                    {
                        x = chosen.x + 1,
                        y = chosen.y,
                        camefrom = chosen,
                        WayFromStart = chosen.WayFromStart + 1,
                        Estimated = Measure(chosen.x+1, chosen.y, xgoal, ygoal),
                    });
                }
            }
            if (0 <= chosen.x - 1)
            {
                if (patharr[chosen.x - 1, chosen.y] != "#")
                {
                    res.Add(new Waypoint
                    {
                        x = chosen.x - 1,
                        y = chosen.y,
                        camefrom = chosen,
                        WayFromStart = chosen.WayFromStart + 1,
                        Estimated = Measure(chosen.x-1, chosen.y, xgoal, ygoal),
                    });
                }
            }
            if (chosen.y + 1 < patharr.GetLength(1))
            {
                if (patharr[chosen.x, chosen.y+1] != "#")
                {
                    res.Add(new Waypoint
                    {
                        x = chosen.x,
                        y = chosen.y+1,
                        camefrom = chosen,
                        WayFromStart = chosen.WayFromStart + 1,
                        Estimated = Measure(chosen.x, chosen.y+1, xgoal, ygoal),
                    });
                }
            }
            if (chosen.y - 1 >=0)
            {
                if (patharr[chosen.x, chosen.y - 1] != "#")
                {
                    res.Add(new Waypoint
                    {
                        x = chosen.x,
                        y = chosen.y - 1,
                        camefrom = chosen,
                        WayFromStart = chosen.WayFromStart + 1,
                        Estimated = Measure(chosen.x, chosen.y-1, xgoal, ygoal),
                    });
                }
            }
            return res;
        }
        //---------------Heuristics measuring the approximate distance to goal-----//
        public static int Measure(int xfrom, int yfrom, int xto, int yto)
        {
            return Math.Abs(xfrom - xto) + Math.Abs(yfrom - yto);
        }
        // --------------------------Getting the Road----------------------------- //
        public static string[,] GetOriginalRoad (string[,] arr, Waypoint current)
        {
            var lst = new List<Waypoint>();
            while (current != null)
            {
                lst.Add(current);
                current = current.camefrom;
            }
            for (int i = 1; i<lst.Count-1;i++)
            {
                arr[lst[i].x, lst[i].y]  = "*";
            }
            return arr;
        }
        // -----------------------Astar find path--------------------------------- //
        public static string[,] AStar (string[,] field, int start_x, int start_y, int goal_x, int goal_y)
        {
            var OpenSet = new List<Waypoint>();
            var ClosedSet = new List<Waypoint>();
            var CurrentPoint = new Waypoint
            {
                x = start_x,
                y = start_y,
                camefrom = null,
                WayFromStart = 0,
                Estimated = Measure(start_x, start_y, goal_x, goal_y)
            };
            OpenSet.Add(CurrentPoint);
            while (OpenSet.Count > 0)
            {
                CurrentPoint = OpenSet.OrderBy(node => node.EstimatedFull).First();
                if (CurrentPoint.x == goal_x && CurrentPoint.y == goal_y)
                {
                    Console.WriteLine(String.Format("Nodes processed: {0}", ClosedSet.Count));
                    return GetOriginalRoad(field, CurrentPoint);
                }
                OpenSet.Remove(CurrentPoint);
                ClosedSet.Add(CurrentPoint);
                foreach (var NeighborPoint in GetNeighbors(CurrentPoint, goal_x, goal_y, field))
                {
                    if (ClosedSet.Count(node => (node.x == NeighborPoint.x && node.y == NeighborPoint.y)) > 0)
                        continue;
                    var opennode = OpenSet.FirstOrDefault(node => (node.x == NeighborPoint.x && node.y == NeighborPoint.y));
                    if (opennode == null)
                    {
                        OpenSet.Add(NeighborPoint);
                    }
                    else
                    {
                        if( opennode.WayFromStart > NeighborPoint.WayFromStart)
                        {
                            opennode.camefrom = CurrentPoint;
                            opennode.WayFromStart = NeighborPoint.WayFromStart;
                        }
                    }
                }
            }
            Console.WriteLine(String.Format("Nodes processed: {0}", ClosedSet.Count));
            return null;
        }
        // ----------------------------------------------------------------------- //
        // ----------------------------------------------------------------------- //
        // ---------- This block of code contains Wave method components-----------//
        // ----------------------------------------------------------------------- //
        // ----------------------------------------------------------------------- //

        // ------------------class to represent a point--------------------------- //
        public class Point
        {
            public int x { get; set; }
            public int y { get; set; }
            public Point(int ox, int oy)
            {
                x = ox;
                y = oy;
            }
        }
        // ---------function to restore the original way--------------------------- //
        public static string[,] RestoreRoad(string[,] field, int[,] map, int start_x, int start_y, int value)
        {
            if (value > 1)
            {
                foreach (Tuple<int, int> t in new List<Tuple<int, int>> { Tuple.Create(1, 0), Tuple.Create(0, 1), Tuple.Create(-1, 0), Tuple.Create(0, -1) })
                {
                    if (0 <= start_x + t.Item1 && 9 >= start_x + t.Item1 && 0 <= start_y + t.Item2 &&
                        9 >= start_y + t.Item2)
                    {
                        if (map[start_x + t.Item1, start_y + t.Item2] == value - 1)
                        {
                            field[start_x + t.Item1, start_y + t.Item2] = "*";
                            return RestoreRoad(field, map, start_x + t.Item1, start_y + t.Item2, value - 1);
                        }
                    }
                }
            }
            return field;
        }
        // ---------------Wave pathfinding algorithm-------------------- //
        public static string[,] Wave(string[,] symbarr, int x_start, int y_start, int x_goal, int y_goal)
        {
            var new_roads = new int[10, 10];
            List<Point> start = new List<Point>();
            start.Add(new Point(x_start, y_start));
            List<Point> Fill(List<Point> coords, int value)
            {
                var res = new List<Point>();
                foreach (var Coord in coords)
                {
                    if (Coord.x + 1 < symbarr.GetLength(0))
                    {
                        if (new_roads[Coord.x + 1, Coord.y] == 0 && symbarr[Coord.x + 1, Coord.y] != "#")
                        {
                            new_roads[Coord.x + 1, Coord.y] = value;
                            res.Add(new Point(Coord.x + 1, Coord.y));
                        }
                    }
                    if (Coord.x - 1 >= 0)
                    {
                        if (new_roads[Coord.x - 1, Coord.y] == 0 && symbarr[Coord.x - 1, Coord.y] != "#")
                        {
                            new_roads[Coord.x - 1, Coord.y] = value;
                            res.Add(new Point(Coord.x - 1, Coord.y));
                        }
                    }
                    if (Coord.y - 1 >= 0)
                    {
                        if (new_roads[Coord.x, Coord.y - 1] == 0 && symbarr[Coord.x, Coord.y - 1] != "#")
                        {
                            new_roads[Coord.x, Coord.y - 1] = value;
                            res.Add(new Point(Coord.x, Coord.y - 1));
                        }
                    }
                    if (Coord.y + 1 < symbarr.GetLength(1))
                    {
                        if (new_roads[Coord.x, Coord.y + 1] == 0 && symbarr[Coord.x, Coord.y + 1] != "#")
                        {
                            new_roads[Coord.x, Coord.y + 1] = value;
                            res.Add(new Point(Coord.x, Coord.y + 1));
                        }
                    }
                }
                if (new_roads[x_goal, y_goal] == 0 && res.Count > 0)
                    return Fill(res, value + 1);
                else return null;
            }
            Fill(start, 1);
            if (new_roads[x_goal, y_goal] != 0)
                return RestoreRoad(symbarr, new_roads, x_goal, y_goal, new_roads[x_goal, y_goal]);
            else return null;

        }
        // ----------------------------------------------------------------------- //
        // ----------------------------------------------------------------------- //
        static void Main(string[] args)
        {
            Dictionary<string, Delegate> Dict = new Dictionary<string, Delegate>(); // Dictionary of existing algorithms
            Dict.Add("astar", new Func<string[,], int, int, int, int, string[,]>(AStar));
            Dict.Add("wave", new Func<string[,], int, int, int, int, string[,]>(Wave));
            var field = GetMap(Directory.GetCurrentDirectory() + @"\map.txt");
            var start = findvalue("0", field);
            var goal = findvalue("X", field);
            bool TryAgain = true;
            string[,] res= null;
            DateTime now = DateTime.Now;
            while (TryAgain)
            {
                try
                {
                    foreach (string k in Dict.Keys)
                    {
                        Console.WriteLine(String.Format("Type {0} to solve using {0}", k));
                    }
                    var key = Console.ReadLine();
                    now = DateTime.Now;
                    res = (string[,])Dict[key].DynamicInvoke(field, start[0], start[1], goal[0], goal[1]);
                    TryAgain = false;
                }
                catch (KeyNotFoundException)
                {
                    Console.WriteLine("ERROR: no such algorithm exists, please try again!\n#------------------------------------------------#");
                }
            }
             Console.WriteLine(String.Format("Executed in {0}", DateTime.Now - now));
            if (res != null)
            {
                for (int a = 0; a < res.GetLength(0); a++)
                {
                    Console.Write("\n");
                    for (int j = 0; j < res.GetLength(1); j++)
                    {
                        Console.Write(res[a, j] + " ");
                    }
                }
            }
            else
                Console.WriteLine("the way doesn't seem to exist");
            Console.WriteLine("\npress enter to save result, press any other key to exit");
            if (Console.ReadKey().Key == ConsoleKey.Enter)
            {
                PrintResult(res);
                Console.WriteLine("Press any key to exit");
                Console.ReadKey();
            }
        }
    }
}
