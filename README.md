# 2D Unity Tilemap Generator with Wave Function Collapse

This Unity project implements a 2D tilemap generator using the Wave Function Collapse (WFC) algorithm. The generator populates a grid-based tilemap with tiles that follow specific adjacency rules, ensuring that the generated map adheres to predetermined patterns. This approach is useful for procedural content generation in games, such as creating dungeons, landscapes, or puzzle grids.

## Features

- **Wave Function Collapse**: The core algorithm reduces tile options based on neighboring constraints, ensuring a coherent and connected map.
- **Pathfinding**: A built-in Breadth-First Search (BFS) checks for a valid path from a start tile to a goal tile, ensuring the generated map is navigable.
- **Dynamic Tile Placement**: The generator dynamically places tiles based on adjacency rules, iteratively collapsing tiles with constrained options until the map is fully generated.
- **Edge Handling**: The algorithm accounts for edge cases by using an edge mask, preventing tiles at the boundaries from violating adjacency rules.

## Demo

[![Wave Function Collapse Tilemap Generator](https://img.youtube.com/vi/VIDEO_ID/0.jpg)](https://www.youtube.com/watch?v=VIDEO_ID)

*Click the image above to watch a demo video on YouTube.*

> Replace `VIDEO_ID` with the actual ID of your YouTube video.

## How It Works

1. **Initialization**: The tilemap is scanned to identify pre-filled tiles (edges) and empty tiles. The algorithm initializes possible tile options for each empty tile based on predefined patterns.
   
2. **Propagation**: The algorithm iteratively reduces the possible tiles for each cell by comparing the edges of neighboring tiles, ensuring that adjacent tiles are compatible.

3. **Collapse**: For each iteration, the algorithm identifies and collapses cells with a single valid tile option. The process repeats until all tiles are collapsed, or no more valid options are available.

4. **Pathfinding**: Once the map is generated, the BFS algorithm checks if a valid path exists between the start and goal tiles. If a path exists, the generation is successful; otherwise, the player is prompted to retry.

## Usage

1. **Start Tile**: Assign the start tile coordinates in the Unity Inspector.
2. **Goal Tile**: Assign the goal tile coordinates in the Unity Inspector.
3. **Fill Tiles**: Assign a set of tiles to be used by the generator in the Unity Inspector.
4. **Run the Generator**: Use the `FillBoard()` function to generate the tilemap.

## Example

When the generator runs, it will create a map like this:

- Tiles are placed respecting the rules defined by the generator.
- The map will always have a path from the start tile to the goal tile if possible.

## Script Breakdown

- **WaveCollapse.cs**:
  - **Awake()**: Initializes the tilemap and options buffer.
  - **Start()**: Initializes and runs the WFC algorithm.
  - **Propagate()**: Reduces tile options based on neighboring tiles.
  - **CollapseStep()**: Collapses tiles with only one valid option.
  - **FillBoard()**: Fills the board using WFC until all tiles are placed.
  - **ClearBoard()**: Resets the board for a new generation.
  - **SearchPath()**: Uses BFS to check for a valid path between the start and goal tiles.
  - **ProcessState()**: Displays success or failure based on pathfinding results.

## Requirements

- Unity 2021.3 or later
- TextMeshPro for UI components

## Contributing

Feel free to fork this repository and submit pull requests. Contributions are welcome!

## License

This project is licensed under the MIT License - see the [LICENSE](LICENSE) file for details.
