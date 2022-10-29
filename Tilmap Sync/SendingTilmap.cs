using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using Unity.Netcode;
using UnityEngine.Tilemaps;

public class SendingTilmap : NetworkBehaviour
{
    [SerializeField] private Tile[] tiles;
    [SerializeField] private Tilemap map;
    List<Vector2Int> lodedTiled = new List<Vector2Int>();

    private bool start = false;

    private void Awake()
    {

    }

    private void Start()
    {

    }

    private void Update()
    {
        if (IsClient)
        {
            if (!start)
                clientStart();
        }
        
        if (IsServer)
        {
            if (!start)
                serverStart();

            if (Input.GetKeyDown(KeyCode.T))
            {
                newTile(new Vector2Int(Random.Range(-5, 5), Random.Range(-5, 5)), tiles[0]);
            }
        }
    }

    private void serverStart()
    {
        start = true;
        Debug.Log("startTileGen");
        genIland();
    }

    private void genIland()
    {
        newTile(new Vector2Int(0, -3), tiles[0]);
        newTile(new Vector2Int(-1, -3), tiles[0]);
        newTile(new Vector2Int(1, -3), tiles[0]);
        newTile(new Vector2Int(-2, -3), tiles[0]);
        newTile(new Vector2Int(2, -3), tiles[0]);
    }

    public void newTile(Vector2Int pos, Tile material)
    {
        map.SetTile(new Vector3Int(pos.x, pos.y, 0), material);
        if (IsServer)
        {
            sendTilemap(pos);
            lodedTiled.Add(pos);
        }
    }

    public void removeTile(Vector2Int pos)
    {
        map.SetTile(new Vector3Int(pos.x, pos.y, 0), null);
        lodedTiled.Remove(pos);
    }

    private void sendTilemap(Vector2Int pos)
    {
        int id = 0;
        int i = 0;
        foreach(Tile tile in tiles)
        {
            if(tile == map.GetTile(new Vector3Int(pos.x, pos.y, 0)))
            {
                id = i;
            }
            i++;
        }
        GetUpdateClientRpc(pos, id + 1);
    }

    private void sendTilemapto(Vector2Int pos, ulong pid)
    {
        int id = 0;
        int i = 0;
        foreach (Tile tile in tiles)
        {
            if (tile == map.GetTile(new Vector3Int(pos.x, pos.y, 0)))
            {
                id = i;
            }
            i++;
        }
        UpdateToClientRpc(pos, id + 1, pid);
    }

    [ClientRpc]
    private void GetUpdateClientRpc(Vector2Int pos, int id)
    {
        if (!IsHost)
        {
            if (id != 0)
                newTile(pos, tiles[id - 1]);
            else
                removeTile(pos);
        }
    }

    [ClientRpc]
    private void UpdateToClientRpc(Vector2Int pos, int id, ulong pid)
    {
        if (pid != OwnerClientId)
            return;
        if (!IsHost)
        {
            Debug.Log("wow");
            if (id != 0)
                newTile(pos, tiles[id - 1]);
            else
                removeTile(pos);
        }
    }

    [ServerRpc(RequireOwnership = false)]
    private void newPlServerRpc(ulong pid)
    {
        if (IsServer)
        {
            Debug.Log(OwnerClientId);
            SendAllToPlayer(pid);
        }
    }

    private void SendAllToPlayer(ulong pid)
    {
        Debug.Log("player : " + pid);
        foreach (Vector2Int pos in lodedTiled)
        {
            sendTilemapto(pos, pid);
        }
    }
    private void SendAllPlayer()
    {
        foreach (Vector2Int pos in lodedTiled)
        {
            sendTilemap(pos);
        }
    }

    private void clientStart()
    {
        Debug.Log("hi");
        newPlServerRpc(OwnerClientId);
        if(!IsHost)
            start = true;
    }
}
