using UnityEngine;

public class PlayerVisualController : MonoBehaviour
{
    [SerializeField] private Animator animator;
    [SerializeField] private Transform pivotGunTransform;
    [SerializeField] private Transform canvasTransform;

    private readonly int isMovingHash = Animator.StringToHash("IsWalking");
    private readonly int isShootingHash = Animator.StringToHash("IsShooting");
    private const float VELOCITY_THRESHOLD = 0.1f;
    private bool isFacingRight = true;
    private Vector3 originalPlayerScale;
    private Vector3 originalGunPivotScale;
    private Vector3 originalCanvasScale;
    private bool init; //Make sure RendererVisuals & UpdateScaleTransform just run after Start() is Call
    private int SHOOTING_LAYER_INDEX = 1;

    private void Start()
    {
        originalPlayerScale = this.transform.localScale;
        originalGunPivotScale = pivotGunTransform.localScale;
        originalCanvasScale = canvasTransform.localScale;
        animator.SetLayerWeight(SHOOTING_LAYER_INDEX, 1);

        init = true; 
    }

    public void RendererVisuals(Vector2 velocity, bool isShooting)
    {
        if (!init) return;
        animator.SetBool(isMovingHash, IsMoving(velocity));
        animator.SetBool(isShootingHash, isShooting);
    }

    private bool IsMoving(Vector2 velocity)
    {
        return velocity.x > VELOCITY_THRESHOLD || velocity.x < -VELOCITY_THRESHOLD;
    }

    public void UpdateScaleTransform(Vector2 velocity)
    {
        if (!init) return;
        if (velocity.x > 0.1f) isFacingRight = true;
        else if (velocity.x < -0.1f) isFacingRight = false;

        SetTransformLocalScaleBaseOnDirection(this.transform, originalPlayerScale);
        SetTransformLocalScaleBaseOnDirection(pivotGunTransform, originalGunPivotScale);
        SetTransformLocalScaleBaseOnDirection(canvasTransform, originalCanvasScale);
    }

    private void SetTransformLocalScaleBaseOnDirection(Transform trans, Vector3 originalScale)
    {
        var xValue = isFacingRight ? originalScale.x : -originalScale.y;
        trans.localScale = new Vector3(xValue, originalScale.y, originalScale.z);
    }
}
