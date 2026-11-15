using UnityEngine;

// Fliegt zu einem Zielpunkt und verschwindet dort. Reine Optik fuer Schuesse.
public class FlyingBall : MonoBehaviour
{
    Vector3 target;
    float speed;

    public void Init(Vector3 targetPoint, float flySpeed)
    {
        target = targetPoint;
        speed = flySpeed;
    }

    void Update()
    {
        transform.position = Vector3.MoveTowards(transform.position, target, speed * Time.deltaTime);

        if ((transform.position - target).sqrMagnitude < 0.01f)
            Destroy(gameObject);
    }
}
