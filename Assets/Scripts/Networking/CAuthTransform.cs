using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using Unity.Netcode.Components;

[AddComponentMenu("Netcode/CAuth Network Transform")]
public class CAuthTransform : NetworkTransform
{
    protected override bool OnIsServerAuthoritative()
    {
        return false;
        //return !this.IsOwner;
    }
}
