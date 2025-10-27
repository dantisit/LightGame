using TMPro;
using UnityEngine;
using System.Text.RegularExpressions;
using System.Collections.Generic;

[ExecuteAlways]
public class TMPInputFieldFormatter : MonoBehaviour
{
    [System.Serializable]
    public class RegexReplacement
    {
        public enum CaseTransform
        {
            None,
            Upper,
            Lower
        }
        
        public string pattern;
        public string replacement;
        public RegexOptions options;
        public CaseTransform caseTransform = CaseTransform.None; 
    }
    
    [SerializeField] private List<RegexReplacement> replacements = new List<RegexReplacement>()
    {
        new RegexReplacement { pattern = @"[^A-Z0-9]", replacement = "" }, // Cleanup
        new RegexReplacement { pattern = @"(.{3})(?=.)", replacement = "$1-" } // Format
    };
    
    [SerializeField] private TMP_InputField inputField;
    private bool isUpdating;
    private string lastText = "";
    
    void OnValidate()
    {
        inputField ??= GetComponent<TMP_InputField>();
        
        if (!Application.isPlaying && inputField != null)
        {
            Format(inputField.text);
        }
    }
    
    void OnEnable()
    {
        if (inputField != null)
        {
            inputField.onValueChanged.AddListener(OnInputChanged);
            lastText = inputField.text;
        }
    }

    void OnDisable()
    {
        if (inputField != null)
        {
            inputField.onValueChanged.RemoveListener(OnInputChanged);
        }
    }
    
    void Start()
    {
        if (inputField != null)
            Format(inputField.text);
    }
    
    void Update()
    {
        if (!Application.isPlaying && inputField != null)
        {
            if (inputField.text != lastText)
            {
                Format(inputField.text);
                lastText = inputField.text;
            }
        }
    }
    
    void OnInputChanged(string input)
    {
        Format(input);
    }
    
    void Format(string input)
    {
        if (isUpdating || inputField == null) return;
        isUpdating = true;
        
        int caretPos = inputField.caretPosition;
        string beforeCaret = input.Substring(0, Mathf.Min(caretPos, input.Length));
        
        // Apply all replacements in sequence
        string formatted = ApplyReplacements(input);
        
        if (inputField.text != formatted)
        {
            inputField.text = formatted;
            
            // Adjust caret position
            string formattedBeforeCaret = ApplyReplacements(beforeCaret);
            inputField.caretPosition = formattedBeforeCaret.Length;
        }
        
        lastText = formatted;
        isUpdating = false;
    }
    
    private string ApplyReplacements(string input)
    {
        string result = input;
        
        foreach (var rep in replacements)
        {
            if (string.IsNullOrEmpty(rep.pattern)) continue;
            
            result = rep.caseTransform switch
            {
                RegexReplacement.CaseTransform.Upper => result.ToUpper(),
                RegexReplacement.CaseTransform.Lower => result.ToLower(),
                _ => result
            };
            result = Regex.Replace(result, rep.pattern, rep.replacement ?? "", rep.options);
        }
        
        return result;
    }
}