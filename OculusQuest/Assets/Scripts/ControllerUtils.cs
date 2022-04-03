using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public enum TrackpadPos
{
    TOP,LEFT,RIGHT,BOTTOM,MIDDLE,UNKNOWN
}

public enum ControllerSide
{
    LEFT,RIGHT
}

public class ControllerUtils
{


    public static TrackpadPos GetTrackpadPos(float xAxis,float yAxis)
    {
        float r = Mathf.Sqrt(Mathf.Pow(xAxis, 2.0f) + Mathf.Pow(yAxis, 2.0f));
        if (r <= 0.447f) //= 1/sqrt(5)
        {
            return TrackpadPos.MIDDLE;
        }
        else
        {
            float asin = Mathf.Asin(xAxis);
            float acos = Mathf.Acos(yAxis);
            const float piQuart = Mathf.PI / 4.0f;
            if (asin < piQuart && asin > -piQuart)
            {
                if (acos < piQuart && acos >= 0.0f)
                {
					return TrackpadPos.BOTTOM;
                }
                else
                {
					return TrackpadPos.TOP;
                }
            }
            else
            {
                if (asin > 0.0f)
                {
                    return TrackpadPos.RIGHT;
                }
                else
                {
                    return TrackpadPos.LEFT;
                }
            }
        }
    }

    public static TrackpadPos GetTrackpadPos(ControllerSide side)
    {
        float xAxis = 0.0f;
        float yAxis = 0.0f;
        if(side==ControllerSide.LEFT)
        {
            xAxis = Input.GetAxisRaw("LeftTrackpadX");
            yAxis = Input.GetAxisRaw("LeftTrackpadY");
        }
        else if(side==ControllerSide.RIGHT)
        {
            xAxis = Input.GetAxisRaw("RightTrackpadX");
            yAxis = Input.GetAxisRaw("RightTrackpadY");
        }

        return GetTrackpadPos(xAxis, yAxis);
    }

}
