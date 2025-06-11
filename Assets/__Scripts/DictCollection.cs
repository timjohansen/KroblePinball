using System.Collections;
using System.Collections.Generic;
using UnityEngine;

[System.Serializable]
public class DictCollection<T>
{
    // This is a quick and dirty re-implementation of a dictionary collection to get around Unity's inability to 
    // edit C# dictionaries inside the editor.
    
    [System.Serializable]
    public class Entry
    {
        public string name;
        public T obj;
    }

    public List<Entry> entries = new List<Entry>();

    public Entry Get(string objName)
    {
        return entries.Find(x => x.name == objName);
    }
}
