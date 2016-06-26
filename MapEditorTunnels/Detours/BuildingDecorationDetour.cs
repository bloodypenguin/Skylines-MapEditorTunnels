using ColossalFramework;
using ColossalFramework.Math;
using MapEditorTunnels.Redirection;
using UnityEngine;

namespace MapEditorTunnels.Detours
{
    [TargetType(typeof(BuildingDecoration))]
    public class BuildingDecorationDetour
    {

        [RedirectReverse]
        private static bool FindConnectNode(FastList<ushort> buffer, Vector3 pos, NetInfo info2,
            out NetTool.ControlPoint point)
        {
            UnityEngine.Debug.Log("Failed to detour FindConnectNode()");
            point = new NetTool.ControlPoint();
            return false;
        }

        [RedirectReverse]
        private static bool RequireFixedHeight(BuildingInfo buildingInfo, NetInfo info2, Vector3 pos)
        {
            UnityEngine.Debug.Log("Failed to detour RequireFixedHeight()");
            return false;
        }


        [RedirectMethod]
        public static void LoadPaths(BuildingInfo info, ushort buildingID, ref Building data, float elevation)
        {
            if (info.m_paths == null)
                return;
            NetManager instance = Singleton<NetManager>.instance;
            instance.m_tempNodeBuffer.Clear();
            instance.m_tempSegmentBuffer.Clear();
            for (int index1 = 0; index1 < info.m_paths.Length; ++index1)
            {
                BuildingInfo.PathInfo pathInfo = info.m_paths[index1];
                if (pathInfo.m_netInfo != null && pathInfo.m_nodes != null && pathInfo.m_nodes.Length != 0)
                {
                    Vector3 position = data.CalculatePosition(pathInfo.m_nodes[0]);
                    bool flag1 = RequireFixedHeight(info, pathInfo.m_netInfo, pathInfo.m_nodes[0]);
                    if (!flag1)
                        position.y = NetSegment.SampleTerrainHeight(pathInfo.m_netInfo, position, false, pathInfo.m_nodes[0].y + elevation);
                    Ray ray = new Ray(position + new Vector3(0.0f, 8f, 0.0f), Vector3.down);
                    NetTool.ControlPoint controlPoint;
                    if (!FindConnectNode(instance.m_tempNodeBuffer, position, pathInfo.m_netInfo, out controlPoint))
                    {
                        if (NetTool.MakeControlPoint(ray, 16f, pathInfo.m_netInfo, true, NetNode.Flags.Untouchable, NetSegment.Flags.Untouchable, Building.Flags.All, pathInfo.m_nodes[0].y + elevation - pathInfo.m_netInfo.m_buildHeight, true, out controlPoint))
                        {
                            Vector3 vector3 = controlPoint.m_position - position;
                            if (!flag1)
                                vector3.y = 0.0f;
                            if ((double)vector3.sqrMagnitude > (double)pathInfo.m_maxSnapDistance * (double)pathInfo.m_maxSnapDistance)
                            {
                                controlPoint.m_position = position;
                                controlPoint.m_elevation = 0.0f;
                                controlPoint.m_node = (ushort)0;
                                controlPoint.m_segment = (ushort)0;
                            }
                            else
                                controlPoint.m_position.y = position.y;
                        }
                        else
                            controlPoint.m_position = position;
                    }
                    if ((int)controlPoint.m_node != 0)
                    {
                        instance.m_tempNodeBuffer.Add(controlPoint.m_node);
                    }
                    else
                    {
                        ushort node;
                        ushort segment;
                        int cost;
                        int productionRate;
                        if (NetTool.CreateNode(pathInfo.m_netInfo, controlPoint, controlPoint, controlPoint, NetTool.m_nodePositionsSimulation, 0, false, false, false, false, pathInfo.m_invertSegments, false, (ushort)0, out node, out segment, out cost, out productionRate) == ToolBase.ToolErrors.None)
                        {
                            instance.m_tempNodeBuffer.Add(node);
                            controlPoint.m_node = node;
                            if (pathInfo.m_forbidLaneConnection != null && pathInfo.m_forbidLaneConnection.Length > 0 && pathInfo.m_forbidLaneConnection[0])
                                instance.m_nodes.m_buffer[(int)node].m_flags |= NetNode.Flags.ForbidLaneConnection;
                            //begin mod
                            if (pathInfo.m_netInfo.m_netAI.IsUnderground())
                            {
                                NetManager.instance.m_nodes.m_buffer[node].m_flags |= NetNode.Flags.Underground;
                                NetManager.instance.m_nodes.m_buffer[node].m_flags &= ~NetNode.Flags.OnGround;
                            }
                            //end mod
                        }
                    }
                    for (int index2 = 1; index2 < pathInfo.m_nodes.Length; ++index2)
                    {
                        position = data.CalculatePosition(pathInfo.m_nodes[index2]);
                        bool flag2 = RequireFixedHeight(info, pathInfo.m_netInfo, pathInfo.m_nodes[index2]);
                        if (!flag2)
                            position.y = NetSegment.SampleTerrainHeight(pathInfo.m_netInfo, position, false, pathInfo.m_nodes[index2].y + elevation);
                        ray = new Ray(position + new Vector3(0.0f, 8f, 0.0f), Vector3.down);
                        NetTool.ControlPoint endPoint;
                        if (!FindConnectNode(instance.m_tempNodeBuffer, position, pathInfo.m_netInfo, out endPoint))
                        {
                            if (NetTool.MakeControlPoint(ray, 16f, pathInfo.m_netInfo, true, NetNode.Flags.Untouchable, NetSegment.Flags.Untouchable, Building.Flags.All, pathInfo.m_nodes[index2].y + elevation - pathInfo.m_netInfo.m_buildHeight, true, out endPoint))
                            {
                                Vector3 vector3 = endPoint.m_position - position;
                                if (!flag2)
                                    vector3.y = 0.0f;
                                if ((double)vector3.sqrMagnitude > (double)pathInfo.m_maxSnapDistance * (double)pathInfo.m_maxSnapDistance)
                                {
                                    endPoint.m_position = position;
                                    endPoint.m_elevation = 0.0f;
                                    endPoint.m_node = (ushort)0;
                                    endPoint.m_segment = (ushort)0;
                                }
                                else
                                    endPoint.m_position.y = position.y;
                            }
                            else
                                endPoint.m_position = position;
                        }
                        NetTool.ControlPoint middlePoint = endPoint;
                        if (pathInfo.m_curveTargets != null && pathInfo.m_curveTargets.Length >= index2)
                        {
                            middlePoint.m_position = data.CalculatePosition(pathInfo.m_curveTargets[index2 - 1]);
                            if (!flag1 || !flag2)
                                middlePoint.m_position.y = NetSegment.SampleTerrainHeight(pathInfo.m_netInfo, middlePoint.m_position, false, pathInfo.m_curveTargets[index2 - 1].y + elevation);
                        }
                        else
                            middlePoint.m_position = (controlPoint.m_position + endPoint.m_position) * 0.5f;
                        middlePoint.m_direction = VectorUtils.NormalizeXZ(middlePoint.m_position - controlPoint.m_position);
                        endPoint.m_direction = VectorUtils.NormalizeXZ(endPoint.m_position - middlePoint.m_position);
                        ushort firstNode;
                        ushort lastNode;
                        ushort segment;
                        int cost;
                        int productionRate;
                        if (NetTool.CreateNode(pathInfo.m_netInfo, controlPoint, middlePoint, endPoint, NetTool.m_nodePositionsSimulation, 1, false, false, false, false, false, pathInfo.m_invertSegments, false, (ushort)0, out firstNode, out lastNode, out segment, out cost, out productionRate) == ToolBase.ToolErrors.None)
                        {
                            instance.m_tempNodeBuffer.Add(lastNode);
                            instance.m_tempSegmentBuffer.Add(segment);
                            endPoint.m_node = lastNode;
                            if (pathInfo.m_forbidLaneConnection != null && pathInfo.m_forbidLaneConnection.Length > index2 && pathInfo.m_forbidLaneConnection[index2])
                                instance.m_nodes.m_buffer[(int)lastNode].m_flags |= NetNode.Flags.ForbidLaneConnection;
                            //begin mod
                            if (pathInfo.m_netInfo.m_netAI.IsUnderground())
                            {
                                NetManager.instance.m_nodes.m_buffer[firstNode].m_flags |= NetNode.Flags.Underground;
                                NetManager.instance.m_nodes.m_buffer[firstNode].m_flags &= ~NetNode.Flags.OnGround;
                            }
                            if (pathInfo.m_netInfo.m_netAI.IsUnderground())
                            {
                                NetManager.instance.m_nodes.m_buffer[lastNode].m_flags |= NetNode.Flags.Underground;
                                NetManager.instance.m_nodes.m_buffer[lastNode].m_flags &= ~NetNode.Flags.OnGround;
                            }
                            //end mod

                        }
                        controlPoint = endPoint;
                        flag1 = flag2;
                    }
                }
            }
            for (int index = 0; index < instance.m_tempNodeBuffer.m_size; ++index)
            {
                ushort node = instance.m_tempNodeBuffer.m_buffer[index];
                if ((instance.m_nodes.m_buffer[(int)node].m_flags & NetNode.Flags.Untouchable) == NetNode.Flags.None)
                {
                    if ((int)buildingID != 0)
                    {
                        if ((data.m_flags & Building.Flags.Active) == Building.Flags.None && instance.m_nodes.m_buffer[(int)node].Info.m_canDisable)
                            instance.m_nodes.m_buffer[(int)node].m_flags |= NetNode.Flags.Disabled;
                        instance.m_nodes.m_buffer[(int)node].m_flags |= NetNode.Flags.Untouchable;
                        instance.UpdateNode(node);
                        instance.m_nodes.m_buffer[(int)node].m_nextBuildingNode = data.m_netNode;
                        data.m_netNode = node;
                    }
                    else
                        instance.UpdateNode(node);
                }
            }
            for (int index = 0; index < instance.m_tempSegmentBuffer.m_size; ++index)
            {
                ushort segment = instance.m_tempSegmentBuffer.m_buffer[index];
                if ((instance.m_segments.m_buffer[(int)segment].m_flags & NetSegment.Flags.Untouchable) == NetSegment.Flags.None)
                {
                    if ((int)buildingID != 0)
                    {
                        instance.m_segments.m_buffer[(int)segment].m_flags |= NetSegment.Flags.Untouchable;
                        instance.UpdateSegment(segment);
                    }
                    else
                    {
                        if ((Singleton<ToolManager>.instance.m_properties.m_mode & ItemClass.Availability.AssetEditor) != ItemClass.Availability.None && (instance.m_segments.m_buffer[(int)segment].Info.m_availableIn & ItemClass.Availability.AssetEditor) == ItemClass.Availability.None)
                            instance.m_segments.m_buffer[(int)segment].m_flags |= NetSegment.Flags.Untouchable;
                        instance.UpdateSegment(segment);
                    }
                }
            }
            instance.m_tempNodeBuffer.Clear();
            instance.m_tempSegmentBuffer.Clear();
        }
    }
}