using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace SaveSystem {
    public interface ISaveable 
    {
        public void LoadData(SaveData data);
        public void SaveData(SaveData data);
    }
}
