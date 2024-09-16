using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using TMPro;

public class BallShoot : MonoBehaviour
{

    public Transform cannonPoint;
    public GameObject projectilePrefab;
    public GameObject textMeshProPrefab; 
    public LayerMask groundLayer; 
    public float textOffset = 2.0f; 

    private float elevationAngle = 45f; // Default value
    private float projectileSpeed = 10f; // Default value
    private float horizontalAngle = 30f; // Default value

    // public DollyCameraController dollyCamController;

    public Material glowMaterial;

    // Structure to store projectile and associated data
    private class ProjectileData
    {
        public GameObject projectile;
        public Rigidbody rb;
        public LineRenderer lineRenderer;
        public List<Vector3> pathPoints = new List<Vector3>();
        public Coroutine pathCoroutine;
        public float distanceToImpact; 
        public TextMeshPro distanceTextPro; 

        public ProjectileData(GameObject proj, Rigidbody body, LineRenderer lr, TextMeshPro textPro)
        {
            projectile = proj;
            rb = body;
            lineRenderer = lr;
            distanceTextPro = textPro;
        }
    }

    private List<ProjectileData> activeProjectiles = new List<ProjectileData>();

    void Update()
    {
        // Clean up inactive projectiles
        for (int i = activeProjectiles.Count - 1; i >= 0; i--)
        {
            if (activeProjectiles[i].projectile == null)
            {
                activeProjectiles.RemoveAt(i);
            }
        }
    }

   
    public void SetElevationAngleAndSpeed(float angle, float speed)
    {
        elevationAngle = angle;
        projectileSpeed = speed;
    }

    
    public void SetHorizontalAngle(float angle)
    {
        horizontalAngle = angle;
    }

    public void ShootProjectile()
    {
        // Calculate angles in radians
        float elevationInRadians = elevationAngle * Mathf.Deg2Rad;
        float horizontalInRadians = horizontalAngle * Mathf.Deg2Rad;

        // Calculate the initial direction based on angles
        Vector3 direction = new Vector3(
            Mathf.Cos(horizontalInRadians) * Mathf.Cos(elevationInRadians),
            Mathf.Sin(elevationInRadians),
            Mathf.Sin(horizontalInRadians) * Mathf.Cos(elevationInRadians)
        );

        // Instantiate the projectile and get its Rigidbody
        GameObject projectile = Instantiate(projectilePrefab, cannonPoint.position, Quaternion.identity);
        Rigidbody projectileRb = projectile.GetComponent<Rigidbody>();

        // Set initial velocity
        projectileRb.velocity = direction * projectileSpeed;

        // Add a LineRenderer for this projectile
        LineRenderer lineRenderer = projectile.AddComponent<LineRenderer>();
        lineRenderer.startWidth = 0.2f;
        lineRenderer.endWidth = 0.2f;
        lineRenderer.material = glowMaterial;
        lineRenderer.positionCount = 0;

        // Create a gradient for the line renderer
        Gradient gradient = new Gradient();
        gradient.SetKeys(
            new GradientColorKey[] { new GradientColorKey(Color.red, 0.0f), new GradientColorKey(Color.green, 1.0f) },
            new GradientAlphaKey[] { new GradientAlphaKey(1.0f, 0.0f), new GradientAlphaKey(1.0f, 1.0f) }
        );
        lineRenderer.colorGradient = gradient;

        // Check if the prefab is assigned and has a TextMeshPro component
        if (textMeshProPrefab != null)
        {
            GameObject distanceTextObj = Instantiate(textMeshProPrefab, projectile.transform.position, Quaternion.identity);
            TextMeshPro distanceTextPro = distanceTextObj.GetComponent<TextMeshPro>();

            if (distanceTextPro != null)
            {
                distanceTextPro.text = ""; // Initial empty text
                distanceTextPro.color = Color.white;
                distanceTextPro.alignment = TextAlignmentOptions.Center;
                distanceTextPro.fontSize = 24;

                // Create a new projectile data structure and start tracking it
                ProjectileData newProjectileData = new ProjectileData(projectile, projectileRb, lineRenderer, distanceTextPro);
                newProjectileData.pathCoroutine = StartCoroutine(DrawAndKeepPath(newProjectileData, distanceTextPro));

                // Add the projectile data to the active projectiles list
                activeProjectiles.Add(newProjectileData);
            }
            else
            {
                Debug.LogError("TextMeshPro component not found on the instantiated prefab.");
            }
        }
        else
        {
            Debug.LogError("textMeshProPrefab is not assigned in the Inspector.");
        }

        // dollyCamController.StartFollowingPath();
    }

