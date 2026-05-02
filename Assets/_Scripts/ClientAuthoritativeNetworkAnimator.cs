using UnityEngine;
using Unity.Netcode.Components;

public class ClientAuthoritativeNetworkAnimator : NetworkAnimator
{
    protected override bool OnIsServerAuthoritative()
    {
        return false;
    }
}
