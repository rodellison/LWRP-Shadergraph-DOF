using UnityEngine;
using UnityEngine.Rendering;
using UnityEngine.Rendering.Universal;

public class DepthOfFieldController : MonoBehaviour
{
    private Ray raycast;
    private RaycastHit hit;
    private bool isHit;
    private float hitDistance;
    public Volume volume;
    public float focusSpeed;

    private DepthOfField _depthOfField;

    private bool _dofModeBokeh;
    private bool _dofModeGuassian;
    public bool drawGizmosForDebug;


    public void SetGuassian()
    {
        _depthOfField.mode.value = DepthOfFieldMode.Gaussian;
        _dofModeBokeh = false;
        _dofModeGuassian = true;
    }

    public void SetBokeh()
    {
        _depthOfField.mode.value = DepthOfFieldMode.Bokeh;
        _dofModeGuassian = false;
        _dofModeBokeh = true;
    }

    // Start is called before the first frame update
    void Start()
    {
        volume.profile.TryGet<DepthOfField>(out _depthOfField);

        switch (_depthOfField.mode.value)
        {
            case DepthOfFieldMode.Bokeh:
                _dofModeBokeh = true;
                break;
            case DepthOfFieldMode.Gaussian:
                _dofModeGuassian = true;
                break;
        }
    }

    // Update is called once per frame
    void Update()
    {
        if (Input.GetKey(KeyCode.G))
            SetGuassian();
        if (Input.GetKey(KeyCode.B))
            SetBokeh();

        isHit = false;

        raycast = new Ray(transform.position, transform.forward * 100f);
        if (Physics.Raycast(raycast, out hit, 100f))
        {
            isHit = true;
            hitDistance = Vector3.Distance(transform.position, hit.point);
            //      Debug.Log("Hit");
        }
        else
        {
            if (hitDistance < 100f)
            {
                hitDistance++;
            }
        }

        SetFocus();
    }

    void SetFocus()
    {
        Debug.Log("Setting focal distance to " + hitDistance);
        if (_dofModeBokeh)
        {
            _depthOfField.focusDistance.value = Mathf.Lerp(_depthOfField.focusDistance.value, hitDistance,
                Time.deltaTime * focusSpeed);
        }
        else if (_dofModeGuassian)
        {
            if (hitDistance <= 5)
                _depthOfField.gaussianStart.value = Mathf.Lerp(_depthOfField.gaussianStart.value, 6f,
                    Time.deltaTime * focusSpeed);
            else
            {
                _depthOfField.gaussianStart.value = Mathf.Lerp(_depthOfField.gaussianStart.value,
                    hitDistance + hitDistance * 0.25f,
                    Time.deltaTime * focusSpeed);
                _depthOfField.gaussianEnd.value = _depthOfField.gaussianStart.value + hitDistance * 2f;
            }
        }
    }

    private void OnDrawGizmos()
    {
        if (!drawGizmosForDebug)
            return;

        if (isHit)
        {
            Gizmos.DrawSphere(hit.point, 0.1f);
            Debug.DrawRay(transform.position,
                transform.forward * Vector3.Distance(transform.position, hit.point));
        }
        else
        {
            Debug.DrawRay(transform.position, transform.forward * 100f);
        }
    }
}