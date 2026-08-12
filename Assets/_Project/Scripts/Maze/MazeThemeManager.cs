using UnityEngine;

public class MazeThemeManager : MonoBehaviour
{
    [System.Serializable]
    public struct MazeTheme
    {
        public string themeName;
        public Material skyboxMaterial;
        public Material wallMaterial;
        public Material floorMaterial;
        public Color fogColor; // Her temaya özel sis rengi
    }

    [Header("3'lü Kombinasyon Listesi")]
    public MazeTheme[] themes;

    [Header("1. Temanın Seçilme Şansı (%)")]
    [Range(1, 100)]
    public int firstThemeChancePercent = 50;

    

    [Header("Referanslar")]
    public MazeBuilder mazeBuilder;

    void Awake()
    {
        ApplyRandomTheme();
    }

    public void ApplyRandomTheme()
    {
        if (themes == null || themes.Length == 0) return;

        int selectedIndex = 0;

        if (themes.Length > 1)
        {
            int randomRoll = Random.Range(1, 101);

            if (randomRoll <= firstThemeChancePercent)
            {
                selectedIndex = 0;
            }
            else
            {
                selectedIndex = Random.Range(1, themes.Length);
            }
        }

        MazeTheme selectedTheme = themes[selectedIndex];

        // 1. Gökyüzünü bas
        if (selectedTheme.skyboxMaterial != null)
        {
            RenderSettings.skybox = selectedTheme.skyboxMaterial;
            DynamicGI.UpdateEnvironment();
        }

        
       
        // 3. Duvar ve Zemin materyallerini MazeBuilder'a ver
        if (mazeBuilder != null)
        {
            if (selectedTheme.wallMaterial != null)
                mazeBuilder.wallMaterial = selectedTheme.wallMaterial;

            if (selectedTheme.floorMaterial != null)
                mazeBuilder.floorMaterial = selectedTheme.floorMaterial;
        }
    }
}
