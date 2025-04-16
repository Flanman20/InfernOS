using UnityEngine;
public class SystemCrashManager : MonoBehaviour
{
    public SystemMessageBox MessageBox;
    public SystemCMDBox CMDBox;
    private bool isCrashTriggered = false;
    void Update()
    {
        if (FindObjectOfType<TestCrash>().IsCrashed && !isCrashTriggered)
        {
            isCrashTriggered = true;
            // Trigger external systems
            MessageBox.ShowMessage("InfernOS has crashed.", "Error");
            CMDBox.ShowMessage("InfernOS terminal simulation.", "Error");
        }
    }
}