using System.Collections;
using UnityEngine;
using UnityEngine.SceneManagement;

// Silently visits CarShop on boot so CarSelectionManager can resolve the saved car's
// sprite from the CarSeller objects here, then hides the shop UI and returns to MainMenu.
public class CarShopAutoReturn : MonoBehaviour
{
    [SerializeField] private GameObject objectToDeactivate;
    [SerializeField] private float delay = 2f;
    [SerializeField] private string mainMenuSceneName = "MainMenu";

    private void Start()
    {
        StartCoroutine(DeactivateThenReturn());
    }

    private IEnumerator DeactivateThenReturn()
    {
        yield return new WaitForSeconds(delay);

        if (objectToDeactivate != null)
        {
            objectToDeactivate.SetActive(false);
        }

        SceneManager.LoadScene(mainMenuSceneName);
    }
}
