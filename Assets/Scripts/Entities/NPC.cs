using System;
using UnityEngine;
using System.Collections;
using UnityEngine.Networking;
using UnityEngine.AI;
using System.Collections.Generic;
using System.IO;
using Random = UnityEngine.Random;

public class NPC : MonoBehaviour
{
    public static List<NPC> All = new();

    [Header("Skin")]
    [SerializeField] Material material;
    [SerializeField] private string username = "FuzeIII";
    [SerializeField] private Renderer[] renderers;
    
    [Header("Job")]
    [SerializeField] private GameObject hatSocket;
    
    [Header("Job Hats")]
    [SerializeField] private JobHat[] jobHats;
    
    [Serializable]
    public class JobHat
    {
        public string jobType;
        public GameObject hatPrefab;
    }
    
    private Dictionary<string, GameObject> hatDictionary = new();

    [Header("AI")]
    [SerializeField] private float wanderRadius = 100f;
    [SerializeField] private float wanderMinDelay = 2f;
    [SerializeField] private float wanderMaxDelay = 15f;
    [SerializeField] private float workRadius = 10f;
    [SerializeField] private float arrivalDistance = 2f;
    [SerializeField] private float jobSearchInterval = 3f; // Nouvelle variable pour contrôler l'intervalle de recherche

    private Animator animator;
    private NavMeshAgent agent;
    private NPCType jobtype = NPCType.Villager;
    private Coroutine jobSearchCoroutine;
    private Coroutine movementCoroutine;
    private GameObject currentHat;
    private Transform workplace;

