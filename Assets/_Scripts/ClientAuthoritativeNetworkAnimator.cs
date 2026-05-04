using UnityEngine;
using Unity.Netcode.Components;

public class ClientAuthoritativeNetworkAnimator : NetworkAnimator
{
    protected override void Awake()
    {
        if (Animator == null)
        {
            Animator = GetComponent<Animator>();
        }

        base.Awake();
    }

    protected override bool OnIsServerAuthoritative()
    {
        return false;
    }
}
