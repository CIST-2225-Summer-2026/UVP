using System.Collections;
using UnityEngine;
 
public class GetPosition : MonoBehaviour
{
    [SerializeField]
 

    private void Start()
    {
        
       // for every x time
       getPosition();
       getRotation();
       getTime();
       Debug.Log("Time: " + getTime());
       sleep(1);

    }
 
    public void getPosition() //template
    {
        // get position of robot
        float[] x = { 0f, 1f, 2f };
        float[] y = { 0f, 1f, 2f };
        float[] z = { 0f, 1f, 2f };
        // print x y and z to console
        Debug.Log("Position: " + x + ", " + y + ", " + z);
    }
 
    public void getRotation() //code for a compass?
    {
        // get the cardinal direction of the robot
        North = Vector3.forward;_mTempVector = mRobotTransform.forward;
        transform.rotation = Quaternion.AngleAxis(_mTempAngle, kReferenceVector);

          if (_mTempVector == Vector3.zero)
        {
            _mTempVector = new Vector3(1, 0, 0);
        }
         _mTempAngle = Mathf.Atan2(_mTempVector.x, _mTempVector.z);
        _mTempAngle = (_mTempAngle * Mathf.Rad2Deg + 90f) * 2f;

         _mTempVector.y = 0f;
        _mTempVector = _mTempVector.normalized;

        // get distance to reference, ensure y equals 0 and normalize
        _mTempVector = _mTempVector - kReferenceVector;
        _mTempVector.y = 0;
        _mTempVector = _mTempVector.normalized;

        // if the distance between the two vectors is 0, this causes an issue with angle computation afterwards  
        if (_mTempVector == Vector3.zero)
        {
            _mTempVector = new Vector3(1, 0, 0);
        }

        // compute the rotation angle in radians and adjust it 
        _mTempAngle = Mathf.Atan2(_mTempVector.x, _mTempVector.z);
        _mTempAngle = (_mTempAngle * Mathf.Rad2Deg + 90f) * 2f;

        // get rotation
        currentAngle = Quaternion.AngleAxis(_mTempAngle, kReferenceVector);
        Debug.Log("Rotation: " + currentAngle);   
    }

    public float getTime()
    {
        return Time.time;

    }
 
    
   
}