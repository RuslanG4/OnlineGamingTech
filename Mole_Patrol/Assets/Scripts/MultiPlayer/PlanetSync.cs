using System.Collections;
using Unity.Netcode;
using UnityEngine;

public class PlanetSync : NetworkBehaviour
{
    public NetworkVariable<uint> planetSeed = new NetworkVariable<uint>();
    public NetworkVariable<uint> seed = new NetworkVariable<uint>();
    public bool SeedIsSet => seed.Value != 0;

    public override void OnNetworkSpawn()
    {
        if (IsServer)
        {
            seed.Value = (uint)UnityEngine.Random.Range(1, int.MaxValue);
        }
    }
}
