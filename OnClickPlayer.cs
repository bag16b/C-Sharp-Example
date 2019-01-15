using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using Photon.Pun;

public class OnClickPlayer : MonoBehaviourPun {
	
	void FixedUpdate () {

        if (photonView.IsMine && Input.GetMouseButton(0))
        {
            RaycastHit hit;
            Ray ray = Camera.main.ScreenPointToRay(Input.mousePosition);

            if (Physics.Raycast(ray, out hit, 90f, 1 << 9 | 1 << 13)) // platforms and solids
            {
                if (hit.collider.tag.Equals("Breakable"))
                {
                    if (hit.collider.gameObject.GetComponent<BreakableManager>().inRange)
                    {
                        hit.collider.gameObject.GetComponent<BreakableManager>().ReduceHealth();
                        //Cmd_blockLoseHealth(hit.collider.gameObject);
                    }
                }
            }
        }
    }


    
}
