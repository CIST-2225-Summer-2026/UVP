 public class MoveRobot : MonoBehaviour
{
 
 private float SetHeading()
    {
        // make object robot

        //get rotation component
        rotation = GetPosition.getRotation();
        //get position component
        position = GetPosition.getPosition(); 
        Vector3 moveDirection = new Vector3(rotation.x, rotation.y, rotation.z);
 
        return moveDirection;
    }
private void Update()
    {
        var speed = 1 + GetPosition.getPosition();
        time = GetPosition.getTime();
        Debug.Log("Time: " + time);
        // when time passes x seconds, move robot forward
        async void MoveForward()
        {
            await Task.Delay(1000);
            heading = SetHeading();
            controller.Move(heading * speed * Time.deltaTime);
            Transform.Translate(0, 0, speed * Time.deltaTime);
        }
        MoveForward();
    }

}