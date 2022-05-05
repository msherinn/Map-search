namespace MapSearch;

public struct Point
{
    public float Lat { get; set; }
    public float Lon { get; set; }
    public string Category { get; set; }
    public string Type { get; set; }
    public string Name { get; set; }
    public string Street { get; set; }
    public string Building { get; set; }

    public Point(float lat, float lon)
    {
        Lat = lat;
        Lon = lon;
        Category = "";
        Type = "";
        Name = "";
        Street = "";
        Building = "";
    }
}