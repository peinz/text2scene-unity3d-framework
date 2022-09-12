using System.Collections.Generic;

class OsmData {
    public OsmBounds Bounds { get; private set; }
    public Dictionary<ulong, OsmNode> Nodes { get; private set; }
    public Dictionary<ulong, OsmWay> Ways { get; private set; }
    public List<OsmRelation> Relations { get; private set; }

    public OsmData(
        OsmBounds bounds,
        Dictionary<ulong, OsmNode> nodes,
        Dictionary<ulong, OsmWay> ways,
        List<OsmRelation> relations
    )
    {
        Bounds = bounds;
        Nodes = nodes;
        Ways = ways;
        Relations = relations;
    }
}