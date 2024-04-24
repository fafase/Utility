using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using Tools;
using System.Threading.Tasks;
using System;
using Cysharp.Threading.Tasks;

public class TestScript : MonoBehaviour
{
    private void Start()
    {
        IUserPrefs userPrefs = new UserPrefs();
        userPrefs.Init();
        string remoteJson = @"{
            player: 'MyName',
            score: '1000',
            lastLogin : '12/20/2015'
        }";

        userPrefs.SetUserPrefsFromRemote(remoteJson);
        userPrefs.SetValue("player", "Name");
        //userPrefs.SetValue("score", 2000);
        //userPrefs.SetValue("lastLogin", DateTime.Now);
        if(userPrefs.TryGetObject("player", out string name, "")) 
        {
            Debug.Log("Success " + name);
        }
        else { Debug.Log("No success"); }
    }
}