//Auteur: Benjamin Roy
//Date: 5 mars 2025
//Description: Ce script gere l'animation du mouvement de la camera dans le menu principal.
using System.Collections;
using UnityEngine;

public class CameraMainMenuAnimationHandler : MonoBehaviour
{
    [SerializeField]
    private float distance = 1f;        //La distance par rapport au point d'origine pour la
                                        //direction de la camera
    [SerializeField]
    private float animationSpeed = 1f;  //La vitesse de l'animation du mouvement
    [SerializeField]
    private Transform cameraLookAt;     //Le transform du gameobjet que la camera regarde constamment

    private Vector3 positionOrigin;     //La position d'origine de la camera
    private Vector3 lastPositionCamera; //La derniere position de la camera avant le debut de la
                                        //nouvelle iteration d'animation
    private Vector3 direction;          //La direction du mouvement de la camera

    // Start is called before the first frame update
    void Start() {
        positionOrigin = transform.position;
        lastPositionCamera = transform.position;
        SetNewDirection();
        StartCoroutine("Animation");
    }

    /// <summary>
    /// Fonction qui gere l'animation du mouvement de la camera
    /// </summary>
    /// <returns></returns>
    private IEnumerator Animation() {
        for (float t = 0; t <= 1f; t += Time.deltaTime * animationSpeed) {
            //Interpolation
            transform.position = Vector3.Slerp(
                lastPositionCamera, 
                direction, t * Mathf.Sin(t * Mathf.PI)
            );
            transform.LookAt(cameraLookAt);
            yield return null;
        }
        
        //Calcule des nouveaux point d'interpolation
        lastPositionCamera = transform.position;
        SetNewDirection();

        StartCoroutine("Animation");
    }

    /// <summary>
    /// Fonction qui gere le calcule de la nouvelle direction du mouvement de la camera
    /// </summary>
    private void SetNewDirection() {
        float x = Random.Range(0f, 1f);
        float y = Random.Range(0f, 1f);
        float z = Random.Range(0f, 1f);
        direction = positionOrigin + (new Vector3(x, y, z)).normalized * distance;
    }
}