    private void Awake()
    {
        animator = GetComponent<Animator>();
        agent = GetComponent<NavMeshAgent>();
        
        InitializeHatDictionary();
        
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

    private void Start()
    {
        if (string.IsNullOrEmpty(username) || username == "FuzeIII")
        {
            username = GetRandomUsernameFromFile();
        }

        if(!string.IsNullOrEmpty(username))
            ApplyMinecraftSkin(username);

        // Chance d'être un idiot du village au spawn (1/25)
        if (Random.Range(0, 25) == 0 || username.ToLower().Equals("trump") || username.ToLower().Equals("fuzeiii"))
        {
            SetJobType(NPCType.Idiot);
        }

        movementCoroutine = StartCoroutine(MovementLoop());
        jobSearchCoroutine = StartCoroutine(JobSearchLoop());
    }

    private void InitializeHatDictionary()
    {
        hatDictionary.Clear();
        
        foreach (JobHat jobHat in jobHats)
        {
            if (!string.IsNullOrEmpty(jobHat.jobType) && jobHat.hatPrefab != null)
            {
                if (hatDictionary.ContainsKey(jobHat.jobType))
                {
                    Debug.LogWarning($"Chapeau en double pour le métier {jobHat.jobType}. Le dernier sera utilisé.");
                }
                hatDictionary[jobHat.jobType] = jobHat.hatPrefab;
            }
        }
        
        // Debug.Log($"[NPC] -> Dictionnaire des chapeaux initialisé avec {hatDictionary.Count} métiers.");
    }

    private void Update()
    {
        if (animator != null && agent != null)
        {
            animator.SetFloat("Speed", agent.velocity.magnitude);
        }
    }

    private IEnumerator MovementLoop()
    {
        while (true)
        {
            if (agent != null && agent.isOnNavMesh)
            {
                if (jobtype is NPCType.Villager or NPCType.Idiot)
                {
                    if (TryGetRandomNavMeshPosition(transform.position, wanderRadius, out Vector3 destination))
                    {
                        agent.SetDestination(destination);
                    }
                    yield return new WaitForSeconds(Random.Range(wanderMinDelay, wanderMaxDelay));
                }
                else if (workplace != null)
                {
                    yield return StartCoroutine(WorkBehavior());
                }
                else
                {
                    // Le NPC a un job mais pas de workplace - chercher à nouveau
                    Debug.LogWarning($"{username} a un job ({jobtype}) mais pas de workplace. Retour en villageois.");
                    SetJobType(NPCType.Villager);
                    yield return new WaitForSeconds(1f);
                }
            }
            else
            {
                yield return new WaitForSeconds(0.5f);
            }
        }
    }

    private IEnumerator WorkBehavior()
    {
        // Vérifier si le workplace existe encore
        if (workplace == null)
        {
            //Debug.LogWarning($"{username} : Le lieu de travail n'existe plus. Retour en villageois.");
            SetJobType(NPCType.Villager);
            yield break;
        }

        float distanceToWorkplace = Vector3.Distance(transform.position, workplace.position);
        
        if (distanceToWorkplace > workRadius)
        {
            // Debug.Log($"[NPC] -> {username} se dirige vers son lieu de travail");
            agent.SetDestination(workplace.position);
            
            while (Vector3.Distance(transform.position, workplace.position) > arrivalDistance)
            {
                yield return new WaitForSeconds(0.5f);
                
                if (agent == null || !agent.isOnNavMesh || workplace == null)
                    yield break;
            }
            
            // Debug.Log($"[NPC] -> {username} est arrivé à son lieu de travail");
        }
        
        Vector3 workDestination;
        if (TryGetRandomNavMeshPosition(workplace.position, workRadius, out workDestination))
        {
            agent.SetDestination(workDestination);
        }
        
        yield return new WaitForSeconds(Random.Range(wanderMinDelay * 2, wanderMaxDelay * 2));
    }

    private IEnumerator JobSearchLoop()
    {
        while (true)
        {
            yield return new WaitForSeconds(jobSearchInterval);
            
            if (jobtype == NPCType.Villager)
            {
                //Debug.Log("[NPC] -> Recherche de travail pour " + username);
                FindJob();
            }
            else if (jobtype == NPCType.Idiot)
            {
                // L'idiot du village a une chance sur 50 de redevenir un villageois
                if (Random.Range(0, 50) == 0)
                {
                    SetJobType(NPCType.Villager);
                    // Debug.Log($"[NPC] -> {username} n'est plus un idiot du village ! Il peut maintenant chercher du travail.");
                }
            }
            else
            {
                // Vérifier si le workplace existe encore
                if (workplace == null || !IsStillAssignedToWorkplace())
                {
                    // Debug.Log($"[NPC] -> {username} n'est plus assigné à son lieu de travail. Retour en villageois.");
                    SetJobType(NPCType.Villager);
                    workplace = null;
                }
            }
            // IMPORTANT : On ne fait plus de yield break ici, la boucle continue toujours
        }
    }

    // Nouvelle méthode pour vérifier si le NPC est toujours assigné à son workplace
    private bool IsStillAssignedToWorkplace()
    {
        if (workplace == null) return false;

        if (jobtype == NPCType.Miner)
        {
            GoldMine mine = workplace.GetComponent<GoldMine>();
            return mine != null && mine.assignedNPCs.Contains(this);
        }
        else if (jobtype == NPCType.Milkman)
        {
            MilkFactory factory = workplace.GetComponent<MilkFactory>();
            return factory != null && factory.assignedNPCs.Contains(this);
        }
        else if (jobtype == NPCType.Mayor)
        {
            TownHall townHall = workplace.GetComponent<TownHall>();
            return townHall != null && townHall.npc == this;
        }

        return false;
    }

    private bool TryGetRandomNavMeshPosition(Vector3 origin, float radius, out Vector3 result)
    {
        for (int i = 0; i < 10; i++)
        {
            Vector3 randomDirection = Random.insideUnitSphere * radius;
            randomDirection += origin;
            if (NavMesh.SamplePosition(randomDirection, out NavMeshHit hit, radius, NavMesh.AllAreas))
            {
                result = hit.position;
                return true;
            }
        }
        result = origin;
        return false;
    }

    public void ApplyMinecraftSkin(string pseudo)
    {
        StartCoroutine(DownloadAndApplySkin(pseudo));
    }

    private IEnumerator DownloadAndApplySkin(string pseudo)
    {
        string url = $"https://mineskin.eu/skin/{pseudo}";

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
                Debug.LogWarning($"Échec du téléchargement de la skin pour {pseudo}: {request.error}");
            }
        }
    }

    private string GetRandomUsernameFromFile()
    {
        string path = Path.Combine(Application.streamingAssetsPath, "players.txt");

        if (File.Exists(path))
        {
            try
            {
                string[] lines = File.ReadAllLines(path);
                if (lines.Length > 0)
                {
                    int index = Random.Range(0, lines.Length);
                    return lines[index].Trim();
                }
            }
            catch (Exception e)
            {
                Debug.LogError($"Erreur lors de la lecture de players.txt: {e.Message}");
            }
        }

        Debug.LogWarning("Aucun pseudo trouvé dans players.txt !");
        return "DefaultUser";
    }

    public void RegisterNPC()
    {
        if(!All.Contains(this))
            All.Add(this);
    }

    public void UnRegisterNPC()
    {
        if (All.Contains(this))
            All.Remove(this);
    }

    public void OnEnable()
    {
        RegisterNPC();
    }

    public void OnDisable()
    {
        UnRegisterNPC();
    }

    private void OnDestroy()
    {
        RemoveFromAssignedJobs();
        
        if (currentHat != null)
        {
            DestroyImmediate(currentHat);
        }
    }

    private void RemoveFromAssignedJobs()
    {
        foreach (GoldMine mine in GoldMine.All)
        {
            if (mine.assignedNPCs.Contains(this))
            {
                mine.assignedNPCs.Remove(this);
            }
        }

        foreach (MilkFactory factory in MilkFactory.All)
        {
            if (factory.assignedNPCs.Contains(this))
            {
                factory.assignedNPCs.Remove(this);
            }
        }
    }

    public NPCType GetJobType()
    {
        return jobtype;
    }

    public void SetJobType(NPCType newType)
    {
        NPCType previousJobType = jobtype;
        
        // Si on change de job, se retirer de l'ancien workplace
        if (previousJobType != newType && workplace != null)
        {
            RemoveFromCurrentWorkplace();
        }
        
        jobtype = newType;
        
        // Si on redevient villageois, reset le workplace
        if (newType == NPCType.Villager || newType == NPCType.Idiot)
        {
            workplace = null;
        }
        
        UpdateHat(previousJobType, newType);
    }

    // Nouvelle méthode pour se retirer du workplace actuel
    private void RemoveFromCurrentWorkplace()
    {
        if (workplace == null) return;

        GoldMine mine = workplace.GetComponent<GoldMine>();
        if (mine != null && mine.assignedNPCs.Contains(this))
        {
            mine.assignedNPCs.Remove(this);
            // Debug.Log($"[NPC] -> {username} s'est retiré de la mine d'or");
        }

        MilkFactory factory = workplace.GetComponent<MilkFactory>();
        if (factory != null && factory.assignedNPCs.Contains(this))
        {
            factory.assignedNPCs.Remove(this);
            // Debug.Log($"[NPC] -> {username} s'est retiré de l'usine de lait");
        }
    }

    public void SetWorkplace(Transform newWorkplace)
    {
        workplace = newWorkplace;
        // Debug.Log($"[NPC] -> {username} a maintenant pour lieu de travail : {workplace.name}");
    }

    private void UpdateHat(NPCType previousType, NPCType newType)
    {
        if (currentHat != null)
        {
            DestroyImmediate(currentHat);
            currentHat = null;
        }

        if (newType != NPCType.Villager && newType != NPCType.Idiot)
        {
            string jobTypeName = newType.ToString();
            
            if (hatDictionary.ContainsKey(jobTypeName) && hatDictionary[jobTypeName] != null && hatSocket != null)
            {
                currentHat = Instantiate(hatDictionary[jobTypeName], hatSocket.transform);
                
                currentHat.transform.localPosition = Vector3.zero;
                currentHat.transform.localRotation = Quaternion.identity;
                currentHat.transform.localScale = Vector3.one;
                
                // Debug.Log($"[NPC] -> {username} porte maintenant un chapeau de {jobTypeName}");
            }
            else
            {
                Debug.LogWarning($"Aucun chapeau trouvé pour le métier {jobTypeName}");
            }
        }
        else if (newType == NPCType.Idiot)
        {
            if (hatDictionary.ContainsKey("Idiot") && hatDictionary["Idiot"] != null && hatSocket != null)
            {
                currentHat = Instantiate(hatDictionary["Idiot"], hatSocket.transform);
                
                currentHat.transform.localPosition = Vector3.zero;
                currentHat.transform.localRotation = Quaternion.identity;
                currentHat.transform.localScale = Vector3.one;
                
                // Debug.Log($"[NPC] -> {username} porte maintenant un chapeau d'idiot du village");
            }
        }
    }
    
    public void FindJob()
    {
        if (jobtype != NPCType.Villager)
        {
            return;
        }
        
        // Chercher dans les mairies
        foreach (TownHall townHall in TownHall.All)
        {
            if (!townHall.npc)
            {
                townHall.npc = this;
                SetJobType(NPCType.Mayor);
                SetWorkplace(townHall.transform);
                // Debug.Log($"[NPC] -> {username} a trouvé un travail de maire !");
                return;
            }
        }

        // Chercher dans les mines d'or
        foreach (GoldMine mine in GoldMine.All)
        {
            if (mine.assignedNPCs.Count < mine.maxNPCs)
            {
                mine.assignedNPCs.Add(this);
                SetJobType(NPCType.Miner);
                SetWorkplace(mine.transform);
                // Debug.Log($"[NPC] -> {username} a trouvé un travail de mineur !");
                return;
            }
        }
        
        // Chercher dans les usines de lait
        foreach (MilkFactory factory in MilkFactory.All)
        {
            if (factory.assignedNPCs.Count < factory.maxNPCs)
            {
                factory.assignedNPCs.Add(this);
                SetJobType(NPCType.Milkman);
                SetWorkplace(factory.transform);
                // Debug.Log($"[NPC] -> {username} a trouvé un travail de laitier !");
                return;
            }
        }
    }

    // Méthodes utilitaires pour gérer les chapeaux depuis l'extérieur
    public void AddJobHat(string jobType, GameObject hatPrefab)
    {
        if (hatDictionary.ContainsKey(jobType))
        {
            hatDictionary[jobType] = hatPrefab;
            // Debug.Log($"[NPC] -> Chapeau mis à jour pour le métier {jobType}");
        }
        else
        {
            hatDictionary.Add(jobType, hatPrefab);
            // Debug.Log($"[NPC] -> Nouveau chapeau ajouté pour le métier {jobType}");
        }
    }

    public void RemoveJobHat(string jobType)
    {
        if (hatDictionary.ContainsKey(jobType))
        {
            hatDictionary.Remove(jobType);
            // Debug.Log($"[NPC] -> Chapeau retiré pour le métier {jobType}");
        }
    }

    public GameObject GetCurrentHat()
    {
        return currentHat;
    }

    public Dictionary<string, GameObject> GetAllJobHats()
    {
        return new Dictionary<string, GameObject>(hatDictionary);
    }

    public bool HasHatForJob(string jobType)
    {
        return hatDictionary.ContainsKey(jobType) && hatDictionary[jobType] != null;
    }

    [ContextMenu("Refresh Hat Dictionary")]
    public void RefreshHatDictionary()
    {
        InitializeHatDictionary();
    }

    // Méthodes utilitaires pour le debugging
    public Transform GetWorkplace()
    {
        return workplace;
    }

    public bool IsAtWorkplace()
    {
        if (workplace == null) return false;
        return Vector3.Distance(transform.position, workplace.position) <= workRadius;
    }

    // Nouvelle méthode publique pour forcer une recherche d'emploi
    public void ForceJobSearch()
    {
        if (jobtype == NPCType.Villager)
        {
            FindJob();
        }
    }

    public string GetDisplayName()
    {
        return username;
    }

    private void OnMouseDown()
    {
        // Si on clique sur un NPC et qu'on n'est pas en train de construire
        if (BuildManager.Instance != null && BuildManager.Instance.IsHoldingBuilding())
            return;
            
        if (UserInterface.Instance != null)
        {
            UserInterface.Instance.SetSelectedNPC(this);
            // Debug.Log($"[NPC] -> NPC sélectionné: {GetDisplayName()}");
        }
    }
}