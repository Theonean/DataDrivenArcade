using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace SaveSystem {
    public interface ISaveable 
    {
        public void LoadData(SaveData data, int playerNum);
        public SaveData SaveData(SaveData data, int playerNum);
    }
}
