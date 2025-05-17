using Unity.Collections.LowLevel.Unsafe;
using Unity.Netcode;
using UnityEngine;

public class ParticleRPCRelay : NetworkBehaviour
{
    public static ParticleRPCRelay Instance;

    private void Awake()
    {
        Instance = this;
    }

    [ClientRpc]
    public void PlayHitParticleClientRpc(Vector3 position)
    {
        ParticleManager.Instance.PlayHitParticle(position);
    }

    [ClientRpc]
    public void PlayExplosionParticleClientRpc(Vector3 position)
    {
        ParticleManager.Instance.PlayExplosionParticle(position);
    }

    [ClientRpc]
    public void PlayDeathParticleClientRpc(Vector3 position)
    {
        ParticleManager.Instance.PlayDeathParticle(position);
    }
    [ClientRpc]
    public void roundClientRpc(int round)
    {
        UpdateWaveCount.Instance.UpdateWaveText(round);
    }

    [ClientRpc]
    public void PlayTowerPlaceParticleClientRpc(Vector3 position)
    {
        ParticleManager.Instance.PlayTowerPlaceParticle(position);
    }
}
