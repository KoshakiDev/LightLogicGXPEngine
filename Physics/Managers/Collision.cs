using System.Collections.Generic;

namespace GXPEngine.PhysicsCore
{
    public class Collision
    {
        public Collision() { }

        public void CheckAllFor(Vec2 point, out CollisionData[] collisionDatas)
        {
            Collider[] colliders = new Collider[Physics.Colliders.Count];
            Physics.Colliders.CopyTo(colliders);
            List<CollisionData> collisionList = new List<CollisionData>();
            foreach (Collider other in colliders)
            {
                if (!ValidateExpediency(point, other)) continue;
                Check(other, point, out CollisionData collisionData);
                if (collisionData.isEmpty) continue;
                Physics.OnCollision(collisionData);
                collisionList.Add(collisionData);
            }
            collisionDatas = collisionList.ToArray();
        }
        public void CheckAllFor(Collider self, out CollisionData[] collisionDatas)
        {
            Collider[] colliders = new Collider[Physics.Colliders.Count];
            Physics.Colliders.CopyTo(colliders);
            List<CollisionData> collisionList = new List<CollisionData>();
            foreach (Collider other in colliders)
            {
                if (self == other) continue;
                if (self.Owner.ExcludedLayerMasks.Contains(other.Owner.LayerMask)) continue;
                if (!ValidateExpediency(self, other)) continue;
                Check(self, other, out CollisionData collisionData);
                if (collisionData.isEmpty) continue;
                Physics.OnCollision(collisionData);
                collisionList.Add(collisionData);
            }
            collisionDatas = collisionList.ToArray();

            if (self.Owner != null)
                return;

            if (!Physics.CurrentCollisions.ContainsKey(self.Owner))
                Physics.CurrentCollisions.Add(self.Owner, collisionDatas);
        }
        public void CheckAllFor(Collider self, bool triggers, out CollisionData[] collisionDatas)
        {
            Collider[] colliders = new Collider[Physics.Colliders.Count];
            Physics.Colliders.CopyTo(colliders);
            List<CollisionData> collisionList = new List<CollisionData>();
            foreach (Collider other in colliders)
            {
                if (self == other) continue;
                if (other.IsTrigger != triggers) continue;
                if (!ValidateExpediency(self, other)) continue;
                if (self.Owner.ExcludedLayerMasks.Contains(other.Owner.LayerMask)) continue;
                Check(self, other, out CollisionData collisionData);
                if (collisionData.isEmpty) continue;
                Physics.OnCollision(collisionData);
                collisionList.Add(collisionData);
            }
            collisionDatas = collisionList.ToArray();

            if (self.Owner != null)
                return;

            if (!Physics.CurrentCollisions.ContainsKey(self.Owner))
                Physics.CurrentCollisions.Add(self.Owner, collisionDatas);
        }
        public void CheckAllFor(Collider self, bool triggers, string layerMask, out CollisionData[] collisionDatas)
        {
            Collider[] colliders = new Collider[Physics.Colliders.Count];
            Physics.Colliders.CopyTo(colliders);
            List<CollisionData> collisionList = new List<CollisionData>();
            foreach (Collider other in colliders)
            {
                if (self == other) continue;
                if (other.IsTrigger != triggers) continue;
                if (self.Owner.ExcludedLayerMasks.Contains(other.Owner.LayerMask)) continue;
                if (!other.Owner.CompareLayerMask(layerMask)) continue;
                if (!ValidateExpediency(self, other)) continue;
                Check(self, other, out CollisionData collisionData);
                if (collisionData.isEmpty) continue;
                Physics.OnCollision(collisionData);
                collisionList.Add(collisionData);
            }
            collisionDatas = collisionList.ToArray();

            if (self.Owner != null)
                return;

            if (!Physics.CurrentCollisions.ContainsKey(self.Owner))
                Physics.CurrentCollisions.Add(self.Owner, collisionDatas);
        }
        public void CheckAllFor(Collider self, bool triggers, string[] layerMasks, out CollisionData[] collisionDatas)
        {
            Collider[] colliders = new Collider[Physics.Colliders.Count];
            Physics.Colliders.CopyTo(colliders);
            List<CollisionData> collisionList = new List<CollisionData>();
            foreach (Collider other in colliders)
            {
                if (self == other) continue;
                if (other.IsTrigger != triggers) continue;
                if (self.Owner.ExcludedLayerMasks.Contains(other.Owner.LayerMask)) continue;
                bool inLayerMasks = false;
                foreach (string layer in layerMasks)
                    if (other.Owner.CompareLayerMask(layer))
                    {
                        inLayerMasks = true;
                        break;
                    }
                if (!inLayerMasks) continue;
                if (!ValidateExpediency(self, other)) continue;
                Check(self, other, out CollisionData collisionData);
                if (collisionData.isEmpty) continue;
                Physics.OnCollision(collisionData);
                collisionList.Add(collisionData);
            }
            collisionDatas = collisionList.ToArray();

            if (self.Owner != null)
                return;

            if (!Physics.CurrentCollisions.ContainsKey(self.Owner))
                Physics.CurrentCollisions.Add(self.Owner, collisionDatas);
        }

