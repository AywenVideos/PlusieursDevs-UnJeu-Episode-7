using System;
using System.Collections;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using AttackObjects;
using UnityEngine;
using UnityEngine.AI;
using UnityEngine.Networking;
using UnityEngine.UI;
using Random = UnityEngine.Random;

namespace Entities
{
    public class Unit : MonoBehaviour
    {
        public static List<Unit> All { get; set; } = new();
        
        
        [Header("Skin")]
        [SerializeField] Material material;
        public string username = "FuzeIII";
        [SerializeField] private Renderer[] renderers;
        public bool isMcSkin = false; // défini si on doit appliquer un skin venant de MC ou non.

        [Header("AI")]
        public GameObject target;
        public int attackDamage;
        public int attackRadius;
        public int attackCooldown;
        
        public int health;
        public int remainingHealth;
        
        private Animator animator;
        private NavMeshAgent agent;
        private Slider healthBar;
        private bool isEnemy;

        public Unit Initialize(UnitSO unitData)
        {
            username = unitData.unitName;
            health = unitData.health;
            attackDamage = unitData.attackDamage;
            remainingHealth = health;
            GameObject pref = Instantiate(unitData.healthBarPrefab, transform); 
            healthBar = pref.GetComponentInChildren<Slider>();
            healthBar.maxValue = health;
            pref.transform.localPosition = new Vector3(0, 2, 0);
            pref.transform.localScale = new Vector3(0.05f, 0.05f, 0.05f);
            isEnemy = unitData.isEnemy;
            
            if (isMcSkin)
                ApplyMinecraftSkin();

            return this;
        }
        
        private void Awake()
        {
            animator = GetComponent<Animator>();
            agent = GetComponent<NavMeshAgent>();
            if (!agent)
                agent = gameObject.AddComponent<NavMeshAgent>();
            agent.baseOffset = 0;
            
            // Ajouter un collider pour la sélection si nécessaire
            if (GetComponent<Collider>() == null)
            {
                CapsuleCollider collider = gameObject.AddComponent<CapsuleCollider>();
                collider.center = new Vector3(0, 1, 0);
                collider.height = 2f;
                collider.radius = 0.5f;
            }
            
            // S'assurer que le gameObject est sur le layer approprié
            gameObject.layer = LayerMask.NameToLayer("TileInstance");
        }

        private void Start() =>
            StartCoroutine(EnemmySearchLoop());

        private void Update()
        {
            if (healthBar)
            {
                healthBar.transform.parent.localRotation = Quaternion.LookRotation(transform.position - PlayerCamera.cam.transform.position);
                healthBar.value = remainingHealth;
            }
            if (animator != null && agent != null)
            {
                animator.SetFloat("Speed", agent.velocity.magnitude);
                if (!agent.hasPath || agent.pathStatus == NavMeshPathStatus.PathInvalid)
                {
                    Batiment blocking = FindBlockingBatiment();
                    if (blocking != null)
                    {
                        AttackBatiment(blocking);
                    }
                }
            }
        }

        private Batiment FindBlockingBatiment()
        {
            if (isEnemy) // les ennemies vont pas taper leur propre batiment
                return null;

            List<TileData> datas = TileManager.Instance.GetTilesInRadius(transform.position, 1);
            
            foreach (TileData data in datas)
            {
                Batiment bat = data.Instance.GetComponent<Batiment>();
                if (bat != null)
                    return bat;
            }
            return null;
        }

        private void AttackBatiment(Batiment blocking)
        {
            Debug.Log(gameObject.name + " attaque " + blocking.gameObject.name);
            blocking.health -= attackDamage;
            blocking.OnDamage();
            if (blocking.health <= 0)
                Destroy(blocking.gameObject);
        }

        private IEnumerator EnemmySearchLoop()
        {
            while (true)
            {
                yield return new WaitForSeconds(1f);
                Batiment bat = FindNearestBatiment(transform.position, 50f);
                Unit enemy = FindNearestEnemy(transform.position, 50f);
                if (bat)
                    target = bat.gameObject;
                else if (enemy)
                    target = enemy.gameObject;
                else
                    target = null;
                if (target)
                {
                    Debug.Log("Found target: " + target.transform.position);
                    agent.SetDestination(target?.transform.position ?? Vector3.zero);
                }
                
            }
            // ReSharper disable once IteratorNeverReturns
        }


        private Unit FindNearestEnemy(Vector3 origin, float radius)
        {
            Unit[] allEnemies = FindObjectsOfType<Unit>();
            Unit nearest = null;
            float minDist = float.MaxValue;

            foreach (Unit enemy in allEnemies.Where(x => x.isEnemy != isEnemy)) // = "est-ce que l'un est l'ennemi de celui-ci ?"
            {
                float dist = Vector3.Distance(origin, enemy.transform.position);
                if (dist < radius && dist < minDist)
                {
                    minDist = dist;
                    nearest = enemy;
                }
            }

            return nearest;
        }

        private Batiment FindNearestBatiment(Vector3 origin, float radius)
        {
            Batiment[] allEnemies = FindObjectsOfType<Batiment>();
            Batiment nearest = null;
            float minDist = float.MaxValue;

            foreach (Batiment enemy in allEnemies)
            {
                float dist = Vector3.Distance(origin, enemy.transform.position);
                if (dist < radius && dist < minDist)
                {
                    minDist = dist;
                    nearest = enemy;
                }
            }

            return nearest;
        }

        public void ApplyMinecraftSkin()
        {
            StartCoroutine(DownloadAndApplySkin());
        }

        private IEnumerator DownloadAndApplySkin()
        {
            string url = $"https://mineskin.eu/skin/{username}";

            using (UnityWebRequest request = UnityWebRequestTexture.GetTexture(url))
            {
                yield return request.SendWebRequest();

                if (request.result == UnityWebRequest.Result.Success)
                {
                    Texture2D downloadedTexture = DownloadHandlerTexture.GetContent(request);
                    downloadedTexture.filterMode = FilterMode.Point;
                    downloadedTexture.mipMapBias = 0;

                    if (downloadedTexture != null && material != null)
                    {
                        Material newMaterial = new Material(material);
                        newMaterial.mainTexture = downloadedTexture;

                        foreach (Renderer rend in renderers)
                        {
                            rend.material = newMaterial;
                        }
                    }
                }
                else
                {
                    Debug.LogWarning($"Échec du téléchargement de la skin pour {username}: {request.error}");
                }
            }
        }

        public void OnEnable()
        {
            if(!All.Contains(this))
                All.Add(this);
        }

        public void OnDisable()
        {
            if (All.Contains(this))
                All.Remove(this);
        }

        public string GetDisplayName()
        {
            return username;
        }

        private void OnMouseDown()
        {
            if (BuildManager.Instance != null && BuildManager.Instance.IsHoldingBuilding())
                return;
                
            if (UserInterface.Instance != null)
            {
                UserInterface.Instance.SetSelectedUnit(this);
            }
        }
    }
}