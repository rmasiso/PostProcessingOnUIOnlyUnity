using UnityEngine;
using UnityEngine.UI;

// had to add this becuase was having some errors
#if UNITY_EDITOR
using UnityEditor;
using UnityEditor.SceneManagement; 
#endif

public class ImageOrMaterialColorControllerWithBlendControl : MonoBehaviour
{
       
    // Blend Mode Enums exposed in the Inspector -- useful if for not having to click on material everytime / go into shader everytime
    public UnityEngine.Rendering.BlendMode blendSrcMode = UnityEngine.Rendering.BlendMode.OneMinusSrcAlpha;
    public UnityEngine.Rendering.BlendMode blendDstMode = UnityEngine.Rendering.BlendMode.SrcAlpha;
    public UnityEngine.Rendering.BlendOp blendOperation = UnityEngine.Rendering.BlendOp.Add;

    [Tooltip("-1 means use shader's default queue ('Opaque'). Or specify 2000-3000 for Transparent queue, 2450 for \"Transparent\" Queue.")]
    public int renderQueue = -1; // -1 means use shader's default queue.  Or specify 2000-3000 for Transparent queue.


    private int blendSrcValue;
    private int blendDstValue;
    private int blendOpValue; 

    public Image myImage; // if this is a UI Image element, drag the 'image' component to this slot.
    public Material sourceMaterial; // the material with the special shader we created
    public Color myCustomColor = Color.white; // so that we see the color pop-up in inspector

    private Material newMaterial; // this is the material we are creating so we don't modify original
    public bool isImageType = false; // whether this is an image UI element 
    public bool isMeshType = true; // whether this script is on a 3d object

    void Awake() 
    {
        CreateMaterialInstance();
        UpdateColor();
        UpdateBlendModes();
    }

   void OnValidate()
    {
    
        #if UNITY_EDITOR
              // check if the object and scene are still valid before scheduling delayed call. 
                if (this == null || !gameObject.scene.isLoaded) return;

                EditorApplication.delayCall += () =>
                {
                    if (this == null || !gameObject.scene.isLoaded) return;

                    CreateMaterialInstance();
                    UpdateColor();
                    UpdateBlendModes();

                    // force Canvas update for UI elements (important for Edit mode updates)
                    if (myImage != null && isImageType)
                    {
                        Canvas.ForceUpdateCanvases(); 
                    }
                    //extra step for also updating scene view for mesh renderers in edit mode
                    if (TryGetComponent(out Renderer rend) && isMeshType)
                    {
                        EditorUtility.SetDirty(rend);
                        EditorSceneManager.MarkSceneDirty(gameObject.scene);
                    }
                };
        #endif

    }

    void CreateMaterialInstance()
    {
        //if (sourceMaterial == null) return;

        if (newMaterial == null) // super important for real-time update in editor and inspector
        // if the newMaterial has already been created, don't create a new one!

        {
            newMaterial = new Material(sourceMaterial); // creates copy of material, so we dont modify original

            // if this script is on a 3d object (not UI element)
            if (isMeshType)
            {
                TryGetComponent<Renderer>(out Renderer rend);
                rend.material = newMaterial;

            // if this script is on a canvas element
            } else if (isImageType)
            {
                myImage.material = newMaterial;
            }
        }

        if (renderQueue != -1)
        {
            newMaterial.renderQueue = renderQueue;
        }

        
    }

    void UpdateColor()
    {
        // this is where we set the color!!
        newMaterial.SetColor("_Color", myCustomColor);
    }
    
    void UpdateBlendModes()
    {
        if (newMaterial == null) return;

        blendSrcValue = (int)blendSrcMode;
        blendDstValue = (int)blendDstMode;
        blendOpValue = (int)blendOperation;  // Convert blend operation to int

        newMaterial.SetInt("_SrcFactor", blendSrcValue); 
        newMaterial.SetInt("_DstFactor", blendDstValue); 
        newMaterial.SetInt("_Opp", blendOpValue);        

        // logging in case of issues
         //Debug.Log($"Blend Src: {blendSrcMode} ({blendSrcValue}), Blend Dst: {blendDstMode} ({blendDstValue}), Blend Op: {blendOperation} ({blendOpValue})");


    }
        private void OnDestroy()
    {
        if (newMaterial != null) 
        {
            Destroy(newMaterial);
        }
    }
}