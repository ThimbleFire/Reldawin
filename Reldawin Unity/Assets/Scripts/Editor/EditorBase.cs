using System;

public class EditorBase : EditorWindow
{
    protected interface IXMLStream < T >
    {
        T Load(string path);
        void Save(T obj, string path);
    }

    protected XMLStream < T > : IXMLStream < T >
    {
        public T Load(string FileName)
        {
            private const string ServerDir = "C:/Users/Retri/Documents/GitHub/Reldawin/ReldawinServerMaster/ReldawinServerMaster/bin/Debug";

            T obj;

            try
            {
                XmlSerializer serializer = new XmlSerializer( typeof( T ) );
                FileStream stream = new FileStream( Application.streamingAssetsPath + FileName, FileMode.Open );
                obj = serializer.Deserialize( stream ) as T;
                stream.Close();
            }
            catch
            {
                Debug.LogWarning( "Could not open " + FileName );
            }

            return obj;
        }

        public void Save(T obj, string FileName)
        {       
            try
            {
                using(XmlSerializer serializer = new XmlSerializer( typeof( T ) ) )
                {                 
                    using(FileStream stream = new FileStream( Application.streamingAssetsPath + FileName, FileMode.Create ) )
                    {
                        serializer.Serialize( stream, obj );
                        stream.Close();
                    }

                    using(FileStream stream = new FileStream( ServerDir + FileName, FileMode.Create ) )
                    {
                        serializer.Serialize( stream, obj );
                        stream.Close();
                    }
                }
            }
            catch
            {
                Debug.LogWarning( "Could not save " + FileName );
            }
        }
    }

    protected enum WindowState { Main, Create }
    
    protected WindowState windowState { get; set; }
    protected int LoadIndex { get; set; }    
    protected string FileName { get; set; }
    protected bool Loaded { get; set; }
        
    private int y { get; set; }
    protected void AddRow() { y += 20; }
    
    protected void LoadProperties() { }
    protected void SaveProperties() { }
    
    protected static void ShowWindow()
    {
        //Override me
    }
    
    private void OnGUI()
    {
        focusedWindow.minSize = new Vector2( 400, 100 );
        
        if ( !Loaded )
        {
            Load();
        }

        switch ( windowState )
        {
            case WindowState.Main:
                MainWindow();
                break;
            case WindowState.Create:
                CreationWindow();
                break;
        }
    }
    
    protected void MainWindow()
    {
        PaintLoadList();
        
        if ( GUILayout.Button("New") )
        {
            windowState = WindowState.Create;
        }
        if ( GUILayout.Button("Load Selected") )
        {
            LoadProperties();
            windowState = WindowState.Create;
        }        
    }
    
    //Override this method and end with this.Base();
    protected void CreationWindow()
    {
        PaintSaveButton();
        PaintBackButton();
    }
    
    protected void PaintIntField(string label, ref int value)
    {
        value = EditorGUI.IntField(new Rect(4, y, position.width - 12, 20), label, value);
        AddRow();
    }
    
    protected void PaintFloatField(string label, ref float value)
    {
        value = EditorGUI.FloatField(new Rect(4, y, position.width - 12, 20), label, value);
        AddRow();
    }
    
    protected void PaintTextField(string label, ref string value)
    {
        value = EditorGUI.TextField(new Rect(4, y, position.width - 12, 20), label, value);
        AddRow();
    }
    
    protected void PaintFloatRange(string label, ref float min, ref float max, float minRange, float maxRange)
    {
        EditorGUIUtility.wideMode = true;
        {
            EditorGUI.MinMaxSlider(
                new Rect( 4, y, position.width - 12, 20 ),
                new GUIContent( label ),
                min, 
                max,
                minRange, 
                maxRange                
                );
        }
        EditorGUIUtility.wideMode = false;
        AddRow();
    }
    
    protected void PaintHorizontalLine()
    {
        Handles.color = Color.gray;
        Handles.DrawLine( new Vector3( 4, y + 11 ), new Vector3( position.width - 8, y + 11 ) );
        AddRow();
    }
    
    protected void PaintPopup(string label, string[] options, ref int value)
    {
        value = EditorGUI.Popup(new Rect(4, y, position.width - 8, 20, value, options);
        AddRow();
    }
    
    protected void PaintSpriteField(string label, ref Sprite sprite, ref string fileName)
    {
        sprite (Sprite)EditorGUI.ObjectField(new Rect(4, y, 64, 64), sprite, typeof(Sprite), false);
        
        if(sprite != null)
        {
            fileName = sprite.Name;
            EditorGUI.LabelField(new Rect(74, y, position.width - 86, 20, label);
            AddRow();
            EditorGUI.LabelField(new Rect(74, y, position.width - 86, 20, label);
            AddRow();
            AddRow();
            AddRow();
        }
        else
        {
            AddRow();
            AddRow();
            AddRow();
            AddRow();
        }
    }
}