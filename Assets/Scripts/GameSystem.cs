using System.Collections;
using System.Collections.Generic;
using System.IO;
using System.Runtime.Serialization.Formatters.Binary;
using UnityEngine;
using UnityEngine.SceneManagement;
using UnityStandardAssets.Characters.FirstPerson;

[System.Serializable]
struct SerializableVector3
{
    public SerializableVector3(Vector3 vec3)
    {
        x = vec3.x;
        y = vec3.y;
        z = vec3.z;
    }

    public float x, y, z;
}

[System.Serializable]
struct SerializableQuaternion
{
    public SerializableQuaternion(Quaternion quat)
    {
        x = quat.x;
        y = quat.y;
        z = quat.z;
        w = quat.w;
    }

    public float x, y, z, w;
}
[System.Serializable]
class GameData
{
    public GameData()
    {
        MovableObjectPos = new List<SerializableVector3>();
        MovableObjectMaterialIndex = new List<int>();
        PlayerPosition = new SerializableVector3(Vector3.zero);
        PlayerRotation = new SerializableQuaternion(Quaternion.identity);
        SceneIndex = 2;
    }
    public List<SerializableVector3> MovableObjectPos;
    public List<int> MovableObjectMaterialIndex;
    public SerializableVector3 PlayerPosition;
    public SerializableQuaternion PlayerRotation;
    public int SceneIndex;
}

public class GameSystem : MonoBehaviour
{
    public static string SavedGameAvailablePlayerPrefsKey = "SavedGameAvailable";
    public System.Action OnSaveGame = null;
    public UiSystem UiSystem = null;

    private bool m_mainSceneLoaded = false;
    private int m_activeMainScene = 2;
    private List <Scene> m_activeMainScenes;
    void Awake()
    {
        m_activeMainScenes = new List <Scene>();
        loadUi();
    }

    void Update()
    {
        if (m_mainSceneLoaded)
        {
            if (Input.GetKeyDown(KeyCode.Escape) && UiSystem.InGameMenu.activeInHierarchy != true)
            {
                UiSystem.PauseGame(true);
            }
        }
    }

    public void AddAdditionalMainScene(Scene scene)
    {
        m_activeMainScenes.Add(scene);
    }

    public void RemoveAdditionalMainScene(Scene scene)
    {
        m_activeMainScenes.Remove(scene);
    }

    private void loadUi()
    {
        SceneManager.LoadScene(1, LoadSceneMode.Additive);
        StartCoroutine("findUiSystem");
    }
    IEnumerator findUiSystem()
    {
        yield return new WaitForSeconds(0.1f);
        UiSystem = GameObject.FindObjectOfType<UiSystem>();
        UiSystem.GameSystem = this;
    }

    public void UnloadMainScene()
    {
        if (m_mainSceneLoaded)
        {
            SceneManager.UnloadSceneAsync(m_activeMainScene);

            if (m_activeMainScenes.Count != 0)
                foreach (var scene in m_activeMainScenes)
                    if (scene.IsValid())
                        SceneManager.UnloadSceneAsync(scene.name);

            m_mainSceneLoaded = false;
        }
    }
    public AsyncOperation LoadMainScene(int index)
    {
        UnloadMainScene();
        m_activeMainScene = index;
        AsyncOperation op = SceneManager.LoadSceneAsync(index, LoadSceneMode.Additive);
        m_mainSceneLoaded = true;
        return op;
    }

    private List<OnCollisionMaterialChanger> findMovablesInWorld()
    {
        List<OnCollisionMaterialChanger>  movables = new List<OnCollisionMaterialChanger>();
        var gos = GameObject.FindGameObjectsWithTag("Movable");
        foreach (var m in gos)
        {
            movables.Add(m.GetComponent<OnCollisionMaterialChanger>());
        }
        return movables;
    }

    private Player findPlayerInWorld()
    {
        return GameObject.FindWithTag("Player").GetComponent<Player>();
    }


    public void SaveData()
    {
        if (!Directory.Exists("Saves"))
            Directory.CreateDirectory("Saves");

        BinaryFormatter formatter = new BinaryFormatter();
        FileStream saveFile = File.Create("Saves/save.binary");
        formatter.Serialize(saveFile, getGameData());
        saveFile.Close();

        PlayerPrefs.SetInt(SavedGameAvailablePlayerPrefsKey, 1);
        if (OnSaveGame != null)
            OnSaveGame();
    }

    public void LoadData()
    {
        BinaryFormatter formatter = new BinaryFormatter();
        FileStream saveFile = File.Open("Saves/save.binary", FileMode.Open);
        StartCoroutine(setGameData(formatter.Deserialize(saveFile) as GameData));
        saveFile.Close();
    }

    private GameData getGameData()
    {
        var movables = findMovablesInWorld();
        var player = findPlayerInWorld();
        GameData data = new GameData();
        foreach (var movable in movables)
        {
            data.MovableObjectMaterialIndex.Add(movable.CurrentMaterialIndex);
            data.MovableObjectPos.Add(new SerializableVector3(movable.transform.position));
        }
        data.PlayerPosition = new SerializableVector3(player.transform.position);
        data.PlayerRotation = new SerializableQuaternion(player.transform.rotation);
        data.SceneIndex = m_activeMainScene;
        return data;
    }

    private IEnumerator setGameData(GameData data)
    {
        AsyncOperation async = LoadMainScene(data.SceneIndex);
        while (async.progress < 1)
            yield return null;

        var player = findPlayerInWorld();
        player.SetPosition(new Vector3(data.PlayerPosition.x, data.PlayerPosition.y, data.PlayerPosition.z));
        player.SetRotation(new Quaternion(data.PlayerRotation.x, data.PlayerRotation.y, data.PlayerRotation.z, data.PlayerRotation.w));

        var movables = findMovablesInWorld();
        for (int i = 0; i < data.MovableObjectPos.Count; ++i)
        {
            movables[i].CurrentMaterialIndex = data.MovableObjectMaterialIndex[i];
            var pos = data.MovableObjectPos[i];
            movables[i].transform.position = new Vector3(pos.x, pos.y, pos.z);
        }
    }
}
