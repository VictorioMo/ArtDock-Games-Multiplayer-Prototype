using Unity.Netcode;
using Unity.Netcode.Transports.UTP;
using UnityEngine;

public class DevConnectionMenu : MonoBehaviour
{
    private string ipAddress = "127.0.0.1";
    private string portText = "7777";

    private void OnGUI()
    {
        Debug.Log("On GUI Called");

        NetworkManager manager = NetworkManager.Singleton;

        if (manager == null)
            return;

        GUILayout.BeginArea(
            new Rect(10, 10, 250, 180),
            GUI.skin.box
        );

        if (!manager.IsListening)
        {
            GUILayout.Label("Server IP:");
            ipAddress = GUILayout.TextField(ipAddress);

            GUILayout.Label("Port:");
            portText = GUILayout.TextField(portText);

            if (ushort.TryParse(portText, out ushort port))
            {
                UnityTransport transport =
                    manager.GetComponent<UnityTransport>();

                if (GUILayout.Button("HOST"))
                {
                    transport.SetConnectionData(
                        "127.0.0.1",
                        port,
                        "0.0.0.0"
                    );

                    manager.StartHost();
                }

                if (GUILayout.Button("JOIN"))
                {
                    transport.SetConnectionData(
                        ipAddress,
                        port
                    );

                    manager.StartClient();
                }
            }
        }
        else
        {
            GUILayout.Label(
                manager.IsHost ? "HOSTING" : "CLIENT"
            );

            if (GUILayout.Button("DISCONNECT"))
            {
                manager.Shutdown();
            }
        }

        GUILayout.EndArea();
    }
}