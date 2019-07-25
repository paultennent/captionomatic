using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using UnityEngine.EventSystems;
using System.IO;
using System;

public class SaveLoad : MonoBehaviour
{
    
    public GameObject[] trackedObjects;
    public string presetName;
    public string presetFolder=".\\presets\\";
    public InputField presetField;
    
    // Start is called before the first frame update
    void Start()
    {
        
    }

    // Update is called once per frame
    void Update()
    {
        if( EventSystem.current.currentSelectedGameObject!=null && EventSystem.current.currentSelectedGameObject.GetComponent<InputField>()!=null)
        {
            // editing name of preset
            // in input field, ignore keyboard
            return;
        }
        if(Input.GetKeyDown("["))
        {
            Prev();
        }
        if(Input.GetKeyDown("]"))
        {
            Next();
        }
    }
    
    public void Load(){
        string strJSON= File.ReadAllText(presetName);
        SaveData s = JsonUtility.FromJson<SaveData>(strJSON);
        foreach(Writable w in s.saveObjs){
            GameObject target = GameObject.Find(w.name);
            if(target!=null)
            {
                target.transform.position = w.position;
                target.transform.rotation = w.rotation;
            }
        }
    }
    
    public void Save(){
        string s = "";
        List<Writable> ws = new List<Writable>();
        for(int i=0;i<trackedObjects.Length;i++){
            GameObject go = trackedObjects[i];
            Writable w = new Writable(go.name,go.transform.position,go.transform.rotation);
            ws.Add(w);
        }
        SaveData sd = new SaveData(ws);
        s += JsonUtility.ToJson(sd);
        File.WriteAllText(presetName,s);
        Debug.Log("Config Written to "+presetName);
    }

    string[] ListPresets(out int currentIndex)
    {
        string[] presets=Directory.GetFiles(presetFolder,"*.preset");
        Array.Sort(presets);
        currentIndex=-1;
        for(int c=0;c<presets.Length;c++)
        {
            print(presetName+":"+presets[c]);
            if(String.Compare(presetName,presets[c],true)<=0)
            {
                currentIndex=c;
                break;
            }
        }
        return presets;
    }

    public void Next(){
        int index=0;
        string [] allPresets=ListPresets(out index);
        print(index);
        index+=1;
        if(index>=allPresets.Length)index=0;
        if(allPresets.Length>0)
        {
            presetName=allPresets[index];
            presetField.text=Path.GetFileNameWithoutExtension(presetName);
            Load();
        }        
    }

    public void Prev(){
        int index=0;
        string [] allPresets=ListPresets(out index);
        index-=1;
        print(index);
        if(index<0)index=allPresets.Length-1;
        if(allPresets.Length>0)
        {
            presetName=allPresets[index];
            presetField.text=Path.GetFileNameWithoutExtension(presetName);
            Load();
        }        
    }

    public void LoadPreset(string name){
        presetField.text = name;
        SetPresetName(name);
        Load();
    }

    
    public void SetPresetName(string name){
        presetName=presetFolder+presetField.text+".preset";
    }
    
    
    [Serializable]
    public class SaveData{
        public List<Writable> saveObjs;
        
        public SaveData(List<Writable> l){
            saveObjs = l;
        }  
    }
    
    [Serializable]
    public class Writable{
        public string name;
        public Vector3 position;
        public Quaternion rotation;
        
        public Writable(string name, Vector3 pos, Quaternion rot){
            this.name = name;
            this.rotation = rot;
            this.position = pos;
        }
        
    }
}
