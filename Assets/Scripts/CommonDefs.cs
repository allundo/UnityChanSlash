using UnityEngine;
using System;

// UnityEngine.Debug.Log(System.Reflection.MethodBase.GetCurrentMethod().Name + ": " + gameObject.name, gameObject);

#if UNITY_EDITOR
using UniRx;

// Handles event double firing issue on Editor.
public class InputControl
{
    private bool isFired = false;
    public bool CanFire()
    {
        if (isFired) return false;
        isFired = true;
        Observable.NextFrame().Subscribe(_ => isFired = false);
        return true;
    }
}
#endif

public enum Terrain
{
    Path = 0,
    Ground,
    Wall,
    Pall, // Gate or Pall
    Door,
    DownStair,
    UpStair,
}

public struct Pos
{
    public int x;
    public int y;

    public Pos(int x = 0, int y = 0)
    {
        this.x = x;
        this.y = y;
    }

    public Pos IncX()
    {
        return AddX(1);
    }

    public Pos IncY()
    {
        return AddY(1);
    }

    public Pos DecX()
    {
        return SubX(1);
    }

    public Pos DecY()
    {
        return SubY(1);
    }

    public Pos Add(int x, int y)
    {
        return new Pos(this.x + x, this.y + y);
    }

    public Pos AddX(int x)
    {
        return new Pos(this.x + x, y);
    }

    public Pos AddY(int y)
    {
        return new Pos(x, this.y + y);
    }

    public Pos Sub(int x, int y)
    {
        return new Pos(this.x - x, this.y - y);
    }

    public Pos SubX(int x)
    {
        return new Pos(this.x - x, y);
    }

    public Pos SubY(int y)
    {
        return new Pos(x, this.y - y);
    }

    public Pos North()
    {
        return DecY();
    }

    public Pos East()
    {
        return IncX();
    }

    public Pos South()
    {
        return IncY();
    }

    public Pos West()
    {
        return DecX();
    }

    public bool IsNull => this.x == 0 && this.y == 0;

    public override int GetHashCode()
    {
        return x.GetHashCode() ^ y.GetHashCode();
    }
    public override bool Equals(object obj)
    {
        if (!(obj is Pos)) return false;

        Pos compare = (Pos)obj;
        return this.x == compare.x && this.y == compare.y;
    }

    public static Pos operator +(Pos a, Pos b)
    {
        return new Pos(a.x + b.x, a.y + b.y);
    }

    public static Pos operator -(Pos a, Pos b)
    {
        return new Pos(a.x - b.x, a.y - b.y);
    }

    public static Pos operator *(Pos a, int n)
    {
        return new Pos(a.x * n, a.y * n);
    }
    public static Pos operator /(Pos a, int n)
    {
        return new Pos(a.x / n, a.y / n);
    }

    public static bool operator ==(Pos a, Pos b)
    {
        return a.x == b.x && a.y == b.y;
    }

    public static bool operator !=(Pos a, Pos b)
    {
        return !(a == b);
    }
}

public interface IDirection
{
    Pos GetForward(Pos pos);
    Pos GetLeft(Pos pos);
    Pos GetRight(Pos pos);
    Pos GetBackward(Pos pos);

    Vector3 LookAt { get; }

    IDirection Left { get; }
    IDirection Right { get; }
    IDirection Backward { get; }

    Dir Enum { get; }
    Vector3 Angle { get; }
    Quaternion Rotate { get; }

    bool IsSame(IDirection dir);
    bool IsLeft(IDirection dir);
    bool IsRight(IDirection dir);
    bool IsInverse(IDirection dir);
}

public abstract class Direction
{
    public static North north = new North();
    public static East east = new East();
    public static South south = new South();
    public static West west = new West();

    public static IDirection Convert(Dir dir)
    {
        switch (dir)
        {
            case Dir.N:
                return north;
            case Dir.E:
                return east;
            case Dir.S:
                return south;
            case Dir.W:
                return west;
            default:
                throw new ArgumentException("Invalid enum: " + dir + " cannot convert to Direction.");
        }
    }

    public abstract bool IsSame(IDirection dir);

    public abstract Vector3 Angle { get; }
    public Quaternion Rotate => Quaternion.Euler(Angle);

    public bool IsLeft(IDirection dir) => IsSame(dir?.Right);
    public bool IsRight(IDirection dir) => IsSame(dir?.Left);
    public bool IsInverse(IDirection dir) => IsSame(dir?.Backward);

    public override bool Equals(object obj)
    {
        if (obj == null || GetType() != obj.GetType())
        {
            return false;
        }

        return IsSame(obj as IDirection);
    }

