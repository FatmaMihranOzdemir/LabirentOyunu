// ÖRNEK: MazeBuilder'a nasıl abone olunur.
// Kişi 2 ve Kişi 3 kendi scriptlerinde bu deseni kullanmalı.
// OnMazeBuilt += ... şeklinde doğrudan abone OLMAYIN; Start sırası garanti değil,
// ilk labirenti kaçırırsınız. SubscribeAndSyncNow bunu çözer.
using UnityEngine;

public class MazeSubscriberTest : MonoBehaviour
{
    public MazeBuilder mazeBuilder;

    void Start()
    {
        mazeBuilder.SubscribeAndSyncNow(OnMazeReady);
    }

    void OnMazeReady(MazeGridData grid)
    {
        Debug.Log($"[Test Abone] Labirent alındı! {grid.Width}x{grid.Height}, Giriş: {grid.StartPosition}, Çıkış: {grid.ExitPosition}");
    }

    void OnDestroy()
    {
        if (mazeBuilder != null) mazeBuilder.Unsubscribe(OnMazeReady);
    }
}
