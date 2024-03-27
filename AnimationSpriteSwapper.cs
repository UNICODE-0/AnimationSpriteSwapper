#if UNITY_EDITOR
using UnityEngine;
using UnityEditor;
using System.IO;
using System.Collections.Generic;
using System.Linq;
using System;

public class AnimationSpriteSwapper : EditorWindow 
{
    private AnimationClip _animationClip;
    private Sprite _oldSprite;
    private Sprite _newSprite;
    private bool _isPathBySprite = true;

    private string _oldPath = "";
    private string _newPath = "";

    private List<Texture2D> _oldImages = new List<Texture2D>();
    private List<Texture2D> _newImages = new List<Texture2D>();

    private Vector2 _scrollPosition = Vector2.zero;

    private GUIStyle _arrowStyle;
    private bool _isStylesInited = false;

    private bool _isPreview = false;
    [MenuItem("Window/AnimationSpriteSwapper")]
    private static void ShowWindow() 
    {
        GetWindow(typeof(AnimationSpriteSwapper));
    }
    private void InitStyles()
    {
        _arrowStyle = new(GUI.skin.label);
        _isStylesInited = true;
    }
    private void OnGUI() 
    {
        if(!_isStylesInited) InitStyles();

        _animationClip = (AnimationClip)EditorGUILayout.ObjectField("Animation Clip", _animationClip, typeof(AnimationClip), false);

        _isPathBySprite = GUILayout.Toggle(_isPathBySprite, "Use sprite path");
       
        GUILayout.BeginHorizontal();

        GUILayout.FlexibleSpace();
        GUILayout.BeginVertical();
        GUILayout.Label("Old path:");
        if(_isPathBySprite) _oldSprite = (Sprite)EditorGUILayout.ObjectField("", _oldSprite, typeof(Sprite), false, GUILayout.Width(60));
        else _oldPath = GUILayout.TextArea(_oldPath);
        GUILayout.EndVertical();
        
        GUILayout.BeginVertical();
        GUILayout.Label("New path:");
        if(_isPathBySprite) _newSprite = (Sprite)EditorGUILayout.ObjectField("", _newSprite, typeof(Sprite), false, GUILayout.Width(60));
        else _newPath = GUILayout.TextArea(_newPath);
        GUILayout.EndVertical();
        GUILayout.FlexibleSpace();

        GUILayout.EndHorizontal();

        GUILayout.Space(10);
        GUILayout.BeginHorizontal();
        if (GUILayout.Button("Preview", GUILayout.Height(50)))
        {
            Swap(true);
        }
        if (GUILayout.Button("Swap", GUILayout.Height(50)))
        {
            Swap(false);
        }
        GUILayout.EndHorizontal();
        
        if(_oldImages.Count > 0 || _newImages.Count > 0)
        {
            if (GUILayout.Button("Clear view"))
            {
                _oldImages.Clear();
                _newImages.Clear();
            }

            if(_isPreview) GUILayout.Label("=== PREVIEW ===");
            else GUILayout.Label("=== SUCCESS REPLACEMENT ===");
        }
        
        _scrollPosition = GUILayout.BeginScrollView(_scrollPosition);
        for (int i = 0; i < _oldImages.Count && i < _newImages.Count; i++)
        {
            GUI.DrawTexture(new Rect(0, i * 80, 50, 50), _oldImages[i]);

            GUILayout.Label("\n                ->", _arrowStyle);

            GUI.DrawTexture(new Rect(80, i * 80, 50, 50), _newImages[i]);
            GUILayout.Label("\n\n");
        }

        GUILayout.EndScrollView();
    }
    private void Swap(bool previewMode)
    {
        _oldImages.Clear();
        _newImages.Clear();
        List<string> oldGUIDs = new List<string>();
        List<string> newGUIDs = new List<string>();

        try
        {
            string oldPath = "";
            string newPath = "";

            if(_oldPath.Length == 0)
            {
                oldPath = AssetDatabase.GetAssetPath(_oldSprite).Replace(_oldSprite.name + ".png", " ");
            }
            if(_newPath.Length == 0)
            {
                newPath = AssetDatabase.GetAssetPath(_newSprite).Replace(_newSprite.name + ".png", " ");
            }

            string[] metafilesPaths = Directory.GetFiles(oldPath, "*.meta", SearchOption.TopDirectoryOnly);
            string[] _oldImagesPath = Directory.GetFiles(oldPath, "*.png", SearchOption.TopDirectoryOnly);

            for (int i = 0; i < metafilesPaths.Length; i++)
            {
                if(!previewMode) oldGUIDs.Add(File.ReadAllLines(metafilesPaths[i])[1].Substring(6));
                
                Texture2D t = new Texture2D(1, 1);
                ImageConversion.LoadImage(t, File.ReadAllBytes(_oldImagesPath[i]));
                _oldImages.Add(t);
            }

            metafilesPaths = Directory.GetFiles(newPath, "*.meta", SearchOption.TopDirectoryOnly);
            string[] _newImagesPath = Directory.GetFiles(newPath, "*.png", SearchOption.TopDirectoryOnly);

            for (int i = 0; i < metafilesPaths.Length; i++)
            {
                 if(!previewMode) newGUIDs.Add(File.ReadAllLines(metafilesPaths[i])[1].Substring(6));
                
                Texture2D t = new Texture2D(1, 1);
                ImageConversion.LoadImage(t, File.ReadAllBytes(_newImagesPath[i]));
                _newImages.Add(t);
            }

            if(!previewMode)
            {
                string pathToAnimation = AssetDatabase.GetAssetPath(_animationClip);
                string animationText = File.ReadAllText(pathToAnimation);
                string oldAnimationText = animationText;

                bool uslessReplacment = false;
                for (int i = 0; i < oldGUIDs.Count; i++)
                {
                    animationText = animationText.Replace(oldGUIDs[i], newGUIDs[i]);
                    if(oldGUIDs[i] == newGUIDs[i]) uslessReplacment = true;
                }

                if(oldAnimationText == animationText || uslessReplacment) 
                    EditorUtility.DisplayDialog("Warrning", "There was a replacement for the same sprites", "Ok");

                File.WriteAllText(pathToAnimation, animationText);
                AssetDatabase.Refresh();
                _arrowStyle.normal.textColor = Color.green;
            } else _arrowStyle.normal.textColor = Color.white;

            _isPreview = previewMode;
        }
        catch (System.Exception ex)
        {
            Debug.Log(ex.Message);
        }
    }
}
#endif