﻿using OpenTK.Graphics.OpenGL;
using System;
using System.Collections.Generic;

namespace AgOpenGPS
{
    public partial class CPlot
    {
        // the list of possible bounds points
        public List<vec4> turnClosestList = new List<vec4>();

        public int turnSelected, closestTurnNum;

        //point at the farthest turn segment from pivotAxle
        public vec3 closestTurnPt = new vec3(-10000, -10000, 9);

        public void FindClosestTurnPoint(bool isYouTurnRight, vec3 fromPt, double headAB)
        {
            //initial scan is straight ahead of pivot point of vehicle to find the right turnLine/boundary
            vec3 pt = new vec3();
            vec3 rayPt = new vec3();

            bool isFound = false;
            int closestTurnNum = 99;

            double cosHead = Math.Cos(headAB);
            double sinHead = Math.Sin(headAB);

            for (int b = 1; b < mf.maxCrossFieldLength; b += 2)
            {
                pt.easting = fromPt.easting + (sinHead * b);
                pt.northing = fromPt.northing + (cosHead * b);

                if (plots[0].IsPointInTurnWorkArea(pt))
                {
                    for (int t = 1; t < plots.Count; t++)
                    {
                        if (plots[t].isDriveThru) continue;
                        if (plots[t].isDriveAround) continue;
                        if (plots[t].IsPointInTurnWorkArea(pt))
                        {
                            isFound = true;
                            closestTurnNum = t;
                            rayPt.easting = pt.easting;
                            rayPt.northing = pt.northing;
                            break;
                        }
                    }
                    if (isFound) break;
                }
                else
                {
                    closestTurnNum = 0;
                    rayPt.easting = pt.easting;
                    rayPt.northing = pt.northing;
                    break;
                }
            }

            //second scan is straight ahead of outside of tool based on turn direction
            double scanWidthL, scanWidthR;
            if (isYouTurnRight) //its actually left
            {
                scanWidthL = -(mf.tool.toolWidth * 0.25) - (mf.tool.toolWidth * 0.5);
                scanWidthR = (mf.tool.toolWidth * 0.25) - (mf.tool.toolWidth * 0.5);
            }
            else
            {
                scanWidthL = -(mf.tool.toolWidth * 0.25) + (mf.tool.toolWidth * 0.5);
                scanWidthR = (mf.tool.toolWidth * 0.25) + (mf.tool.toolWidth * 0.5);
            }

            //isYouTurnRight actuall means turning left - Painful, but it switches later
            boxA.easting = fromPt.easting + (Math.Sin(headAB + glm.PIBy2) * scanWidthL);
            boxA.northing = fromPt.northing + (Math.Cos(headAB + glm.PIBy2) * scanWidthL);

            boxB.easting = fromPt.easting + (Math.Sin(headAB + glm.PIBy2) * scanWidthR);
            boxB.northing = fromPt.northing + (Math.Cos(headAB + glm.PIBy2) * scanWidthR);

            boxC.easting = boxB.easting + (Math.Sin(headAB) * boxLength);
            boxC.northing = boxB.northing + (Math.Cos(headAB) * boxLength);

            boxD.easting = boxA.easting + (Math.Sin(headAB) * boxLength);
            boxD.northing = boxA.northing + (Math.Cos(headAB) * boxLength);

            //determine if point is inside bounding box of the 1 turn chosen above
            turnClosestList.Clear();

            vec4 inBox;

            int ptCount = plots[closestTurnNum].turnLine.Count;
            for (int p = 0; p < ptCount; p++)
            {
                if ((((boxB.easting - boxA.easting) * (plots[closestTurnNum].turnLine[p].northing - boxA.northing))
                        - ((boxB.northing - boxA.northing) * (plots[closestTurnNum].turnLine[p].easting - boxA.easting))) < 0) { continue; }

                if ((((boxD.easting - boxC.easting) * (plots[closestTurnNum].turnLine[p].northing - boxC.northing))
                        - ((boxD.northing - boxC.northing) * (plots[closestTurnNum].turnLine[p].easting - boxC.easting))) < 0) { continue; }

                if ((((boxC.easting - boxB.easting) * (plots[closestTurnNum].turnLine[p].northing - boxB.northing))
                        - ((boxC.northing - boxB.northing) * (plots[closestTurnNum].turnLine[p].easting - boxB.easting))) < 0) { continue; }

                if ((((boxA.easting - boxD.easting) * (plots[closestTurnNum].turnLine[p].northing - boxD.northing))
                        - ((boxA.northing - boxD.northing) * (plots[closestTurnNum].turnLine[p].easting - boxD.easting))) < 0) { continue; }

                //it's in the box, so add to list
                inBox.easting = plots[closestTurnNum].turnLine[p].easting;
                inBox.northing = plots[closestTurnNum].turnLine[p].northing;
                inBox.heading = plots[closestTurnNum].turnLine[p].heading;
                inBox.index = closestTurnNum;

                //which turn/headland is it from
                turnClosestList.Add(inBox);
            }

            if (turnClosestList.Count == 0)
            {
                if (isYouTurnRight) //its actually left
                {
                    scanWidthL = -(mf.tool.toolWidth * 0.5);
                    scanWidthR = 0;
                }
                else
                {
                    scanWidthL = 0;
                    scanWidthR = (mf.tool.toolWidth * 0.5);
                }

                //isYouTurnRight actuall means turning left - Painful, but it switches later
                boxA.easting = fromPt.easting + (Math.Sin(headAB + glm.PIBy2) * scanWidthL);
                boxA.northing = fromPt.northing + (Math.Cos(headAB + glm.PIBy2) * scanWidthL);

                boxB.easting = fromPt.easting + (Math.Sin(headAB + glm.PIBy2) * scanWidthR);
                boxB.northing = fromPt.northing + (Math.Cos(headAB + glm.PIBy2) * scanWidthR);

                boxC.easting = boxB.easting + (Math.Sin(headAB) * boxLength);
                boxC.northing = boxB.northing + (Math.Cos(headAB) * boxLength);

                boxD.easting = boxA.easting + (Math.Sin(headAB) * boxLength);
                boxD.northing = boxA.northing + (Math.Cos(headAB) * boxLength);

                //determine if point is inside bounding box of the 1 turn chosen above
                turnClosestList.Clear();

                ptCount = plots[closestTurnNum].turnLine.Count;

                for (int p = 0; p < ptCount; p++)
                {
                    if ((((boxB.easting - boxA.easting) * (plots[closestTurnNum].turnLine[p].northing - boxA.northing))
                            - ((boxB.northing - boxA.northing) * (plots[closestTurnNum].turnLine[p].easting - boxA.easting))) < 0) { continue; }

                    if ((((boxD.easting - boxC.easting) * (plots[closestTurnNum].turnLine[p].northing - boxC.northing))
                            - ((boxD.northing - boxC.northing) * (plots[closestTurnNum].turnLine[p].easting - boxC.easting))) < 0) { continue; }

                    if ((((boxC.easting - boxB.easting) * (plots[closestTurnNum].turnLine[p].northing - boxB.northing))
                            - ((boxC.northing - boxB.northing) * (plots[closestTurnNum].turnLine[p].easting - boxB.easting))) < 0) { continue; }

                    if ((((boxA.easting - boxD.easting) * (plots[closestTurnNum].turnLine[p].northing - boxD.northing))
                            - ((boxA.northing - boxD.northing) * (plots[closestTurnNum].turnLine[p].easting - boxD.easting))) < 0) { continue; }

                    //it's in the box, so add to list
                    inBox.easting = plots[closestTurnNum].turnLine[p].easting;
                    inBox.northing = plots[closestTurnNum].turnLine[p].northing;
                    inBox.heading = plots[closestTurnNum].turnLine[p].heading;
                    inBox.index = closestTurnNum;

                    //which turn/headland is it from
                    turnClosestList.Add(inBox);
                }
            }
            //which of the points is closest
            //closestTurnPt.easting = -20000; closestTurnPt.northing = -20000;
            ptCount = turnClosestList.Count;
            if (ptCount != 0)
            {
                double totalDist = 0.75 * Math.Sqrt(((fromPt.easting - rayPt.easting) * (fromPt.easting - rayPt.easting))
                + ((fromPt.northing - rayPt.northing) * (fromPt.northing - rayPt.northing)));

                //determine closest point
                double minDistance = 9999999;
                for (int i = 0; i < ptCount; i++)
                {
                    double dist = Math.Sqrt(((fromPt.easting - turnClosestList[i].easting) * (fromPt.easting - turnClosestList[i].easting))
                                    + ((fromPt.northing - turnClosestList[i].northing) * (fromPt.northing - turnClosestList[i].northing)));

                    //double distRay = ((rayPt.easting - turnClosestList[i].easting) * (rayPt.easting - turnClosestList[i].easting))
                    //                + ((rayPt.northing - turnClosestList[i].northing) * (rayPt.northing - turnClosestList[i].northing));

                    if (minDistance >= dist && dist > totalDist)
                    {
                        minDistance = dist;
                        closestTurnPt.easting = turnClosestList[i].easting;
                        closestTurnPt.northing = turnClosestList[i].northing;
                        closestTurnPt.heading = turnClosestList[i].heading;
                    }
                }
                if (closestTurnPt.heading < 0) closestTurnPt.heading += glm.twoPI;
            }
        }

