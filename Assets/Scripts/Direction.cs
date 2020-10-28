using UnityEngine;

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


    public static Pos operator +(Pos a, Pos b)
    {
        return new Pos(a.x + b.x, a.y + b.y);
    }

    public static Pos operator -(Pos a, Pos b)
    {
        return new Pos(a.x - b.x, a.y - b.y);
    }
}
public interface Direction
{
    Pos GetForward(Pos pos);
    Pos GetLeft(Pos pos);
    Pos GetRight(Pos pos);
    Pos GetBackward(Pos pos);

    Vector3 LookAt { get; }

    Terrain Door { get; }
    Direction Left { get; }
    Direction Right { get; }
    Direction Backward { get; }
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

    public Terrain Door => Terrain.VDoor;
    public Direction Left => new West();
    public Direction Right => new East();
    public Direction Backward => new South();
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

    public Terrain Door => Terrain.HDoor;
    public Direction Left => new North();
    public Direction Right => new South();
    public Direction Backward => new West();
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

    public Terrain Door => Terrain.VDoor;
    public Direction Left => new East();
    public Direction Right => new West();
    public Direction Backward => new North();
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

    public Terrain Door => Terrain.HDoor;
    public Direction Left => new South();
    public Direction Right => new North();
    public Direction Backward => new East();

}