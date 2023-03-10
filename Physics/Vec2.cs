using System;

public struct Vec2
{
    #region Fields
    public float x;
    public float y;
    #endregion

    #region Properties
    public Vec2 normalized => this * (1 / length);
    public float length => Mathf.Sqrt(x * x + y * y);
    public double angleInDeg => Math.Atan2(y, x) - Math.Atan2(1, 0);
    public double angleInRad => DegToRad(angleInDeg);
    #endregion

    #region Constructor
    public Vec2(float x, float y) { this.x = x; this.y = y; }
    #endregion

    #region Angles
    public static double DegToRad(double degrees) => degrees * Mathf.PI / 180;
    public static double RadToDeg(double radians) => radians * 180 / Mathf.PI;
    public void SetAngleDegrees(float degrees) => SetXY(Mathf.Cos(degrees) * length, Mathf.Sin(degrees) * length);
    public void SetAngleRadians(float radians) => SetXY(Mathf.Cos((float)RadToDeg(radians)) * length, Mathf.Sin((float)RadToDeg(radians)) * length);
    public void RotateDegrees(float degrees) => SetAngleDegrees((float)angleInDeg + degrees);
    public void RotateRadians(float radians) => SetAngleRadians((float)angleInRad + radians);
    #endregion

    #region Presets
    public static Vec2 Zero => new Vec2(0, 0);
    public static Vec2 Up => new Vec2(0, 1);
    public static Vec2 Down => new Vec2(0, -1);
    public static Vec2 Left => new Vec2(-1, 0);
    public static Vec2 Right => new Vec2(1, 0);
    #endregion

    #region Operators
    public static Vec2 operator +(Vec2 a, Vec2 b) => new Vec2(a.x + b.x, a.y + b.y);
    public static Vec2 operator +(Vec2 a, float b) => new Vec2(a.x + b, a.y + b);
    public static Vec2 operator +(float a, Vec2 b) => new Vec2(a + b.x, a + b.y);
    public static Vec2 operator -(Vec2 a, Vec2 b) => new Vec2(a.x - b.x, a.y - b.y);
    public static Vec2 operator -(Vec2 a, float b) => new Vec2(a.x - b, a.y - b);
    public static Vec2 operator -(float a, Vec2 b) => new Vec2(a - b.x, a - b.y);
    public static Vec2 operator *(Vec2 a, Vec2 b) => new Vec2(a.x * b.x, a.y * b.y);
    public static Vec2 operator *(Vec2 a, float b) => new Vec2(a.x * b, a.y * b);
    public static Vec2 operator *(float a, Vec2 b) => new Vec2(a * b.x, a * b.y);
    public static Vec2 operator /(Vec2 a, Vec2 b) => new Vec2(a.x / b.x, a.y / b.y);
    public static Vec2 operator /(Vec2 a, float b) => new Vec2(a.x / b, a.y / b);
    public static Vec2 operator /(float a, Vec2 b) => new Vec2(a / b.x, a / b.y);
    public static Vec2 operator ^(float a, Vec2 b) => new Vec2(Mathf.Pow(a, b.x), Mathf.Pow(a, b.y));
    public static Vec2 operator ^(Vec2 a, float b) => new Vec2(Mathf.Pow(a.x, b), Mathf.Pow(a.y, b));
    public static Vec2 operator ^(Vec2 a, Vec2 b) => new Vec2(Mathf.Pow(a.x, b.x), Mathf.Pow(a.y, b.x));
    #endregion

    #region Controllers
    public void SetXY(float newX, float newY) { x = newX; y = newY; }
    public void Normalize() => this = length == 0? this : this * Mathf.Pow(length, -1);
    public static Vec2 Lerp(Vec2 origin, Vec2 destination, float factor) => ((1 - factor) * origin) + (factor * destination);
    public float Dot() => x * x + y * y;
    public override string ToString() => $"({x},{y})";
    #endregion
}
