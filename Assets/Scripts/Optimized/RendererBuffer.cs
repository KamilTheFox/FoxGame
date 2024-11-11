using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using UnityEngine;
using UnityEngine.Rendering;

[Serializable]
public class RendererBuffer : IEnumerable, IDisposable, IPropertysRendererUpdate
{
    private SpriteRenderer sprite;

    private Renderer[] originalRenderers;

    private Renderer[] renderersLOD_1;
    private Renderer[] renderersLOD_2;
    private Renderer[] renderersLOD_3;
    private Renderer[] renderersLOD_4;
    private Renderer[] renderersLOD_5;
    private Renderer[] renderersLOD_6;

    public bool debug;

    [SerializeField]
    private byte[] levelLods;

    [SerializeField] private int currentLODLevel = 0;

    private bool isDisposed = false;

    private const float OFFSET = 1.1f;

    public Transform Transform { get; private set; }

    private static Camera rendererSpriteCamera;

    RenderTexture renderTexture;

    private Camera RendererSpriteCamera
    {
        get
        {
            if (rendererSpriteCamera == null)
            {
                GameObject tempCameraObject = new GameObject("Temp Camera");
                Camera tempCamera = tempCameraObject.AddComponent<Camera>();
                tempCamera.clearFlags = CameraClearFlags.SolidColor;
                tempCamera.backgroundColor = Color.clear;
                tempCamera.cullingMask = LayerMask.GetMask("TransparentFX");
                tempCamera.orthographic = true;
                rendererSpriteCamera = tempCamera;
            }
            if (rendererSpriteCamera.gameObject.activeSelf == false)
                rendererSpriteCamera.gameObject.SetActive(true);
            return rendererSpriteCamera;
        }
    }

    public bool IsDynamicSprite { private get; set; }

    public bool IsGenerateSprite { private get; set; } = true;

    private bool isCreatedSprite;

    private bool was3DMode;

    private Bounds? bounds;

    public InputJobPropertyData inputJobProperty => Settings.InputJobPropertyRendererData;

    public RendererBuffer(GameObject gameObject)
    {
        SetGameObject(gameObject);
    }

    public void SetGameObject(GameObject gameObject)
    {
        if (gameObject == null)
            throw new ArgumentNullException(nameof(gameObject));

        List<Renderer> list = new List<Renderer>();
        Transform = gameObject.transform;
        list.AddRange(gameObject.GetComponentsInChildren<Renderer>());
        if (gameObject.gameObject.TryGetComponent<Renderer>(out var r))
        {
            list.Add(r);
        }
        originalRenderers = list.ToArray();
    }

