using System;
using System.Collections;
using System.Collections.Generic;
using System.Globalization;
using UnityEngine;

public static class UtilsApiary
{
    public static void SetDateTime(string key, DateTime value)
    {
        string  convertedToString = value.ToString(format:"u");
        PlayerPrefs.SetString(key, convertedToString);
    }
    public static DateTime GetDateTime(string key, DateTime defaultvalue)
    {
        if (PlayerPrefs.HasKey(key))
        {
            string stored = PlayerPrefs.GetString(key);
            DateTime result = DateTime.ParseExact(stored, "u", CultureInfo.InvariantCulture);
            return result;
        }
        else
        {
            return defaultvalue;
        }
    }
}
