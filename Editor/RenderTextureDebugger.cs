using System.Collections;
using System.Collections.Generic;
using System.Text;
using UnityEngine;
using UnityEngine.Rendering;
using UnityEditor;
using UnityEditor.Graphs;

namespace UTJ
{
    public class RenderTextureDebugger : EditorWindow
    {

        [MenuItem("Tools/UTJ/RenderTextureDebugger")]
        public static void Create()
        {
            RenderTextureDebugger.GetWindow<RenderTextureDebugger>();
        }

        private const int OffsetXForTexture = 20;
        private Vector2 scrollPos;
        private StringBuilder sb = new StringBuilder(32);
        private Material depthMaterial;
        private Material drawMaterial;
        private float depthMin = 0.0f;
        private float depthMax = 1.0f;

        private int textureHeightSize = 100;
        private bool limitTextureHeight = true;
        private bool flipY = false;
        private string searchStr = "";
        private int colorMode;

        private readonly static GUIContent[] SelectList = new GUIContent[]{
            new GUIContent("処理なし"),
            new GUIContent("ガンマ→リニア"),
            new GUIContent("リニア→ガンマ"),
        };


        // Update is called once per frame
        void Update()
        {
            this.Repaint();
        }
        void OnGUI()
        {
            var renderTextures = Resources.FindObjectsOfTypeAll<RenderTexture>();

            RenderTexture[] editorRenderTextures = Editor.FindObjectsOfType<RenderTexture>();

            int cnt = 0;

            string condition = this.searchStr.Trim();
            foreach (RenderTexture renderTexture in renderTextures)
            {
                if (!IsRenderTextureIsForEditor(renderTexture, editorRenderTextures))
                {
                    ++cnt;
                }
            }



            Rect rect =  EditorGUILayout.GetControlRect(GUILayout.Height(80));

            // scroll bar
            scrollPos = EditorGUILayout.BeginScrollView(scrollPos);

            foreach (RenderTexture renderTexture in renderTextures)
            {
                if (!IsRenderTextureIsForEditor(renderTexture, editorRenderTextures))
                {
                    if (condition.Length == 0 || renderTexture.name.Contains(condition))
                    {
                        int ysize = textureHeightSize;
                        if (limitTextureHeight)
                        {
                            ysize = (int)Mathf.Min(textureHeightSize, renderTexture.height);
                        }
                        try
                        {
                            var drawRect = EditorGUILayout.GetControlRect(GUILayout.Height(ysize + 60));
                            DrawTextureInfo(renderTexture, ref drawRect, ysize);
                        }catch(System.Exception e)
                        {
                            Debug.LogError(e);
                        }
                    }
                    ++cnt;
                }
            }
            EditorGUILayout.EndScrollView();
            DrawHeader(ref rect, cnt);
        }

        private void DrawHeader(ref Rect rect,int cnt)
        {
            float originX = rect.x;
            // header info
            EditorGUI.DrawRect(new Rect(0, 0, 1024, 80), Color.gray);
            rect.y = 0;
            sb.Length = 0;
            rect.width = 250;
            sb.Append("RenderTextureNum ").Append(cnt);
            rect.height = 15;
            EditorGUI.LabelField(rect, sb.ToString());

            rect.x = originX;
            rect.y += 20;


            EditorGUI.LabelField(rect, "色空間");
            rect.x += 40;
            colorMode = EditorGUI.Popup(rect, colorMode, SelectList);

            rect.x += 260;
            EditorGUI.LabelField(rect, "depth");
            rect.x += 40;
            EditorGUI.MinMaxSlider(rect, ref depthMin, ref depthMax, 0.0f, 1.0f);


            rect.x = originX;
            rect.y += 20;

            EditorGUI.LabelField(rect, "size");
            rect.x += 50;
            textureHeightSize = EditorGUI.IntSlider(rect, textureHeightSize, 50, 512);

            rect.x += rect.width + 5;
            flipY = EditorGUI.Toggle(rect, flipY);
            rect.x += 15;
            EditorGUI.LabelField(rect, "Y反転");

            SetYFlip(this.drawMaterial, flipY);
            SetYFlip(this.depthMaterial, flipY);

            rect.x += 50;
            limitTextureHeight = EditorGUI.Toggle(rect, limitTextureHeight);
            rect.x += 15;
            EditorGUI.LabelField(rect, "Textureサイズ以上はいかない");
 
            rect.x = originX;
            rect.y += 21;

            EditorGUI.LabelField(rect, "search");
            rect.x += 50;
            GUI.SetNextControlName("RenderTextureDebugger.searchStr");
            searchStr = GUI.TextArea(rect, searchStr);




            rect.x = originX;
            this.ApplyDepthParam(depthMin, depthMax);

        }


