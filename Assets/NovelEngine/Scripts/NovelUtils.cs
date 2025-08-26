using novel;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public static class NovelUtils
{
    public static bool ConditinalStateMent(float left, CompOP op, float right)
    {

        switch (op)
        {
            case CompOP.GreaterThan:
                if (left > right)
                {
                    return true;
                }
                break;
            case CompOP.LessThan:
                if (left < right)
                {
                    return true;
                }
                break;
            case CompOP.GreaterThanOrEqual:
                if (left >= right)
                {
                    return true;
                }
                break;
            case CompOP.LessThanOrEqual:
                if (left <= right)
                {
                    return true;
                }
                break;
            case CompOP.Equal:
                if (left == right)
                {
                    return true;
                }
                break;
            case CompOP.NotEqual:
                if (left != right)
                {
                    return true;
                }
                break;
            default:
                Debug.LogError("Error : 정의되지 않은 연산자");
                break;
        }

        return false;
    }
}
