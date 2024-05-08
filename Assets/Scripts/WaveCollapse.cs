using System;
using System.Collections;
using System.Collections.Generic;
using UnityEditor.Experimental.GraphView;
using UnityEngine;
using UnityEngine.Tilemaps;
using static UnityEditor.Progress;
using static UnityEngine.RuleTile.TilingRuleOutput;
using TMPro;

public class WaveCollapse : MonoBehaviour
{
    [SerializeField]
    private Vector2Int startTile;
    [SerializeField]
    private Vector2Int goalTile;
    [SerializeField]
    private TextMeshProUGUI statusText;
    [SerializeField]
    private GameObject nextLevelButton;

    Tilemap tilemap;
    BoundsInt bounds;
    TileBase[] allTiles;
    bool[,] edgemask;
    [SerializeField] TileBase[] fillTiles;
    Dictionary<string, TileBase> allOptions;
    Dictionary<Vector2Int, Dictionary<string, TileBase>> optionsBuffer;

    private void Awake() { // Awake is called when the script instance is being loaded
        tilemap = GetComponent<Tilemap>(); //get the tilemap the script is attached to
        bounds = tilemap.cellBounds; //get the bounds of the tilemap
        allTiles = tilemap.GetTilesBlock(bounds); //get all the tiles in the tilemap
        edgemask = new bool[bounds.size.x, bounds.size.y]; //mask to hold edges
        optionsBuffer = new Dictionary<Vector2Int, Dictionary<string, TileBase>>(); //Tracks possible options for each tile
        allOptions = new Dictionary<string, TileBase>();
        foreach(TileBase tile in fillTiles) {
            allOptions.Add(tile.name, tile);
        }
        nextLevelButton.SetActive(false);
    }
    void Start() { // Start is called before the first frame update
        InitializeTileOptions(); //fill the edgemask and options buffer
        Propagate(); //Reduce inital options for tiles given edge constraints
    }

    //fucntion that reduces the options for each tile based on the edges of its neighbors
    public void Propagate(){
        var updatedOptionsBuffer = new Dictionary<Vector2Int, Dictionary<string, TileBase>>();

        //loop over the options buffer (empty tiles) 
        foreach(var entry in optionsBuffer){
            var options = new Dictionary<string,TileBase>(entry.Value);

            //loop over the neighbors of the tile
            foreach (var dir in new[] { Vector2Int.up, Vector2Int.right, Vector2Int.down, Vector2Int.left }) {
                (char edge, int str_pos) = GetNeighborTile(entry.Key.x, entry.Key.y, dir);

                if (edge != 'n') { //if there is a neighbor
                    //remove all options that do not have a matching edge
                    foreach (var optionKey in new List<string>(options.Keys)) {
                        if (optionKey[str_pos] != edge){
                            options.Remove(optionKey);
                        }
                    }
                }
            }
            updatedOptionsBuffer[entry.Key] = options;   
        }
        optionsBuffer = updatedOptionsBuffer;          
    }

    //function that sets(collapses) all tiles that have one avalible option
    public void CollapseStep(){
        bool changed = false;
        var keysToCheck = new List<Vector2Int>(optionsBuffer.Keys);
        
        foreach(var key in keysToCheck){ // loop over the options buffer copy
            var options = optionsBuffer[key];

            if(options.Count == 1){ //single option => collapse the tile
                changed = true;
                string k = new List<string>(options.Keys)[0];
                //set the tile, update the all tiles array, remove from the options buffer
                tilemap.SetTile(new Vector3Int(key.x + bounds.xMin, key.y + bounds.yMin, 0), options[k]);
                optionsBuffer.Remove(key);
            }            
        }
        
        if(changed){ //update the allTiles array and propagate changes
            allTiles = tilemap.GetTilesBlock(bounds); 
            Propagate();
        }
    }

    //function to fill board with random of avalible options then update the options buffer
    public void FillBoard(){
 
        while (optionsBuffer.Count != 0) {
            //ensure the board is fully collapsed
            Propagate();
            CollapseStep();

            if(optionsBuffer.Count == 0){
                break;
            }
            //get the tile with the least amount of options
            int minEntryCount = int.MaxValue;
            KeyValuePair<Vector2Int, Dictionary<string, TileBase>> minEntry = new KeyValuePair<Vector2Int, Dictionary<string, TileBase>>();
            foreach(var entry in optionsBuffer){
                if(entry.Value.Count < minEntryCount){
                    minEntryCount = entry.Value.Count;
                    minEntry = entry;
                }
            }  

            //collapse the tile with the least amount of options
            Dictionary<string, TileBase> options = minEntry.Value;
            if(options.Count != 0){ //get a random option
                string[] keys = new List<string>(options.Keys).ToArray();
                string randomKey = keys[UnityEngine.Random.Range(0, keys.Length)];
                tilemap.SetTile(new Vector3Int(minEntry.Key[0] + bounds.xMin, minEntry.Key[1] + bounds.yMin, 0), options[randomKey]);
            } 
            else { //if there are no options, set a grass tile
                tilemap.SetTile(new Vector3Int(minEntry.Key[0] + bounds.xMin, minEntry.Key[1] + bounds.yMin, 0), allOptions["0000"]);
                Debug.Log("No options for X: " + minEntry.Key[0] + " Y: " + minEntry.Key[1]);
            }
            
            //update the all tiles array
            allTiles = tilemap.GetTilesBlock(bounds);
            optionsBuffer.Remove(minEntry.Key);
        }

        ProcessState();
    }

