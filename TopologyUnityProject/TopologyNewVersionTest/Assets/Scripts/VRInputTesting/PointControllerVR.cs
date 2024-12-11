using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.InputSystem;


public class PointControllerVR : MonoBehaviour
{
    // Start is called before the first frame update

    [SerializeField] private InputActionAsset ActionAsset;

    [SerializeField] private InputActionReference JoyStickR;

    
    private void OnEnable() 
    {
        if (ActionAsset != null) 
        {
            ActionAsset.Enable(); 
        } 
    } 

    // Update is called once per frame
    void Update()
    {
        //ActionAsset.Enable(); 
        //Debug.Log(TriggerPressed.action.ReadValue<float>());
        transform.Translate(JoyStickR.action.ReadValue<Vector2>() * Time.deltaTime);
        
    }
}
