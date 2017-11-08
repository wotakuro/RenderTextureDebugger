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

        [MenuItem("Tools/RenderTextureDebugger")]
        public static void Create()
        {
            RenderTextureDebugger.GetWindow<RenderTextureDebugger>();
        }

        private const int OffsetXForTexture = 10;
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

            Rect rect = new Rect(10, 60, 400, 60);

            rect.x = 10;
            int cnt = 0;
            rect.x -= this.scrollPos.x;
            rect.y -= this.scrollPos.y;

            string condition = this.searchStr.Trim();
            foreach (RenderTexture renderTexture in renderTextures)
            {
                if (!IsRenderTextureIsForEditor(renderTexture, editorRenderTextures))
                {

                    if (condition.Length == 0 || renderTexture.name.Contains(condition))
                    {
                        DrawTextureInfo(renderTexture, ref rect);
                    }
                    ++cnt;
                }
            }

            Rect windowRect = rect;


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

            // scroll bar
            scrollPos = EditorGUILayout.BeginScrollView(scrollPos);
            GUILayoutUtility.GetRect(new GUIContent(string.Empty), GUIStyle.none, GUILayout.Width(windowRect.x + scrollPos.x + 10), GUILayout.Height(windowRect.y + scrollPos.y + 10));
            EditorGUILayout.EndScrollView();


        }

        private void DrawTextureInfo(RenderTexture renderTexture, ref Rect rect)
        {
            const int fontSize = 20;
            float originRectX = rect.x;
            // draw name
            rect.height = fontSize;
            rect.width = 800;
            sb.Length = 0;
            sb.Append(renderTexture.name).Append(" (").Append(renderTexture.format).Append(") size:").Append(renderTexture.width).Append("x").Append(renderTexture.height);
            sb.Append(" depth:").Append(renderTexture.depth);

            EditorGUI.LabelField(rect, sb.ToString());
            rect.y += rect.height;

            // draw texture
            rect.height = textureHeightSize;
            rect.width = (rect.height * renderTexture.width) / renderTexture.height;
            float offsetDepthTexture = rect.width + 10.0f;
            rect.x += OffsetXForTexture;

            if (renderTexture.format != RenderTextureFormat.Depth)
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
    }
}