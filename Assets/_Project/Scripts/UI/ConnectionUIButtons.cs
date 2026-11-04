using UnityEngine;

public class ConnectionUIButtons : MonoBehaviour
{
    [SerializeField] ConnectionUI connUI;

    public void OnHostButtonPresss()
    {
        connUI.Host();
    }

    public void OnJoinButtonPresss()
    {
        connUI.Join();
    }
}
