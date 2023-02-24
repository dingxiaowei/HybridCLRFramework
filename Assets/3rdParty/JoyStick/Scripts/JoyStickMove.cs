using UnityEngine;
using UnityEngine.EventSystems;

public class JoyStickMove : MonoBehaviour, IBeginDragHandler, IDragHandler, IEndDragHandler
{
    public static JoyStickMove Instance;
    public float maxRadius = 150;
    private RectTransform upperSprite;
    private Vector2 originAnchoredPosition;
    private Vector2 vector2Move = Vector2.zero;
    private bool isDrag = false;

    public delegate void OnMoveStart();
    public event OnMoveStart onMoveStart;
    public delegate void OnMoving(Vector2 vector2Move);
    public event OnMoving onMoving;
    public delegate void OnMoveEnd();
    public event OnMoveEnd onMoveEnd;

    public delegate void OnRotat(float rotatY);
    public event OnRotat onRotat;

    private bool onMoveEndCanTrigger = false;

    void Awake()
    {
        Instance = this;
    }

    void Start()
    {
        this.upperSprite = transform.GetChild(0).GetComponentInChildren<RectTransform>();
        this.originAnchoredPosition = upperSprite.anchoredPosition;
    }

    public void OnDrag(PointerEventData eventData)
    {
        this.upperSprite.anchoredPosition += eventData.delta;
        this.upperSprite.anchoredPosition = Vector2.ClampMagnitude(this.upperSprite.anchoredPosition, this.maxRadius);
        this.vector2Move = this.upperSprite.anchoredPosition / this.maxRadius;
        if (onMoving != null)
        {
            onMoving(this.vector2Move);
        }

        if (onRotat != null)
        {
            onRotat(-Vector2.SignedAngle(new Vector2(0, 1), this.vector2Move));
        }
    }
    public void OnBeginDrag(PointerEventData eventData)
    {
        this.isDrag = true;
        if (this.onMoveStart != null)
        {
            onMoveStart();
        }
    }

    public void OnEndDrag(PointerEventData eventData)
    {
        this.isDrag = false;
        this.upperSprite.anchoredPosition = this.originAnchoredPosition;
        if (onMoveEnd != null)
        {
            onMoveEnd();
        }
    }
#if UNITY_EDITOR 
    public void Update()
    {
        if (!this.isDrag)
        {
            if (onMoving != null)
            {
                this.vector2Move.x = Input.GetAxis("Horizontal");
                this.vector2Move.y = Input.GetAxis("Vertical");
                if (this.vector2Move.x != 0 || this.vector2Move.y != 0)
                {
                    if (onMoveStart != null)
                    {
                        onMoveStart();
                        onMoveEndCanTrigger = true;
                    }
                    if (onMoving != null)
                    {
                        onMoving(this.vector2Move);
                    }
                    if (onRotat != null)
                    {
                        onRotat(-Vector2.SignedAngle(new Vector2(0, 1), this.vector2Move));
                    }
                }
                else
                {
                    if (onMoveEnd != null && onMoveEndCanTrigger)
                    {
                        onMoveEnd();
                        onMoveEndCanTrigger = false;
                    }
                }
            }
        }
    }
#endif
}