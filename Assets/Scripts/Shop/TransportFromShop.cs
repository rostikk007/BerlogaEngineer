using UnityEngine.SceneManagement;
using UnityEngine;

public class TransportFromShop : MonoBehaviour
{
    public static int PlayerPosss = 100;

    public void Transition()
    {
        PlayerPosss = 1;
        TransportInShop.isLoadingSceneShop = false;
        SceneManager.LoadScene("MainMap");
    }
}