    public void GenerateLODSprite()
    {
            if (debug)
                Debug.Log("Start GenerateSprite");
            int oldlayer = Transform.gameObject.layer;

            SetVisible(false);
            foreach (var renderer in GetRenderersLOD(0))
            {
                if (renderer == null) continue;
                renderer.gameObject.layer = LayerMask.NameToLayer("TransparentFX");
                renderer.transform.hasChanged = true;
                renderer.enabled = true;
            }
            if (debug)
                Debug.Log("Set priperty renderer");
            if (this.bounds == null)
            {
                this.bounds = CalculateBounds();
            }
            if (debug)
                Debug.Log("calculate Bounds");
            var bounds = this.bounds.Value;

            float maxSize = Mathf.Max(bounds.size.x, bounds.size.y, bounds.size.z);

            Vector3 dirCam = (CameraControll.MainCamera.transform.position - bounds.center).normalized;

            float XorZ = dirCam.y < dirCam.x ? bounds.size.x : bounds.size.z;

            int x = (int)(256 * XorZ / maxSize);

            renderTexture = new RenderTexture(x, 256, 24);

            RendererSpriteCamera.transform.position = bounds.center + dirCam * (maxSize * OFFSET);

            RendererSpriteCamera.orthographicSize = maxSize * 0.5f * OFFSET;

            RendererSpriteCamera.transform.LookAt(bounds.center);

            RendererSpriteCamera.targetTexture = renderTexture;

            if (debug)
                Debug.Log("Cam Renderer");

            RendererSpriteCamera.Render();
            Texture2D texture = new Texture2D(renderTexture.width, renderTexture.height, TextureFormat.RGBA32, false);
            RenderTexture.active = renderTexture;
            texture.ReadPixels(new Rect(0, 0, renderTexture.width, renderTexture.height), 0, 0);
            texture.Apply();

            Sprite spriteCreaded = Sprite.Create(texture, new Rect(0, 0, texture.width, texture.height), new Vector2(0.5f, 0f));

            bool isSetScale = false;

            if (debug)
                Debug.Log("Generate gameObject");

            if (this.sprite == null)
            {
                GameObject spriteObject = new GameObject("LOD Sprite");
                this.sprite = spriteObject.AddComponent<SpriteRenderer>();
                spriteObject.transform.SetParent(Transform);
                this.sprite.transform.localPosition = Vector3.zero;
                this.sprite.flipX = true;
                isSetScale = true;
            }
            if (MathF.Abs(dirCam.y) < 0.5F)
            {
                Vector3 vect = this.sprite.transform.position;
                vect.y = bounds.min.y;
                this.sprite.transform.position = vect;
            }
            else
                this.sprite.transform.localPosition = Vector3.zero;

            this.sprite.sprite = spriteCreaded;



            if (isSetScale)
            {
                if (debug)
                    Debug.Log("SetScale");
                Bounds boundSprite = this.sprite.sprite.bounds;

                float scaleX = bounds.size.x / boundSprite.size.x;
                float scaleY = bounds.size.y / boundSprite.size.y;
                float scaleZ = bounds.size.z / boundSprite.size.z;

                float scale = Mathf.Min(Mathf.Max(scaleX, scaleZ), scaleY);

                Vector3 newScale = new Vector3(scale, scale, scale);

                if (this.sprite.transform.parent != null)
                {
                    Vector3 parentScale = this.sprite.transform.parent.lossyScale;
                    newScale.x /= parentScale.x;
                    newScale.y /= parentScale.y;
                    newScale.z /= parentScale.z;
                }

                this.sprite.transform.localScale = newScale;
            }

            foreach (var renderer in GetRenderersLOD(0))
            {
                if (renderer == null) continue;
                renderer.gameObject.layer = oldlayer;
                renderer.enabled = false;
            }
            if (debug)
                Debug.Log("Succsess");
            SetVisible(true);
            RenderTexture.active = null;
            RendererSpriteCamera.gameObject.SetActive(false);
            renderTexture.Release();
    }
    ///Пока что НЕ РАБОТАЕТ (Ломает камеру. Разберись с этим позже) 
    private IEnumerator GenerateLODSpriteAsync()
    {
        if(isStreamingRenderer)
            yield break;
        isStreamingRenderer = true;
        int oldLayer = Transform.gameObject.layer;
        SetVisible(false);

        foreach (var renderer in GetRenderersLOD(0))
        {
            if (renderer == null) continue;
            renderer.gameObject.layer = LayerMask.NameToLayer("TransparentFX");
            renderer.transform.hasChanged = true;
            renderer.enabled = true;
        }

        Bounds bounds = CalculateBounds();
        float maxSize = Mathf.Max(bounds.size.x, bounds.size.y, bounds.size.z);
        float minSize = Mathf.Min(bounds.size.x, bounds.size.y, bounds.size.z);
        int x = (int)(256 * minSize / maxSize);

        renderTexture = new RenderTexture(x, 256, 24);
        renderTexture.enableRandomWrite = true;
        renderTexture.Create();

        cmd = new CommandBuffer();
        cmd.SetRenderTarget(renderTexture);
        cmd.ClearRenderTarget(true, true, Color.clear);

        // Настройка камеры для Command Buffer

        RendererSpriteCamera.transform.position = bounds.center + (CameraControll.MainCamera.transform.position - bounds.center).normalized * (maxSize * OFFSET);

        RendererSpriteCamera.orthographicSize = maxSize * 0.5f * OFFSET;

        RendererSpriteCamera.transform.LookAt(bounds.center);

        Matrix4x4 projectionMatrix = RendererSpriteCamera.projectionMatrix;
        Matrix4x4 viewMatrix = RendererSpriteCamera.worldToCameraMatrix;

        cmd.SetViewProjectionMatrices(viewMatrix, projectionMatrix);

        // Рендеринг объектов
        var renderers = GetRenderersLOD(0);
        foreach (var renderer in renderers)
        {
            if (renderer == null) continue;
            cmd.DrawRenderer(renderer, renderer.sharedMaterial);
        }

        // Выполнение Command Buffer
        Graphics.ExecuteCommandBuffer(cmd);

        // Асинхронное чтение пикселей
        AsyncGPUReadback.Request(renderTexture, 0, TextureFormat.RGBA32, OnCompleteReadback);

        yield return new WaitUntil(() => readbackCompleted);

        // Создание спрайта и настройка SpriteRenderer
        // (Этот код остается в основном потоке)
        Sprite sprite = Sprite.Create(texture, new Rect(0, 0, texture.width, texture.height), new Vector2(0.5f, 0f));

        if (this.sprite == null)
        {
            GameObject spriteObject = new GameObject("LOD Sprite");
            this.sprite = spriteObject.AddComponent<SpriteRenderer>();
            spriteObject.transform.SetParent(Transform);
            this.sprite.transform.localPosition = Vector3.zero;
        }
        this.sprite.sprite = sprite;

        Bounds boundSprite = this.sprite.bounds;

        float scaleX = bounds.size.x / boundSprite.size.x;
        float scaleY = bounds.size.y / boundSprite.size.y;
        float scaleZ = bounds.size.z / boundSprite.size.z;

        float scale = Mathf.Min(Mathf.Max(scaleX, scaleZ), scaleY);

        Vector3 newScale = new Vector3(scale, scale, 1f);

        if (this.sprite.transform.parent != null)
        {
            Vector3 parentScale = this.sprite.transform.parent.lossyScale;
            newScale.x /= parentScale.x;
            newScale.y /= parentScale.y;
            newScale.z /= parentScale.z;
        }

        this.sprite.transform.localScale = newScale;

        foreach (var renderer in GetRenderersLOD(0))
        {
            if (renderer == null) continue;
            renderer.gameObject.layer = oldLayer;
            renderer.enabled = false;
        }
        SetVisible(true);

        // Очистка
        cmd.Release();
        renderTexture.Release();
        isStreamingRenderer = false;
        yield break;
    }

