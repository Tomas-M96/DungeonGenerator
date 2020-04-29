using System;

[Serializable]
public class IntRange
{

    public int mMin; // minimum value in this range
    public int mMax; // maximum value in this range

    //Constructor to set the values.
    public IntRange(int min, int max)
    {
        mMin = min;
        mMax = max;
    }

    // Get a random value from the range
    public int Random
    {
        get { return UnityEngine.Random.Range(mMin, mMax); }
    }
}
