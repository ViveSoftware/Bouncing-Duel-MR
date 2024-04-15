using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.AI;

namespace AnchorSharing
{
    public abstract class SceneComponentBase : MonoBehaviour
    {
        public ScenePlaneData Data { get; private set; }

        public void SetData(ScenePlaneData data)
        {
            Data = data;
            transform.position = Data.position;
            transform.rotation = Data.rotation;

            initialize();                     
        }

        public void UpdateData(ScenePlaneData data)
        {
            Data = data;
            transform.position = Data.position;
            transform.rotation = Data.rotation;

            updateData();
        }

        protected abstract void initialize();
        protected abstract void updateData();   
    }
}
