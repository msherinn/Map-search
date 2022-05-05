﻿using System.Linq;
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
        var sorted = new List<Point>();
        if (root.Axis == "x")
        {
            sorted.Add(root.Content[0]);
            for (var i = 1; i < root.Content.Count; i++)
            {
                var inserted = false;
                for (var j = 0; j < sorted.Count; j++)
                {
                    if (root.Content[i].Lat < sorted[j].Lat)
                    {
                        sorted.Insert(j, root.Content[i]);
                        inserted = true;
                    }
                }

                if (inserted == false)
                {
                    sorted.Add(root.Content[i]);
                }
            }

            var median = sorted.Count / 2;
            var left = new TreeNode(sorted[0].Lat, root.MinLon, sorted[median].Lat, root.MaxLon, "y");
            left.Content = sorted.Take(median).ToList();
            left.Parent = root;
            var right = new TreeNode(sorted[median + 1].Lat, root.MinLon, sorted[median + 1].Lat, root.MaxLon, "y");
            right.Content = sorted.Skip(median).ToList();
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
            sorted.Add(root.Content[0]);
            for (var i = 1; i < root.Content.Count; i++)
            {
                var inserted = false;
                for (var j = 0; j < sorted.Count; j++)
                {
                    if (root.Content[i].Lon < sorted[j].Lon)
                    {
                        sorted.Insert(j, root.Content[i]);
                        inserted = true;
                    }
                }

                if (inserted == false)
                {
                    sorted.Add(root.Content[i]);
                }
            }

            var median = sorted.Count / 2;
            var left = new TreeNode(root.MinLat, sorted[0].Lon, root.MaxLat, sorted[median].Lon, "x");
            left.Content = sorted.Take(median).ToList();
            left.Parent = root;
            var right = new TreeNode(root.MinLat, sorted[median + 1].Lon, root.MaxLat, sorted[median + 1].Lon, "x");
            right.Content = sorted.Skip(median).ToList();
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

        /*List<string[]> results = new List<string[]>();
        for (var i = 0; i < data.Count; i++)
        {
            var goalLat = float.Parse(data[i][0]);
            var goalLon = float.Parse(data[i][1]);
            var distance = Haversine(lat, goalLat, lon, goalLon);
            if (distance < radius)
            {
                results.Add(data[i]);
            }
        }

        var j = 1;
        foreach (var result in results)
        {
            Console.WriteLine(j + ". lat: " + result[LATITUDE] + ", lon: " + result[LONGITUDE] + ", category: " + result[CATEGORY]
                              + ", type: " + result[TYPE] + ", name: " + result[NAME] + ", street: " + result[STREET] + ", building: " + result[BUILDING]);
            j++;
        }*/

        Console.ReadLine();
    }
}

//MapSearch.exe --db=ukraine_poi.csv --lat=30,212 --long=35,872 --size=2000

