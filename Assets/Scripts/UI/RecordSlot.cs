using UnityEngine;
using UnityEngine.UI;
using TMPro;

public class RecordSlot : MonoBehaviour
{
    [Header("UI")]
    [SerializeField] private Image characterImage;
    [SerializeField] private TextMeshProUGUI rankText;
    [SerializeField] private TextMeshProUGUI stageText;
    [SerializeField] private TextMeshProUGUI infoText;

    [Header("Grade Color")]
    [SerializeField] private Color ColorS = new Color(0, 0, 0);
    [SerializeField] private Color ColorA = new Color(0, 0, 0);
    [SerializeField] private Color ColorB = new Color(0, 0, 0);
    [SerializeField] private Color ColorC = new Color(0, 0, 0);

    public void Setup(PlayData record, Sprite sprite, int rank)
    {
        if (sprite != null)
        {
            characterImage.sprite = sprite;
        }

        rankText.text = $"{rank}";
        rankText.color = GetRankColor(rank);
        stageText.text = $"Stage {record.maxStage}\n<size=60%>Lv.{record.level}</size>";
        infoText.text = $"{record.className} <size=70%>({record.grade})</size>\n<size=50%>{record.playDate}</size>";
        infoText.color = GetGradeColor(record.grade);
    }

    // 등급에 색 표시
    private Color GetGradeColor(string grade)
    {
        return grade switch
        {
            "S" => ColorS,
            "A" => ColorA,
            "B" => ColorB,
            "C" => ColorC,
            _ => Color.white
        };
    }

    // 상위 기록에 색 표시
    private Color GetRankColor(int rank)
    {
        return rank switch
        {
            1 => new Color(1f, 0.84f, 0f),       // 금
            2 => new Color(0.75f, 0.75f, 0.75f), // 은
            3 => new Color(0.8f, 0.5f, 0.2f),    // 동
            _ => Color.white
        };
    }

    private void OnDestroy()
    {
        if (characterImage != null && characterImage.sprite != null)
        {
            Texture2D tex = characterImage.sprite.texture;
            Destroy(characterImage.sprite);
            if (tex != null) Destroy(tex);
        }
    }
}