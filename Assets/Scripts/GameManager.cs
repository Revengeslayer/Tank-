using Photon.Pun;
using Photon.Realtime;
using System.Collections;
using System.Collections.Generic;
using Tanks;
using UnityEngine;
using UnityEngine.SceneManagement;

public class GameManager : MonoBehaviourPunCallbacks
{
    // Start is called before the first frame update
    public static GameManager instance;
    public static GameObject localPlayer;


    private GameObject defaultSpawnPoint;

    string gameVersion = "1";
    void Awake()
    {
        if (instance != null)
        {
            Debug.LogErrorFormat(gameObject,
                    "Multiple instances of {0} is not allow", GetType().Name);
            DestroyImmediate(gameObject);
            return;
        }
        PhotonNetwork.AutomaticallySyncScene = true;
        DontDestroyOnLoad(gameObject);
        instance = this;

        defaultSpawnPoint = new GameObject("Default SpawnPoint");
        defaultSpawnPoint.transform.position = new Vector3(0, 0, 0);
        defaultSpawnPoint.transform.SetParent(transform, false);
    }
    void Start()
    {
        PhotonNetwork.ConnectUsingSettings();
        PhotonNetwork.GameVersion = gameVersion;

        SceneManager.sceneLoaded += OnSceneLoaded;

    }
    public override void OnConnected()
    {
        Debug.Log("PUN Connected");
    }
    public override void OnConnectedToMaster()
    {
        Debug.Log("PUN Connected to Master");
    }
    public override void OnDisconnected(DisconnectCause cause)
    {
        Debug.LogWarningFormat("PUN Disconnected was called by PUN with reason {0}", cause);
    }

    public void JoinGameRoom()
    {
        var options = new RoomOptions
        {
            MaxPlayers = 6
        };
        PhotonNetwork.JoinOrCreateRoom("Kingdom", options, null);
    }
    public override void OnJoinedRoom()
    {
        if (PhotonNetwork.IsMasterClient)
        {
            Debug.Log("Created room!!");
            PhotonNetwork.LoadLevel("GameScene");
        }
        else
        {
            Debug.Log("Joined room!!");
        }
    }

    public override void OnJoinRoomFailed(short returnCode, string message)
    {
        Debug.LogWarningFormat("加入失敗 " + message);
    }

    private void OnSceneLoaded(Scene scene, LoadSceneMode mode)
    {
        var spawnPoint = GetRandomSpawnPoint();
        if (!PhotonNetwork.InRoom)
        {
            return;
        }
        localPlayer = PhotonNetwork.Instantiate("TankPlayer", spawnPoint.position, spawnPoint.rotation, 0);
        Debug.Log("Player Instance ID: " + localPlayer.GetInstanceID());
    }

    #region 工具

    public static List<GameObject> GetAllObjectsOfTypeInScene<T>()
    {
        var objectsInScene = new List<GameObject>();
        foreach (var go in (GameObject[])Resources.FindObjectsOfTypeAll(typeof(GameObject)))
        {
            if (go.hideFlags == HideFlags.NotEditable ||
                go.hideFlags == HideFlags.HideAndDontSave)
                continue;
            if (go.GetComponent<T>() != null)
                objectsInScene.Add(go);
        }
        return objectsInScene;
    }

    private Transform GetRandomSpawnPoint()
    {
        var spawnPoints = GetAllObjectsOfTypeInScene<SpawnPoint>();
        return spawnPoints.Count == 0
        ? defaultSpawnPoint.transform
        : spawnPoints[Random.Range(0, spawnPoints.Count)].transform;
    }

    #endregion
}

