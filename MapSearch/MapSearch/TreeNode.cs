namespace MapSearch;

public class TreeNode
{
    public TreeNode? Parent { get; set; }
    public TreeNode? LeftChild { get; set; }
    public TreeNode? RightChild { get; set; }
    public List<Point> Content { get; set; }
    public string Axis { get; set; }
    public float MinLat { get; }
    public float MinLon { get; }
    public float MaxLat { get; }
    public float MaxLon { get; }

    public struct Coordinates
    {
        public static float MaxLat { get; set; }
        public static float MaxLon { get; set; }
        public static float MinLat { get; set; }
        public static float MinLon { get; set; }
    }

    public TreeNode(float minLat, float minLon, float maxLat, float maxLon, string axis)
    {
        Coordinates.MinLat = minLat;
        Coordinates.MinLon = minLon;
        Coordinates.MaxLat = maxLat;
        Coordinates.MaxLon = maxLon;
        Axis = axis;
        Content = new List<Point>();
    }
}