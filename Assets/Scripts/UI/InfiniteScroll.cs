using UnityEngine;

public class InfiniteScroll : MonoBehaviour
{
    [SerializeField] private float scrollSpeed = 100f; // Adjust speed
    [SerializeField] private RectTransform[] images; // Assign all image RectTransforms
    [SerializeField] private float imageWidth = 800f; // width of each image

    private void Start()
    {
        // Initially position the container so that the first image is visible
        SetInitialPosition();
    }

    private void Update()
    {
        // Move all images left
        foreach (var img in images)
        {
            img.anchoredPosition += Vector2.left * (scrollSpeed * Time.unscaledDeltaTime);

            // If an image moves off-screen, reposition it
            if (!(img.anchoredPosition.x < (-imageWidth * 2))) continue;
            
            RepositionImage(img);
        }
    }

    private  void SetInitialPosition()
    {
        // Place the first image at the starting position and ensure others are off-screen
        for (int i = 0; i < images.Length; i++)
        {
            float initialPosition = -i * imageWidth;
            images[i].anchoredPosition = new Vector2(initialPosition, images[i].anchoredPosition.y);
        }
    }

    private float GetMaxImageX()
    {
        var maxX = float.MinValue;
        foreach (var img in images)
        {
            if (img.anchoredPosition.x > maxX)
                maxX = img.anchoredPosition.x;
        }
        return maxX;
    }
    
    private void RepositionImage(RectTransform img)
    {
        // Move the image to the right end (resetting it)
        // Find the farthest-right position by looking at all the images
        var maxX = GetMaxImageX();
        img.anchoredPosition = new Vector2(maxX + (imageWidth + 25), img.anchoredPosition.y);
    }
}