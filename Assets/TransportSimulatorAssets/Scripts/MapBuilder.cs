﻿using UnityEngine;
using System.Collections;
using System.Collections.Generic;
using UnityEngine.UI;

/*
    Copyright (c) 2020 Alen Smajic

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
/// This script generates all railroads, station and roads using the information
/// from OsmData. 
/// </summary>
class MapBuilder : MonoBehaviour
{
    // Here are the various public transport colors stored which are used
    // upon generating the roads and railroads.
    [System.Serializable]
    public class RouteEntry
    {
        public OsmRoute Type;
        public Material RouteMaterial;
        public Station StationPrefab;
    }
    public RouteEntry[] RouteMaterials;
    public static Material selected_way;

    OsmData osmData;
    Dictionary<ulong, Station> createdStations = new Dictionary<ulong, Station>();

    /// <summary>
    /// This script will visualize osmData
    /// </summary>
    /// <returns></returns>
    public IEnumerator BuildAllObjects(OsmData osmData)
    {
        this.osmData = osmData;

        for(int i=0; i<osmData.Relations.Count; i++){
            var relation = osmData.Relations[i];
            if(relation.Route != null){
                CreateStations(relation);
            }
        }

        yield return WayBuilder(); // Roads and railroads are being instantiated.
    }

    /// <summary>
    /// We iterate over the relation instances to generate the Station objects and
    /// the corresponding station UIs.
    /// </summary>
    /// <param name="r">relation instance</param>
    void CreateStations(OsmRelation r)
    {       
        foreach (ulong NodeID in r.StoppingNodeIDs)
        {
            if(!osmData.Nodes.ContainsKey(NodeID)){
                continue;
            }

            OsmNode stationNode = osmData.Nodes[NodeID];

            Station stationPrefab = null;
            foreach(var entry in RouteMaterials){
                if(r.Route == entry.Type){
                    stationPrefab = entry.StationPrefab;
                    break;
                }
            }

            if(stationPrefab == null) continue;

            Station station;
            if (createdStations.ContainsKey(NodeID)) station = createdStations[NodeID];
            else {
                station = Instantiate(stationPrefab);
                station.transform.position = stationNode - osmData.Bounds.Centre;
                station.SetName(stationNode.Name);

                createdStations.Add(NodeID, station);
            }

            station.AddTransportLines(stationNode.TransportLines);

        }
    }

    /// <summary>
    /// This function generates the road and railrooads. It also shows a loading
    /// screen with the progress percentage.
    /// </summary>
    /// <returns></returns>
    IEnumerator WayBuilder()
    {
        foreach (KeyValuePair<ulong, OsmWay> w in osmData.Ways)
        {
            Material material = null;
            foreach(var entry in RouteMaterials){
                if(w.Value.Routes.Contains(entry.Type)){
                    material = entry.RouteMaterial;
                    break;
                }
            }

            if(!material) continue;
                                             
            GameObject go = new GameObject();
            var waytext = go.AddComponent<Text>();
            foreach (string tramline in w.Value.TransportLines)
            {
                waytext.text += tramline + ", ";
            }
            Vector3 localOrigin = GetCentre(w.Value);
            go.transform.position = (localOrigin - osmData.Bounds.Centre) + Vector3.up*0.1f;


            MeshFilter mf = go.AddComponent<MeshFilter>();
            MeshRenderer mr = go.AddComponent<MeshRenderer>();

            mr.material = material;

            // Here we store the vectors, normales and indexes.
            List<Vector3> vectors = new List<Vector3>();  
            List<Vector3> normals = new List<Vector3>();
            List<int> indicies = new List<int>();

            for (int i = 1; i < w.Value.NodeIDs.Count; i++)
            {
                OsmNode p1 = osmData.Nodes[w.Value.NodeIDs[i - 1]];
                OsmNode p2 = osmData.Nodes[w.Value.NodeIDs[i]];

                Vector3 s1 = p1 - localOrigin;  
                Vector3 s2 = p2 - localOrigin;

                Vector3 diff = (s2 - s1).normalized;

                // The width of road and railroads is set to 1 meter.
                var cross = Vector3.Cross(diff, Vector3.up) * 1.0f; 

                Vector3 v1 = s1 + cross;
                Vector3 v2 = s1 - cross;
                Vector3 v3 = s2 + cross;
                Vector3 v4 = s2 - cross;

                vectors.Add(v1);  
                vectors.Add(v2);
                vectors.Add(v3);
                vectors.Add(v4);

                normals.Add(Vector3.up);  
                normals.Add(Vector3.up);
                normals.Add(Vector3.up);
                normals.Add(Vector3.up);

                int idx1, idx2, idx3, idx4;
                idx4 = vectors.Count - 1;
                idx3 = vectors.Count - 2;
                idx2 = vectors.Count - 3;
                idx1 = vectors.Count - 4;

                indicies.Add(idx1);
                indicies.Add(idx3);
                indicies.Add(idx2);

                indicies.Add(idx3);
                indicies.Add(idx4);
                indicies.Add(idx2);
            }
            go.name += w.Value.ID;
            mf.mesh.vertices = vectors.ToArray();
            mf.mesh.normals = normals.ToArray();
            mf.mesh.triangles = indicies.ToArray();

            yield return null;

            // Lastly we store the Unity coordinates of every generated way. This is then used later on
            // when we want to move the transport vehicles across the ways.
            for(int i = 0; i < w.Value.NodeIDs.Count; i++)
            {
                OsmNode p1 = osmData.Nodes[w.Value.NodeIDs[i]];
                w.Value.UnityCoordinates.Add(p1 - osmData.Bounds.Centre);
            }
        }
    }

    /// <summary>
    /// Returns the center point of an object. This information is being used as the reference
    /// for placing the object inside the Unity world.
    /// </summary>
    /// <param name="way">way instance</param>
    /// <returns></returns>
    protected Vector3 GetCentre(OsmWay way)  
    {
        Vector3 total = Vector3.zero;

        foreach (var id in way.NodeIDs)
        {
            total += osmData.Nodes[id];  
        }
        return total / way.NodeIDs.Count;
    }  
}