        public void BuildTurnLines()
        {
            //update the GUI values for boundaries
            mf.fd.UpdateFieldBoundaryGUIAreas();

            if (plots.Count == 0)
            {
                //mf.TimedMessageBox(1500, " No Boundaries", "No Turn Lines Made");
                return;
            }

            //to fill the list of line points
            vec3 point = new vec3();

            //determine how wide a headland space
            double totalHeadWidth = mf.yt.uturnDistanceFromBoundary;

            //inside boundaries
            for (int j = 0; j < plots.Count; j++)
            {
                plots[j].turnLine.Clear();
                if (plots[j].isDriveThru || plots[j].isDriveAround) continue;

                int ptCount = plots[j].bndLine.Count;

                for (int i = ptCount - 1; i >= 0; i--)
                {
                    //calculate the point outside the boundary
                    point.easting = plots[j].bndLine[i].easting + (-Math.Sin(glm.PIBy2 + plots[j].bndLine[i].heading) * totalHeadWidth);
                    point.northing = plots[j].bndLine[i].northing + (-Math.Cos(glm.PIBy2 + plots[j].bndLine[i].heading) * totalHeadWidth);
                    point.heading = plots[j].bndLine[i].heading;
                    if (point.heading < -glm.twoPI) point.heading += glm.twoPI;

                    //only add if outside actual field boundary
                    if (j == 0 == plots[j].IsPointInsideBoundaryEar(point))
                    {
                        vec3 tPnt = new vec3(point.easting, point.northing, point.heading);
                        plots[j].turnLine.Add(tPnt);
                    }
                }
                plots[j].FixTurnLine(totalHeadWidth, mf.tool.toolWidth * 0.33);
            }
        }

        public void DrawTurnLines()
        {
            GL.LineWidth(mf.ABLine.lineWidth);
            GL.Color3(0.3555f, 0.6232f, 0.20f);
            //GL.PointSize(2);

            for (int i = 0; i < plots.Count; i++)
            {
                if (plots[i].isDriveAround) continue;
                //turnArr[i].DrawTurnLine();
                {
                    ////draw the turn line oject
                    int ptCount = plots[i].turnLine.Count;
                    if (ptCount < 1) continue;

                    GL.Begin(PrimitiveType.LineLoop);
                    for (int h = 0; h < ptCount; h++) GL.Vertex3(plots[i].turnLine[h].easting, plots[i].turnLine[h].northing, 0);
                    GL.End();
                }
            }
        }
    }
}