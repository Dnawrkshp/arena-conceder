using System.Reflection;
using System.IO;
using UnityEngine;
using ArenaConceder;

public static class GameHook
{
    static GameObject hook = null;

    public static void Hook()
    {
        if (!hook)
        {
            hook = new GameObject();
            hook.AddComponent<Conceder>();
            Debug.Log("Hooked!");
            Object.DontDestroyOnLoad(hook);
        }
    }
}
