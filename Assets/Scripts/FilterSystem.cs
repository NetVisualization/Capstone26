using UnityEngine;

public class FilterSystem : MonoBehaviour
{
    [SerializeField] private NodeSpawnerScript spawner;

    private void Awake()
    {
        if (!spawner)
            spawner = FindFirstObjectByType<NodeSpawnerScript>();
    }

    public void HideAllConnections()
    {
        foreach (var c in spawner.Connections)
            if (c) c.SetActive(false);
    }

    public void ShowAllConnections()
    {
        foreach (var c in spawner.Connections)
            if (c) c.SetActive(true);
    }
}
