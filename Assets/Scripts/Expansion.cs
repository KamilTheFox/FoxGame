using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using System.Linq;

public static class Expansion
{
   public static Transform[] GetChilds(this Transform transform)
    {
        List<Transform> list = new List<Transform>();
        foreach (Transform t in transform)
            list.Add(t);
        return list.ToArray();
    }

    public static Transform FirstToLowerPrefix(this Transform transform, string namePrefix)
    {
        return transform.GetChilds().FirstOrDefault(obj => obj.name.ToLower().IndexOf(namePrefix) == 0);
    }
    #region https://forum.unity.com/threads/change-rendering-mode-via-script.476437/
    public static void ToOpaqueMode(this Material material)
    {
        material.SetOverrideTag("RenderType", "");
        material.SetInt("_SrcBlend", (int)UnityEngine.Rendering.BlendMode.One);
        material.SetInt("_DstBlend", (int)UnityEngine.Rendering.BlendMode.Zero);
        material.SetInt("_ZWrite", 1);
        material.DisableKeyword("_ALPHATEST_ON");
        material.DisableKeyword("_ALPHABLEND_ON");
        material.DisableKeyword("_ALPHAPREMULTIPLY_ON");
        material.renderQueue = -1;
    }

    public static void ToFadeMode(this Material material)
    {
        material.SetOverrideTag("RenderType", "Transparent");
        material.SetInt("_SrcBlend", (int)UnityEngine.Rendering.BlendMode.SrcAlpha);
        material.SetInt("_DstBlend", (int)UnityEngine.Rendering.BlendMode.OneMinusSrcAlpha);
        material.SetInt("_ZWrite", 0);
        material.DisableKeyword("_ALPHATEST_ON");
        material.EnableKeyword("_ALPHABLEND_ON");
        material.DisableKeyword("_ALPHAPREMULTIPLY_ON");
        material.renderQueue = (int)UnityEngine.Rendering.RenderQueue.Transparent;
    }

    public static Material CopyAllMaterialStandartProperties(this Material targetMaterial, Material sourceMaterial)
    {

        // Основные свойства
        targetMaterial.SetTexture("_MainTex", sourceMaterial.GetTexture("_MainTex"));
        targetMaterial.SetColor("_Color", sourceMaterial.GetColor("_Color"));

        // Metallic и Smoothness
        targetMaterial.SetFloat("_Metallic", sourceMaterial.GetFloat("_Metallic"));
        targetMaterial.SetFloat("_Glossiness", sourceMaterial.GetFloat("_Glossiness"));
        targetMaterial.SetTexture("_MetallicGlossMap", sourceMaterial.GetTexture("_MetallicGlossMap"));

        // Normal map
        // Сохраняем настройки нормалей
        if (targetMaterial.HasProperty("_BumpMap"))
        {
            Texture normalMap = targetMaterial.GetTexture("_BumpMap");
            if (normalMap != null)
            {
                targetMaterial.EnableKeyword("_NORMALMAP");
                targetMaterial.SetTexture("_BumpMap", normalMap);
                if (targetMaterial.HasProperty("_BumpScale"))
                {
                    targetMaterial.SetFloat("_BumpScale", sourceMaterial.GetFloat("_BumpScale"));
                }
            }
        }

        // Occlusion
        targetMaterial.SetTexture("_OcclusionMap", sourceMaterial.GetTexture("_OcclusionMap"));
        targetMaterial.SetFloat("_OcclusionStrength", sourceMaterial.GetFloat("_OcclusionStrength"));

        // Emission
        targetMaterial.SetColor("_EmissionColor", sourceMaterial.GetColor("_EmissionColor"));
        targetMaterial.SetTexture("_EmissionMap", sourceMaterial.GetTexture("_EmissionMap"));

        // Detail maps
        targetMaterial.SetTexture("_DetailAlbedoMap", sourceMaterial.GetTexture("_DetailAlbedoMap"));
        targetMaterial.SetTexture("_DetailNormalMap", sourceMaterial.GetTexture("_DetailNormalMap"));
        targetMaterial.SetFloat("_DetailNormalMapScale", sourceMaterial.GetFloat("_DetailNormalMapScale"));
        targetMaterial.SetTexture("_DetailMask", sourceMaterial.GetTexture("_DetailMask"));

        // UV sets
        targetMaterial.SetFloat("_UVSec", sourceMaterial.GetFloat("_UVSec"));

        // Если у материала есть основная текстура, убеждаемся, что она правильно настроена
        if (targetMaterial.HasProperty("_MainTex"))
        {
            Texture mainTex = targetMaterial.GetTexture("_MainTex");
            if (mainTex != null)
            {
                targetMaterial.SetTexture("_MainTex", mainTex);
                targetMaterial.mainTextureScale = sourceMaterial.mainTextureScale;
                targetMaterial.mainTextureOffset = sourceMaterial.mainTextureOffset;
            }
        }


        return targetMaterial;
        /*
        // Копируем основные текстуры и их настройки
        string[] textureNames = { "_MainTex", "_MetallicGlossMap", "_BumpMap", "_ParallaxMap", "_OcclusionMap", "_EmissionMap", "_DetailMask", "_DetailAlbedoMap", "_DetailNormalMap" };
        foreach (string texName in textureNames)
        {
            targetMaterial.SetTexture(texName, sourceMaterial.GetTexture(texName));
            if (sourceMaterial.GetTexture(texName) != null)
            {
                targetMaterial.SetTextureOffset(texName, sourceMaterial.GetTextureOffset(texName));
                targetMaterial.SetTextureScale(texName, sourceMaterial.GetTextureScale(texName));
            }
        }

        // Копируем цвета
        targetMaterial.SetColor("_Color", sourceMaterial.GetColor("_Color"));
        targetMaterial.SetColor("_EmissionColor", sourceMaterial.GetColor("_EmissionColor"));

        // Копируем числовые параметры
        string[] floatNames = { "_Cutoff", "_Glossiness", "_GlossMapScale", "_SmoothnessTextureChannel", "_Metallic", "_BumpScale", "_Parallax", "_OcclusionStrength", "_DetailNormalMapScale", "_UVSec"};
        foreach (string floatName in floatNames)
        {
            targetMaterial.SetFloat(floatName, sourceMaterial.GetFloat(floatName));
        }

        //targetMaterial.SetInt("_SrcBlend", sourceMaterial.GetInt("_SrcBlend"));
        //targetMaterial.SetInt("_DstBlend", sourceMaterial.GetInt("_DstBlend"));
        //targetMaterial.SetInt("_ZWrite", sourceMaterial.GetInt("_ZWrite"));

        //targetMaterial.SetVector("_EmissionColor", sourceMaterial.GetVector("_EmissionColor"));

        //string[] keywords = {"_NORMALMAP", "_EMISSION", "_PARALLAXMAP",  "_DETAIL_MULX2", "_SMOOTHNESS_TEXTURE_ALBEDO_CHANNEL_A", "_SPECULARHIGHLIGHTS_OFF", "_GLOSSYREFLECTIONS_OFF" };
        //foreach (string keyword in keywords)
        //{
        //    if (sourceMaterial.IsKeywordEnabled(keyword))
        //        targetMaterial.EnableKeyword(keyword);
        //    else
        //        targetMaterial.DisableKeyword(keyword);
        //}


        return targetMaterial;
        */
    }
    #endregion
}
