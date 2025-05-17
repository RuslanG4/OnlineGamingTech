using Unity.Netcode;
using UnityEngine;

public class CoinManager : NetworkBehaviour
{
    public static CoinManager Instance;

    NetworkVariable<int> coinAmount = new NetworkVariable<int>(0);

    private bool initialised = false;
    public override void OnNetworkSpawn()
    {
        Instance = this;
    }

    public void initialiseValue(int coinCount)
    {
        if (!initialised)
        {
            coinAmount.Value = coinCount;
            initialised = true; 
        }
    }

    public void addCoins( int addition)
    {
        coinAmount.Value += addition;   
    }

    public int getCoins()
    {
        return coinAmount.Value;    
    }

}