    public void ClearBoard(){
        for(int x = 0; x < bounds.size.x ;x++){
            for(int y= 0; y < bounds.size.y; y++){
                if(!edgemask[x,y]){
                    tilemap.SetTile(new Vector3Int(x + bounds.xMin, y + bounds.yMin, 0), null);
                }
            }
        }
        allTiles = tilemap.GetTilesBlock(bounds);
        optionsBuffer = new Dictionary<Vector2Int, Dictionary<string, TileBase>>();
        InitializeTileOptions(); // refill the options buffer
        Propagate();    // reduce the options
    }

    public bool SearchPath()
    {
        // perform BFS to check if path exists from start to goal
        int xbounds = bounds.size.x;
        int ybounds = bounds.size.y;
        Queue<Vector2Int> queue = new Queue<Vector2Int> ();
        bool[] visited = new bool[xbounds*ybounds];
        visited[startTile.x + startTile.y * xbounds] = true;
        queue.Enqueue(startTile);
        // tilemap.SetTile(new Vector3Int(startTile.x + bounds.xMin, startTile.y + bounds.yMin, 0), null);
        Debug.Log("Starting BFS");
        while(queue.Count > 0)
        {
            Vector2Int currentTile = queue.Dequeue();
            // tilemap.SetTile(new Vector3Int(currentTile.x + bounds.xMin, currentTile.y + bounds.yMin, 0), null);
            int currx = currentTile.x;
            int curry = currentTile.y;
            // Debug.Log("x: " + currentTile.x.ToString() + "y: " + currentTile.y.ToString());
            if (currentTile == goalTile) return true;
            foreach (Vector2Int dir in new[] { new Vector2Int(0,1), new Vector2Int(1,0), new Vector2Int(0,-1), new Vector2Int(-1,0) })
            {
                // check if out of bounds
                if (currx + dir.x < 0 || currx + dir.x >= xbounds
                    || curry + dir.y < 0 || curry + dir.y >= ybounds) continue;
                Vector2Int neighborTile = new Vector2Int(currx + dir.x, curry + dir.y);
                if (IsConnected(currentTile, neighborTile, dir))
                {
                    if (!visited[neighborTile.x + neighborTile.y * xbounds])
                    {
                        visited[neighborTile.x + neighborTile.y * xbounds] = true;
                        queue.Enqueue(neighborTile);
                    }
                }
            }
        }

        return false;
    }

    private void ProcessState()
    {
        if (SearchPath())
        {
            // win
            statusText.text = "You Win!";
            nextLevelButton.SetActive(true);
        } else
        {
            // lose
            statusText.text = "Try again";
        }
    }

////////////////////////////////////////////////////////////////////////
/// Helper functions 
////////////////////////////////////////////////////////////////////////

    //function to fill the edge mask and options buffer on start
    private void InitializeTileOptions(){
        for(int x = 0; x < bounds.size.x ;x++){
            for(int y= 0; y < bounds.size.y; y++){
                TileBase tile = allTiles[x + y * bounds.size.x];

                edgemask[x,y] = tile != null; //count iititialy non empty tiles as edges
                if(!edgemask[x,y]){
                    optionsBuffer[new Vector2Int(x, y)] = new Dictionary<string, TileBase>(allOptions);
                }
            }
        }
    }

    // helper function to get the neighbor tile of a given location with direction
    private (char,int) GetNeighborTile(int x, int y, Vector2Int direction) {
        int nx = x + direction.x;
        int ny = y + direction.y;
        int our_idx = -1;
        char key = 'n';
        if (nx >= 0 && nx < bounds.size.x && ny >= 0 && ny < bounds.size.y && allTiles[nx + ny * bounds.size.x] != null) {
            our_idx = (direction.x, direction.y) switch {
                (0, 1) => 0,
                (1, 0) => 1,
                (0, -1) => 2,
                (-1, 0) => 3,
                _ => our_idx // Default case, unchanged
            };
            //get the opposite index
            key = allTiles[nx + ny * bounds.size.x].name[(our_idx + 2) % 4];
        }
        return (key, our_idx);
    }

    // checks if currentTile and neighborTile are connected
    private bool IsConnected(Vector2Int currentTile, Vector2Int neighborTile, Vector2Int dir)
    {
        char port1, port2;
        int port1idx = -1, port2idx = -1;
        switch (dir.x, dir.y)
        {   
            case (0, 1):
                port1idx = 0;
                break;
            case (1, 0):
                port1idx = 1;
                break;
            case (0, -1):
                port1idx = 2;
                break;
            case (-1, 0):
                port1idx = 3;
                break;
            default:
                Console.WriteLine("This should never run.");
                break;
        }
        port2idx = (port1idx + 2) % 4;
        port1 = allTiles[currentTile.x + currentTile.y * bounds.size.x].name[port1idx];
        port2 = allTiles[neighborTile.x + neighborTile.y * bounds.size.x].name[port2idx];
        Debug.Log("Current tile: " + allTiles[currentTile.x + currentTile.y * bounds.size.x].name);
        Debug.Log("x: " + currentTile.x + ", y: " + currentTile.y);
        Debug.Log("Neighbor tile: " + allTiles[neighborTile.x + neighborTile.y * bounds.size.x].name);
        Debug.Log("x: " + neighborTile.x + ", y: " + neighborTile.y);
        Debug.Log("Port1: " + port1 + ", port 2: " + port2);
        bool connected = (port1 == port2) && (port1 == '1') ? true : false;
        Debug.Log("Connected: " + connected);
        return connected;
    }
}
