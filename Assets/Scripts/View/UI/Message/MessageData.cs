using System;

public enum FaceID
{
    NONE = -1,
    DEFAULT = 0,
    ANGRY,
    SMILE,
    DISATTRACT,
    NOTICE
}

public struct MessageData
{
    public string[] sentences;
    public FaceID[] faces;
}