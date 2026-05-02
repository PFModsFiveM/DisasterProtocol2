using UnityEngine;
using Unity.Netcode.Components;

public class ClientAuthoritativeNetworkTransform : NetworkTransform
{
    protected override bool OnIsServerAuthoritative()
    {
        return false;
    }
}