        public void Check(Collider self, Collider other, out CollisionData collisionData)
        {
            string selfTypeName = self.GetType().Name;
            string otherTypeName = other.GetType().Name;
            checkForConvex(self, ref selfTypeName);
            checkForConvex(other, ref otherTypeName);

            switch (selfTypeName + " & " + otherTypeName)
            {
                case ("PolygonCollider & PolygonCollider"):
                    collisionData = PolygonToPolygon(self as PolygonCollider, other as PolygonCollider);
                    break;
                case ("CircleCollider & PolygonCollider"):
                    collisionData = CircleToPoly(self as CircleCollider, other as PolygonCollider);
                    break;
                case ("PolygonCollider & CircleCollider"):
                    collisionData = CircleToPoly(other as CircleCollider, self as PolygonCollider);
                    break;
                case ("CircleCollider & CircleCollider"):
                    collisionData = CircleToCircle(self as CircleCollider, other as CircleCollider);
                    break;
                case ("PolygonCollider & LineCollider"):
                    collisionData = PolygonToLine(self as PolygonCollider, other as LineCollider);
                    break;
                case ("LineCollider & PolygonCollider"):
                    collisionData = PolygonToLine(self as PolygonCollider, other as LineCollider);
                    break;
                case ("CircleCollider & LineCollider"):
                    collisionData = LineToCircle(other as LineCollider, self as CircleCollider);
                    break;
                case ("LineCollider & CircleCollider"):
                    collisionData = LineToCircle(self as LineCollider, other as CircleCollider);
                    break;
                case ("LineCollider & LineCollider"):
                    collisionData = LineToLine(self as LineCollider, other as LineCollider);
                    break;
                default:
                    collisionData = CollisionData.Empty;
                    break;
            }
            void checkForConvex(Collider collider, ref string typeName)
            {
                if (collider is RectCollider)
                    typeName = typeof(PolygonCollider).Name;
            }
        }
        public void Check(Collider self, Vec2 other, out CollisionData collisionData)
        {
            string selfTypeName = self.GetType().Name;
            checkForConvex(self, ref selfTypeName);

            switch (selfTypeName)
            {
                case ("PolygonCollider"):
                    collisionData = new CollisionData(PolygonPoint((self as PolygonCollider).TransformedPoints, other.x, other.y), self.Owner);
                    break;
                case ("CircleCollider"):
                    collisionData = CircleToPoint(self as CircleCollider, other);
                    break;
                case ("LineCollider"):
                    collisionData = LineToPoint(self as LineCollider, other);
                    break;
                default:
                    collisionData = CollisionData.Empty;
                    break;
            }
            void checkForConvex(Collider collider, ref string typeName)
            {
                if (collider is RectCollider)
                    typeName = typeof(PolygonCollider).Name;
            }
        }

