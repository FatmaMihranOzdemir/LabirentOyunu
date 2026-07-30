using UnityEngine;

namespace Maze
{
    public static class MockMazeProvider
    {
        /// <summary>
        /// Kişi 2 ve Kişi 3'ün kendi sistemlerini (Player ve Minimap) 
        /// test edebilmeleri için hazır 3x3 sahte labirent verisi üretir.
        /// </summary>
        public static MazeGridData GetMock3x3()
        {
            MazeGridData mockData = new MazeGridData(3, 3);

            // Başlangıç ve Bitiş belirle
            mockData.StartPosition = new Vector2Int(0, 0);
            mockData.ExitPosition = new Vector2Int(2, 2);

            mockData.GetCell(0, 0).Type = CellType.Start;
            mockData.GetCell(2, 2).Type = CellType.Exit;

            // Örnek yol açma (0,0 ile 0,1 arası duvarı kaldır)
            mockData.GetCell(0, 0).WallNorth = false;
            mockData.GetCell(0, 1).WallSouth = false;

            // (0,1 ile 1,1 arası duvarı kaldır)
            mockData.GetCell(0, 1).WallEast = false;
            mockData.GetCell(1, 1).WallWest = false;

            // (1,1 ile 2,1 arası duvarı kaldır)
            mockData.GetCell(1, 1).WallEast = false;
            mockData.GetCell(2, 1).WallWest = false;

            // (2,1 ile 2,2 arası duvarı kaldır - çıkışa ulaş)
            mockData.GetCell(2, 1).WallNorth = false;
            mockData.GetCell(2, 2).WallSouth = false;

            return mockData;
        }
    }
}
