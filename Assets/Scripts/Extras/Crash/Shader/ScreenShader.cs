using UnityEngine;

[ExecuteInEditMode]
public class ScreenShearEffect : MonoBehaviour
{
    [SerializeField] private Material shearMaterial;
    [Range(0f, 1f)] public float shearIntensity = 0.1f;
    public float shearDuration = 1.0f;

    private float timer = 0f;
    private bool isShearing = false;

    private void OnRenderImage(RenderTexture src, RenderTexture dest)
    {
        if (isShearing && shearMaterial != null)
        {
            shearMaterial.SetFloat("_ShearAmount", shearIntensity);
            Graphics.Blit(src, dest, shearMaterial);
        }
        else
        {
            Graphics.Blit(src, dest);
        }
    }

    public void StartShearing(float intensity, float duration)
    {
        shearIntensity = intensity;
        shearDuration = duration;
        timer = 0f;
        isShearing = true;
    }

    private void Update()
    {
        if (isShearing)
        {
            timer += Time.unscaledDeltaTime; // Works without regular deltaTime
            if (timer >= shearDuration)
            {
                isShearing = false;
            }
        }
    }
}