        private bool ValidateExpediency(Collider self, Collider other)
        {
            self.Owner.TryGetComponent(typeof(Rigidbody), out Component selfComponent);
            other.Owner.TryGetComponent(typeof(Rigidbody), out Component otherComponent);

            float selfPredictedActiveRadius = self.ActiveRadius + 5 + (selfComponent is null ? 0 : (selfComponent as Rigidbody).ActualVelocity.length);
            float otherPredictedActiveRadius = other.ActiveRadius + 5 + (otherComponent is null ? 0 : (otherComponent as Rigidbody).ActualVelocity.length);

            bool isExpedient = Vec2.Distance(self.TransformedLogicalCenterOfMass, other.TransformedLogicalCenterOfMass) < selfPredictedActiveRadius + otherPredictedActiveRadius;

            if (Settings.CollisionDebug)
            {
                self.Owner.Rigidbody.AddCollisionDebugToStack(() =>
                {
                    Settings.ColliderDebug.Stroke(75, 0, 75);
                    Settings.ColliderDebug.Stroke(75, 0, 75);
                    Settings.ColliderDebug.Ellipse
                    (
                        self.TransformedLogicalCenterOfMass.x + Camera.Position.x,
                        self.TransformedLogicalCenterOfMass.y + Camera.Position.y,
                        selfPredictedActiveRadius * 2,
                        selfPredictedActiveRadius * 2
                    );
                    Settings.ColliderDebug.Ellipse
                    (
                        other.TransformedLogicalCenterOfMass.x + Camera.Position.x,
                        other.TransformedLogicalCenterOfMass.y + Camera.Position.y,
                        otherPredictedActiveRadius * 2,
                        otherPredictedActiveRadius * 2
                    );
                    if (isExpedient)
                    {
                        Settings.ColliderDebug.Polygon
                        (
                            other.TransformedLogicalCenterOfMass.x + Camera.Position.x,
                            other.TransformedLogicalCenterOfMass.y + Camera.Position.y,
                            self.TransformedLogicalCenterOfMass.x + Camera.Position.x,
                            self.TransformedLogicalCenterOfMass.y + Camera.Position.y
                        );
                    }
                });
            }

            return isExpedient;
        }
        private bool ValidateExpediency(Vec2 point, Collider other, float activeRadius = 5)
        {
            other.Owner.TryGetComponent(typeof(Rigidbody), out Component otherComponent);
            float otherPredictedActiveRadius = other.ActiveRadius + activeRadius + (otherComponent is null ? 0 : (otherComponent as Rigidbody).ActualVelocity.length);
            return Vec2.Distance(point, other.TransformedLogicalCenterOfMass) < otherPredictedActiveRadius;
        }

