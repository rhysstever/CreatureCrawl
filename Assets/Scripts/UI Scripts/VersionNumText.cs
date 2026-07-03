using TMPro;
using UnityEngine;

public class VersionNumText : MonoBehaviour
{
    [SerializeField]
    private TMP_Text versionText;

    void Start()
    {
        versionText.text = string.Format("v{0}", Application.version);
    }
}
