using System.Collections;
using System.Collections.Generic;
using System.Threading.Tasks;
using UnityEngine;

public class AplicationController : MonoBehaviour
{

    [SerializeField] private ClientSingleton clientPrefab;

    [SerializeField] private HostSingleton hostPrefab;
    
    private async void Start()
    {
        //makes this code persist as we change scenes
        DontDestroyOnLoad(gameObject);

        //is this dedicated server? dedicated server don't need players or graphics rendering on them
        await LaunchInMode(SystemInfo.graphicsDeviceType == UnityEngine.Rendering.GraphicsDeviceType.Null);
    }

    private async Task LaunchInMode(bool isDedicatedServer) {
        if(isDedicatedServer){

        }else{
            HostSingleton hostSingleton = Instantiate(hostPrefab);
            hostSingleton.CreateHost();

            ClientSingleton clientSingleton = Instantiate(clientPrefab);
            bool authenticated = await clientSingleton.CreateClient();

            if(authenticated){
                clientSingleton.GameManager.GoToMenu();
            }
        }
    }

}