        private CollisionData PolygonToPolygon(PolygonCollider selfCollider, PolygonCollider otherCollider)
        {
            // NOT IMPLEMENTED // NOT FINISHED // NOT WORKING
            Vec2[] selfPoints = selfCollider.TransformedPoints;
            Vec2[] otherPoints = otherCollider.TransformedPoints;

            if (!boundingBoxOverlap(selfPoints, otherPoints))
                return CollisionData.Empty;

            if (findSmallestOverlap(out Vec2 minAxis) == -1)
                return CollisionData.Empty;

            findDirectionOfCollision(out Vec2 collisionNormal);

            Vec2[] collisionPoints = findCollisionPoints(selfPoints, otherPoints, collisionNormal);

            return new CollisionData(selfCollider.Owner, otherCollider.Owner, 0, collisionPoints, Vec2.Zero, false);

            bool boundingBoxOverlap(Vec2[] pointsA, Vec2[] pointsB)
            {
                float minA, maxA, minB, maxB;

                minA = pointsA[0].x;
                maxA = pointsA[0].x;
                for (int i = 1; i < pointsA.Length; i++)
                {
                    if (pointsA[i].x < minA)
                        minA = pointsA[i].x;

                    else if (pointsA[i].x > maxA)
                        maxA = pointsA[i].x;
                }

                minB = pointsB[0].x;
                maxB = pointsB[0].x;

                for (int i = 1; i < pointsB.Length; i++)
                {
                    if (pointsB[i].x < minB)
                        minB = pointsB[i].x;

                    else if (pointsB[i].x > maxB)
                        maxB = pointsB[i].x;
                }

                if (maxA < minB || maxB < minA)
                    return false;

                minA = pointsA[0].y;
                maxA = pointsA[0].y;
                for (int i = 1; i < pointsA.Length; i++)
                {
                    if (pointsA[i].y < minA)
                        minA = pointsA[i].y;

                    else if (pointsA[i].y > maxA)
                        maxA = pointsA[i].y;
                }
                minB = pointsB[0].y;
                maxB = pointsB[0].y;
                for (int i = 1; i < pointsB.Length; i++)
                {
                    if (pointsB[i].y < minB)
                        minB = pointsB[i].y;

                    else if (pointsB[i].y > maxB)
                        maxB = pointsB[i].y;
                }

                if (maxA < minB || maxB < minA)
                    return false;

                return true;
            }
            void projectPointsOntoAxis(Vec2[] points, Vec2 axis, out float min, out float max)
            {
                min = points[0].x * axis.x + points[0].y * axis.y;
                max = min;

                for (int i = 1; i < points.Length; i++)
                {
                    float dotProduct = points[i].x * axis.x + points[i].y * axis.y;

                    if (dotProduct < min)
                        min = dotProduct;

                    else if (dotProduct > max)
                        max = dotProduct;
                }
            }
            float intervalOverlap(float minA, float maxA, float minB, float maxB)
            {
                float overlap = 0;

                if (maxA > minB && maxB > minA)
                {
                    float minOverlap = Mathf.Min(maxA, maxB) - Mathf.Max(minA, minB);
                    float totalLength = Mathf.Max(maxA, maxB) - Mathf.Min(minA, minB);

                    overlap = minOverlap / totalLength;
                }

                return overlap;
            }
            sbyte findSmallestOverlap(out Vec2 fminAxis)
            {
                float minOverlap = float.PositiveInfinity;
                fminAxis = Vec2.Zero;

                for (int i = 0; i < selfPoints.Length; i++)
                {
                    Vec2 axis = selfPoints[i + 1 == selfPoints.Length ? 0 : i + 1] - selfPoints[i];
                    axis = new Vec2(-axis.y, axis.x).normalized;

                    projectPointsOntoAxis(selfPoints, axis, out float selfMin, out float selfMax);
                    projectPointsOntoAxis(otherPoints, axis, out float otherMin, out float otherMax);
                    float overlap = intervalOverlap(selfMin, selfMax, otherMin, otherMax);

                    if (overlap == 0) return -1;
                    else if (overlap < minOverlap)
                    {
                        minOverlap = overlap;
                        fminAxis = axis;
                    }
                }
                return 0;
            }
            void findDirectionOfCollision(out Vec2 fcollisionNormal)
            {
                fcollisionNormal = minAxis;
                if (Vec2.DotSecondNormalized(new Vec2(selfCollider.Owner.x, selfCollider.Owner.y), fcollisionNormal) > Vec2.DotSecondNormalized(new Vec2(otherCollider.Owner.x, otherCollider.Owner.y), fcollisionNormal))
                {
                    fcollisionNormal = Vec2.Zero - fcollisionNormal;
                    swap(ref selfPoints, ref otherPoints);
                }
            }
            Vec2[] findCollisionPoints(Vec2[] pointsA, Vec2[] pointsB, Vec2 fcollisionNormal)
            {
                List<Vec2> fcollisionPoints = new List<Vec2>();
                for (int i = 0; i < pointsA.Length; i++)
                {
                    Vec2 pointA1 = pointsA[i];
                    Vec2 pointA2 = pointsA[i + 1 == pointsA.Length ? 0 : i + 1];

                    Vec2 edge = pointA2 - pointA1;
                    Vec2 edgeNormal = new Vec2(-edge.y, edge.x).normalized;

                    if (Vec2.DotSecondNormalized(edgeNormal, fcollisionNormal) > 0)
                        continue;

                    for (int j = 0; j < pointsB.Length; j++)
                    {
                        Vec2 pointB = pointsB[j];

                        float distance = Vec2.DotSecondNormalized(pointB - pointA1, edgeNormal);

                        if (distance > 0 && distance < edge.length)
                            fcollisionPoints.Add(pointB);
                    }
                }
                return fcollisionPoints.ToArray();
            }
            void swap<T>(ref T a, ref T b)
            {
                T temp = a;
                a = b;
                b = temp;
            }
        }
        private CollisionData CircleToCircle(CircleCollider self, CircleCollider other)
        {
            Vec2 selfPrediction = (self.Owner.Rigidbody is null ? Vec2.Zero : self.Owner.Rigidbody.ActualVelocity);

            Vec2 otherPrediction = (other.Owner.Rigidbody is null ? Vec2.Zero : other.Owner.Rigidbody.ActualVelocity);

            Vec2 relativeVelocity = (selfPrediction - otherPrediction);

            Vec2 selfCenter = self.Owner.position + selfPrediction + self.Offset;

            Vec2 otherCenter = other.Owner.position + other.Offset;

            Vec2 relativePosition = ((selfCenter - selfPrediction) - otherCenter);

            float distance = Vec2.Distance(selfCenter, otherCenter);

            Vec2 collisionPoint = otherCenter + (selfCenter - otherCenter).normalized * other.Radius;

            if (Settings.CollisionDebug)
            {
                self.Owner.Rigidbody.AddCollisionDebugToStack(() =>
                {
                    Settings.ColliderDebug.Stroke(125, 75, 255);
                    Settings.ColliderDebug.Polygon(new float[]
                    {
                        selfCenter.x + Camera.Position.x,
                        selfCenter.y + Camera.Position.y,
                        collisionPoint.x + Camera.Position.x,
                        collisionPoint.y + Camera.Position.y
                    });
                    Settings.ColliderDebug.Stroke(200, 0, 255);
                    Settings.ColliderDebug.Polygon(new float[]
                    {
                        otherCenter.x + Camera.Position.x,
                        otherCenter.y + Camera.Position.y,
                        collisionPoint.x + Camera.Position.x,
                        collisionPoint.y + Camera.Position.y
                    });
                    Settings.ColliderDebug.Stroke(255, 0, 0);
                    Settings.ColliderDebug.Ellipse(collisionPoint.x + Camera.Position.x, collisionPoint.y + Camera.Position.y, 10, 10);
                });
            }

            if (Vec2.DotSecondNormalized(relativeVelocity.normalized, relativePosition) > 0 || distance > self.Radius + other.Radius)
                return CollisionData.Empty;

            return new CollisionData(self.Owner, other.Owner, timeOfImpact(), new Vec2[] { collisionPoint }, (collisionPoint - otherCenter).normalized, distance <= Mathf.Abs(self.Radius - other.Radius));

            float timeOfImpact() => (collisionPoint - (selfCenter - selfPrediction)).length / selfCenter.length;
        }
        private CollisionData PolygonToLine(PolygonCollider self, LineCollider other)
        {
            bool hasCollision = false;
            Vec2 collisionPoint = Vec2.Zero;

            float closestDistance = float.MaxValue;
            Vec2 collisionNormal = Vec2.Zero;

            int next = 0;
            for (int current = 0; current < self.Points.Length; current++)
            {
                next = current + 1;
                if (next == self.Points.Length) next = 0;

                Vec2 segmentStart = self.TransformedPoints[current];
                Vec2 segmentEnd = self.TransformedPoints[next];

                if (LineLine(other.TransformedStart(), other.TransformedEnd(), segmentStart, segmentEnd, out Vec2 point))
                {
                    hasCollision = true;
                    float distance = Vec2.Distance(other.TransformedStart(), point);

                    if (distance < closestDistance)
                    {
                        closestDistance = distance;
                        collisionPoint = point;
                        collisionNormal = (segmentEnd - segmentStart).normal;
                    }
                }
            }
            if (hasCollision)
                return new CollisionData(self.Owner, other.Owner, 0, new Vec2[] { collisionPoint }, collisionNormal, false);

            return CollisionData.Empty;
        }
        private CollisionData LineToCircle(LineCollider self, CircleCollider other)
        {
            // NOT IMPLEMENTED // NOT FINISHED // NOT WORKING // NOT NEEDED THOUGH
            Vec2 circleCenter = new Vec2(other.Owner.x, other.Owner.y);
            float radius = other.Radius;

            Vec2 lineDirection = self.LineEnd - self.LineStart;
            float a = Vec2.DotSecondNormalized(lineDirection, lineDirection);
            float b = Vec2.DotSecondNormalized(2 * (self.LineStart - circleCenter), lineDirection);
            float c = Vec2.DotSecondNormalized((circleCenter - self.LineStart), circleCenter - self.LineStart) - radius * radius;

            float discriminant = b * b - 4 * a * c;
            if (discriminant < 0)
                return CollisionData.Empty;

            float t1 = (-b - Mathf.Sqrt(discriminant)) / (2 * a);
            float t2 = (-b + Mathf.Sqrt(discriminant)) / (2 * a);

            if (t1 >= 0 && t1 <= 1)
            {
                Vec2 collisionPoint = self.LineStart + lineDirection * t1;
                Vec2 collisionNormal = (collisionPoint - circleCenter).normalized;
                return new CollisionData(null, other.Owner, 0, new Vec2[] { collisionPoint }, collisionNormal, false);
            }
            else if (t2 >= 0 && t2 <= 1)
            {
                Vec2 collisionPoint = self.LineStart + lineDirection * t2;
                Vec2 collisionNormal = (collisionPoint - circleCenter).normalized;
                return new CollisionData(null, other.Owner, 0, new Vec2[] { collisionPoint }, collisionNormal, false);
            }
            return CollisionData.Empty;
        }
        private CollisionData LineToLine(LineCollider self, LineCollider other)
        {
            // NOT IMPLEMENTED // NOT FINISHED // NOT WORKING // NOT NEEDED THOUGH
            float denominator = (self.LineEnd.y - self.LineStart.y) * (other.LineEnd.x - other.LineStart.x)
                              - (self.LineEnd.x - self.LineStart.x) * (other.LineEnd.y - other.LineStart.y);

            if (denominator == 0)
                return CollisionData.Empty;

            float ua = ((self.LineEnd.x - self.LineStart.x) * (other.LineStart.y - self.LineStart.y)
                      - (self.LineEnd.y - self.LineStart.y) * (other.LineStart.x - self.LineStart.x)) / denominator;

            float ub = ((other.LineEnd.x - other.LineStart.x) * (other.LineStart.y - self.LineStart.y)
                      - (other.LineEnd.y - other.LineStart.y) * (other.LineStart.x - self.LineStart.x)) / denominator;

            if (ua < 0 || ua > 1 || ub < 0 || ub > 1)
                return CollisionData.Empty;

            Vec2 collisionPoint = new Vec2(
                self.LineStart.x + ua * (self.LineEnd.x - self.LineStart.x),
                self.LineStart.y + ua * (self.LineEnd.y - self.LineStart.y)
            );
            return new CollisionData(self.Owner, other.Owner, 0, new[] { collisionPoint }, Vec2.Zero, false);
        }

