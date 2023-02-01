using UnityEngine;

public class BugRepairer : MonoBehaviour
{
    [SerializeField] Transform[] Scene;

    GameObject Player;

    RaycastHit hit;

    string _textureName;

    //////////////////////////////////////////////////////////////////////////////////////////////	
    void Start()
    {
        Player = GameObject.Find("Player");
    }

    //////////////////////////////////////////////////////////////////////////////////////////////	
    void Update()
    {
        DebugTools();
    }

    void FixedUpdate()
    {
        IsInside();
    }
    //////////////////////////////////////////////////////////////////////////////////////////////	
    //void OnGUI()
    //{
    //    if (_textureName != "") GUI.Label(new Rect(100, 100, 200, 200), _textureName);
    //}

    void DebugTools()
    {
        //if (Input.GetKey(KeyCode.LeftAlt))
        //{
        //    PlayerSynthesis.characterController.height = PlayerSynthesis.InstallCrouchHeight;

        //    Physics.Raycast(Player.transform.position, -Player.transform.up, out hit, Mathf.Infinity);

        //    _textureName = GetSurfaceIndex(hit.collider, hit.point);
        //}
        //else _textureName = "";

        if (Input.GetKeyDown(KeyCode.Escape)) Application.Quit();

        Tp();
    }

    void Tp()
    {
        Vector3 pos = Scene[4].transform.position, pos0 = Scene[5].transform.position;

        if (Input.GetKey(KeyCode.N))
        {
            pos.y -= 0.1f;
            pos0.y -= 0.1f;
        }
        if (Input.GetKey(KeyCode.M))
        {
            pos.y += 0.1f;
            pos0.y += 0.1f;
        }

        Scene[4].transform.position = pos;
        Scene[5].transform.position = pos0;

        if (Input.GetKey(KeyCode.Keypad1)) Player.transform.position = Scene[0].position;
        if (Input.GetKey(KeyCode.Keypad2)) Player.transform.position = Scene[1].position;
        if (Input.GetKey(KeyCode.Keypad3)) Player.transform.position = Scene[2].position;
        if (Input.GetKey(KeyCode.Keypad4)) Player.transform.position = Scene[3].position;
        if (Input.GetKey(KeyCode.Keypad5)) Player.transform.position = Scene[4].position;
        if (Input.GetKey(KeyCode.Keypad6)) Player.transform.position = Scene[5].position;
    }

    //////////////////////////////////////////////////////////////////////////////////////////////	
    //void isInside()
    //{
    //    Collider[] hitColliders = Physics.OverlapSphere(Foot.transform.position, minDistance);

    //    bool inRoom = false;

    //    foreach (var col in hitColliders) if (col.name == "Room") inRoom = true;

    //    if (inRoom == true)
    //    {
    //        inRoom = false;
    //        PlayerSynthesis.isInside = true;
    //    }
    //    else PlayerSynthesis.isInside = false;
    //}
    void IsInside()
    {
        PlayerSynthesis.isInside = false;

        Physics.Raycast(Player.transform.position, -Player.transform.up, out hit, Mathf.Infinity);

        if (GetSurfaceIndex(hit.collider, hit.point) == "build_building_02_a") PlayerSynthesis.isInside = true;
    }

    //////////////////////////////////////////////////////////////////////////////////////////////	
    string GetSurfaceIndex(Collider col, Vector3 worldPos)
    {
        string textureName = "";

        if (col.GetType() == typeof(TerrainCollider))
        {
            Terrain terrain = col.GetComponent<Terrain>();
            TerrainData terrainData = terrain.terrainData;
            float[] textureMix = GetTerrainTextureMix(worldPos, terrainData, terrain.GetPosition());
            int textureIndex = GetTextureIndex(worldPos, textureMix);
            textureName = terrainData.splatPrototypes[textureIndex].texture.name;
        }
        else textureName = GetMeshMaterialAtPoint(worldPos, new Ray(Vector3.zero, Vector3.zero));

        return textureName;
    }

    string GetMeshMaterialAtPoint(Vector3 worldPosition, Ray ray)
    {
        if (ray.direction == Vector3.zero)
        {
            ray = new Ray(worldPosition + Vector3.up * 0.01f, Vector3.down);
        }

        if (!Physics.Raycast(ray, out hit))
        {
            return "";
        }

        Renderer r = hit.collider.GetComponent<Renderer>();
        MeshCollider mc = hit.collider as MeshCollider;

        if (r == null || r.sharedMaterial == null || r.sharedMaterial.mainTexture == null || r == null)
        {
            return "";
        }
        else if (!mc || mc.convex)
        {
            return r.material.mainTexture.name;
        }

        int materialIndex = -1;
        Mesh m = mc.sharedMesh;
        int triangleIdx = hit.triangleIndex;
        int lookupIdx1 = m.triangles[triangleIdx * 3];
        int lookupIdx2 = m.triangles[triangleIdx * 3 + 1];
        int lookupIdx3 = m.triangles[triangleIdx * 3 + 2];
        int subMeshesNr = m.subMeshCount;

        for (int i = 0; i < subMeshesNr; i++)
        {
            int[] tr = m.GetTriangles(i);

            for (int j = 0; j < tr.Length; j += 3)
            {
                if (tr[j] == lookupIdx1 && tr[j + 1] == lookupIdx2 && tr[j + 2] == lookupIdx3)
                {
                    materialIndex = i;

                    break;
                }
            }

            if (materialIndex != -1) break;
        }

        string textureName = r.materials[materialIndex].mainTexture.name;

        return textureName;
    }

    float[] GetTerrainTextureMix(Vector3 worldPos, TerrainData terrainData, Vector3 terrainPos)
    {
        int mapX = (int)(((worldPos.x - terrainPos.x) / terrainData.size.x) * terrainData.alphamapWidth);
        int mapZ = (int)(((worldPos.z - terrainPos.z) / terrainData.size.z) * terrainData.alphamapHeight);

        float[,,] splatmapData = terrainData.GetAlphamaps(mapX, mapZ, 1, 1);

        float[] cellMix = new float[splatmapData.GetUpperBound(2) + 1];

        for (int n = 0; n < cellMix.Length; n++) cellMix[n] = splatmapData[0, 0, n];

        return cellMix;
    }

    int GetTextureIndex(Vector3 worldPos, float[] textureMix)
    {
        float maxMix = 0;
        int maxIndex = 0;

        for (int n = 0; n < textureMix.Length; n++)
        {
            if (textureMix[n] > maxMix)
            {
                maxIndex = n;
                maxMix = textureMix[n];
            }
        }

        return maxIndex;
    }
}
