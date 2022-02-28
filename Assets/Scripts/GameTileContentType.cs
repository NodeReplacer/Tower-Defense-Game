//Tile Objects are just a means to track tile information. We don't change
//these object directly. 
//So we'll introduce seperate content and place them directly onto the
//board.
public enum GameTileContentType {
    Empty, Destination, Wall, SpawnPoint, Tower
}