        private CollisionData CircleToPoint(CircleCollider selfCollider, Vec2 point)
        {
            return new CollisionData(Vec2.Distance(selfCollider.LogicalCenterOfMass, point) < selfCollider.ActiveRadius, selfCollider.Owner);
        }
        private CollisionData LineToPoint(LineCollider selfCollider, Vec2 point)
        {
            // NOT IMPLEMENTED // NOT FINISHED // NOT WORKING // NOT NEEDED THOUGH
            return CollisionData.Empty;
        }

        private CollisionData CircleToPoly(CircleCollider self, PolygonCollider other)
        {
            Vec2 selfPrediction = self.Owner.Rigidbody is null ? Vec2.Zero : self.Owner.Rigidbody.ActualVelocity;
            Vec2[] vertices = other.TransformedPoints;
            float cx = self.Owner.x + self.Offset.x;
            float cy = self.Owner.y + self.Offset.y;
            float r = self.Radius;
            int next = 0;
            Vec2 closestPoint = new Vec2(cx, cy);
            for (int current = 0; current < vertices.Length; current++)
            {
                next = current + 1;
                if (next == vertices.Length) next = 0;

                Vec2 vc = vertices[current];
                Vec2 vn = vertices[next];

                bool collision = LineCircle(vc, vn, self, out Vec2 curClosestPoint);
                closestPoint = curClosestPoint;
                if (collision) return new CollisionData(self.Owner, other.Owner, CircleTOI(self.Owner.position, r, selfPrediction), new Vec2[] { closestPoint }, (vn - vc).normal, false);
            }
            bool centerInside = PolygonPoint(vertices, cx, cy);
            if (centerInside) return new CollisionData(self.Owner, other.Owner, CircleTOI(self.Owner.position, r, selfPrediction), new Vec2[] { closestPoint }, (new Vec2(cx, cy) - closestPoint).normalized, true);

            return CollisionData.Empty;
        }
        private bool LineCircle(Vec2 start, Vec2 end, CircleCollider self, out Vec2 closestPoint)
        {
            Vec2 center = new Vec2(self.Owner.x + self.Offset.x, self.Owner.y + self.Offset.y);
            float r = self.Radius;

            closestPoint = center;

            bool inside1 = PointCircle(start, center, r);
            bool inside2 = PointCircle(end, center, r);

            if (inside1 || inside2)
                return true;

            Vec2 dist = new Vec2(start.x - end.x, start.y - end.y);

            float len = (float)Mathf.Sqrt((dist.x * dist.x) + (dist.y * dist.y));
            float dot = (((center.x - start.x) * (end.x - start.x)) + ((center.y - start.y) * (end.y - start.y))) / (float)Mathf.Pow(len, 2);

            float closestX = start.x + (dot * (end.x - start.x));
            float closestY = start.y + (dot * (end.y - start.y));
            closestPoint = new Vec2(closestX, closestY);

            bool onSegment = LinePoint(start.x, start.y, end.x, end.y, closestX, closestY);
            if (!onSegment) return false;

            if (Settings.CollisionDebug)
            {
                self.Owner.Rigidbody.AddCollisionDebugToStack(() =>
                {
                    Settings.ColliderDebug.Stroke(255, 0, 0);
                    Settings.ColliderDebug.Ellipse(closestX + Camera.Position.x, closestY + Camera.Position.y, 20, 20);
                });
            }

            dist = new Vec2(closestX - center.x, closestY - center.y);
            float distance = (float)Mathf.Sqrt((dist.x * dist.x) + (dist.y * dist.y));

            if (distance <= r)
                return true;

            return false;
        }
        private bool LinePoint(float x1, float y1, float x2, float y2, float px, float py)
        {
            float d1 = (float)Mathf.Sqrt(Mathf.Pow(px - x1, 2) + Mathf.Pow(py - y1, 2));
            float d2 = (float)Mathf.Sqrt(Mathf.Pow(px - x2, 2) + Mathf.Pow(py - y2, 2));

            float lineLen = (float)Mathf.Sqrt(Mathf.Pow(x1 - x2, 2) + Mathf.Pow(y1 - y2, 2));

            float buffer = 0.1f;

            if (d1 + d2 >= lineLen - buffer && d1 + d2 <= lineLen + buffer)
            {
                return true;
            }
            return false;
        }
        private bool PointCircle(Vec2 p, Vec2 c, float r)
        {
            Vec2 dist = new Vec2(p.x - c.x, p.y - c.y);
            float distance = (float)Mathf.Sqrt((dist.x * dist.x) + (dist.y * dist.y));
            return (distance <= r);
        }
        private bool PolygonPoint(Vec2[] vertices, float px, float py)
        {
            bool collision = false;
            int next = 0;
            for (int current = 0; current < vertices.Length; current++)
            {
                next = current + 1;
                if (next == vertices.Length) next = 0;

                Vec2 vc = vertices[current];
                Vec2 vn = vertices[next];

                if (((vc.y > py && vn.y < py) || (vc.y < py && vn.y > py)) && (px < (vn.x - vc.x) * (py - vc.y) / (vn.y - vc.y) + vc.x))
                    collision = !collision;
            }
            return collision;
        }
        private bool LineLine(Vec2 start1, Vec2 end1, Vec2 start2, Vec2 end2, out Vec2 collisionPoint)
        {
            collisionPoint = Vec2.Zero;

            Vec2 dir1 = end1 - start1;
            Vec2 dir2 = end2 - start2;

            float a1 = -dir1.y;
            float b1 = +dir1.x;
            float d1 = -(a1 * start1.x + b1 * start1.y);

            float a2 = -dir2.y;
            float b2 = +dir2.x;
            float d2 = -(a2 * start2.x + b2 * start2.y);

            float seg1_line2_start = a2 * start1.x + b2 * start1.y + d2;
            float seg1_line2_end = a2 * end1.x + b2 * end1.y + d2;

            float seg2_line1_start = a1 * start2.x + b1 * start2.y + d1;
            float seg2_line1_end = a1 * end2.x + b1 * end2.y + d1;

            if (seg1_line2_start * seg1_line2_end > 0 || seg2_line1_start * seg2_line1_end > 0)
            {
                return false;
            }

            float u = seg1_line2_start / (seg1_line2_start - seg1_line2_end);

            collisionPoint = start1 + u * dir1;

            return true;
        }
        private float CircleTOI(Vec2 oldPosition, float radius, Vec2 newPosition)
        {
            return Mathf.Abs(radius - oldPosition.length) / newPosition.length + radius;
        }
        private float GetInterpolationFactor(Vec2 P, Vec2 A, Vec2 B)
        {
            Vec2 AP = P - A;
            Vec2 AB = B - A;
            float ab2 = Vec2.Dot(AB, AB);
            float ap_ab = Vec2.Dot(AP, AB);
            float t = ap_ab / ab2;
            return t;
        }

