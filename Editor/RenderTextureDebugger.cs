using System.Collections;
using System.Collections.Generic;
using System.Text;
using UnityEngine;
using UnityEngine.Rendering;
using UnityEditor;

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

        private int textureHeightSize = 100;
        private string searchStr = "";


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



            Rect rect =  EditorGUILayout.GetControlRect(GUILayout.Height(60));

            // scroll bar
            scrollPos = EditorGUILayout.BeginScrollView(scrollPos);

            foreach (RenderTexture renderTexture in renderTextures)
            {
                if (!IsRenderTextureIsForEditor(renderTexture, editorRenderTextures))
                {
                    if (condition.Length == 0 || renderTexture.name.Contains(condition))
                    {
                        var drawRect = EditorGUILayout.GetControlRect(GUILayout.Height(textureHeightSize + 60));
                        DrawTextureInfo(renderTexture, ref drawRect);
                    }
                    ++cnt;
                }
            }
            EditorGUILayout.EndScrollView();
            DrawHeader(ref rect, cnt);
        }

        private void DrawHeader(ref Rect rect,int cnt)
        {

            // header info
            EditorGUI.DrawRect(new Rect(0, 0, 1024, 60), Color.gray);
            rect.y = 0;
            sb.Length = 0;
            rect.width = 250;
            sb.Append("RenderTextureNum ").Append(cnt);
            EditorGUI.LabelField(rect, sb.ToString());

            rect.y += 15;
            rect.height = 20;
            EditorGUI.LabelField(rect, "size");
            rect.x += 50;
            textureHeightSize = EditorGUI.IntSlider(rect, textureHeightSize, 50, 512);
            rect.x -= 50;

            rect.y += 21;

            EditorGUI.LabelField(rect, "search");
            rect.x += 50;
            GUI.SetNextControlName("RenderTextureDebugger.searchStr");
            searchStr = GUI.TextArea(rect, searchStr);
            rect.x -= 50;
        }

        private void DrawTextureInfo(RenderTexture renderTexture, ref Rect rect)
        {
            const int fontSize = 20;
            float originRectX = rect.x;
            float offsetDepthTexture = ((textureHeightSize * renderTexture.width) / renderTexture.height) + 10.0f;
            // draw name
            rect.height = fontSize;
            rect.width = 400;
            sb.Length = 0;
            sb.Append(renderTexture.name).Append(" (").Append(renderTexture.format).Append(") size:").Append(renderTexture.width).Append("x").Append(renderTexture.height);
            sb.Append(" depth:").Append(renderTexture.depth);

            EditorGUI.LabelField(rect, sb.ToString());

            rect.y += rect.height;
            rect.width = 60;
            // save Button

            string defaultSaveFilename = renderTexture.name;
            if (string.IsNullOrEmpty(defaultSaveFilename))
            {
                defaultSaveFilename = "RenderTexture";
            }

            rect.x += OffsetXForTexture;
            if (GUI.Button(rect, "Save"))
            {
                string savePath = EditorUtility.SaveFilePanel("Select saveFile", "", defaultSaveFilename , "png");
                if(!string.IsNullOrEmpty(savePath))
                {
                    SaveRenderTexture(renderTexture, savePath);
                }
            }
            if (renderTexture.depth > 0)
            {
                rect.x += offsetDepthTexture;
                /*
                if (GUI.Button(rect, "Save"))
                {
                    string savePath = EditorUtility.SaveFilePanel("Select saveFile", "", defaultSaveFilename + "_depth", "png");
                    if (!string.IsNullOrEmpty(savePath))
                    {

                    }
                }
                */
                rect.x -= offsetDepthTexture;
            }
            rect.x -= OffsetXForTexture;
            rect.y += rect.height + 5;

            // draw texture
            rect.height = textureHeightSize;
            rect.width = (rect.height * renderTexture.width) / renderTexture.height;
            rect.x += OffsetXForTexture;

            if (renderTexture.format != RenderTextureFormat.Depth && renderTexture.dimension != TextureDimension.Tex3D)
            {
                EditorGUI.DrawTextureTransparent(rect, renderTexture);
                rect.x += offsetDepthTexture;
            }

            if (renderTexture.depth > 0)
            {
                // draw depth texture
                if (!depthMaterial)
                {
					depthMaterial = new Material(Shader.Find("UTJ/RenderDepthDebugger/RenderDepth"));
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

        private void SaveRenderTexture(RenderTexture renderTexture, string file)
        {
            Texture2D tex = new Texture2D(renderTexture.width, renderTexture.height, TextureFormat.RGBA32, false);
            RenderTexture.active = renderTexture;
            tex.ReadPixels(new Rect(0, 0, renderTexture.width, renderTexture.height), 0, 0);
            tex.Apply();

            // Encode texture into PNG
            byte[] bytes = tex.EncodeToPNG();
            Object.DestroyImmediate(tex);

            //Write to a file in the project folder
            System.IO.File.WriteAllBytes(file , bytes);
        }

    }
}