    public override int GetHashCode() => 0;
}

public class North : Direction, IDirection
{
    public Pos GetForward(Pos pos)
    {
        return pos.DecY();
    }
    public Pos GetLeft(Pos pos)
    {
        return pos.DecX();
    }
    public Pos GetRight(Pos pos)
    {
        return pos.IncX();
    }
    public Pos GetBackward(Pos pos)
    {
        return pos.IncY();
    }

    public Vector3 LookAt => new Vector3(0, 0, 1.0f);

    public IDirection Left => Direction.west;
    public IDirection Right => Direction.east;
    public IDirection Backward => Direction.south;

    public Dir Enum => Dir.N;
    public override Vector3 Angle => Vector3.zero;

    public override bool IsSame(IDirection dir) => dir is North;

    public override int GetHashCode() => 0b0001;
}

public class East : Direction, IDirection
{
    public Pos GetForward(Pos pos)
    {
        return pos.IncX();
    }
    public Pos GetLeft(Pos pos)
    {
        return pos.DecY();
    }
    public Pos GetRight(Pos pos)
    {
        return pos.IncY();
    }
    public Pos GetBackward(Pos pos)
    {
        return pos.DecX();
    }

    public Vector3 LookAt => new Vector3(1.0f, 0, 0);

    public IDirection Left => Direction.north;
    public IDirection Right => Direction.south;
    public IDirection Backward => Direction.west;

    public Dir Enum => Dir.E;
    public override Vector3 Angle => new Vector3(0, 90, 0);

    public override bool IsSame(IDirection dir) => dir is East;

    public override int GetHashCode() => 0b0010;
}

public class South : Direction, IDirection
{
    public Pos GetForward(Pos pos)
    {
        return pos.IncY();
    }
    public Pos GetLeft(Pos pos)
    {
        return pos.IncX();
    }
    public Pos GetRight(Pos pos)
    {
        return pos.DecX();
    }
    public Pos GetBackward(Pos pos)
    {
        return pos.DecY();
    }

    public Vector3 LookAt => new Vector3(0, 0, -1.0f);

    public IDirection Left => Direction.east;
    public IDirection Right => Direction.west;
    public IDirection Backward => Direction.north;

    public Dir Enum => Dir.S;
    public override Vector3 Angle => new Vector3(0, 180, 0);

    public override bool IsSame(IDirection dir) => dir is South;

    public override int GetHashCode() => 0b0100;
}

public class West : Direction, IDirection
{
    public Pos GetForward(Pos pos)
    {
        return pos.DecX();
    }
    public Pos GetLeft(Pos pos)
    {
        return pos.IncY();
    }
    public Pos GetRight(Pos pos)
    {
        return pos.DecY();
    }
    public Pos GetBackward(Pos pos)
    {
        return pos.IncX();
    }

    public Vector3 LookAt => new Vector3(-1.0f, 0, 0);

    public IDirection Left => Direction.south;
    public IDirection Right => Direction.north;
    public IDirection Backward => Direction.east;

    public Dir Enum => Dir.W;
    public override Vector3 Angle => new Vector3(0, -90, 0);

    public override bool IsSame(IDirection dir) => dir is West;

    public override int GetHashCode() => 0b1000;
}
public enum Dir
{
    NONE = 0b0000,
    N = 0b0001,
    E = 0b0010,
    S = 0b0100,
    W = 0b1000,
    NE = 0b0011,
    ES = 0b0110,
    SW = 0b1100,
    WN = 0b1001,
    NS = 0b0101,
    EW = 0b1010,
    WNE = 0b1011,
    NES = 0b0111,
    ESW = 0b1110,
    SWN = 0b1101,
    NESW = 0b1111,
}

/// <summary>
/// A, B, C, D means existence of quater(1/4) plates <br>
/// A = North:West, B = North:East, C = South:West, D = South:East <br>
/// This is like a 2x2 matrix: <br>
/// | A B |<br>
/// | C D |<br>
/// So ABCD means full of one plate.
/// </summary>
public enum Plate
{
    NONE = 0b0000,
    A = 0b0001,
    B = 0b0010,
    C = 0b0100,
    D = 0b1000,
    AB = 0b0011,
    BC = 0b0110,
    CD = 0b1100,
    AD = 0b1001,
    AC = 0b0101,
    BD = 0b1010,
    ABD = 0b1011,
    ABC = 0b0111,
    BCD = 0b1110,
    ACD = 0b1101,
    ABCD = 0b1111,
}

public enum ItemType
{
    Potion = 0,
}
public enum AttackType
{
    None = 0,
    Smash,
    Slash,
    Sting,
    Bite,
    Burn = 10,
}
