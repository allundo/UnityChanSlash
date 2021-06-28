using UnityEngine;

public enum Terrain
{
    Path,
    Ground,
    Wall,
    Pall,
    Door,
    Stair,
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

    public Pos AddX(int x)
    {
        return new Pos(this.x + x, y);
    }

    public Pos AddY(int y)
    {
        return new Pos(x, this.y + y);
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

public interface Direction
{
    Pos GetForward(Pos pos);
    Pos GetLeft(Pos pos);
    Pos GetRight(Pos pos);
    Pos GetBackward(Pos pos);

    Vector3 LookAt { get; }

    Direction Left { get; }
    Direction Right { get; }
    Direction Backward { get; }

    bool IsSame(Direction dir);
    bool IsLeft(Direction dir);
    bool IsRight(Direction dir);
    bool IsInverse(Direction dir);
}

public class North : Direction
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

    public Direction Left => new West();
    public Direction Right => new East();
    public Direction Backward => new South();

    public bool IsSame(Direction dir) => dir is North;
    public bool IsLeft(Direction dir) => dir is West;
    public bool IsRight(Direction dir) => dir is East;
    public bool IsInverse(Direction dir) => dir is South;
}

public class East : Direction
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

    public Direction Left => new North();
    public Direction Right => new South();
    public Direction Backward => new West();

    public bool IsSame(Direction dir) => dir is East;
    public bool IsLeft(Direction dir) => dir is North;
    public bool IsRight(Direction dir) => dir is South;
    public bool IsInverse(Direction dir) => dir is West;
}

public class South : Direction
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

    public Direction Left => new East();
    public Direction Right => new West();
    public Direction Backward => new North();

    public bool IsSame(Direction dir) => dir is South;
    public bool IsLeft(Direction dir) => dir is East;
    public bool IsRight(Direction dir) => dir is West;
    public bool IsInverse(Direction dir) => dir is North;
}

public class West : Direction
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

    public Direction Left => new South();
    public Direction Right => new North();
    public Direction Backward => new East();

    public bool IsSame(Direction dir) => dir is West;
    public bool IsLeft(Direction dir) => dir is South;
    public bool IsRight(Direction dir) => dir is North;
    public bool IsInverse(Direction dir) => dir is East;

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
