using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class DissolveManager : MonoBehaviour
{
    [SerializeField]
    List<Material> materials = new List<Material>();

    float startTime;
    public float transitionDuration = 2f;
    private bool isTransitioning = true;
    [SerializeField]
    private bool isUnvissibleAtAwake = true;

    private void Awake()
    {
        if (isUnvissibleAtAwake)
        {
            SetValue(1);
        }
  
    }

    // Start is called before the first frame update
    void Start()
    {
        startTime = Time.time;
    }

    // Update is called once per frame
    void Update()
    {
        DissolveFromStealth();
    }

    void DissolveFromStealth()
    {
        if (!isTransitioning)
            return;


        float elapsedTime = Time.time - startTime;

        // Calculate the transition value using Mathf.Lerp
        float transitionValue = Mathf.Lerp(1f, 0f, elapsedTime / transitionDuration);
        SetValue(transitionValue);
        if (transitionValue <= 0)
        {
            isTransitioning = false;
            Destroy(this);
        }
        // Do something with the transition value, such as updating a material property, etc.
        // For example, you can assign it to the material's alpha channel:
        // renderer.material.color = new Color(1f, 1f, 1f, transitionValue);

        // If you want to set the value directly, you can use the following line:
        // float value = transitionValue;
    }

    public void SetValue(float value)
    {
        for (int i = 0; i < materials.Count; i++)
        {
            materials[i].SetFloat("_Dissolve", value);
        }
    }
}
