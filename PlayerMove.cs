using System.Collections;
using System.Collections.Generic;
using UnityEngine;

using Photon.Pun;

//This Script is for PlayerMovement / when not in drone form
public class PlayerMove :  MonoBehaviourPun {
    public float GroundSpeed;//7
    public float AirSpeed;//4
    public float jumpForce;//21
    public float distToGround;//0.01
    private Rigidbody PlayerRigidbody;
    public GameObject Miner;
    private int jumping;
    private bool jumpVeto;


    private float timer;

    void Start () {
        Cursor.lockState = CursorLockMode.Confined;
        PlayerRigidbody = Miner.GetComponent<Rigidbody>();
        if (photonView.IsMine)
        {
            Camera.main.gameObject.GetComponent<CameraFollow>().player = gameObject.transform.GetChild(0).gameObject;
        }
        jumping = 0;
        jumpVeto = false;
    }


    void FixedUpdate() {
        if (!photonView.IsMine)
        {
            return;
        }


        float xSpeed = Input.GetAxis("Horizontal");
        float jumpVal = Input.GetAxis("Jump");
        float currentSpeed = AirSpeed;
        int layer = 1 << 9 | 1 << 13;

        //bounce glitch insurance
        //if (jumping == 0 && PlayerRigidbody.velocity.y > 0)
        //{
            //PlayerRigidbody.velocity = new Vector3(PlayerRigidbody.velocity.x, 0f, 0f); // ensure we aren't bobbing or bouncing on the ground
            //Debug.Log("NOBOUNCE");
            //jumpVeto = true;
        //}



        //check if there's a block directly down from your left side or on your right side, if so, you can jump, and you move at ground speed.
        //jumpVeto is for bounce glitch
        if (jumpVeto || Physics.Raycast(Miner.transform.position, Vector3.down, distToGround, layer) || Physics.Raycast(new Vector3(Miner.transform.position.x-.2f, Miner.transform.position.y, Miner.transform.position.z), -Vector3.up, distToGround, layer) || Physics.Raycast(new Vector3(Miner.transform.position.x + .2f, Miner.transform.position.y, Miner.transform.position.z), -Vector3.up, distToGround, layer))
        {
            
            if (jumpVal == 1) {
                PlayerRigidbody.AddForce(new Vector3(0f, jumpForce, 0f), ForceMode.Impulse);
                jumping = 1;
            }
            else
            {
                jumping = 0;
            }
            currentSpeed = GroundSpeed;
        }
        if(Input.GetAxis("Vertical") < 0)
        {
            
            timer += Time.deltaTime;
            
        
        }
        else
        {
            timer = 0;
        }
        PlayerRigidbody.velocity = new Vector3(xSpeed*currentSpeed, PlayerRigidbody.velocity.y, 0f);
        Physics.IgnoreLayerCollision(9, 11, (PlayerRigidbody.velocity.y > 0.0f) || (timer > .01f));

        jumpVeto = false;

    }

    

    private void OnDisable()
    {
        PlayerRigidbody.velocity = Vector3.zero;
       
    }
    private void OnEnable()
    {
       
    }
}
