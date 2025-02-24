using TMPro;
using UnityEngine;

public class SpawnIndicator : MonoBehaviour
{
    [SerializeField] SystemEnum.eCharType spawnerType;
    [SerializeField] TMP_Text spawnerText;
    [SerializeField] SpriteRenderer spawnerRenderer;

    public void SetIndicator(SystemEnum.eCharType type, Color indicatorColor )
    {
        spawnerType = type;
        spawnerText.SetText(type.ToString());
        spawnerRenderer.color = indicatorColor;
    }
#if UNITY_EDITOR
    public void UpdateColor(Color newcolor)
    {
        spawnerRenderer.color = newcolor;
    }
#endif
}
