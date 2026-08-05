using UnityEngine;
using UnityEditor;

public class SnapWallsEditor : EditorWindow
{
    [MenuItem("Tools/Labirent/Duvarları Kenetle")]
    public static void SnapWalls()
    {
        // Labirent_Grup altındaki tüm duvarları bul
        GameObject parentObj = GameObject.Find("Labirent_Grup");
        if (parentObj == null)
        {
            Debug.LogError("Hierarchy panelinde 'Labirent_Grup' adında bir obje bulunamadı!");
            return;
        }

        Undo.RegisterCompleteObjectUndo(parentObj, "Duvarları Kenetle");

        foreach (Transform child in parentObj.transform)
        {
            // Pozisyonları en yakın 0.5 veya 1 birime yuvarlayarak boşlukları kapatır
            Vector3 pos = child.localPosition;
            pos.x = Mathf.Round(pos.x * 2f) / 2f;
            pos.y = Mathf.Round(pos.y * 2f) / 2f;
            pos.z = Mathf.Round(pos.z * 2f) / 2f;
            child.localPosition = pos;
        }

        Debug.Log("🤖 Sistem: Tüm duvarlar arasındaki boşluklar kapatıldı ve kenetlendi!");
    }
}
