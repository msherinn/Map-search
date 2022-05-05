using System.Linq;
using System.Xml.Xsl;
using MapSearch;

class Program
{
    static double Haversine(float lat1, float lat2, float lon1, float lon2)
    {
        var R = 6371e3; // metres
        var φ1 = lat1 * Math.PI/180; // φ, λ in radians
        var φ2 = lat2 * Math.PI/180;
        var Δφ = (lat2-lat1) * Math.PI/180;
        var Δλ = (lon2-lon1) * Math.PI/180;

        var a = Math.Sin(Δφ/2) * Math.Sin(Δφ/2) +
            Math.Cos(φ1) * Math.Cos(φ2) *
            Math.Sin(Δλ/2) * Math.Sin(Δλ/2);
        var c = 2 * Math.Atan2(Math.Sqrt(a), Math.Sqrt(1-a));

        var d = (R * c) / 1000; // in kilometers
        return d;
    }

    static int turn = 0;

    static void Split(TreeNode root)
    {
        if (root.Axis == "x")
        {
            root.Content.Sort((x, y) => x.Lat.CompareTo(y.Lat));

            var median = root.Content.Count / 2;
            var left = new TreeNode(root.Content[0].Lat, root.MinLon, root.Content[median].Lat, root.MaxLon, "y");
            left.Content = root.Content.Take(median).ToList();
            left.Parent = root;
            var right = new TreeNode(root.Content[median + 1].Lat, root.MinLon, root.Content[median + 1].Lat, root.MaxLon, "y");
            right.Content = root.Content.Skip(median).ToList();
            right.Parent = root;
            
            root.LeftChild = left;
            root.RightChild = right;

            if (left.Content.Count > 100)
            {
                Split(left);
                Split(right);
            }
            
            return;
        }
        
        else
        {
            root.Content.Sort((x, y) => x.Lon.CompareTo(y.Lon));
            
            var median = root.Content.Count / 2;
            var left = new TreeNode(root.MinLat, root.Content[0].Lon, root.MaxLat, root.Content[median].Lon, "x");
            left.Content = root.Content.Take(median).ToList();
            left.Parent = root;
            var right = new TreeNode(root.MinLat, root.Content[median + 1].Lon, root.MaxLat, root.Content[median + 1].Lon, "x");
            right.Content = root.Content.Skip(median).ToList();
            right.Parent = root;

            root.LeftChild = left;
            root.RightChild = right;
            
            if (left.Content.Count > 100)
            {
                Split(left);
                Split(right);
            }
            
            Console.WriteLine("finished");
            return;
        }
        
    }

    static List<Point> Search(TreeNode root, float lat, float lon, int radius)
    {
        var center = new Point(lat, lon);
        var result = new List<Point>();
        
        if (!root.Content.Contains(center))
        {
            return null;
        }

        else
        {
            if (root.LeftChild == null || root.RightChild == null)
            {
                foreach (var point in root.Content)
                {
                    if (Haversine(lat, point.Lat, lon, point.Lon) < radius)
                    {
                        result.Add(point);
                    }
                }
            }
            else if (root.LeftChild.Content.Contains(center))
            {
                result = Search(root.LeftChild, lat, lon, radius);
            }
            
            else if (root.RightChild.Content.Contains(center))
            {
                result = Search(root.RightChild, lat, lon, radius);
            }
        }

        return result;
    }

    static void Main(string[] args)
    {
        var LATITUDE = 0;
        var LONGITUDE = 1;
        var CATEGORY = 2;
        var TYPE = 3;
        var NAME = 4;
        var STREET = 5;
        var BUILDING = 6;
        var lat = new float();
        var lon = new float();
        var radius = new int();
        var path = "";

        foreach (var arg in args)
        {
            if (arg.Contains("--lat"))
            {
                lat = float.Parse(arg.Substring(6));
            }
            
            else if (arg.Contains("--long"))
            {
                lon = float.Parse(arg.Substring(7));
            }
            
            else if (arg.Contains("--size"))
            {
                radius = int.Parse(arg.Substring(7));
            }
            
            else if (arg.Contains("--db"))
            {
                path = arg.Substring(5);
            }
        }
        
        List<string[]> data = new List<string[]>();
        string[] lines = System.IO.File.ReadAllLines(path);
        for (var i = 0; i < lines.Length; i++)
        {
            var line = lines[i];
            string[] columns = line.Split(';');
            data.Add(columns);
        }

        float minLat = float.Parse(data[0][0]);
        float minLon = float.Parse(data[0][1]);
        float maxLat = float.Parse(data[0][0]);
        float maxLon = float.Parse(data[0][1]);

        for (var i = 0; i < data.Count; i++)
        {
            if (float.Parse(data[i][0]) < minLat)
            {
                minLat = float.Parse(data[i][0]);
            }
            
            else if (float.Parse(data[i][1]) < minLon)
            {
                minLon = float.Parse(data[i][1]);
            }
            
            else if (float.Parse(data[i][0]) > maxLat)
            {
                maxLat = float.Parse(data[i][0]);
            }
            
            else if (float.Parse(data[i][1]) > maxLon)
            {
                maxLon = float.Parse(data[i][1]);
            }
        }

        var root = new TreeNode(minLat, minLon, maxLat, maxLon, "x");

        for (var i = 0; i < data.Count; i++)
        {
            var point = new Point(float.Parse(data[i][0]), float.Parse(data[i][1]));
            point.Category = data[i][CATEGORY];
            point.Type = data[i][TYPE];
            point.Name = data[i][NAME];
            point.Street = data[i][STREET];
            point.Building = data[i][BUILDING];
            root.Content.Add(new Point(float.Parse(data[i][0]), float.Parse(data[i][1])));
        }
        
        Split(root);

        Console.ReadLine();
    }
}

//MapSearch.exe --db=ukraine_poi.csv --lat=30,212 --long=35,872 --size=2000

