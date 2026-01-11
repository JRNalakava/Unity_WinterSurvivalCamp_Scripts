using UnityEngine;
using System.Collections;
using TMPro; // Needed for Text

public class BuildingZone : MonoBehaviour
{
    [Header("Settings")]
    public int woodRequired = 10;
    public float buildSpeed = 0.1f; // How fast we take wood (lower is faster)
    
    [Header("References")]
    public GameObject ghostVisual; // The translucent red/blue fence
    public GameObject realVisual;  // The solid fence
    public TextMeshPro textDisplay; // "0/10"
    public Transform targetCenter; // where the logs fly to
    
    private int currentWood = 0;
    private float timer;

    void Start()
    {
        // Setup: Show Ghost, Hide Real
        if(ghostVisual != null) ghostVisual.SetActive(true);
        if(realVisual != null) realVisual.SetActive(false);
        UpdateText();
    }

    void OnTriggerStay(Collider other)
    {
        // Check if it's the Player
        if (other.CompareTag("Player"))
        {
            timer += Time.deltaTime;

            // Timer check: Take 1 log every 0.1 seconds
            if (timer >= buildSpeed && currentWood < woodRequired)
            {
                PlayerCollector collector = other.GetComponent<PlayerCollector>();
                
                if (collector != null)
                {
                    // 1. Ask Player for wood
                    GameObject log = collector.RemoveWood();

                    if (log != null)
                    {
                        // 2. Trigger the "Antigravity" Fly Effect
                        StartCoroutine(FlyLogToBuilding(log));
                        
                        // 3. Update Logic
                        currentWood++;
                        UpdateText();
                        timer = 0f;
                        
                        // 4. Check for Completion
                        if (currentWood >= woodRequired)
                        {
                            CompleteBuilding();
                        }
                    }
                }
            }
        }
    }

    // THE ANTIGRAVITY EFFECT
    IEnumerator FlyLogToBuilding(GameObject log)
    {
        Vector3 startPos = log.transform.position;
        Vector3 targetPos = (targetCenter != null) ? targetCenter.position : (transform.position + Vector3.up);
        float flightTime = 0.3f;
        float elapsed = 0;

        while (elapsed < flightTime)
        {
            // Lerp moves it smoothly from A to B
            log.transform.position = Vector3.Lerp(startPos, targetPos, elapsed / flightTime);
            elapsed += Time.deltaTime;
            yield return null;
        }

        // Done flying
        Destroy(log); // Poof! It's used up.
    }

    void UpdateText()
    {
        if (textDisplay != null)
        {
            textDisplay.text = currentWood + " / " + woodRequired;
        }
    }

    void CompleteBuilding()
    {
        if(ghostVisual != null) ghostVisual.SetActive(false);
        if(realVisual != null) realVisual.SetActive(true);
        if(textDisplay != null) textDisplay.gameObject.SetActive(false);
        
        // Disable this collider so we can't put more wood in
        GetComponent<Collider>().enabled = false; 

        // TODO: Notify the Campfire Manager that we are done!
        Debug.Log("Building Complete!");
    }
}