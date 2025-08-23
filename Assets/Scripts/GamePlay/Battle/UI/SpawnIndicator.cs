using Core.Foundation.Define;
using TMPro;
using UnityEngine;

public class SpawnIndicator : MonoBehaviour
{
    [SerializeField] private SystemEnum.eCharType spawnerType;
    [SerializeField] private TMP_Text spawnerText;
    [SerializeField] private SpriteRenderer spawnerRenderer;
    public long spawnFixedIndex;

    public void SetIndicator(SystemEnum.eCharType type, Color indicatorColor, long fixedIndex)
    {
        spawnerType = type;
        spawnerText.SetText(type.ToString());
        spawnerRenderer.color = indicatorColor;
        spawnFixedIndex = fixedIndex;
    }
    
#if UNITY_EDITOR
    public void UpdateColor(Color newcolor)
    {
        spawnerRenderer.color = newcolor;
    }
#endif
}
