using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using TMPro;


public class NewBehaviourScript : MonoBehaviour
{
    public BallShoot Shoot; 
    private float heightangle;
    private float groundangle;
    private float Speed;

    public TMP_InputField height;
    public TMP_InputField horizontal;
    public TMP_InputField speed;
    

    private void FixedUpdate() 
    {
        
        if (!float.TryParse(height.text, out heightangle))
        {
            heightangle = 45f; // Default value
        }

        if (!float.TryParse(horizontal.text, out groundangle))
        {
            groundangle = 0f; 
        }

        if (!float.TryParse(speed.text, out Speed))
        {
            Speed = 10f; 
        }
    }

    public void shoo() 
    {  
        Shoot.SetElevationAngleAndSpeed(heightangle, Speed);
        Shoot.SetHorizontalAngle(groundangle);
        Shoot.ShootProjectile();
    }

    public void Destroy(){
        Shoot.DestroyAllProjectiles();
    }
    
}
