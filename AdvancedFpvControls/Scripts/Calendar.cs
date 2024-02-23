using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Calendar : MonoBehaviour
{
    [Header("Time Parameters")]
    public float year;
    public float month;
    public float day;
    public float hour;
    public float minutes;
    [Range(0f, 60f)]
    public float timeScale = 5.0f;
    [Range(0f, 60f)]
    public float morningSpeed = 10.0f;
    [Range(0f, 60f)]
    public float nightSpeed = 6.0f;

    public Season currentSeason;

    void Update()
    {
        UpdateTime();
        SeasonStateHandler();
    }
    void UpdateTime()
    {
        float speed = (hour >= 6 && hour < 18) ? morningSpeed : nightSpeed;
        minutes += Time.deltaTime * (timeScale / speed);
        HandleOverflow();
    }
    void HandleOverflow()
    {
        if (minutes > 60)
        {
            minutes = 0;
            hour++;
        }
        if(hour > 23)
        {
            hour = 0;
            day++;
        }
        if (day > 30)
        {
            day = 1;
            month++;
        }
        if (month > 12)
        {
            month = 1;
            year++;
        }
    }
    void SeasonStateHandler()
    {
        if (month == 12 || month == 1 || month == 2)
        {
            currentSeason = Season.Winter;
        }
        else if (month == 3 || month == 4 || month == 5)
        {
            currentSeason = Season.Spring;
        }
        else if (month == 6 || month == 7 || month == 8)
        {
            currentSeason = Season.Summer;
        }
        else if (month == 9 || month == 10 || month == 11)
        {
            currentSeason = Season.Autumn;
        }
    }
}
public enum Season
{
    Spring = 0,
    Summer = 1,
    Autumn = 2,
    Winter = 3
}
