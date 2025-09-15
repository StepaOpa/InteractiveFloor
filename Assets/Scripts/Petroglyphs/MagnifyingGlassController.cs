using UnityEngine;

public class MagnifyingGlassController : MonoBehaviour
{
    [Header("Настройки перемещения")]
    [SerializeField] private float moveSpeed = 5f; // Скорость перемещения лупы
    [SerializeField] private RectTransform moveArea; // Прямоугольник (например, панель), в пределах которого лупа может двигаться

    private RectTransform rectTransform; // Трансформ самой лупы

    void Awake()
    {
        rectTransform = GetComponent<RectTransform>();
    }

    // Метод для перемещения лупы, который будет вызываться из кнопок
    public void Move(Vector2 direction)
    {
        // Вычисляем новую позицию
        Vector3 newPosition = rectTransform.anchoredPosition + direction * moveSpeed * Time.deltaTime;

        // Ограничиваем движение в пределах заданной области
        if (moveArea != null)
        {
            Rect areaRect = moveArea.rect;
            // Используем Clamp для ограничения координат
            newPosition.x = Mathf.Clamp(newPosition.x, areaRect.xMin, areaRect.xMax);
            newPosition.y = Mathf.Clamp(newPosition.y, areaRect.yMin, areaRect.yMax);
        }

        rectTransform.anchoredPosition = newPosition;
    }

    // Публичный метод для проверки петроглифа
    // GameManager будет вызывать его при нажатии на кнопку "Принять"
    public void TryToFindPetroglyph()
    {
        // Создаем луч из текущей позиции лупы на экране
        Ray ray = Camera.main.ScreenPointToRay(transform.position);
        RaycastHit hit;

        // Пускаем луч и проверяем, попал ли он во что-то
        if (Physics.Raycast(ray, out hit))
        {
            // Пытаемся получить компонент PetroglyphLocation из объекта, в который попали
            PetroglyphLocation petroglyphLocation = hit.collider.GetComponent<PetroglyphLocation>();

            // Если компонент найден, значит мы попали в петроглиф
            if (petroglyphLocation != null)
            {
                // Находим GameManager на сцене и вызываем у него метод проверки
                // Это не самый производительный способ, но для простоты подойдет.
                // В больших проектах лучше использовать прямую ссылку.
                FindObjectOfType<GameManagerPetroglyphs>().CheckFoundPetroglyph(petroglyphLocation);
            }
        }
    }
}