    using System.Collections;
    using System.Collections.Generic;
    using UnityEngine;
    using UnityEditor;
     
    public class heightmapImporter : EditorWindow {
        [SerializeField]
        Terrain myTerrain;
        [SerializeField]
        Texture2D myTexture;
        [SerializeField]
        public float dampen;
        [MenuItem("heightMap/terrain", false, 100)]
     
        public static void  ShowWindow () {
            heightmapImporter myWindow = (heightmapImporter)EditorWindow.GetWindow(typeof(heightmapImporter),false,"heightmap import",true);
            myWindow.Show (true);
        }
     
        //public Transform source;
        //public float scale;
     
        void OnGUI()
        {
            EditorGUILayout.BeginHorizontal ();
            GUILayout.Label ("choose Trrain");
            GUILayout.MinWidth (100f);
            GUILayout.MinHeight (100f);
            myTerrain = EditorGUILayout.ObjectField (myTerrain,typeof(Terrain),true)as Terrain;
            EditorGUILayout.EndHorizontal ();
     
            EditorGUILayout.BeginHorizontal ();
            GUILayout.Label ("choose Texture");
            GUILayout.MinWidth (100f);
            GUILayout.MinHeight (100f);
            myTexture = EditorGUILayout.ObjectField (myTexture,typeof(Texture),true)as Texture2D;
            EditorGUILayout.EndHorizontal ();
     
            EditorGUILayout.BeginHorizontal ();
            GUILayout.MinWidth (100f);
            GUILayout.MinHeight (100f);
            dampen = EditorGUILayout.FloatField ("height division",dampen);
            EditorGUILayout.EndHorizontal ();
     
            EditorGUILayout.BeginHorizontal ();
            if (GUI.Button(new Rect(0,100,200,30), "Assign HeightMap")) {
                assignHeightMap();
            }
            EditorGUILayout.EndHorizontal ();
        }
     
        void assignHeightMap()
        {
            float w = myTexture.width;
            float h = myTexture.height;
            int wt = myTerrain.terrainData.heightmapWidth;
            float[,] heightmapData = myTerrain.terrainData.GetHeights(0,0,wt,wt);
            Color[] mapColors = myTexture.GetPixels ();
            Color[] map = new Color[wt * wt];
     
     
            if (wt != w || h != w) {
                // Resize using nearest-neighbor scaling if texture has no filtering
                if (myTexture.filterMode == FilterMode.Point) {
                    float dx =(float) w/wt;
                    float dy = (float)h/wt;
                    for (int y = 0; y < wt; y++) {
                        if (y%20 == 0) {
     
                            EditorUtility.DisplayProgressBar("Resize", "Calculating texture",Mathf.InverseLerp(0.0f, (float)wt,(float) y));
                        }
                        int thisY = (int)(((dy)*y)*w);
                        int yw = (int)(y*wt);
                        for (int x = 0; x < wt; x++) {
                            map[(int)(yw + x)] = mapColors[(int)(thisY + dx*x)];
                        }
                    }
                }
                // Otherwise resize using bilinear filtering
                else {
                    float ratioX = 1f/((float)wt/(w-1));
                    var ratioY = 1f/((float)wt/(h-1));
                    for (int y = 0; y < wt; y++) {
                        if (y%20 == 0) {
                            EditorUtility.DisplayProgressBar("Resize", "Calculating texture", Mathf.InverseLerp(0.0f, wt, y));
                        }
                        var yy = Mathf.Floor(y*ratioY);
                        var y1 = yy*w;
                        var y2 = (yy+1)*w;
                        int yw = y*wt;
                        for (int x = 0; x < wt; x++) {
                            var xx = Mathf.Floor(x*ratioX);
                         
                            Color bl = mapColors[(int)(y1 + xx)];
                            Color br = mapColors[(int)( y1 + xx+1)];
                            Color tl = mapColors[(int)(y2 + xx)];
                            Color tr = mapColors[(int)(y2 + xx+1)];
                         
                            float xLerp = x*ratioX-xx;
                            map[yw + x] = Color.Lerp(Color.Lerp(bl, br, xLerp), Color.Lerp(tl, tr, xLerp), y*ratioY-yy);
                        }
                    }
                }
                EditorUtility.ClearProgressBar();
            }
            else {
                // Use original if no resize is needed
                map = mapColors;
            }
     
            for (int y = 0; y < wt; y++) {
                for (int x = 0; x < wt; x++) {
                    heightmapData[y,x] = map[y*wt+x].grayscale/dampen;
                    }
            }
            myTerrain.terrainData.SetHeights(0, 0, heightmapData);
     
        }
     
    }