    private bool readbackCompleted = false, isStreamingRenderer;
    private Texture2D texture;
    private CommandBuffer cmd;

    private void OnCompleteReadback(AsyncGPUReadbackRequest request)
    {
        if (request.hasError)
        {
            Debug.Log("GPU readback error detected.");
            return;
        }

        texture = new Texture2D(renderTexture.width, renderTexture.height, TextureFormat.RGBA32, false);
        texture.LoadRawTextureData(request.GetData<byte>());
        texture.Apply();

        readbackCompleted = true;
    }

    private Bounds CalculateBounds()
    {
        Bounds bounds = new Bounds(Transform.position, Vector3.zero);
        Renderer[] renderers = GetRenderersLOD(0);
        foreach (Renderer renderer in renderers)
        {
            if (renderer == null) continue;
            bounds.Encapsulate(renderer.bounds);
        }
        return bounds;
    }

    public void SetCastShadowMode(ShadowCastingMode mode)
    {
        foreach (Renderer renderer in GetRenderersLOD(currentLODLevel))
        {
            if(renderer)
                renderer.shadowCastingMode = mode;
        }
    }
    public void SetVisible(bool visible)
    {
        foreach (Renderer renderer in GetRenderersLOD(currentLODLevel))
        {
            if (renderer)
                renderer.enabled = visible;
        }
    }
    public void SwitchToLOD(int level)
    {
        if (level < 0 || level > 6)
            throw new ArgumentOutOfRangeException(nameof(level));

        if (level == currentLODLevel) return;

        SetVisible(false);

        currentLODLevel = level;

        SetVisible(true);
    }
    public void ActivateSpriteObject(bool visible)
    {
        if (sprite != null)
            sprite.enabled = visible;
    }
    private Renderer[] GetRenderersLOD(int level)
    {
        return level switch
        {
            0 => originalRenderers,
            1 => renderersLOD_1,
            2 => renderersLOD_2,
            3 => renderersLOD_3,
            4 => renderersLOD_4,
            5 => renderersLOD_5,
            6 => renderersLOD_6,
            _ => throw new ArgumentOutOfRangeException(nameof(level)),
        };
    }
    public IEnumerator GetEnumerator()
    {
         return GetRenderersLOD(currentLODLevel).GetEnumerator();
    }
   

    public void SetPropertiesJob(ResultsJobProperty results)
    {
        if (debug)
            Debug.Log("SetPropertiesJob");
        if(IsGenerateSprite)
            if (results.isSpriteVisible && results.isVisible)
            {
                if (was3DMode)
                {
                    if(sprite == null || IsDynamicSprite && sprite != null)
                        GenerateLODSprite();
                    was3DMode = false;
                }
                ActivateSpriteObject(true);
            }
            else
            {
                ActivateSpriteObject(false);
                if (results.isVisible)
                {
                    was3DMode = true;
                }
            }
        SetVisible(results.isVisible && !results.isSpriteVisible);
        if (results.isSpriteVisible)
        {
            if(sprite)
                sprite.transform.rotation = results.rotationSpriteToCamera;
        }

        if (results.isVisible == false) return;

        if (results.useLODlogics)
        {
            SwitchToLOD(results.lodLevel);
        }

        SetCastShadowMode(results.castShadow);
    }

    ~RendererBuffer()
    {
        Dispose(false);
    }
    public void Dispose()
    {
        OptimizedRenderer.RemoveRendererBuffer(this);
        Dispose(true);
        GC.SuppressFinalize(this);
    }
    protected virtual void Dispose(bool disposing)
    {
        if (!isDisposed)
        {
            if (disposing)
            {
                // Освобождаем управляемые ресурсы
                originalRenderers = null;
                renderersLOD_1 = null;
                renderersLOD_2 = null;
                renderersLOD_3 = null;
                renderersLOD_4 = null;
                renderersLOD_5 = null;
                renderersLOD_6 = null;
                sprite = null;
            }

            // Освобождаем неуправляемые ресурсы (если есть)
            isDisposed = true;
        }
    }
}
