using System.Collections.Generic;
using UnityEditor;
using UnityEngine;
using UnityEngine.Serialization;
using UnityEngine.UI;

namespace AttackObjects
{
    public class Batiment : MonoBehaviour
    {
        public float health;
        public float remainingHealth;
        public TileInstance instance;
        //public List<Effect> Effects; TODO
        private Slider healthSlider;

        public void OnDamage()
        {
            if (!healthSlider)
            {
                healthSlider = Instantiate(AssetDatabase.LoadAssetAtPath<GameObject>("Assets/Prefabs/UI/Units/Unit Health.prefab")).GetComponentInChildren<Slider>();
                
                healthSlider.maxValue = health;
                healthSlider.transform.parent.localPosition = new Vector3(0, 2, 0);
                healthSlider.transform.parent.localScale = new Vector3(0.05f, 0.05f, 0.05f);
                healthSlider.transform.parent.localRotation = Quaternion.LookRotation(transform.position - PlayerCamera.cam.transform.position);
            }
            
            healthSlider.value = remainingHealth;
        }
    }
}