        private void SetYFlip(Material mat,bool flag)
        {
            const string Keyword = "FLIP_Y";
            if (mat)
            {
                if (flag)
                {
                    mat.EnableKeyword(Keyword);
                }
                else
                {
                    mat.DisableKeyword(Keyword);
                }
            }
        }

        private void DrawTextureInfo(RenderTexture renderTexture, ref Rect rect,int texHeight)
        {
            const int fontSize = 20;
            float originRectX = rect.x;
            float offsetDepthTexture = ((texHeight * renderTexture.width) / renderTexture.height) + 10.0f;
            if(offsetDepthTexture < 90) { offsetDepthTexture = 90; }
            // draw name
            rect.height = fontSize;
            rect.width = 400;
            sb.Length = 0;
            sb.Append(renderTexture.name).Append(" (").Append(renderTexture.graphicsFormat).Append(") size:").
                Append(renderTexture.width).Append("x").Append(renderTexture.height);
            sb.Append(" depth:").Append(renderTexture.depth).Append(" Flag:").Append(renderTexture.hideFlags);
            rect.width = 600;
            EditorGUI.LabelField(rect, sb.ToString());

            rect.y += rect.height;
            rect.width = 80;
            // save Button
            rect.x += OffsetXForTexture;
            var drawSaveBtnRect = rect;

            rect.x = originRectX;
            rect.y += rect.height + 5;

            // draw texture
            rect.height = texHeight;
            rect.width = (rect.height * renderTexture.width) / renderTexture.height;
            if(rect.width > texHeight * 4)
            {
                rect.width = texHeight * 4;
            }
            rect.x += OffsetXForTexture;

            if (renderTexture.format != RenderTextureFormat.Depth && renderTexture.dimension != TextureDimension.Tex3D)
            {
                if (GUI.Button(drawSaveBtnRect, "Save"))
                {
                    RenderTextureSave.SaveWithDialog(renderTexture);
                }
                if (!drawMaterial)
                {
                    drawMaterial = new Material(Shader.Find("Hidden/RendeerTextureDebug/DebugColorSpace"));
                }
                ChangeDrawMode(colorMode);
                EditorGUI.DrawPreviewTexture(rect, renderTexture, drawMaterial);
                //EditorGUI.DrawTextureTransparent(rect, renderTexture);
                rect.x += offsetDepthTexture;
            }

            if (renderTexture.depth > 0)
            {
                drawSaveBtnRect.x = rect.x;
                if (GUI.Button(drawSaveBtnRect, "SaveDepth"))
                {
                    // todo
                    RenderTextureSave.SaveRenderTextureDepth(renderTexture);
                }
                // draw depth texture
                if (!depthMaterial)
                {
					depthMaterial = new Material(Shader.Find("Hidden/RendeerTextureDebug/RenderDepth"));
                }
                depthMaterial.mainTexture = renderTexture;
                EditorGUI.DrawPreviewTexture(rect, renderTexture, depthMaterial);
            }

            rect.y += rect.height + 5;
            rect.x = originRectX;
        }


        private bool IsRenderTextureIsForEditor(RenderTexture r, RenderTexture[] editorRenderTextures)
        {
            if (r == null || r.name == null) { return false; }


            if (r.name == "SceneView RT" || r.name == "GameView RT")
            {
                return true;
            }
            if (r.name == "")
            {
                if (editorRenderTextures != null)
                {
                    foreach (var editorTexture in editorRenderTextures)
                    {
                        if (r == editorTexture)
                        {
                            return false;
                        }
                    }
                }
                return true;
            }
            return false;
        }

        private void ApplyDepthParam(float min,float max)
        {
            if (!this.depthMaterial)
            {
                return;
            }
            if( min >= max) { min = max - 0.01f; }
            this.depthMaterial.SetFloat("_MinParam", min);
            this.depthMaterial.SetFloat("_MaxParam", max);
        }

        private void ChangeDrawMode(int mode)
        {
            if(!this.drawMaterial)
            {
                return;
            }
            switch (mode)
            {
                case 0:
                    this.drawMaterial.DisableKeyword("LINEAR_TO_GAMMMA");
                    this.drawMaterial.DisableKeyword("GAMMA_TO_LINEAR");
                    break;
                case 1:
                    this.drawMaterial.DisableKeyword("LINEAR_TO_GAMMMA");
                    this.drawMaterial.EnableKeyword("GAMMA_TO_LINEAR");
                    break;
                case 2:
                    this.drawMaterial.DisableKeyword("GAMMA_TO_LINEAR");
                    this.drawMaterial.EnableKeyword("LINEAR_TO_GAMMMA");
                    break;
            }
        }



    }
}