        public bool Raycast(Vec2 start, Vec2 direction, float step, float maxDistance, out CollisionData collisionData)
        {
            Vec2 end = start + direction * step;
            collisionData = new CollisionData(null, null, 0, new Vec2[] { start + direction * maxDistance }, Vec2.Zero, false);
            CollisionData collisionDataBuffer;
            float minDistance = float.MaxValue;
            bool found = false;
            while (Vec2.Distance(start, end) < maxDistance)
            {
                foreach (Collider other in Physics.Colliders)
                {
                    collisionDataBuffer = CollisionData.Empty;
                    if (!ValidateExpediency(end, other, Settings.RaycastStep / 2)) continue;
                    if (other is PolygonCollider poly)
                        polygonLine(poly, start, end);
                    if (collisionDataBuffer.isEmpty) continue;
                    float distance = Vec2.Distance(start, collisionDataBuffer.AverageCollisionPoint());
                    if (distance < minDistance)
                    {
                        minDistance = distance;
                        collisionData = collisionDataBuffer;
                        found = true;
                    }
                }
                if (found)
                    return true;
                end += direction * step;
            }
            return false;

            void polygonLine(PolygonCollider self, Vec2 a, Vec2 b)
            {  
                self.Owner.TryGetComponent(typeof(ColliderSurfaceAttributes), out Component component);
                ColliderSurfaceAttributes attr = component as ColliderSurfaceAttributes;

                bool hasCollision = false;
                Vec2 collisionPoint = Vec2.Zero;

                float closestDistance = float.MaxValue;
                Vec2 collisionNormal = Vec2.Zero;

                int p1i = 0, p2i = 0;
                Vec2 segmentStart = Vec2.Zero;
                Vec2 segmentEnd = Vec2.Zero;


                int next = 0;
                for (int current = 0; current < self.Points.Length; current++)
                {
                    next = current + 1;
                    if (next == self.Points.Length) next = 0;

                    if (LineLine(a, b, self.TransformedPoints[next], self.TransformedPoints[current], out Vec2 point))
                    {
                        hasCollision = true;
                        float distance = Vec2.Distance(a, point);

                        if (distance < closestDistance)
                        {
                            collisionPoint = point;
                            p1i = current;
                            p2i = next;
                            segmentStart = self.TransformedPoints[current];
                            segmentEnd = self.TransformedPoints[next];
                            closestDistance = distance;
                        }
                    }
                }
                if (!hasCollision)
                    return;

                collisionNormal = (segmentEnd - segmentStart).normal;
                if(attr != null)
                {
                    if (attr.LoopedSmoothing)
                    {
                        float t = GetInterpolationFactor(collisionPoint, segmentStart, segmentEnd);
                        if (attr.SmoothenedPoints)
                            collisionPoint = self.CatmullRomPointLooped(p1i, p2i, t);
                        if (attr.SmoothenedNormals)
                            collisionNormal = self.CatmullRomNormalLooped(p1i, p2i, t);
                    }
                    else
                    {
                        float t = GetInterpolationFactor(collisionPoint, segmentStart, segmentEnd);
                        if (attr.SmoothenedPoints)
                            collisionPoint = self.CatmullRomPoint(p1i, p2i, t);
                        if (attr.SmoothenedNormals)
                            collisionNormal = self.CatmullRomNormal(p1i, p2i, t);
                    }
                }

                collisionDataBuffer = new CollisionData(self.Owner, null, 0, new Vec2[] { collisionPoint }, collisionNormal, PolygonPoint(self.TransformedPoints, start.x, start.y));
            }
        }
    }
}
