using UnityEngine;

namespace Maze
{
    /// <summary>
    /// ARTIK GEREKLİ DEĞİL — gerçek labirent üretimi çalışıyor.
    /// Yeni kodda MazeGenerator.Generate(10, 10) kullanın.
    ///
    /// Bu sınıf, gerçek üretim hazır olmadan önce Kişi 2 ve Kişi 3'ün paralel
    /// çalışabilmesi için yazılmıştı. Sadece geriye dönük uyumluluk için duruyor.
    /// </summary>
    [System.Obsolete("MockMazeProvider artık gerekli değil. MazeGenerator.Generate(width, height) kullanın.")]
    public static class MockMazeProvider
    {
        /// <summary>
        /// Kişi 2 ve Kişi 3'ün kendi sistemlerini (Player ve Minimap)
        /// test edebilmeleri için hazır 3x3 sahte labirent verisi üretir.
        /// </summary>
        public static MazeGridData GetMock3x3()
        {
            MazeGridData mockData = new MazeGridData(3, 3);

            mockData.StartPosition = new Vector2Int(0, 0);
            mockData.ExitPosition = new Vector2Int(2, 2);

            mockData.GetCell(0, 0).Type = CellType.Start;
            mockData.GetCell(2, 2).Type = CellType.Exit;

            // (0,0) ile (0,1) arası
            mockData.GetCell(0, 0).WallNorth = false;
            mockData.GetCell(0, 1).WallSouth = false;

            // (0,1) ile (1,1) arası
            mockData.GetCell(0, 1).WallEast = false;
            mockData.GetCell(1, 1).WallWest = false;

            // (1,1) ile (2,1) arası
            mockData.GetCell(1, 1).WallEast = false;
            mockData.GetCell(2, 1).WallWest = false;

            // (2,1) ile (2,2) arası — çıkışa ulaş
            mockData.GetCell(2, 1).WallNorth = false;
            mockData.GetCell(2, 2).WallSouth = false;

            // ÖNEMLİ: Dış kapı deliklerini veride de açıyoruz.
            // MazeBuilder artık istisna mantığı içermez, yalnızca veriye bakar.
            // Bu satırlar olmazsa giriş odası kapalı çıkar ve oyuncu kilitli kalır.
            mockData.GetCell(0, 0).WallSouth = false;  // giriş: güney kenar
            mockData.GetCell(2, 2).WallNorth = false;  // çıkış: kuzey kenar

            return mockData;
        }
    }
}