    IEnumerator DrawAndKeepPath(ProjectileData projectileData, TextMeshPro distanceTextPro)
    {
        Vector3 previousPosition = projectileData.projectile.transform.position;
        bool reachedApex = false; // To track if the projectile has reached its highest point
        Vector3 apexPosition = Vector3.zero; // Store the position of the apex
        float sphereCastRadius = 0.5f; // Small radius for the SphereCast

        while (projectileData.projectile != null)
        {
            Vector3 currentPosition = projectileData.projectile.transform.position;

            // Add the current position to the path points list
            projectileData.pathPoints.Add(currentPosition);

            // Update the LineRenderer with the new path
            projectileData.lineRenderer.positionCount = projectileData.pathPoints.Count;
            projectileData.lineRenderer.SetPositions(projectileData.pathPoints.ToArray());

            // Check if the projectile has reached the apex (when vertical velocity becomes negative)
            if (!reachedApex && projectileData.rb.velocity.y <= 0)
            {
                reachedApex = true;
                apexPosition = currentPosition;

                // Set the position of the TextMeshPro at the apex
                distanceTextPro.transform.position = new Vector3(apexPosition.x, apexPosition.y + textOffset, apexPosition.z);
                Debug.Log("Apex reached at position: " + apexPosition);
            }

            // Change line color based on velocity
            float speedMagnitude = projectileData.rb.velocity.magnitude;
            Color lineColor = Color.Lerp(Color.red, Color.green, Mathf.InverseLerp(0, projectileSpeed, speedMagnitude));
            projectileData.lineRenderer.startColor = lineColor;
            projectileData.lineRenderer.endColor = lineColor;

            // Ensure the raycast is cast from previous to current position
            Vector3 rayDirection = currentPosition - previousPosition;
            float rayDistance = rayDirection.magnitude;

            // Visualize the raycast for debugging
            Debug.DrawRay(previousPosition, rayDirection.normalized * rayDistance, Color.yellow, 0.1f);

            // Use SphereCast to detect ground collision more reliably
            if (Physics.SphereCast(previousPosition, sphereCastRadius, rayDirection.normalized, out RaycastHit hit, rayDistance, groundLayer))
            {
                Debug.Log("Projectile hit detected by SphereCast!");

                // Calculate the distance on the XZ plane (ground level)
                Vector3 cannonPointOnGround = new Vector3(cannonPoint.position.x, 0, cannonPoint.position.z);
                Vector3 impactPointOnGround = new Vector3(hit.point.x, 0, hit.point.z);
                projectileData.distanceToImpact = Vector3.Distance(cannonPointOnGround, impactPointOnGround);

                // Update the TextMeshPro with the impact distance but keep it positioned at the apex
                distanceTextPro.text = $"{projectileData.distanceToImpact:F2} m";
                Debug.Log($"Impact Distance: {projectileData.distanceToImpact:F2} meters");

                yield break; // Exit the coroutine after the projectile hits the ground
            }

            previousPosition = currentPosition;

            // Wait for the next update
            yield return new WaitForSeconds(0.05f);
        }
    }

    public void DestroyAllProjectiles()
    {
        // Loop through all active projectiles and destroy them
        for (int i = activeProjectiles.Count - 1; i >= 0; i--)
        {
            DestroyProjectile(activeProjectiles[i]);
        }

        // Clear the list after destruction
        activeProjectiles.Clear();
        // dollyCamController.ReturnToInitialPosition();
    }

    private void DestroyProjectile(ProjectileData projectileData)
    {
        // Stop the coroutine and destroy the projectile
        if (projectileData.pathCoroutine != null)
        {
            StopCoroutine(projectileData.pathCoroutine);
        }

        if (projectileData.projectile != null)
        {
            Destroy(projectileData.projectile); // Destroy the projectile game object
        }

        // Destroy the TextMeshPro object as well
        if (projectileData.distanceTextPro != null)
        {
            Destroy(projectileData.distanceTextPro.gameObject); // Destroy the TextMeshPro GameObject
        }
    }
}
