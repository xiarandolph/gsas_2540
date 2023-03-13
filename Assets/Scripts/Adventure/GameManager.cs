using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

namespace Adventure
{

// Initializes the world and centers the player
public class GameManager : MonoBehaviour
{
    public GameObject wallPrefab;
    public GameObject floorPrefab;
    public GameObject litFloorPrefab;
    public GameObject goal;
    public GameObject exit;
    
    enum Dir {UP = 1, LEFT = 2, DOWN = 4, RIGHT = 8};   // for bit addressing

    // variables for generating the world
    public float wallDensity = 0.5f;       // density of generated walls
    public float lightDensity = 0.15f;       // density of lit positions
    public int width;
    public int height;
    byte [,] grid;
    bool [,] visited;   // helper array for generation of grid

    // Start is called before the first frame update
    void Start()
    {
        InitializeWorld();
        // position goal somewhere on top border
        goal.transform.position = new Vector3(4*Random.Range(0, width-1), 4*(height-1), 0);
        // position exit at the bottom right corner
        exit.transform.position = new Vector3(4*(width-1), 0, 0);
    }

    // InitializeWorld generates all necessary prefabs within the world
    void InitializeWorld()
    {
        grid = new byte[width, height];
        visited = new bool[width, height];

        // initialize all grid cells to walls in all directions
        for (int i = 0; i < width; ++i)
        {
            for (int j = 0; j < height; ++j)
            {
                grid[i, j] |= 0xF;
            }
        }
        GenerateGrid(0, 0);

        // Instantiate floor & wall prefabs for every position
        /* Each position instantiates 4 prefabs (2x2) of floors, then walls up, up-right,
        and right depending on the grid values. Note that walls are full cubes and not 
        thin panels.
        */
        for (int i = 0; i < width; ++i)
        {
            for (int j = 0; j < height; ++j)
            {
                int x = 4 * i, y = 4 * j;   // Prefab sizes are 2x2, each pos instantiats 2x2 prefabs

                // Instantiate floors w/ lights if at start or based on density
                // lights only at bottom left of each 2x2 because the rest might be walls
                if (Random.value <= lightDensity || (x == 0 && y == 0))
                    Instantiate(litFloorPrefab, new Vector3(x, y, 0), Quaternion.identity);
                else
                    Instantiate(floorPrefab, new Vector3(x, y, 0), Quaternion.identity);

                // instantiate rest of floors in the 2x2 ...
                Instantiate(floorPrefab, new Vector3(x + 2, y, 0), Quaternion.identity);
                Instantiate(floorPrefab, new Vector3(x, y + 2, 0), Quaternion.identity);
                Instantiate(floorPrefab, new Vector3(x + 2, y + 2, 0), Quaternion.identity);

                // Make sure to instantiate top border & right border, otherwise based on density
                if (j == height - 1 || ((grid[i,j] & (byte) Dir.UP) > 0 && Random.value <= wallDensity))
                    Instantiate(wallPrefab, new Vector3(x, y + 2, 0), Quaternion.identity);
                if (i == width - 1 || ((grid[i,j] & (byte) Dir.RIGHT) > 0 && Random.value <= wallDensity))
                    Instantiate(wallPrefab, new Vector3(x + 2, y, 0), Quaternion.identity);
                if (j == height - 1 || i == width - 1 || Random.value <= wallDensity)
                    Instantiate(wallPrefab, new Vector3(x + 2, y + 2, 0), Quaternion.identity); // corner wall
            }
        }
        // Instantiate bottom border
        for (int i = 0; i < width; ++i)
        {
            Instantiate(wallPrefab, new Vector3(4*i, -2, 0), Quaternion.identity);
            Instantiate(wallPrefab, new Vector3(4*i + 2, -2, 0), Quaternion.identity);
        }
        // Instantiate left border
        for (int i = 0; i < height; ++i)
        {
            Instantiate(wallPrefab, new Vector3(-2, 4*i, 0), Quaternion.identity);
            Instantiate(wallPrefab, new Vector3(-2, 4*i + 2, 0), Quaternion.identity);
        }
    }

    // GenerateGrid uses DFS to create a maze out of the grid
    // Everytime DFS goes in a certain direction, the cell is set to not have a wall that direction
    // Maze generation is used along with density to make sure theres always valid paths
    void GenerateGrid(int x, int y)
    {
        visited[x, y] = true;

        List<int> directions = new List<int>(new int[]{0, 1, 2, 3});
        int r;

        while (directions.Count > 0)
        {
            // choose random direction to go to
            r = directions[Random.Range(0, directions.Count)];
            switch (r)
            {
                case 0: // move up
                    if (y + 1 < height && !visited[x, y + 1])
                    {
                        grid[x, y] ^= (byte) Dir.UP;
                        grid[x, y+1] ^= (byte) Dir.DOWN;
                        GenerateGrid(x, y + 1);
                    }
                    break;
                case 1: // move left
                    if (x - 1 >= 0 && !visited[x - 1, y])
                    {
                        grid[x, y] ^= (byte) Dir.LEFT;
                        grid[x-1, y] ^= (byte) Dir.RIGHT;
                        GenerateGrid(x - 1, y);
                    }
                    break;
                case 2: // move down
                    if (y - 1 >= 0 && !visited[x, y - 1])
                    {
                        grid[x, y] ^= (byte) Dir.DOWN;
                        grid[x, y-1] ^= (byte) Dir.UP;
                        GenerateGrid(x, y - 1);
                    }
                    break;
                case 3: // move right
                    if (x + 1 < width && !visited[x + 1, y])
                    {
                        grid[x, y] ^= (byte) Dir.RIGHT;
                        grid[x+1, y] ^= (byte) Dir.LEFT;
                        GenerateGrid(x + 1, y);
                    }
                    break;
            }
            directions.Remove(r);
        }
    }
}

} // namespace Adventure