using Unity.Netcode;
using UnityEngine;

public class DigEffectTrigger : MonoBehaviour
{
    public ParticleSystem digEffect;

    public void PlayDigEffect()
    {
        if (digEffect != null)
        {
            digEffect.Play();
        }
    }

    public void DestroyMoleHole()
    {
        GetComponent<NetworkObject>().Despawn();
    }
}
