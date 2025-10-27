using TMPro;
using UnityEngine;
using System.Text.RegularExpressions;

public class TMPFormatter : MonoBehaviour
{
    [SerializeField] private string pattern = @"(\d{3})(\d{3})(\d{3})";
    [SerializeField] private string replacement = "$1-$2-$3";
    
    [SerializeField] private TMP_Text tmpText;
    private bool isUpdating;
    
    void OnValidate()
    {
        tmpText ??= GetComponent<TMP_Text>();
    }
    
    void OnEnable()
    {
        TMPro_EventManager.TEXT_CHANGED_EVENT.Add(OnTextMeshChanged);
    }

    void OnDisable()
    {
        TMPro_EventManager.TEXT_CHANGED_EVENT.Remove(OnTextMeshChanged);
    }

    void OnTextMeshChanged(Object obj)
    {
        if (obj == tmpText)
        {
            Format(tmpText.text);
        }
    }
    
    void Start()
    {
        Format(tmpText.text);
    }
    
    void Format(string input)
    {
        if (isUpdating || tmpText == null) return;
        isUpdating = true;
        
        string formatted = Regex.Replace(input, pattern, replacement);
        
        if (tmpText.text != formatted)
        {
            tmpText.text = formatted;
        }
        
        isUpdating = false;
    }
}