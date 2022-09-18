using System.Collections.Generic;
using UnityEngine;
using System.Xml;
using UnityEngine.SceneManagement;

// This software has been further expanded by Alen Smajic (2020).

/*
    Copyright (c) 2017 Sloan Kelly

    Permission is hereby granted, free of charge, to any person obtaining a copy
    of this software and associated documentation files (the "Software"), to deal
    in the Software without restriction, including without limitation the rights
    to use, copy, modify, merge, publish, distribute, sublicense, and/or sell
    copies of the Software, and to permit persons to whom the Software is
    furnished to do so, subject to the following conditions:

    The above copyright notice and this permission notice shall be included in all
    copies or substantial portions of the Software.

    THE SOFTWARE IS PROVIDED "AS IS", WITHOUT WARRANTY OF ANY KIND, EXPRESS OR
    IMPLIED, INCLUDING BUT NOT LIMITED TO THE WARRANTIES OF MERCHANTABILITY,
    FITNESS FOR A PARTICULAR PURPOSE AND NONINFRINGEMENT. IN NO EVENT SHALL THE
    AUTHORS OR COPYRIGHT HOLDERS BE LIABLE FOR ANY CLAIM, DAMAGES OR OTHER
    LIABILITY, WHETHER IN AN ACTION OF CONTRACT, TORT OR OTHERWISE, ARISING FROM,
    OUT OF OR IN CONNECTION WITH THE SOFTWARE OR THE USE OR OTHER DEALINGS IN THE
    SOFTWARE.
*/

/// <summary>
/// This script extracts the various data nodes from the XML file and generates
/// the respective instances based on the classes from the serialization folder.
/// The objects are then stored in dictionaries and lists to access these data
/// structures from other scripts.
/// </summary>
class MapReader
{
    public static OsmBounds bounds;

    public static Dictionary<ulong, OsmNode> nodes;

    public static Dictionary<ulong, OsmWay> ways;

    public static List<OsmRelation> relations;

    public OsmData ParseOsmData(string path)
    {
        nodes = new Dictionary<ulong, OsmNode>();
        ways = new Dictionary<ulong, OsmWay>();
        relations = new List<OsmRelation>();

        XmlDocument doc = new XmlDocument();
        try
        {
            // The XML file is being loaded and the data nodes are being extracted using
            // the classes from the serialization folder.
            // doc.Load(FileLoader.ResourceFilePath);
            doc.Load(path);
            SetBounds(doc.SelectSingleNode("/osm/bounds"));
            GetNodes(doc.SelectNodes("/osm/node"));
            GetWays(doc.SelectNodes("/osm/way"));
            GetRelations(doc.SelectNodes("/osm/relation"));

            return new OsmData(bounds, nodes, ways, relations);
        }

        // If the user input path to the XML file is not in XML format, the user is
        // being returned to the Main Menu with an error message.
        catch
        {
            return null;
        }
    }

    /// <summary>
    /// Extracts the boundary information from the XML file and generates the corresponding
    /// instance.
    /// </summary>
    /// <param name="xmlNode">XML node</param>
    void SetBounds(XmlNode xmlNode)
    {
        bounds = new OsmBounds(xmlNode);
    }

    /// <summary>
    /// Extracts the nodes information from the XML file and generates the corresponding
    /// instances.
    /// </summary>
    /// <param name="xmlNodeList">XML node</param>
    void GetNodes(XmlNodeList xmlNodeList)
    {
        foreach (XmlNode n in xmlNodeList)
        {
            OsmNode node = new OsmNode(n);
            nodes[node.ID] = node;
        }
    }

    /// <summary>
    /// Extracts the way information from the XML file and generates the corresponding
    /// instances.
    /// </summary>
    /// <param name="xmlNodeList">XML node</param>
    void GetWays(XmlNodeList xmlNodeList)
    {
        foreach (XmlNode node in xmlNodeList)
        {
            OsmWay way = new OsmWay(node);

            if (way.IsRailway == true)
            {
                ways[way.ID] = way;
            }
            else if (way.IsStreet == true)
            {
                ways[way.ID] = way;
            }
        }
    }

    /// <summary>
    /// Extract the relation information from the XML file and generates the corresponding
    /// instances.
    /// </summary>
    /// <param name="xmlNodeList">XML node</param>
    void GetRelations(XmlNodeList xmlNodeList)
    {
        foreach (XmlNode node in xmlNodeList)
        {
            OsmRelation relation = new OsmRelation(node);

            if (relation.Route == true)
            {
                relations.Add(relation);

                TagPublicTransportWays(relation);
                foreach (ulong NodeID in relation.StoppingNodeIDs)
                {
                    try
                    {
                        nodes[NodeID].TransportLines.Add(relation.Name);
                        relation.StationNames.Add(nodes[NodeID].StationName);
                    }
                    catch (KeyNotFoundException)
                    {
                        continue;
                    }
                }
            }
        }
    }

    /// <summary>
    /// If public transport roads and railroads are activated in the options menu,
    /// this function will extract the last data informations from these nodes.
    /// </summary>
    /// <param name="r">XML node</param>
    void TagPublicTransportWays(OsmRelation r)
    {
        foreach (ulong WayID in r.WayIDs)
        {
            try
            {
                switch (r.TransportType)
                {
                    case "subway":
                        ways[WayID].TransportLines.Add(r.Name);
                        ways[WayID].PublicTransportRailway = true;
                        ways[WayID].TransportTypes.Add("subway");
                        break;
                    case "tram":
                        ways[WayID].TransportLines.Add(r.Name);
                        ways[WayID].PublicTransportRailway = true;
                        ways[WayID].TransportTypes.Add("tram");
                        break;
                    case "train":
                        ways[WayID].TransportLines.Add(r.Name);
                        ways[WayID].PublicTransportRailway = true;
                        ways[WayID].TransportTypes.Add("train");
                        break;
                    case "railway":
                        ways[WayID].TransportLines.Add(r.Name);
                        ways[WayID].PublicTransportRailway = true;
                        ways[WayID].TransportTypes.Add("railway");
                        break;
                    case "light_rail":
                        ways[WayID].TransportLines.Add(r.Name);
                        ways[WayID].PublicTransportRailway = true;
                        ways[WayID].TransportTypes.Add("light_rail");
                        break;
                    case "bus":
                        ways[WayID].TransportLines.Add(r.Name);
                        ways[WayID].PublicTransportStreet = true;
                        ways[WayID].TransportTypes.Add("bus");
                        break;
                }
            }
            catch (KeyNotFoundException)
            {
                continue;
            }
        }
    }
}
